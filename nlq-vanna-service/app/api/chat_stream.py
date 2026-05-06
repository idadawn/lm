"""SSE chat stream endpoint.

POST /api/v1/chat/stream

Streams ReasoningStep events while Vanna generates and executes SQL, then
emits a response_metadata envelope and a terminal done event.

ADR-3 SSE mapping:
  spec       — DDL / Q&A knowledge retrieved from Qdrant (emitted by QdrantStoreMixin)
  rule       — terminology / judgment_rule docs (emitted by QdrantStoreMixin)
  condition  — WHERE-clause predicates (extract_conditions, capped at 8)
  grade      — grade-type result terminal step
  record     — generic result terminal step
  fallback   — any error path

Architect A6 / MUST-C:
  Vanna generate_sql runs inside asyncio.to_thread (worker thread).
  The emitter's put_step() uses loop.call_soon_threadsafe to safely
  deliver steps across the thread boundary into the event loop queue.
"""

from __future__ import annotations

import asyncio
import logging
from typing import AsyncIterator
from uuid import uuid4

from fastapi import APIRouter, Depends, Request
from sse_starlette.sse import EventSourceResponse

from app.api.schemas import ChatRequest, ReasoningStep, StreamEvent
from app.adapters.mysql_runner import _check_sql_allowed
from app.deps import get_vanna_app, verify_token
from app.reasoning.emitter import ReasoningEmitter
from app.reasoning.mapper import build_terminal_step
from app.reasoning.sql_parser import extract_conditions

logger = logging.getLogger(__name__)

router = APIRouter()


