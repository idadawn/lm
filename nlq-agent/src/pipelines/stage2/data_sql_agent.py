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
from dataclasses import dataclass
from typing import Any

from src.models.ddl import METRIC_SQL_TEMPLATES, get_all_ddl
from src.models.schemas import (
    AgentContext,
    FilterCondition,
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


class ConditionEvalError(ValueError):
    """条件评估类型不匹配错误。"""


@dataclass(frozen=True)
class DiagnosticSpec:
    """诊断字段规格：描述一个 condition 字段在诊断 SELECT 中的映射关系。"""

    metric_template: str
    sql_expression: str
    alias: str
    canonical_key: str


DIAGNOSTIC_FIELD_REGISTRY: dict[str, DiagnosticSpec] = {
    "合格率": DiagnosticSpec(
        metric_template="合格率",
        sql_expression="ROUND(SUM(qualified_count) * 100.0 / NULLIF(SUM(total_count), 0), 2)",
        alias="actual_合格率",
        canonical_key="合格率",
    ),
    "抽样数量": DiagnosticSpec(
        metric_template="合格率",
        sql_expression="SUM(sample_count)",
        alias="actual_抽样数量",
        canonical_key="抽样数量",
    ),
    "qualified_rate": DiagnosticSpec(
        metric_template="合格率",
        sql_expression="ROUND(SUM(qualified_count) * 100.0 / NULLIF(SUM(total_count), 0), 2)",
        alias="actual_合格率",
        canonical_key="合格率",
    ),
    "sample_count": DiagnosticSpec(
        metric_template="合格率",
        sql_expression="SUM(sample_count)",
        alias="actual_抽样数量",
        canonical_key="抽样数量",
    ),
}


def canonical_field_key(field: str) -> str | None:
    """将字段名归一化为中文 canonical key；未注册返回 None。"""
    spec = DIAGNOSTIC_FIELD_REGISTRY.get(field)
    if spec is None:
        return None
    return spec.canonical_key


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
            condition_events = await self._backfill_conditions(context, query_result)
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
        """调用 LLM 生成 SQL。Trend / root_cause intent 直接使用模板。"""
        if context.intent.intent == IntentType.TREND:
            return self._generate_trend_sql(context)

        if context.intent.intent == IntentType.ROOT_CAUSE:
            return self._generate_root_cause_sql(context)

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

    def _generate_trend_sql(self, context: AgentContext) -> dict[str, Any]:
        """Trend intent: 直接使用合格率_趋势模板生成 SQL。"""
        template_entry = METRIC_SQL_TEMPLATES.get("合格率_趋势")
        if not template_entry:
            return {"sql": "", "explanation": "合格率_趋势模板不存在"}

        time_window = context.intent.extracted_entities.get("time_window", 6)
        extra_where = ""

        sql = template_entry["sql_template"].format(
            time_window_months=time_window,
            extra_where=extra_where,
        )

        return {
            "sql": sql.strip(),
            "explanation": f"近{time_window}个月按产品规格分组的合格率月度趋势",
        }

    def _generate_root_cause_sql(self, context: AgentContext) -> dict[str, Any]:
        """Root cause intent: 直接使用合格率_归因模板生成 SQL。"""
        template_entry = METRIC_SQL_TEMPLATES.get("合格率_归因")
        if not template_entry:
            return {"sql": "", "explanation": "合格率_归因模板不存在"}

        dimension_keys = context.intent.extracted_entities.get(
            "dimension_keys", ["F_PRODUCT_SPEC_CODE"]
        )
        time_window = context.intent.extracted_entities.get("time_window", 1)
        extra_where = ""

        dimension = dimension_keys[0] if dimension_keys else "F_PRODUCT_SPEC_CODE"

        sql = template_entry["sql_template"].format(
            dimension=dimension,
            time_window_months=time_window,
            extra_where=extra_where,
        )

        return {
            "sql": sql.strip(),
            "explanation": f"按{dimension}维度分析合格率归因",
        }

    async def _backfill_conditions(
        self,
        context: AgentContext,
        query_result: dict[str, Any],
    ) -> list[str]:
        """回填 condition 步骤的 actual 和 satisfied 值。"""
        sse_events: list[str] = []
        first_row = query_result["rows"][0] if query_result.get("rows") else None

        # F2 短路：主查询行已含字段时直接回填，不发诊断 query
        short_circuited_keys: set[str] = set()
        for filter_cond in context.filters:
            cn_key = canonical_field_key(filter_cond.display_name or filter_cond.field)
            if cn_key and first_row and cn_key in first_row:
                actual_value = first_row[cn_key]
                try:
                    satisfied = self._evaluate_condition(
                        actual_value, filter_cond.operator, filter_cond.value
                    )
                except ConditionEvalError:
                    satisfied = False
                updated = self._emitter.update_condition_step(
                    filter_cond.field, actual_value, satisfied
                )
                if updated:
                    sse_events.append(
                        self._emitter.emit_reasoning_step(updated)
                    )
                logger.debug(
                    "F2 short-circuit: field=%s actual=%s from main query row",
                    filter_cond.field,
                    actual_value,
                )
                short_circuited_keys.add(cn_key)

        # 若所有注册表字段均已短路，跳过诊断 query
        diag_needed = any(
            canonical_field_key(f.display_name or f.field) is not None
            and canonical_field_key(f.display_name or f.field) not in short_circuited_keys
            for f in context.filters
        )
        if not diag_needed:
            return sse_events

        # 优先使用诊断 SELECT 回填剩余字段
        events = await self._diagnostic_select_for_condition(
            context, query_result
        )
        if events:
            return sse_events + events

        # 降级：直接从查询结果回填
        if first_row:
            for filter_cond in context.filters:
                actual_value = first_row.get(filter_cond.field)
                if actual_value is not None:
                    try:
                        satisfied = self._evaluate_condition(
                            actual_value, filter_cond.operator, filter_cond.value
                        )
                    except ConditionEvalError:
                        satisfied = False
                    updated = self._emitter.update_condition_step(
                        filter_cond.field, actual_value, satisfied
                    )
                    if updated:
                        sse_events.append(
                            self._emitter.emit_reasoning_step(updated)
                        )

        return sse_events

    async def _diagnostic_select_for_condition(
        self,
        context: AgentContext,
        query_result: dict[str, Any],
    ) -> list[str]:
        """Per-condition diagnostic SELECT，复用 metric CTE 回填 condition 步骤。

        替代 _BACKFILL_ALIASES 方案：为每个 condition 字段生成独立 SELECT，
        复用合格率 metric CTE，使用规范中文字段键，通过 validate_sql 安全校验。
        """
        sse_events: list[str] = []

        # 从查询结果提取 product_spec_id
        spec_ids = sorted({
            str(row.get("F_PRODUCT_SPEC_ID") or row.get("product_spec_id", ""))
            for row in query_result["rows"]
            if row.get("F_PRODUCT_SPEC_ID") or row.get("product_spec_id")
        })
        if not spec_ids:
            return sse_events

        # 收集注册表命中的字段，按 metric_template 分组
        fields_by_metric: dict[str, list[DiagnosticSpec]] = {}
        name_to_filter: dict[str, FilterCondition] = {}
        for f in context.filters:
            key = canonical_field_key(f.display_name or f.field)
            if key is None:
                continue
            spec = DIAGNOSTIC_FIELD_REGISTRY.get(key)
            if spec is None:
                continue
            name_to_filter[key] = f
            existing = fields_by_metric.setdefault(spec.metric_template, [])
            if not any(s.canonical_key == spec.canonical_key for s in existing):
                existing.append(spec)

        if not fields_by_metric:
            return sse_events

        # 检查 metrics 中是否包含所需模板
        available_metrics = {m.name for m in context.metrics}
        for metric_template in list(fields_by_metric.keys()):
            if metric_template not in available_metrics:
                del fields_by_metric[metric_template]

        if not fields_by_metric:
            return sse_events

        spec_csv = ", ".join(f"'{s}'" for s in spec_ids)

        for metric_template, specs in fields_by_metric.items():
            if metric_template not in METRIC_SQL_TEMPLATES:
                logger.warning("诊断查询未找到 metric template: %s", metric_template)
                continue

            # 构建 CTE
            template_sql = METRIC_SQL_TEMPLATES[metric_template]["sql_template"].format(
                group_by_clause="F_PRODUCT_SPEC_ID",
                start_date="2020-01-01",
                end_date="2099-12-31",
                extra_where="",
            )

            # 构建 SELECT 列（注册表驱动，无需 if-else）
            projections = [f"{spec.sql_expression} AS {spec.alias}" for spec in specs]
            projection_sql = ", ".join(projections)

            diagnostic_sql = (
                f"WITH base AS ({template_sql}) "
                f"SELECT F_PRODUCT_SPEC_ID AS product_spec_id, {projection_sql} "
                f"FROM base "
                f"WHERE F_PRODUCT_SPEC_ID IN ({spec_csv}) "
                f"GROUP BY F_PRODUCT_SPEC_ID"
            )

            # SQL 安全校验
            is_valid, error = self._db.validate_sql(diagnostic_sql)
            if not is_valid:
                logger.warning("诊断 SQL 校验失败: %s", error)
                continue

            # 执行诊断查询
            try:
                diag_result = await self._db.execute_query(diagnostic_sql)
            except Exception as exc:
                logger.warning("诊断 SQL 执行失败: %s", exc)
                continue

            # 将结果扇出到 condition 步骤
            for row in diag_result["rows"]:
                for spec in specs:
                    actual_val = row.get(spec.alias)
                    cond = name_to_filter.get(spec.canonical_key)
                    if actual_val is None or cond is None:
                        continue
                    try:
                        satisfied = self._evaluate_condition(
                            actual_val, cond.operator, cond.value
                        )
                    except ConditionEvalError:
                        satisfied = False
                    updated = self._emitter.update_condition_step(
                        cond.field, actual_val, satisfied
                    )
                    if updated:
                        sse_events.append(
                            self._emitter.emit_reasoning_step(updated)
                        )

        return sse_events

    def _evaluate_condition(
        self, actual: Any, operator: str, expected: Any
    ) -> bool:
        """评估条件是否满足，按算子类型分支处理。

        算子分类：
        - 数值比较: <=, >=, =, <, >
        - 列表匹配: IN, NOT IN
        - 范围匹配: BETWEEN
        """
        op = operator.upper()

        # 列表匹配
        if op in ("IN", "NOT IN"):
            if not isinstance(expected, (list, tuple)):
                raise ConditionEvalError(
                    f"算子 {operator} 需要列表类型期望值，"
                    f"实际得到 {type(expected).__name__}"
                )
            result = actual in expected
            return not result if op == "NOT IN" else result

        # 范围匹配
        if op == "BETWEEN":
            if not isinstance(expected, (list, tuple)) or len(expected) != 2:
                raise ConditionEvalError(
                    f"BETWEEN 需要包含两个元素的列表，实际得到 {expected!r}"
                )
            try:
                lo, hi = float(expected[0]), float(expected[1])
                actual_f = float(actual)
            except (ValueError, TypeError) as exc:
                raise ConditionEvalError(
                    f"无法将 BETWEEN 操作数转为数值: "
                    f"actual={actual!r}, range={expected!r}"
                ) from exc
            return lo <= actual_f <= hi

        # 数值比较
        try:
            actual_f = float(actual)
            expected_f = float(expected)
        except (ValueError, TypeError) as exc:
            raise ConditionEvalError(
                f"无法将比较操作数转为数值: "
                f"actual={actual!r}, expected={expected!r}"
            ) from exc

        comparisons: dict[str, bool] = {
            "<=": actual_f <= expected_f,
            ">=": actual_f >= expected_f,
            "=": actual_f == expected_f,
            "<": actual_f < expected_f,
            ">": actual_f > expected_f,
        }
        if op in comparisons:
            return comparisons[op]
        raise ConditionEvalError(f"未知算子: {operator}")

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

        # Trend summary: show first→last rate change per spec
        if context.intent.intent == IntentType.TREND:
            rows = query_result["rows"]
            by_spec: dict[str, list[dict[str, Any]]] = {}
            for r in rows:
                spec = r.get("product_spec_id", "unknown")
                by_spec.setdefault(spec, []).append(r)

            time_window = context.intent.extracted_entities.get("time_window", 6)
            parts: list[str] = []
            for spec_id, spec_rows in list(by_spec.items())[:3]:
                sorted_rows = sorted(spec_rows, key=lambda r: r.get("month_bucket", ""))
                if len(sorted_rows) >= 2:
                    first_rate = sorted_rows[0].get("qualified_rate", 0)
                    last_rate = sorted_rows[-1].get("qualified_rate", 0)
                    parts.append(f"{spec_id}: {first_rate}%→{last_rate}%")
                elif sorted_rows:
                    parts.append(f"{spec_id}: {sorted_rows[0].get('qualified_rate', 0)}%")

            if parts:
                return f"近{time_window}月合格率趋势: " + "; ".join(parts)
            return f"趋势查询完成，共{query_result['row_count']}条结果"

        # Root cause summary: show worst performer
        if context.intent.intent == IntentType.ROOT_CAUSE:
            rows = query_result["rows"]
            if rows:
                worst = min(rows, key=lambda r: r.get("delta_from_overall", 0))
                dim_key = worst.get("dimension_key", "")
                dim_val = worst.get("dimension_value", "")
                rate = worst.get("qualified_rate", 0)
                delta = worst.get("delta_from_overall", 0)
                return (
                    f"{dim_key}={dim_val} 合格率最低: {rate}%"
                    f"（低于整体 {abs(delta)}%）"
                )
            return "归因分析完成，未查询到数据"

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
