"""Knowledge graph traversal tools for RootCauseAgent.

提供 LangGraph 可调用的 ``traverse_judgment_path`` 工具，把"炉号 → 检测指标 → 判定
公式 → 判定阈值 → 产品规格 → 等级"这条多跳路径序列化为 ReasoningStep 字典列表。

Module helpers (migrated from ``root_cause_agent.py``)
------------------------------------------------------

- ``_get_record``: 从 ``lab_intermediate_data`` 拉单条记录（左联 ``lab_product_spec``）。
- ``_get_rules_for_spec_and_grade``: Cypher 查询 (规格+等级) 对应的判定规则。
- ``_evaluate_rule_conditions``: 把 record 套用到规则的 conditions 上，返回 satisfied/failed。
- 字段名解析与可读化（``_FIELD_LABELS`` / ``_humanize_field`` / ``_resolve_field_name`` /
  ``_compare_condition`` / ``_format_expected`` / ``_describe_condition``）。

公共 API
--------

- :func:`traverse_judgment_path`: ``@tool`` 形态，给 LangGraph 节点调用。
"""

from __future__ import annotations

import json
import uuid
from typing import Any

from langchain_core.tools import tool

from app.knowledge_graph.manager import get_knowledge_graph
from app.tools.sql_tools import execute_safe_sql

# --------------------------------------------------------------------------- #
# Field label dictionary (中文化字段名)
# --------------------------------------------------------------------------- #

_FIELD_LABELS: dict[str, str] = {
    "F_WIDTH": "带宽",
    "F_PERF_PS_LOSS": "Ps铁损",
    "F_PERF_HC": "矫顽力",
    "F_LAM_FACTOR": "叠片系数",
    "F_THICKNESS_RANGE": "厚度极差",
}


# --------------------------------------------------------------------------- #
# SQL helper (record fetch)
# --------------------------------------------------------------------------- #


async def _get_record(
    furnace_no: str | None, batch_no: str | None
) -> dict[str, Any] | None:
    """Fetch one record and normalize key fields needed for attribution."""
    conditions: list[str] = []
    params: dict[str, Any] = {}

    if furnace_no:
        # 数据库中部分炉号尾随有 CR/LF（如 '1乙...-1\r'），用 REPLACE 去除后再比较。
        conditions.append(
            "REPLACE(REPLACE(d.F_FURNACE_NO, CHAR(13), ''), CHAR(10), '') = :furnace_no"
        )
        params["furnace_no"] = furnace_no.strip()
    if batch_no:
        conditions.append("d.F_FURNACE_BATCH_NO = :batch_no")
        params["batch_no"] = batch_no

    if not conditions:
        return None

    # NOTE: f-string interpolates only hardcoded column predicates from the
    # `conditions` list above ("d.F_FURNACE_NO = :furnace_no" / "d.F_FURNACE_BATCH_NO = :batch_no").
    # User input flows exclusively via SQLAlchemy bind params (:furnace_no / :batch_no),
    # so the f-string is safe under sql_tools' whitelist + parameterization rules.
    where_clause = " OR ".join(conditions)
    sql = f"""
        SELECT
            d.*,
            d.F_FURNACE_NO as furnace_no,
            d.F_FURNACE_BATCH_NO as batch_no,
            p.F_CODE as spec_code,
            d.F_LABELING as grade
        FROM lab_intermediate_data d
        LEFT JOIN lab_product_spec p ON d.F_PRODUCT_SPEC_ID = p.F_Id
        WHERE {where_clause}
        LIMIT 1
    """  # noqa: S608
    results = await execute_safe_sql(sql, params)
    return results[0] if results else None


# --------------------------------------------------------------------------- #
# KG helpers (rules + condition evaluation)
# --------------------------------------------------------------------------- #


