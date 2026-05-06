"""Query tools module.

Provides core tool functions for QueryAgent:
- get_formula_definition_tool: Get metric formula definition
- query_metric_tool: Execute metric query
- get_grade_rules_tool: Get grade judgment rules
"""

import json
from typing import Any

from langchain_core.tools import tool

from app.tools.sql_tools import (
    execute_safe_sql,
    validate_column_name,
    validate_time_range_sql,
)


@tool
async def get_formula_definition_tool(metric_name: str) -> dict[str, Any]:
    """Get formula definition and metadata by metric name.

    Used to convert Chinese metric names (e.g., "PsIronLoss") to database column names.

    Args:
        metric_name: Metric name, e.g., "PsIronLoss", "LaminationFactor", "ThicknessRange"

    Returns:
        Metric metadata dict containing:
        - id: Formula ID
        - name: Metric name
        - column_name: Database column name (e.g., PerfPsLoss)
        - formula: Formula expression
        - unit: Unit (e.g., W/kg)
        - formula_type: Formula type (CALC/JUDGE/NO)
        - description: Metric description
    """
    sql = """
        SELECT
            F_Id as id,
            F_FORMULA_NAME as name,
            F_COLUMN_NAME as column_name,
            F_FORMULA as formula,
            F_UNIT_NAME as unit,
            F_FORMULA_TYPE as formula_type,
            F_REMARK as description
        FROM lab_intermediate_data_formula
        WHERE F_FORMULA_NAME = :metric_name
           OR F_COLUMN_NAME = :metric_name
        LIMIT 1
    """

    results = await execute_safe_sql(sql, {"metric_name": metric_name})

    if not results:
        # Try fuzzy match for suggestions
        sql_fuzzy = """
            SELECT
                F_Id as id,
                F_FORMULA_NAME as name,
                F_COLUMN_NAME as column_name,
                F_FORMULA as formula,
                F_UNIT_NAME as unit,
                F_FORMULA_TYPE as formula_type,
                F_REMARK as description
            FROM lab_intermediate_data_formula
            WHERE F_FORMULA_NAME LIKE :pattern
               OR F_COLUMN_NAME LIKE :pattern
            LIMIT 1
        """
        results = await execute_safe_sql(sql_fuzzy, {"pattern": f"%{metric_name}%"})
        if results:
            # Fuzzy match found but not exact match
            return {
                "found": False,
                "error": f"Metric not found: {metric_name}",
                "suggestions": await _get_similar_metrics(metric_name),
            }

    if not results:
        return {
            "found": False,
            "error": f"Metric not found: {metric_name}",
            "suggestions": await _get_similar_metrics(metric_name),
        }

    result = results[0]
    result["found"] = True
    return result


async def _get_similar_metrics(metric_name: str) -> list[str]:
    """Get similar metric names as suggestions."""
    sql = """
        SELECT F_FORMULA_NAME as name
        FROM lab_intermediate_data_formula
        WHERE F_FORMULA_NAME LIKE :pattern
        LIMIT 5
    """
    results = await execute_safe_sql(sql, {"pattern": f"%{metric_name[0]}%"})
    return [r["name"] for r in results]


