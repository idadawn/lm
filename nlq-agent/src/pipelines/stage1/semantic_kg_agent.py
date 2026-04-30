"""
Stage 1: Semantic & KG Agent（语义解析与图谱检索）

职责：
1. 意图分类 — 判断用户问题属于哪种类型
2. 向量检索 — 从 Qdrant 检索相关的业务知识
3. 语义解析 — 提取结构化过滤条件和指标定义
4. 推理链输出 — 通过 SSE 实时推送 reasoning_step

输出：AgentContext（传递给 Stage 2）
"""

from __future__ import annotations

import json
import logging
import re
from typing import Any, AsyncIterator

from src.core.settings import get_settings
from src.models.schemas import (
    AgentContext,
    FilterCondition,
    IntentClassification,
    IntentType,
    MetricDefinition,
    ReasoningStep,
    ReasoningStepKind,
)
from src.services.embedding_client import EmbeddingClient
from src.services.llm_client import LLMClient
from src.services.qdrant_service import QdrantService
from src.services.sse_emitter import SSEEmitter
from src.utils.prompts import (
    INTENT_CLASSIFICATION_SYSTEM,
    INTENT_CLASSIFICATION_USER,
    STAGE1_SEMANTIC_SYSTEM,
    STAGE1_SEMANTIC_USER,
)

logger = logging.getLogger(__name__)

# Trend 关键词 — 用于 _classify_intent 的关键词优先路由
_TREND_KEYWORDS = frozenset({"趋势", "变化趋势", "走势", "环比", "同比"})
_TREND_WINDOW_RE = re.compile(r"(?:近|最近)\s*(\d+)\s*(?:个?月|周|季度|年)")

# Root cause 关键词 — 用于 _classify_intent 的关键词优先路由
_ROOT_CAUSE_KEYWORDS = frozenset({"为什么", "原因", "下降", "为何", "找出原因", "不合格", "异常", "偏低"})
_ROOT_CAUSE_PERIOD_RE = re.compile(r"(?:上|前)\s*(\d+)\s*(?:个?月)")

# BY_SHIFT 关键词 — 班次对比类查询优先路由
_SHIFT_KEYWORDS = frozenset({"班次", "白班", "晚班", "早班", "中班", "夜班", "不同班次", "各班次"})
_SHIFT_WINDOW_RE = re.compile(r"(?:近|最近)\s*(\d+)\s*(?:个?月|周|季度|年)")