async def _get_rules_for_spec_and_grade(
    graph: Any, spec_code: str, grade: str
) -> list[dict[str, Any]]:
    """Query the knowledge graph for one spec-grade rule."""
    query = """
        MATCH (s:ProductSpec {code: $spec_code})-[:HAS_RULE]->(r:JudgmentRule)
        WHERE toUpper(r.name) STARTS WITH $grade
        RETURN r {
            .id, .name, .priority, .qualityStatus, .color, .isDefault, .conditionJson
        } as rule
        ORDER BY r.priority DESC
    """
    results = await graph.query_async(
        query, spec_code=spec_code, grade=grade.upper()
    )
    parsed_rules: list[dict[str, Any]] = []
    for item in results:
        rule = dict(item.get("rule", {}))
        condition_json = rule.get("conditionJson")
        try:
            conditions = json.loads(condition_json) if condition_json else []
        except json.JSONDecodeError:
            conditions = []
        rule["conditions"] = conditions
        parsed_rules.append(rule)
    return parsed_rules


def _resolve_field_name(condition: dict[str, Any]) -> str:
    """Resolve likely field name keys from condition JSON."""
    for key in ("field", "column", "column_name", "metric", "metric_name"):
        value = condition.get(key)
        if isinstance(value, str) and value:
            return value
    return ""


def _compare_condition(actual_value: Any, condition: dict[str, Any]) -> bool:
    """Compare one condition against the record value."""
    if actual_value is None:
        return False

    operator = str(condition.get("operator", "<="))
    threshold = condition.get("value")
    if threshold is None and "min" in condition and "max" in condition:
        try:
            actual = float(actual_value)
            return float(condition["min"]) <= actual <= float(condition["max"])
        except (TypeError, ValueError):
            return False

    try:
        actual = float(actual_value)
        expected = float(threshold)
    except (TypeError, ValueError):
        actual_text = str(actual_value)
        expected_text = str(threshold)
        if operator == "==":
            return actual_text == expected_text
        if operator == "!=":
            return actual_text != expected_text
        return False

    if operator == "<=":
        return actual <= expected
    if operator == "<":
        return actual < expected
    if operator == ">=":
        return actual >= expected
    if operator == ">":
        return actual > expected
    if operator == "==":
        return actual == expected
    if operator == "!=":
        return actual != expected
    return False


def _format_expected(condition: dict[str, Any]) -> str:
    """Render expected threshold text for one condition."""
    if "min" in condition and "max" in condition:
        return f"{condition['min']} ~ {condition['max']}"
    operator = condition.get("operator", "<=")
    value = condition.get("value")
    return f"{operator} {value}"


def _humanize_field(field_name: str) -> str:
    """Map database field names to readable Chinese labels."""
    return _FIELD_LABELS.get(field_name, field_name or "未知字段")


def _describe_condition(condition: dict[str, Any]) -> str:
    """Render readable condition text."""
    field_name = _humanize_field(_resolve_field_name(condition))
    return f"{field_name} {_format_expected(condition)}"


def _evaluate_rule_conditions(
    record: dict[str, Any], rule: dict[str, Any]
) -> dict[str, list[dict[str, Any]]]:
    """Evaluate all rule conditions against one record.

    上游 KG 中 conditionJson 偶尔会是 ["F_WIDTH", ...] 这类字符串数组而非
    dict 数组——这种条件没有 operator/value，无法直接评估，统计在
    ``unstructured`` 列表里供上层告知用户，但不计入 satisfied/failed。
    """
    satisfied: list[dict[str, Any]] = []
    failed: list[dict[str, Any]] = []
    unstructured: list[str] = []

    for condition in rule.get("conditions", []):
        if not isinstance(condition, dict):
            text_label = str(condition) if condition is not None else "未知条件"
            unstructured.append(text_label)
            continue
        field_name = _resolve_field_name(condition)
        actual_value = record.get(field_name) if field_name else None
        comparison = _compare_condition(actual_value, condition)
        detail = {
            "field": field_name,
            "label": condition.get("label") or _humanize_field(field_name),
            "actual": actual_value,
            "expected": _format_expected(condition),
            "condition": _describe_condition(condition),
        }
        if comparison:
            satisfied.append(detail)
        else:
            failed.append(detail)

    return {"satisfied": satisfied, "failed": failed, "unstructured": unstructured}


# --------------------------------------------------------------------------- #
# Internal: build the ReasoningStep sequence for one (record, grade) pair.
# Pure-data variant used both by the @tool wrapper and by root_cause_agent
# when it iterates and dispatches custom events.
# --------------------------------------------------------------------------- #