@tool
async def query_metric_tool(
    column_name: str,
    aggregation: str,
    time_range_sql: str,
    shift: str | None = None,
    group_by_date: bool = False,
) -> dict[str, Any]:
    """Execute metric aggregation query.

    Query aggregation values (average, max, min, etc.) from LAB_INTERMEDIATE_DATA table.

    Args:
        column_name: Database column name, e.g., "PerfPsLoss", "LaminationFactor"
        aggregation: Aggregation function, options: AVG, MAX, MIN, SUM, COUNT
        time_range_sql: Time range SQL condition, e.g.,
            "DetectionDate >= DATE_SUB(NOW(), INTERVAL 7 DAY)"
        shift: Shift filter, e.g., "A", "B", "C"
        group_by_date: Whether to group by date (for trend charts)

    Returns:
        Query result dict containing:
        - value: Aggregation value (or value list if grouped by date)
        - count: Record count
        - column_name: Column name
        - aggregation: Aggregation method
    """
    # Validate aggregation function
    allowed_aggs = ["AVG", "MAX", "MIN", "SUM", "COUNT"]
    if aggregation.upper() not in allowed_aggs:
        return {
            "error": (
                f"Unsupported aggregation function: {aggregation}, "
                f"supported: {', '.join(allowed_aggs)}"
            )
        }

    # Validate column name security
    if not validate_column_name(column_name):
        return {"error": f"Invalid column name: {column_name}"}

    # Validate time range SQL security
    if not validate_time_range_sql(time_range_sql):
        return {"error": "Invalid time range condition"}

    # Build WHERE clause
    where_clauses = [time_range_sql]
    params: dict[str, Any] = {}

    if shift:
        where_clauses.append("F_SHIFT = :shift")
        params["shift"] = shift

    where_clause = " AND ".join(where_clauses)

    try:
        if group_by_date:
            # Group by date query (for trend charts)
            sql = f"""
                SELECT
                    DATE(F_DETECTION_DATE) as date,
                    {aggregation.upper()}({column_name}) as value,
                    COUNT(*) as count
                FROM lab_intermediate_data
                WHERE {where_clause}
                    AND {column_name} IS NOT NULL
                GROUP BY DATE(F_DETECTION_DATE)
                ORDER BY date
            """  # noqa: S608
        else:
            # Single aggregation value query
            sql = f"""
                SELECT
                    {aggregation.upper()}({column_name}) as value,
                    COUNT(*) as count
                FROM lab_intermediate_data
                WHERE {where_clause}
                    AND {column_name} IS NOT NULL
            """  # noqa: S608

        results = await execute_safe_sql(sql, params)

        if not results or results[0].get("value") is None:
            return {
                "value": None,
                "count": 0,
                "column_name": column_name,
                "aggregation": aggregation,
                "message": "No data found matching criteria",
            }

        if group_by_date:
            return {
                "values": results,
                "count": sum(r.get("count", 0) for r in results),
                "column_name": column_name,
                "aggregation": aggregation,
            }

        return {
            "value": float(results[0]["value"]),
            "count": results[0]["count"],
            "column_name": column_name,
            "aggregation": aggregation,
        }

    except Exception as e:
        return {
            "error": str(e),
            "column_name": column_name,
            "aggregation": aggregation,
        }


@tool
async def get_grade_rules_tool(
    formula_id: int,
    metric_value: float,
    spec_id: int | None = None,
) -> dict[str, Any]:
    """Get grade judgment rules for metric and determine grade based on value.

    Read judgment rules from LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL table,
    determine grade based on provided metric value.

    Args:
        formula_id: Formula ID (from get_formula_definition_tool)
        metric_value: Metric value for grade determination
        spec_id: Product spec ID (optional, for spec-specific judgment criteria)

    Returns:
        Grade judgment result containing:
        - grade: Determined grade name (e.g., "Premium")
        - quality_status: Quality status
        - color: Grade color indicator
        - all_rules: All available judgment rules
        - matched_rule: Matched rule details
    """
    # Get judgment rules
    sql = """
        SELECT
            F_Id as id,
            F_FORMULA_ID as formula_id,
            F_NAME as name,
            F_PRIORITY as priority,
            F_QUALITY_STATUS as quality_status,
            F_COLOR as color,
            F_IS_DEFAULT as is_default,
            F_CONDITION as condition_json
        FROM lab_intermediate_data_judgment_level
        WHERE F_FORMULA_ID = :formula_id
        ORDER BY F_PRIORITY DESC
    """

    results = await execute_safe_sql(sql, {"formula_id": formula_id})

    if not results:
        return {
            "found": False,
            "error": f"No judgment rules found for formula_id={formula_id}",
            "grade": None,
        }

    # Parse rules and determine grade
    all_rules = []
    matched_rule = None

    for rule in results:
        # Parse conditions
        conditions = []
        if rule.get("condition_json"):
            try:
                conditions = json.loads(rule["condition_json"])
            except json.JSONDecodeError:
                conditions = []

        rule_info = {
            "id": rule["id"],
            "name": rule["name"],
            "priority": rule["priority"],
            "quality_status": rule["quality_status"],
            "color": rule["color"],
            "is_default": rule["is_default"],
            "conditions": conditions,
        }
        all_rules.append(rule_info)

        # Check if conditions are met (if no conditions, treat as default rule)
        if matched_rule is None:
            if not conditions:
                # Default rule (no conditions)
                if rule["is_default"]:
                    matched_rule = rule_info
            elif _check_conditions(metric_value, conditions):
                matched_rule = rule_info

    # If no condition rules matched, use default rule
    if matched_rule is None:
        default_rules = [r for r in all_rules if r.get("is_default")]
        if default_rules:
            matched_rule = default_rules[0]
        else:
            matched_rule = all_rules[-1] if all_rules else None

    return {
        "found": True,
        "grade": matched_rule["name"] if matched_rule else "Unknown",
        "quality_status": matched_rule["quality_status"] if matched_rule else None,
        "color": matched_rule["color"] if matched_rule else "#999999",
        "all_rules": all_rules,
        "matched_rule": matched_rule,
        "metric_value": metric_value,
    }