@router.post("/api/v1/chat/stream", dependencies=[Depends(verify_token)])
async def chat_stream(
    req: ChatRequest,
    request: Request,
    vn=Depends(get_vanna_app),
) -> EventSourceResponse:
    """Stream reasoning steps + final metadata for a natural-language question.

    SSE event format:
        event: message
        data: <StreamEvent JSON>

    Terminal event:
        event: message
        data: {"type": "done"}
    """
    loop = asyncio.get_running_loop()
    emitter = ReasoningEmitter(loop=loop)

    async def event_generator() -> AsyncIterator[dict]:  # type: ignore[type-arg]
        question = req.messages[-1].content
        try:
            vn.attach_emitter(emitter)

            # ----------------------------------------------------------------
            # Phase 1: generate SQL (runs in worker thread via to_thread)
            # QdrantStoreMixin will call emitter.put_step() from that thread
            # using call_soon_threadsafe — safe because of ReasoningEmitter.put_step
            # ----------------------------------------------------------------
            sql_task: asyncio.Task[str] = asyncio.create_task(
                asyncio.to_thread(vn.generate_sql, question)
            )

            # Consume reasoning steps emitted during SQL generation
            async for step in emitter.aiter_until_done(sql_task):
                yield {
                    "event": "message",
                    "data": StreamEvent(
                        type="reasoning_step", reasoning_step=step
                    ).model_dump_json(exclude_none=True),
                }

            raw_sql: str = await sql_task

            # ----------------------------------------------------------------
            # Phase 1.5: 清洗 LLM 输出
            # - 剥末尾 ; 与注释（白名单不允许 ; 但末尾分号是合法习惯）
            # - 剥 markdown ```sql ... ``` 代码围栏
            # - 检查是否真是 SQL（首词 SELECT）；不是则发显式 fallback 后早退
            # ----------------------------------------------------------------
            sql = raw_sql.strip()
            # markdown 代码围栏
            if sql.startswith("```"):
                lines = sql.splitlines()
                # 首行 ``` 或 ```sql 去掉，末行 ``` 去掉
                if lines and lines[0].lstrip("`").strip().lower() in ("", "sql", "mysql"):
                    lines = lines[1:]
                if lines and lines[-1].strip().startswith("```"):
                    lines = lines[:-1]
                sql = "\n".join(lines).strip()
            # 末尾分号（LLM 习惯加），白名单不允许 ; 出现在 SQL 内部，但末尾去掉无害
            sql = sql.rstrip().rstrip(";").rstrip()

            # 不以 SELECT 起手 → LLM 没产 SQL（多半返回自然语言解释）
            first_word = (sql.split()[0].upper() if sql.split() else "")
            if first_word != "SELECT":
                explain_step = ReasoningStep(
                    kind="fallback",
                    label="模型未生成有效 SQL",
                    detail=raw_sql[:300],
                    meta={"reason": "non_select_response"},
                )
                emitter.put_step(explain_step)
                yield {
                    "event": "message",
                    "data": StreamEvent(
                        type="reasoning_step", reasoning_step=explain_step
                    ).model_dump_json(exclude_none=True),
                }
                # 走 metadata 仍把推理链返回，让 UI 展示推理过程
                session_id = req.session_id or uuid4().hex
                meta = StreamEvent(
                    type="response_metadata",
                    response_payload={
                        "session_id": session_id,
                        "response": "模型回复了自然语言而非 SQL，请尝试更具体的提问（包含字段名/表名/时间范围）",
                        "data": [],
                        "reasoning_steps": [
                            s.model_dump(exclude_none=True) for s in emitter.snapshot()
                        ],
                        "context": {"sql": "", "row_count": 0, "raw_response": raw_sql[:500]},
                    },
                )
                yield {
                    "event": "message",
                    "data": meta.model_dump_json(exclude_none=True),
                }
                return  # 不进 phase 2-5

            # ----------------------------------------------------------------
            # Phase 2: parse WHERE clause → condition steps (same coroutine)
            # _enqueue used directly (no thread boundary here)
            # ----------------------------------------------------------------
            for cond in extract_conditions(sql):
                emitter.put_step(cond)
                yield {
                    "event": "message",
                    "data": StreamEvent(
                        type="reasoning_step", reasoning_step=cond
                    ).model_dump_json(exclude_none=True),
                }

            # ----------------------------------------------------------------
            # Phase 3: execute SQL (worker thread, no emitter calls)
            # ----------------------------------------------------------------
            import pandas as pd  # noqa: PLC0415

            _check_sql_allowed(sql)  # enforce whitelist before execution

            # 单独捕获 SQL 执行错误（列不存在 / 类型错 / 语法错），把 SQL 与错误
            # 详情作为 fallback step + response_metadata 返回，便于用户理解为何失败
            try:
                df: pd.DataFrame = await asyncio.to_thread(vn.run_sql, sql)
            except Exception as exec_exc:  # noqa: BLE001
                logger.exception("SQL execution failed: %s", exec_exc)
                err_msg = str(exec_exc)
                exec_err_step = ReasoningStep(
                    kind="fallback",
                    label="SQL 执行失败",
                    detail=err_msg[:500],
                    meta={"sql": sql},
                )
                emitter.put_step(exec_err_step)
                yield {
                    "event": "message",
                    "data": StreamEvent(
                        type="reasoning_step", reasoning_step=exec_err_step
                    ).model_dump_json(exclude_none=True),
                }
                session_id = req.session_id or uuid4().hex
                meta = StreamEvent(
                    type="response_metadata",
                    response_payload={
                        "session_id": session_id,
                        "response": "SQL 执行失败：" + err_msg[:200],
                        "data": [],
                        "reasoning_steps": [
                            s.model_dump(exclude_none=True) for s in emitter.snapshot()
                        ],
                        "context": {
                            "sql": sql,
                            "row_count": 0,
                            "error": err_msg[:500],
                        },
                    },
                )
                yield {
                    "event": "message",
                    "data": meta.model_dump_json(exclude_none=True),
                }
                return

            # ----------------------------------------------------------------
            # Phase 4: terminal step (record / grade / fallback)
            # ----------------------------------------------------------------
            terminal: ReasoningStep = build_terminal_step(df, question, sql)
            emitter.put_step(terminal)
            yield {
                "event": "message",
                "data": StreamEvent(
                    type="reasoning_step", reasoning_step=terminal
                ).model_dump_json(exclude_none=True),
            }

            # ----------------------------------------------------------------
            # Phase 5: response_metadata envelope
            # ----------------------------------------------------------------
            session_id = req.session_id or uuid4().hex
            # 转 JSON-safe 行数据（处理 NaN / datetime / Decimal），上限 200 行
            import json as _json  # noqa: PLC0415

            data_rows = _json.loads(
                df.head(200).to_json(
                    orient="records", date_format="iso", force_ascii=False
                )
            )
            metadata = StreamEvent(
                type="response_metadata",
                response_payload={
                    "session_id": session_id,
                    "response": _summarize(df),
                    "data": data_rows,
                    "reasoning_steps": [
                        s.model_dump(exclude_none=True) for s in emitter.snapshot()
                    ],
                    "context": {
                        "sql": sql,
                        "row_count": len(df),
                        "columns": list(df.columns),
                    },
                },
            )
            yield {
                "event": "message",
                "data": metadata.model_dump_json(exclude_none=True),
            }

        except Exception as exc:  # noqa: BLE001
            logger.exception("chat_stream failed: %s", exc)
            err_step = ReasoningStep(
                kind="fallback",
                label="生成失败",
                detail=None,
            )
            yield {
                "event": "message",
                "data": StreamEvent(
                    type="reasoning_step", reasoning_step=err_step
                ).model_dump_json(exclude_none=True),
            }
            yield {
                "event": "message",
                "data": StreamEvent(
                    type="error", error="生成失败，请重试或联系管理员"
                ).model_dump_json(exclude_none=True),
            }

        finally:
            vn.detach_emitter()
            emitter.close()

            # done — payload is {"type":"done"} per plan ADR-3 (not "[DONE]")
            yield {
                "event": "message",
                "data": StreamEvent(type="done").model_dump_json(  # line 149
                    exclude_none=True
                ),
            }

    return EventSourceResponse(event_generator())


# ---------------------------------------------------------------------------
# Private helpers
# ---------------------------------------------------------------------------


def _summarize(df: "pd.DataFrame") -> str:
    """One-line human summary of query result for response_payload.response."""
    row_count = len(df)
    if row_count == 0:
        return "查询无结果。"
    if row_count == 1:
        return "查询命中 1 条记录。"
    return f"查询命中 {row_count} 条记录。"
