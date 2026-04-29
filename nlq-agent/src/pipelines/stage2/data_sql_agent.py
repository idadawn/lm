"""
Stage 2: Data & SQL Agent（数据查询与分析）

职责：
1. SQL 生成 — 基于 AgentContext 和 DDL 生成安全的只读 SQL
2. SQL 执行 — 在 MySQL 中执行查询
3. SQL 修正 — 如果执行失败，尝试自动修正（最多重试 2 次）
4. 回答生成 — 基于查询结果和业务上下文生成最终回答
5. 推理链回填 — 更新 condition 步骤的 actual/satisfied 值

输入：AgentContext（来自 Stage 1）
输出：SSE 事件流（text + reasoning_step + response_metadata）
"""

from __future__ import annotations

import json
import logging
from typing import Any

from src.core.settings import get_settings
from src.models.ddl import METRIC_SQL_TEMPLATES, get_all_ddl
from src.models.schemas import (
    AgentContext,
    IntentType,
    ReasoningStep,
    ReasoningStepKind,
)
from src.services.database import DatabaseService
from src.services.llm_client import LLMClient
from src.services.sse_emitter import SSEEmitter
from src.utils.prompts import (
    FINAL_ANSWER_SYSTEM,
    FINAL_ANSWER_USER,
    STAGE2_SQL_CORRECTION_SYSTEM,
    STAGE2_SQL_CORRECTION_USER,
    STAGE2_SQL_GENERATION_SYSTEM,
    STAGE2_SQL_GENERATION_USER,
)

logger = logging.getLogger(__name__)

# SQL 修正最大重试次数
MAX_SQL_RETRIES = 2