def _check_conditions(metric_value: float, conditions: list[dict]) -> bool:
    """Check if metric value satisfies condition list.

    Args:
        metric_value: Metric value
        conditions: Condition list, each condition contains operator, value

    Returns:
        Whether all conditions are satisfied
    """
    if not conditions:
        return True

    for cond in conditions:
        operator = cond.get("operator", "<=")
        threshold = cond.get("value")

        if threshold is None:
            continue

        threshold = float(threshold)

        if operator == "<=" and not (metric_value <= threshold):
            return False
        elif operator == "<" and not (metric_value < threshold):
            return False
        elif operator == ">=" and not (metric_value >= threshold):
            return False
        elif operator == ">" and not (metric_value > threshold):
            return False
        elif operator == "==" and not (metric_value == threshold):
            return False

    return True


# 产品规格带宽映射（规格代码 -> 带宽范围）
SPEC_WIDTH_RANGES = {
    "120": {"min": 119.5, "max": 120.5, "name": "120"},
    "142": {"min": 141.5, "max": 142.5, "name": "142"},
    "170": {"min": 169.5, "max": 170.5, "name": "170"},
    "213": {"min": 212.5, "max": 213.5, "name": "213"},
}


@tool
async def get_product_specs_tool() -> dict[str, Any]:
    """Get all available product specifications.

    Returns list of product specs with code, name, and width range.

    Returns:
        Dict containing list of specs with code, name, width_min, width_max
    """
    sql = """
        SELECT F_Id as id, F_CODE as code, F_NAME as name, F_DETECTION_COLUMNS as detection_columns
        FROM lab_product_spec
        WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0) AND F_ENABLEDMARK = 1
        ORDER BY F_CODE
    """

    results = await execute_safe_sql(sql, {})

    specs = []
    for r in results:
        code = str(r["code"])
        width_range = SPEC_WIDTH_RANGES.get(code, {})
        specs.append(
            {
                "id": str(r["id"]),
                "code": code,
                "name": r["name"],
                "detection_columns": r["detection_columns"],
                "width_min": width_range.get("min"),
                "width_max": width_range.get("max"),
            }
        )

    return {
        "found": len(specs) > 0,
        "specs": specs,
        "count": len(specs),
    }


@tool
async def get_grade_rules_by_spec_tool(
    formula_id: str,
    spec_code: str | None = None,
) -> dict[str, Any]:
    """Get grade judgment rules for a metric, optionally filtered by product spec.

    Different product specs have different judgment criteria. Rules are matched
    by checking the Width conditions in the rule's condition JSON.

    Args:
        formula_id: Formula ID (e.g., "Labeling", "MagneticResult", "LaminationResult")
        spec_code: Product spec code (e.g., "120", "142", "170", "213").
                  If None or "all", returns rules for all specs.

    Returns:
        Dict containing rules grouped by spec
    """
    # 查询判定规则，关联产品规格表获取规格代码
    sql = """
        SELECT
            j.F_Id as id,
            j.F_FORMULA_ID as formula_id,
            j.F_NAME as name,
            j.F_PRIORITY as priority,
            j.F_QUALITY_STATUS as quality_status,
            j.F_COLOR as color,
            j.F_IS_DEFAULT as is_default,
            j.F_CONDITION as condition_json,
            p.F_CODE as spec_code
        FROM lab_intermediate_data_judgment_level j
        LEFT JOIN lab_product_spec p ON j.F_PRODUCT_SPEC_ID = p.F_Id
        WHERE j.F_FORMULA_ID = :formula_id
        ORDER BY j.F_PRIORITY DESC, j.F_NAME
    """

    results = await execute_safe_sql(sql, {"formula_id": formula_id})

    if not results:
        return {
            "found": False,
            "error": f"No judgment rules found for formula_id={formula_id}",
            "rules": [],
        }

    # 按规格分组（使用F_PRODUCT_SPEC_ID关联的规格代码）
    rules_by_spec: dict[str, list[dict]] = {}
    unmatched_rules: list[dict] = []

    for r in results:
        rule_info = {
            "id": str(r["id"]),
            "name": r["name"],
            "priority": r["priority"],
            "quality_status": r["quality_status"],
            "color": r["color"],
            "is_default": r["is_default"],
        }

        # 使用关联表获取的规格代码
        spec = r.get("spec_code")
        if spec:
            if spec not in rules_by_spec:
                rules_by_spec[spec] = []
            rules_by_spec[spec].append(rule_info)
        else:
            unmatched_rules.append(rule_info)

    # 如果指定了规格，只返回该规格的规则
    if spec_code and spec_code.lower() != "all":
        spec_rules = rules_by_spec.get(spec_code, [])
        return {
            "found": len(spec_rules) > 0,
            "formula_id": formula_id,
            "spec_code": spec_code,
            "rules": spec_rules,
            "count": len(spec_rules),
        }

    # 返回所有规格的规则
    return {
        "found": len(rules_by_spec) > 0,
        "formula_id": formula_id,
        "spec_code": "all",
        "rules_by_spec": rules_by_spec,
        "unmatched_rules": unmatched_rules,
        "available_specs": list(rules_by_spec.keys()),
        "total_rules": len(results),
    }


