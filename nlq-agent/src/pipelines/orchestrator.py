"""
Pipeline 编排器

负责串联 Stage 1 和 Stage 2，管理完整的问答流程。
根据意图分类结果决定是否需要执行 Stage 2。
"""

from __future__ import annotations

import logging
from typing import AsyncIterator

from src.models.schemas import (
    ChatRequest,
    IntentType,
    ReasoningStep,
    ReasoningStepKind,
)
from src.pipelines.stage1.semantic_kg_agent import SemanticKGAgent
from src.pipelines.stage2.data_sql_agent import DataSQLAgent
from src.services.database import DatabaseService
from src.services.llm_client import LLMClient
from src.services.qdrant_service import QdrantService
from src.services.sse_emitter import SSEEmitter
from src.utils.prompts import CONCEPTUAL_ANSWER_SYSTEM, CONCEPTUAL_ANSWER_USER

logger = logging.getLogger(__name__)


class PipelineOrchestrator:
    """
    两阶段 Pipeline 编排器。

    根据意图分类结果，选择不同的执行路径：
    - statistical / root_cause → Stage 1 + Stage 2
    - conceptual → Stage 1 only（直接回答）
    - out_of_scope → Fallback
    """

    def __init__(
        self,
        llm: LLMClient,
        qdrant: QdrantService,
        db: DatabaseService,
    ) -> None:
        self._llm = llm
        self._qdrant = qdrant
        self._db = db

    async def stream_chat(self, request: ChatRequest) -> AsyncIterator[str]:
        """
        处理用户请求，返回 SSE 事件流。

        这是整个系统的入口方法，由 FastAPI 路由调用。

        Args:
            request: 用户请求

        Yields:
            格式化后的 SSE 事件字符串
        """
        # 提取最后一条用户消息作为当前问题
        question = ""
        for msg in reversed(request.messages):
            if msg.role == "user":
                question = msg.content
                break

        if not question:
            yield self._make_error_event("未找到用户问题")
            return

        # 创建 SSE 发射器（每次请求独立实例）
        emitter = SSEEmitter()

        try:
            # ── Stage 1: 语义解析与图谱检索 ──────────────────
            stage1 = SemanticKGAgent(self._llm, self._qdrant, emitter)
            context, stage1_events = await stage1.run(question)

            # 逐个 yield Stage 1 的事件
            for event in stage1_events:
                yield event

            # ── 根据意图决定后续路径 ─────────────────────────
            intent_type = context.intent.intent

            if intent_type == IntentType.OUT_OF_SCOPE:
                # 超出范围：直接 fallback
                fallback = ReasoningStep(
                    kind=ReasoningStepKind.FALLBACK,
                    label="问题超出系统范围",
                    detail="本系统仅支持实验室数据分析相关问题",
                )
                yield emitter.emit_reasoning_step(fallback)
                yield emitter.emit_text(
                    "抱歉，您的问题超出了本系统的服务范围。"
                    "本系统专注于实验室检测数据的统计分析，"
                    "包括合格率、铁损、叠片系数等指标的查询和分析。"
                )
                yield emitter.emit_response_metadata()
                yield emitter.emit_done()
                return

            elif intent_type == IntentType.CONCEPTUAL:
                # 概念解释类：Stage 1 直接回答，跳过 Stage 2
                answer_events = await self._conceptual_answer(
                    question, context, emitter
                )
                for event in answer_events:
                    yield event
                yield emitter.emit_response_metadata()
                yield emitter.emit_done()
                return

            else:
                # 统计类 / 根因类：执行 Stage 2
                stage2 = DataSQLAgent(self._llm, self._db, emitter)
                stage2_events = await stage2.run(context)

                for event in stage2_events:
                    yield event

                yield emitter.emit_done()

        except Exception as e:
            logger.exception("Pipeline 执行异常: %s", e)
            yield emitter.emit_error(f"系统内部错误: {e}")
            yield emitter.emit_done()

    async def _conceptual_answer(
        self,
        question: str,
        context,
        emitter: SSEEmitter,
    ) -> list[str]:
        """为概念解释类问题生成回答。"""
        events = []

        # 格式化检索到的文档
        docs_text = "\n\n".join(
            f"- {doc.get('text', '')}"
            for doc in context.retrieved_documents
        )

        messages = [
            {"role": "system", "content": CONCEPTUAL_ANSWER_SYSTEM},
            {
                "role": "user",
                "content": CONCEPTUAL_ANSWER_USER.format(
                    question=question,
                    retrieved_docs=docs_text or "（未检索到相关文档）",
                ),
            },
        ]

        # 流式输出
        async for chunk in self._llm.chat_stream(messages):
            events.append(emitter.emit_text(chunk))

        return events

    def _make_error_event(self, message: str) -> str:
        """生成错误 SSE 事件。"""
        import json

        return f'data: {json.dumps({"type": "error", "error": message}, ensure_ascii=False)}\n\n'