class DataSQLAgent:
    """
    Stage 2 Agent：数据查询与分析。

    处理流程：
        AgentContext（来自 Stage 1）
          ↓
        [1] SQL 生成（LLM + DDL + AgentContext）
          ↓
        [2] SQL 安全验证
          ↓
        [3] SQL 执行（MySQL）
          ↓  ← 失败则进入修正循环 →
        [4] condition 步骤回填
          ↓
        [5] 最终回答生成（LLM 流式输出）
          ↓
        SSE 事件流
    """

    def __init__(
        self,
        llm: LLMClient,
        db: DatabaseService,
        emitter: SSEEmitter,
    ) -> None:
        self._llm = llm
        self._db = db
        self._emitter = emitter

    async def run(self, context: AgentContext) -> list[str]:
        """
        执行 Stage 2 完整流程。

        Args:
            context: Stage 1 输出的 AgentContext

        Returns:
            sse_events: 已格式化的 SSE 事件字符串列表
        """
        sse_events: list[str] = []

        # ── Step 1: SQL 生成 ─────────────────────────────────
        sql_result = await self._generate_sql(context)
        sql = sql_result.get("sql", "")
        sql_explanation = sql_result.get("explanation", "")

        logger.info("生成 SQL: %s", sql)
        logger.info("SQL 说明: %s", sql_explanation)

        # ── Step 2 & 3: SQL 执行（含修正循环）────────────────
        query_result = None
        last_error = ""

        for attempt in range(1 + MAX_SQL_RETRIES):
            try:
                # 安全验证
                is_valid, error = self._db.validate_sql(sql)
                if not is_valid:
                    raise ValueError(error)

                # 执行查询
                query_result = await self._db.execute_query(sql)
                logger.info(
                    "SQL 执行成功: %d 行结果", query_result["row_count"]
                )
                break

            except Exception as e:
                last_error = str(e)
                logger.warning(
                    "SQL 执行失败 (第 %d 次): %s", attempt + 1, last_error
                )

                if attempt < MAX_SQL_RETRIES:
                    # 尝试修正 SQL
                    sql_result = await self._correct_sql(sql, last_error)
                    sql = sql_result.get("sql", sql)
                    logger.info("修正后 SQL: %s", sql)

        # ── Step 4: condition 步骤回填 ────────────────────────
        if query_result and query_result["rows"]:
            condition_events = self._backfill_conditions(context, query_result)
            sse_events.extend(condition_events)

        # ── Step 5: 生成最终回答 ──────────────────────────────
        if query_result:
            # 发射 grade 步骤（摘要）
            summary = self._generate_summary_label(context, query_result)
            grade_step = ReasoningStep(
                kind=ReasoningStepKind.GRADE,
                label=summary,
            )
            sse_events.append(self._emitter.emit_reasoning_step(grade_step))

            # 流式生成最终回答
            answer_events = await self._generate_final_answer(
                context, query_result
            )
            sse_events.extend(answer_events)
        else:
            # SQL 执行失败，发射 fallback
            fallback_step = ReasoningStep(
                kind=ReasoningStepKind.FALLBACK,
                label="数据查询失败",
                detail=f"SQL 执行错误: {last_error}",
            )
            sse_events.append(self._emitter.emit_reasoning_step(fallback_step))
            sse_events.append(
                self._emitter.emit_text(
                    f"抱歉，数据查询遇到问题：{last_error}\n请尝试换一种方式提问。"
                )
            )

        # ── 发射 response_metadata ────────────────────────────
        extra_meta = {"sql": sql, "sql_explanation": sql_explanation}
        if query_result:
            extra_meta["row_count"] = query_result["row_count"]
            extra_meta["truncated"] = query_result.get("truncated", False)
        sse_events.append(self._emitter.emit_response_metadata(extra_meta))

        return sse_events

    # ── 内部方法 ─────────────────────────────────────────────

    async def _generate_sql(self, context: AgentContext) -> dict[str, Any]:
        """调用 LLM 生成 SQL。"""
        # 检查是否有匹配的预定义模板
        sql_templates_text = self._match_sql_templates(context)

        filters_text = json.dumps(
            [f.model_dump() for f in context.filters],
            ensure_ascii=False,
            indent=2,
        )
        metrics_text = json.dumps(
            [m.model_dump() for m in context.metrics],
            ensure_ascii=False,
            indent=2,
        )

        try:
            result = await self._llm.chat_json([
                {"role": "system", "content": STAGE2_SQL_GENERATION_SYSTEM},
                {
                    "role": "user",
                    "content": STAGE2_SQL_GENERATION_USER.format(
                        question=context.user_question,
                        business_context=context.business_explanation,
                        filters=filters_text,
                        metrics=metrics_text,
                        ddl=get_all_ddl(),
                        sql_templates=sql_templates_text,
                    ),
                },
            ])
            return result
        except Exception as e:
            logger.error("SQL 生成失败: %s", e)
            return {"sql": "", "explanation": f"SQL 生成失败: {e}"}

    async def _correct_sql(
        self, original_sql: str, error_message: str
    ) -> dict[str, Any]:
        """调用 LLM 修正 SQL（借鉴 WrenAI sql_correction pipeline）。"""
        try:
            result = await self._llm.chat_json([
                {"role": "system", "content": STAGE2_SQL_CORRECTION_SYSTEM},
                {
                    "role": "user",
                    "content": STAGE2_SQL_CORRECTION_USER.format(
                        original_sql=original_sql,
                        error_message=error_message,
                        ddl=get_all_ddl(),
                    ),
                },
            ])
            return result
        except Exception as e:
            logger.error("SQL 修正失败: %s", e)
            return {"sql": original_sql, "explanation": f"修正失败: {e}"}

    def _backfill_conditions(
        self,
        context: AgentContext,
        query_result: dict[str, Any],
    ) -> list[str]:
        """回填 condition 步骤的 actual 和 satisfied 值。"""
        sse_events = []
        if not query_result["rows"]:
            return sse_events

        first_row = query_result["rows"][0]

        for filter_cond in context.filters:
            # 尝试从结果集中找到对应的值
            actual_value = first_row.get(filter_cond.field)
            if actual_value is not None:
                # 判断是否满足条件
                satisfied = self._evaluate_condition(
                    actual_value, filter_cond.operator, filter_cond.value
                )
                # 更新 emitter 中的步骤
                updated = self._emitter.update_condition_step(
                    filter_cond.field, actual_value, satisfied
                )
                if updated:
                    # 重新发射更新后的 condition 步骤
                    sse_events.append(
                        self._emitter.emit_reasoning_step(updated)
                    )

        return sse_events

    def _evaluate_condition(
        self, actual: Any, operator: str, expected: Any
    ) -> bool:
        """评估条件是否满足。"""
        try:
            actual_f = float(actual)
            expected_f = float(expected) if not isinstance(expected, list) else 0
            if operator == "<=":
                return actual_f <= expected_f
            elif operator == ">=":
                return actual_f >= expected_f
            elif operator == "=":
                return actual_f == expected_f
            elif operator == "<":
                return actual_f < expected_f
            elif operator == ">":
                return actual_f > expected_f
        except (ValueError, TypeError):
            pass
        return True  # 无法判断时默认满足

    async def _generate_final_answer(
        self,
        context: AgentContext,
        query_result: dict[str, Any],
    ) -> list[str]:
        """流式生成最终回答。"""
        sse_events = []

        # 格式化查询结果
        result_text = self._format_query_result(query_result)

        messages = [
            {"role": "system", "content": FINAL_ANSWER_SYSTEM},
            {
                "role": "user",
                "content": FINAL_ANSWER_USER.format(
                    question=context.user_question,
                    business_context=context.business_explanation,
                    query_result=result_text,
                ),
            },
        ]

        # 流式输出
        async for chunk in self._llm.chat_stream(messages):
            sse_events.append(self._emitter.emit_text(chunk))

        return sse_events

    def _generate_summary_label(
        self,
        context: AgentContext,
        query_result: dict[str, Any],
    ) -> str:
        """生成 grade 步骤的摘要标签。"""
        if not query_result["rows"]:
            return "未查询到数据"

        row_count = query_result["row_count"]
        first_row = query_result["rows"][0]

        # 尝试从结果中提取关键数值
        for key in ["qualified_rate", "avg_ps_loss", "total_weight_kg"]:
            if key in first_row:
                value = first_row[key]
                if key == "qualified_rate":
                    return f"合格率: {value}%"
                elif key == "avg_ps_loss":
                    return f"平均铁损: {value} W/kg"
                elif key == "total_weight_kg":
                    return f"总产量: {value} kg"

        return f"查询完成，共 {row_count} 条结果"

    def _match_sql_templates(self, context: AgentContext) -> str:
        """匹配预定义的 SQL 模板。"""
        matched = []
        for metric in context.metrics:
            template = METRIC_SQL_TEMPLATES.get(metric.name)
            if template:
                matched.append(
                    f"### {metric.name}\n"
                    f"说明: {template['description']}\n"
                    f"SQL 模板:\n```sql\n{template['sql_template'].strip()}\n```"
                )

        # 如果没有精确匹配，也提供所有可用模板供参考
        if not matched:
            for name, template in METRIC_SQL_TEMPLATES.items():
                matched.append(
                    f"### {name}\n说明: {template['description']}"
                )

        return "\n\n".join(matched) if matched else "（无匹配的预定义模板）"

    def _format_query_result(self, query_result: dict[str, Any]) -> str:
        """将查询结果格式化为 LLM 可读的文本。"""
        if not query_result["rows"]:
            return "（查询结果为空）"

        columns = query_result["columns"]
        rows = query_result["rows"]

        # 表头
        lines = [" | ".join(columns)]
        lines.append(" | ".join(["---"] * len(columns)))

        # 数据行（最多展示 20 行给 LLM）
        for row in rows[:20]:
            values = [str(row.get(col, "NULL")) for col in columns]
            lines.append(" | ".join(values))

        if len(rows) > 20:
            lines.append(f"... 共 {len(rows)} 行，仅展示前 20 行")

        return "\n".join(lines)