@tool
async def get_judgment_types_tool() -> dict[str, Any]:
    """Get all available judgment types (判定类型).

    Returns list of judgment types like 贴标、磁性能判定、叠片系数判定等.

    Returns:
        Dict containing list of judgment types with formula_id and rule counts
    """
    sql = """
        SELECT
            F_FORMULA_ID as formula_id,
            COUNT(*) as rule_count,
            COUNT(DISTINCT F_NAME) as grade_count
        FROM lab_intermediate_data_judgment_level
        GROUP BY F_FORMULA_ID
        ORDER BY rule_count DESC
    """

    results = await execute_safe_sql(sql, {})

    # 中文名称映射
    name_mapping = {
        "Labeling": "贴标",
        "MagneticResult": "磁性能判定",
        "LaminationResult": "叠片系数判定",
        "ThicknessResult": "厚度判定",
        "FirstInspection": "一次交检",
    }

    types = []
    for r in results:
        formula_id = r["formula_id"]
        types.append(
            {
                "formula_id": formula_id,
                "name": name_mapping.get(formula_id, formula_id),
                "rule_count": r["rule_count"],
                "grade_count": r["grade_count"],
            }
        )

    return {
        "found": len(types) > 0,
        "types": types,
        "count": len(types),
    }


@tool
async def get_first_inspection_config_tool() -> dict[str, Any]:
    """Get first inspection pass rate configuration.

    Queries lab_report_config table to get the grade levels that count as
    "passed" for first inspection rate calculation.

    Returns:
        Dict containing:
        - grades: List of grade names that count as passed (e.g., ["A", "B"])
        - description: Configuration description
    """
    # 真实 lab_report_config 列：F_NAME / F_LEVEL_NAMES (JSON 数组) / F_FORMULA_ID / F_DESCRIPTION
    sql = """
        SELECT
            F_NAME as name,
            F_LEVEL_NAMES as level_names,
            F_FORMULA_ID as formula_id,
            F_DESCRIPTION as description
        FROM lab_report_config
        WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)
          AND F_ENABLEDMARK = 1
          AND F_NAME = '一次交检合格率'
        ORDER BY F_SORT_ORDER ASC
        LIMIT 1
    """

    results = await execute_safe_sql(sql, {})

    if not results:
        # Default configuration if not found
        return {
            "found": True,
            "grades": ["A"],
            "description": "默认：仅 A 级计入一次交检合格率",
        }

    config = results[0]
    level_names_raw = config.get("level_names", "")

    # F_LEVEL_NAMES 通常是 JSON 数组字符串，例如 ["A"] / ["A","B"]
    grades: list[str] = []
    if level_names_raw:
        try:
            parsed = json.loads(level_names_raw)
            if isinstance(parsed, list):
                grades = [str(g) for g in parsed]
            elif isinstance(parsed, str):
                grades = [g.strip() for g in parsed.split(",") if g.strip()]
        except json.JSONDecodeError:
            grades = [g.strip() for g in str(level_names_raw).split(",") if g.strip()]

    if not grades:
        grades = ["A"]

    return {
        "found": True,
        "grades": grades,
        "description": config.get("description") or f"按 {'、'.join(grades)} 计入一次交检合格率",
    }