async def _build_judgment_path_steps(
    furnace_no: str,
    batch_no: str | None,
    target_grade: str,
) -> list[dict[str, Any]]:
    """Walk furnace → metric → formula → threshold → spec → grade.

    Returns ordered ReasoningStep dicts. On any non-fatal failure (no record,
    no graph, no rule, Cypher exception) returns a list ending in a
    ``kind="fallback"`` step rather than raising; callers always receive a
    non-empty list.
    """
    steps: list[dict[str, Any]] = []

    # Step kind=record — fetch the inspection record.
    record = await _get_record(
        furnace_no=furnace_no or None, batch_no=batch_no or None
    )
    if record is None:
        steps.append(
            {
                "id": str(uuid.uuid4()),
                "kind": "fallback",
                "title": "未找到检测记录",
                "summary": f"未找到标识为 {furnace_no or batch_no or '空'} 的检测记录",
                "label": (
                    f"未找到标识为 {furnace_no or batch_no or '空'} 的检测记录"
                ),
                "status": "warning",
            }
        )
        return steps

    resolved_grade = (
        target_grade
        or str(record.get("grade") or record.get("labeling_grade") or "")
    ).upper()
    spec_code = str(record.get("spec_code") or "")

    steps.append(
        {
            "id": str(uuid.uuid4()),
            "kind": "record",
            "title": "命中检测记录",
            "summary": (
                f"炉号 {record.get('furnace_no') or furnace_no}，"
                f"批次 {record.get('batch_no') or batch_no or '未知'}"
            ),
            "label": (
                f"命中检测记录：炉号 {record.get('furnace_no') or furnace_no}，"
                f"批次 {record.get('batch_no') or batch_no or '未知'}"
            ),
            "status": "success",
            "ontology_refs": [
                {
                    "type": "InspectionRecord",
                    "id": f"record:{furnace_no or batch_no}",
                    "label": f"炉号 {furnace_no or batch_no}",
                }
            ],
            "meta": {
                "furnace_no": record.get("furnace_no"),
                "batch_no": record.get("batch_no"),
            },
        }
    )

    if not spec_code:
        steps.append(
            {
                "id": str(uuid.uuid4()),
                "kind": "fallback",
                "title": "缺少规格信息",
                "summary": "记录未关联到产品规格，无法走规则推理",
                "label": "记录未关联到产品规格，无法走规则推理",
                "status": "warning",
            }
        )
        return steps

    # Step kind=spec — record's product spec.
    steps.append(
        {
            "id": str(uuid.uuid4()),
            "kind": "spec",
            "title": "关联产品规格",
            "summary": f"产品规格 {spec_code}",
            "label": f"产品规格 {spec_code}",
            "status": "success",
            "ontology_refs": [
                {
                    "type": "ProductSpec",
                    "id": f"spec:{spec_code}",
                    "label": spec_code,
                }
            ],
            "edge_refs": [
                {
                    "source": f"record:{furnace_no or batch_no}",
                    "target": f"spec:{spec_code}",
                    "relation": "USES_SPEC",
                }
            ],
            "meta": {"spec_code": spec_code},
        }
    )

    if not resolved_grade:
        steps.append(
            {
                "id": str(uuid.uuid4()),
                "kind": "fallback",
                "title": "缺少等级信息",
                "summary": "问题未指定等级且记录无 F_LABELING 字段，无法定位规则",
                "label": "问题未指定等级且记录无 F_LABELING 字段，无法定位规则",
                "status": "warning",
            }
        )
        return steps

    # Step kind=rule — query KG for spec+grade rule.
    graph = get_knowledge_graph()
    if graph is None:
        steps.append(
            {
                "id": str(uuid.uuid4()),
                "kind": "fallback",
                "title": "知识图谱不可用",
                "summary": "知识图谱当前不可用（KG manager 返回 None）",
                "label": "知识图谱当前不可用（KG manager 返回 None）",
                "status": "failed",
            }
        )
        return steps

    try:
        rules = await _get_rules_for_spec_and_grade(
            graph, spec_code=spec_code, grade=resolved_grade
        )
    except Exception as exc:  # noqa: BLE001 — surface to UI as fallback
        steps.append(
            {
                "id": str(uuid.uuid4()),
                "kind": "fallback",
                "title": "知识图谱查询失败",
                "summary": f"知识图谱查询失败：{type(exc).__name__}",
                "label": f"知识图谱查询失败：{type(exc).__name__}",
                "detail": str(exc)[:200],
                "status": "failed",
            }
        )
        return steps

    if not rules:
        steps.append(
            {
                "id": str(uuid.uuid4()),
                "kind": "fallback",
                "title": "未找到判定规则",
                "summary": (
                    f"未在图谱中找到规格 {spec_code} 下 "
                    f"{resolved_grade} 级的判定规则"
                ),
                "label": (
                    f"未在图谱中找到规格 {spec_code} 下 "
                    f"{resolved_grade} 级的判定规则"
                ),
                "status": "warning",
            }
        )
        return steps

    rule = rules[0]
    rule_name = str(rule.get("name") or resolved_grade)
    rule_id = str(rule.get("id") or "")
    steps.append(
        {
            "id": str(uuid.uuid4()),
            "kind": "rule",
            "title": "命中判定规则",
            "summary": f"判定规则：{rule_name}（优先级 {rule.get('priority', 'N/A')}）",
            "label": f"判定规则：{rule_name}（优先级 {rule.get('priority', 'N/A')}）",
            "status": "success",
            "ontology_refs": [
                {
                    "type": "JudgmentRule",
                    "id": f"rule:{rule_id}",
                    "label": rule_name,
                }
            ],
            "edge_refs": [
                {
                    "source": f"spec:{spec_code}",
                    "target": f"rule:{rule_id}",
                    "relation": "HAS_RULE",
                }
            ],
            "meta": {
                "rule_id": rule_id,
                "priority": rule.get("priority"),
                "quality_status": rule.get("qualityStatus"),
            },
        }
    )

    # Step kind=condition × N — evaluate each condition against record.
    evaluation = _evaluate_rule_conditions(record, rule)
    for detail in evaluation["satisfied"]:
        steps.append(
            {
                "id": str(uuid.uuid4()),
                "kind": "condition",
                "title": f"条件满足：{detail['label']}",
                "summary": f"{detail['condition']} — 实际值 {detail['actual']} 满足期望 {detail['expected']}",
                "label": detail["condition"],
                "field": detail["field"],
                "expected": detail["expected"],
                "actual": detail["actual"],
                "satisfied": True,
                "status": "success",
                "ontology_refs": [
                    {
                        "type": "RuleCondition",
                        "id": f"cond:{rule_id}:{detail['field']}",
                        "label": detail["label"] or detail["field"],
                    }
                ],
                "edge_refs": [
                    {
                        "source": f"rule:{rule_id}",
                        "target": f"cond:{rule_id}:{detail['field']}",
                        "relation": "HAS_CONDITION",
                    }
                ],
                "evidence": [
                    {
                        "label": detail["label"] or detail["field"],
                        "value": detail["actual"],
                        "source": "检测记录",
                    }
                ],
            }
        )
    for detail in evaluation["failed"]:
        steps.append(
            {
                "id": str(uuid.uuid4()),
                "kind": "condition",
                "title": f"条件不满足：{detail['label']}",
                "summary": f"{detail['condition']} — 实际值 {detail['actual']} 不满足期望 {detail['expected']}",
                "label": detail["condition"],
                "field": detail["field"],
                "expected": detail["expected"],
                "actual": detail["actual"],
                "satisfied": False,
                "status": "warning",
                "ontology_refs": [
                    {
                        "type": "RuleCondition",
                        "id": f"cond:{rule_id}:{detail['field']}",
                        "label": detail["label"] or detail["field"],
                    }
                ],
                "edge_refs": [
                    {
                        "source": f"rule:{rule_id}",
                        "target": f"cond:{rule_id}:{detail['field']}",
                        "relation": "HAS_CONDITION",
                    }
                ],
                "evidence": [
                    {
                        "label": detail["label"] or detail["field"],
                        "value": detail["actual"],
                        "source": "检测记录",
                    }
                ],
            }
        )
    unstructured = evaluation.get("unstructured", [])
    if unstructured:
        steps.append(
            {
                "id": str(uuid.uuid4()),
                "kind": "condition",
                "title": "未结构化条件",
                "summary": f"知识图谱中存在 {len(unstructured)} 条未结构化的条件，无法直接评估",
                "label": (
                    f"知识图谱中存在 {len(unstructured)} 条未结构化的条件，"
                    f"无法直接评估（仅记录字段：{ '、'.join(unstructured) }）"
                ),
                "field": "",
                "expected": "—",
                "actual": "—",
                "satisfied": None,
                "status": "warning",
                "meta": {"unstructured": True, "fields": unstructured},
            }
        )

    # Step kind=grade — terminal node.
    failed_count = len(evaluation["failed"])
    satisfied_count = len(evaluation["satisfied"])
    structured_total = satisfied_count + failed_count
    if structured_total == 0 and unstructured:
        verdict = (
            f"该规则下没有可机器评估的结构化条件，仅有 {len(unstructured)} 条"
            f"未结构化字段说明；按规则归入 {resolved_grade} 级，"
            f"具体阈值需查阅原规则定义。"
        )
    elif failed_count == 0:
        verdict = (
            f"全部 {satisfied_count} 条条件均满足，判定为 {resolved_grade} 级。"
        )
    else:
        failed_labels = "、".join(item["label"] for item in evaluation["failed"])
        verdict = (
            f"共 {structured_total} 条条件，{satisfied_count} 条满足、"
            f"{failed_count} 条不满足；主要差距在 {failed_labels}；"
            f"按规则归入 {resolved_grade} 级。"
        )

    steps.append(
        {
            "id": str(uuid.uuid4()),
            "kind": "grade",
            "title": f"判定结论：{resolved_grade} 级",
            "summary": verdict,
            "label": verdict,
            "status": "success" if failed_count == 0 else "warning",
            "ontology_refs": [
                {
                    "type": "JudgmentRule",
                    "id": f"rule:{rule_id}",
                    "label": rule_name,
                }
            ],
            "meta": {
                "grade": resolved_grade,
                "satisfied_count": satisfied_count,
                "failed_count": failed_count,
            },
        }
    )

    return steps


