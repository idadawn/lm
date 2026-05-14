"""
Pipeline 编排器

负责串联 Stage 1 和 Stage 2，管理完整的问答流程。
根据意图分类结果决定是否需要执行 Stage 2。
"""

from __future__ import annotations

import logging
import time
from typing import AsyncIterator

from src.core.metrics import (
    ACTIVE_CHAT_STREAMS,
    CHAT_STREAM_DURATION_SECONDS,
    inc_error_count,
    inc_intent_count,
)
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

logger = logging.getLogger(__name__)


class PipelineOrchestrator:
    """
    两阶段 Pipeline 编排器。

    根据意图分类结果，选择不同的执行路径：
    - statistical / trend / root_cause / by_shift / conceptual → Stage 1 + Stage 2
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
        start = time.monotonic()
        ACTIVE_CHAT_STREAMS.inc()
        intent_type = "unknown"
        status = "ok"

        # 提取最后一条用户消息作为当前问题
        question = ""
        for msg in reversed(request.messages):
            if msg.role == "user":
                question = msg.content
                break

        if not question:
            yield self._make_error_event("未找到用户问题")
            ACTIVE_CHAT_STREAMS.dec()
            elapsed = time.monotonic() - start
            inc_intent_count("unknown", "ok")
            CHAT_STREAM_DURATION_SECONDS.observe(elapsed)
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
            intent_type = context.intent.intent.value
            status = "ok"

            if intent_type == IntentType.OUT_OF_SCOPE.value:
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

            else:
                # 统计类 / 根因类 / 概念类：执行 Stage 2
                stage2 = DataSQLAgent(self._llm, self._db, emitter)
                stage2_events = await stage2.run(context)

                for event in stage2_events:
                    yield event

                yield emitter.emit_done()

        except Exception as e:
            status = "error"
            error_code = type(e).__name__
            inc_error_count(error_code)
            logger.exception("Pipeline 执行异常: %s", e)
            yield emitter.emit_error(f"系统内部错误: {e}")
            yield emitter.emit_done()

        finally:
            elapsed = time.monotonic() - start
            ACTIVE_CHAT_STREAMS.dec()
            inc_intent_count(intent_type, status)
            CHAT_STREAM_DURATION_SECONDS.observe(elapsed)

    def _make_error_event(self, message: str) -> str:
        """生成错误 SSE 事件。"""
        import json

        return f'data: {json.dumps({"type": "error", "error": message}, ensure_ascii=False)}\n\n'