@tool
async def query_first_inspection_rate_tool(
    spec_code: str | None = None,
    time_range_sql: str | None = None,
    shift: str | None = None,
) -> dict[str, Any]:
    """Query first inspection pass rate.

    Calculates the percentage of records where F_FIRST_INSPECTION matches
    the configured pass grades.

    Args:
        spec_code: Product spec code (e.g., "120", "142", "170", "213")
        time_range_sql: SQL time range filter (e.g., "F_DETECTION_DATE >= DATE_SUB(NOW(), INTERVAL 7 DAY)")
        shift: Work shift (e.g., "A", "B", "C")

    Returns:
        Dict containing:
        - pass_rate: Pass rate percentage (0-100)
        - total_count: Total number of records
        - pass_count: Number of passed records
        - spec_code: Product spec code
    """
    # 取合格判定等级（来自 lab_report_config 中"一次交检合格率"行的 F_LEVEL_NAMES）
    config = await get_first_inspection_config_tool.ainvoke({})
    pass_grades = config.get("grades", ["A"])

    # 与 lm/api MonthlyQualityReportService 对齐：按重量加权计算，重量优先取
    # lab_intermediate_data.F_SINGLE_COIL_WEIGHT，缺失时回落 lab_raw_data。
    # 不过滤 F_FIRST_INSPECTION IS NOT NULL（lm/api 把 NULL 视为未判定，仍计入总量）。
    conditions: list[str] = []
    params: dict[str, Any] = {}

    if time_range_sql:
        if not validate_time_range_sql(time_range_sql):
            return {"error": "Invalid time range SQL"}
        conditions.append(time_range_sql.replace("F_DETECTION_DATE", "d.F_DETECTION_DATE"))

    if shift:
        conditions.append("d.F_SHIFT = :shift")
        params["shift"] = shift

    if spec_code:
        conditions.append("p.F_CODE = :spec_code")
        params["spec_code"] = spec_code

    where_clause = (" AND ".join(conditions)) if conditions else "1=1"
    params["pass_grades"] = tuple(pass_grades)

    sql = f"""
        SELECT
            COUNT(*) AS total_count,
            SUM(CASE WHEN d.F_FIRST_INSPECTION IN :pass_grades THEN 1 ELSE 0 END) AS pass_count,
            COALESCE(SUM(COALESCE(d.F_SINGLE_COIL_WEIGHT, r.F_SINGLE_COIL_WEIGHT, 0)), 0) AS total_weight,
            COALESCE(SUM(CASE WHEN d.F_FIRST_INSPECTION IN :pass_grades
                              THEN COALESCE(d.F_SINGLE_COIL_WEIGHT, r.F_SINGLE_COIL_WEIGHT, 0)
                              ELSE 0 END), 0) AS pass_weight
        FROM lab_intermediate_data d
        LEFT JOIN lab_product_spec p ON d.F_PRODUCT_SPEC_ID = p.F_Id
        LEFT JOIN lab_raw_data r ON d.F_RAW_DATA_ID = r.F_Id
        WHERE {where_clause}
    """

    results = await execute_safe_sql(sql, params)

    if not results:
        return {
            "pass_rate": 0.0,
            "total_count": 0,
            "pass_count": 0,
            "total_weight_kg": 0.0,
            "pass_weight_kg": 0.0,
            "spec_code": spec_code,
        }

    result = results[0]
    total = int(result.get("total_count") or 0)
    passed = int(result.get("pass_count") or 0)
    total_weight = float(result.get("total_weight") or 0)
    pass_weight = float(result.get("pass_weight") or 0)

    # 合格率：lm/api 是按重量算（与监控页对齐）
    pass_rate = (pass_weight / total_weight * 100) if total_weight > 0 else 0.0

    return {
        "pass_rate": round(pass_rate, 2),
        "total_count": total,
        "pass_count": passed,
        "total_weight_kg": round(total_weight, 2),
        "pass_weight_kg": round(pass_weight, 2),
        "spec_code": spec_code,
        "pass_grades": pass_grades,
    }