# --------------------------------------------------------------------------- #
# Public @tool — surfaces the path for LangGraph agents.
# --------------------------------------------------------------------------- #


@tool
async def traverse_judgment_path(
    furnace_no: str,
    batch_no: str | None,
    target_grade: str,
) -> list[dict[str, Any]]:
    """走"炉号 → 检测指标 → 判定公式 → 判定阈值 → 产品规格 → 等级"多跳路径。

    本工具供 RootCauseAgent 调用，将单条检测记录的判定根因序列化为有序的
    ReasoningStep 字典列表（kind ∈ {record, spec, rule, condition, grade,
    fallback}）。LangGraph 节点拿到结果后通过 ``adispatch_custom_event`` 把
    每一步推送为 SSE ``reasoning_step`` 事件，同时把整个列表写入 state 供
    非流式 ``/chat`` 端点二通道消费。

    返回值的不变性（Ordering Invariants）
    --------------------------------------
    - 列表非空：任何错误路径至少返回一个 ``kind="fallback"`` 步骤；不抛异常。
    - ``kind="record"`` 始终是首步（如果记录命中）。
    - ``kind="condition"`` 不会出现在对应 ``kind="rule"`` 之前。
    - ``kind="grade"`` 若存在，始终是末步。

    Args:
        furnace_no: 炉号（如 "1丙20260110-1"）。可与 batch_no 二选一，至少给一个。
        batch_no: 批次号（可空）。furnace_no 为空时必填。
        target_grade: 目标等级（"A" / "B" / "C"）。空字符串时尝试从记录的
            F_LABELING 字段回退；都拿不到时返回 fallback。

    Returns:
        list[dict[str, Any]]: 有序 ReasoningStep 字典列表，每项含 ``id``、
        ``kind``、``title``、``summary``、``status``、``ontology_refs``、
        ``edge_refs``、``evidence`` 等字段；condition 类还含
        ``field/expected/actual/satisfied``。
    """
    if not furnace_no and not batch_no:
        return [
            {
                "id": str(uuid.uuid4()),
                "kind": "fallback",
                "title": "缺少查询条件",
                "summary": "请提供炉号或批次号",
                "label": "请提供炉号或批次号",
                "status": "warning",
            }
        ]
    return await _build_judgment_path_steps(
        furnace_no=furnace_no,
        batch_no=batch_no,
        target_grade=target_grade or "",
    )