class SemanticKGAgent:
    """
    Stage 1 Agent：语义解析与知识图谱检索。

    处理流程：
        用户问题
          ↓
        [1] 意图分类（LLM）
          ↓
        [2] Qdrant 向量检索（规则 + 规格 + 指标）
          ↓
        [3] 语义解析（LLM）— 提取过滤条件和指标定义
          ↓
        AgentContext + reasoning_steps
    """

    def __init__(
        self,
        llm: LLMClient,
        qdrant: QdrantService,
        emitter: SSEEmitter,
    ) -> None:
        self._llm = llm
        self._qdrant = qdrant
        self._emitter = emitter

    async def run(self, question: str) -> tuple[AgentContext, list[str]]:
        """
        执行 Stage 1 完整流程。

        Args:
            question: 用户原始问题

        Returns:
            (AgentContext, sse_events) 元组
            - AgentContext: 传递给 Stage 2 的上下文
            - sse_events: 已格式化的 SSE 事件字符串列表
        """
        sse_events: list[str] = []

        # ── Step 1: 意图分类 ─────────────────────────────────
        intent = await self._classify_intent(question)
        logger.info("意图分类: %s (%.2f)", intent.intent, intent.confidence)

        # ── Step 2: Qdrant 向量检索 ──────────────────────────
        retrieved = await self._retrieve_knowledge(question, intent)

        # 发射检索到的知识步骤
        for doc_type, docs in retrieved.items():
            if docs:
                kind = self._doc_type_to_step_kind(doc_type)
                for doc in docs[:2]:  # 每类最多展示 2 条
                    step = ReasoningStep(
                        kind=kind,
                        label=doc.get("doc_id", doc_type),
                        detail=doc.get("text", "")[:200],
                        meta={"score": doc.get("score", 0)},
                    )
                    sse_events.append(self._emitter.emit_reasoning_step(step))

        # ── Step 3: 语义解析 ─────────────────────────────────
        if intent.intent == IntentType.CONCEPTUAL:
            # 概念解释类：不需要 Stage 2，直接构建简单上下文
            context = AgentContext(
                user_question=question,
                intent=intent,
                business_explanation=self._merge_doc_texts(retrieved),
                retrieved_documents=self._flatten_docs(retrieved),
                reasoning_steps=self._emitter.get_all_steps(),
            )
        else:
            # 统计/根因类：需要提取过滤条件和指标定义
            parsed = await self._parse_semantics(question, intent, retrieved)

            # 发射 condition 步骤（此时只有 expected，actual 由 Stage 2 回填）
            for f in parsed.get("filters", []):
                step = ReasoningStep(
                    kind=ReasoningStepKind.CONDITION,
                    label=f.get("display_name", f.get("field", "")),
                    field=f.get("field", ""),
                    expected=f"{f.get('operator', '')} {f.get('value', '')}",
                    detail=f"单位: {f.get('unit', 'N/A')}",
                )
                sse_events.append(self._emitter.emit_reasoning_step(step))

            context = AgentContext(
                user_question=question,
                intent=intent,
                business_explanation=parsed.get("business_explanation", ""),
                filters=[
                    FilterCondition(**f) for f in parsed.get("filters", [])
                ],
                metrics=[
                    MetricDefinition(**m) for m in parsed.get("metrics", [])
                ],
                retrieved_documents=self._flatten_docs(retrieved),
                reasoning_steps=self._emitter.get_all_steps(),
            )

        return context, sse_events

    # ── 内部方法 ─────────────────────────────────────────────

    async def _classify_intent(self, question: str) -> IntentClassification:
        """调用 LLM 进行意图分类，root_cause / trend / by_shift 关键词优先路由。"""
        # keyword-based pre-check for root_cause intent
        if any(kw in question for kw in _ROOT_CAUSE_KEYWORDS):
            match = _ROOT_CAUSE_PERIOD_RE.search(question)
            time_window = int(match.group(1)) if match else 1
            return IntentClassification(
                intent=IntentType.ROOT_CAUSE,
                confidence=0.9,
                extracted_entities={
                    "time_window": time_window,
                    "metric": "合格率",
                    "dimension_keys": ["F_PRODUCT_SPEC_CODE", "F_CREATORUSERID"],
                },
                reasoning=f"关键词匹配根因分析类查询，时间窗口={time_window}个月",
            )

        # keyword-based pre-check for trend intent
        if any(kw in question for kw in _TREND_KEYWORDS):
            match = _TREND_WINDOW_RE.search(question)
            time_window = int(match.group(1)) if match else 6
            return IntentClassification(
                intent=IntentType.TREND,
                confidence=0.9,
                extracted_entities={
                    "time_window": time_window,
                    "metric": "合格率",
                    "group_by": "产品规格",
                },
                reasoning=f"关键词匹配趋势类查询，时间窗口={time_window}个月",
            )

        # keyword-based pre-check for by_shift intent
        if any(kw in question for kw in _SHIFT_KEYWORDS):
            match = _SHIFT_WINDOW_RE.search(question)
            time_window = int(match.group(1)) if match else 1
            return IntentClassification(
                intent=IntentType.BY_SHIFT,
                confidence=0.9,
                extracted_entities={
                    "time_window": time_window,
                    "metric": "合格率",
                },
                reasoning=f"关键词匹配班次对比类查询，时间窗口={time_window}个月",
            )

        try:
            result = await self._llm.chat_json([
                {"role": "system", "content": INTENT_CLASSIFICATION_SYSTEM},
                {
                    "role": "user",
                    "content": INTENT_CLASSIFICATION_USER.format(question=question),
                },
            ])
            return IntentClassification(
                intent=IntentType(result.get("intent", "out_of_scope")),
                confidence=float(result.get("confidence", 0.5)),
                extracted_entities=result.get("extracted_entities", {}),
                reasoning=result.get("reasoning", ""),
            )
        except Exception as e:
            logger.warning("意图分类失败，使用默认值: %s", e)
            return IntentClassification(
                intent=IntentType.STATISTICAL,
                confidence=0.3,
                reasoning=f"分类失败，默认使用统计类: {e}",
            )

    async def _retrieve_knowledge(
        self,
        question: str,
        intent: IntentClassification,
    ) -> dict[str, list[dict[str, Any]]]:
        """根据意图类型，从 Qdrant 检索相关知识。"""
        settings = get_settings()

        if intent.intent == IntentType.CONCEPTUAL:
            # 概念类：全量检索
            return await self._qdrant.search_multi_collection(question)

        elif intent.intent == IntentType.ROOT_CAUSE:
            # 根因类：优先检索规则
            results = {}
            results[settings.collection_rules] = await self._qdrant.search(
                settings.collection_rules, question, top_k=5
            )
            results[settings.collection_specs] = await self._qdrant.search(
                settings.collection_specs, question, top_k=3
            )
            return results

        else:
            # 统计类：优先检索指标和规格
            results = {}
            results[settings.collection_metrics] = await self._qdrant.search(
                settings.collection_metrics, question, top_k=5
            )
            results[settings.collection_specs] = await self._qdrant.search(
                settings.collection_specs, question, top_k=3
            )
            results[settings.collection_rules] = await self._qdrant.search(
                settings.collection_rules, question, top_k=2
            )
            return results

    async def _parse_semantics(
        self,
        question: str,
        intent: IntentClassification,
        retrieved: dict[str, list[dict[str, Any]]],
    ) -> dict[str, Any]:
        """调用 LLM 进行语义解析，提取过滤条件和指标定义。"""
        docs_text = self._format_docs_for_prompt(retrieved)

        try:
            result = await self._llm.chat_json([
                {"role": "system", "content": STAGE1_SEMANTIC_SYSTEM},
                {
                    "role": "user",
                    "content": STAGE1_SEMANTIC_USER.format(
                        question=question,
                        intent_type=intent.intent.value,
                        entities=json.dumps(
                            intent.extracted_entities, ensure_ascii=False
                        ),
                        retrieved_docs=docs_text,
                    ),
                },
            ])
            return result
        except Exception as e:
            logger.error("语义解析失败: %s", e)
            return {
                "business_explanation": "语义解析暂时不可用，将尝试直接查询。",
                "filters": [],
                "metrics": [],
            }

    def _doc_type_to_step_kind(self, collection_name: str) -> ReasoningStepKind:
        """将 Collection 名称映射为 ReasoningStep kind。"""
        settings = get_settings()
        mapping = {
            settings.collection_rules: ReasoningStepKind.RULE,
            settings.collection_specs: ReasoningStepKind.SPEC,
            settings.collection_metrics: ReasoningStepKind.SPEC,
        }
        return mapping.get(collection_name, ReasoningStepKind.RECORD)

    def _merge_doc_texts(
        self, retrieved: dict[str, list[dict[str, Any]]]
    ) -> str:
        """合并所有检索文档的文本。"""
        texts = []
        for docs in retrieved.values():
            for doc in docs:
                texts.append(doc.get("text", ""))
        return "\n\n".join(texts)

    def _flatten_docs(
        self, retrieved: dict[str, list[dict[str, Any]]]
    ) -> list[dict[str, Any]]:
        """将多 Collection 的检索结果展平为列表。"""
        flat = []
        for collection, docs in retrieved.items():
            for doc in docs:
                flat.append({**doc, "collection": collection})
        return flat

    def _format_docs_for_prompt(
        self, retrieved: dict[str, list[dict[str, Any]]]
    ) -> str:
        """将检索结果格式化为 prompt 注入文本。"""
        parts = []
        for collection, docs in retrieved.items():
            if docs:
                parts.append(f"### {collection}")
                for i, doc in enumerate(docs, 1):
                    parts.append(
                        f"{i}. [{doc.get('doc_id', 'unknown')}] "
                        f"(相似度: {doc.get('score', 0):.3f})\n"
                        f"   {doc.get('text', '')}"
                    )
        return "\n\n".join(parts) if parts else "（未检索到相关文档）"
