import re
from datetime import date, datetime, timedelta
from typing import Any

from langchain_core.messages import AIMessage, SystemMessage

from app.core.llm_factory import get_llm
from app.tools.query_tools import (
    get_first_inspection_config_tool,
    get_formula_definition_tool,
    get_grade_rules_by_spec_tool,
    get_grade_rules_tool,
    get_product_specs_tool,
    query_first_inspection_rate_tool,
    query_metric_tool,
)

QUERY_AGENT_SYSTEM_PROMPT = """You are an industrial quality data query assistant for steel strip quality data.

Answer user questions based on the provided query results. Include:
1. The metric value with unit
2. Brief explanation of how it was calculated
3. Quality grade if available
4. Any relevant context

Respond in Chinese, matching the user's language."""


async def query_agent_node(state: dict[str, Any]) -> dict[str, Any]:
    """QueryAgent node that executes tools based on extracted entities.

    Instead of relying on LLM function calling (which Ollama models don't support well),
    we directly execute tools based on the entities extracted by intent_classifier.
    """
    entities = _merge_entities_with_context(
        state.get("entities", {}),
        state.get("context", {}),
    )
    messages = state.get("messages", [])
    context = state.get("context", {})
    model_name = state.get("model_name") or None

    # Debug logging
    print(f"[DEBUG] entities: {entities}")
    print(f"[DEBUG] messages count: {len(messages)}")

    # Get the user's question
    user_question = ""
    for msg in reversed(messages):
        if hasattr(msg, "content"):
            # Check for HumanMessage by class name or type
            type_name = type(msg).__name__
            if type_name == "HumanMessage":
                user_question = str(msg.content)
                break
            if hasattr(msg, "type") and msg.type == "human":
                user_question = str(msg.content)
                break
        elif isinstance(msg, dict):
            # Handle both 'type': 'human' and 'role': 'user' formats
            if msg.get("type") == "human" or msg.get("role") == "user":
                user_question = str(msg.get("content", ""))
                break

    # Extract parameters from entities
    metric = entities.get("metric")
    aggregation = entities.get("aggregation", "AVG")
    time_range = entities.get("time_range", {})
    shift = entities.get("shift")
    query_type = entities.get("query_type", "value")

    # Check if this is a first inspection rate query
    if _detect_first_inspection_rate_query(user_question):
        return await _handle_first_inspection_rate_query(
            entities, context, messages=messages, model_name=model_name
        )

    # Check if this is a judgment rules inquiry
    judgment_type = _detect_judgment_inquiry(user_question)

    # Check for multi-turn conversation context
    # If previous conversation was about judgment rules and user just provided a spec code
    prev_judgment_type = context.get("judgment_type")
    prev_awaiting_spec = context.get("awaiting_spec_selection")

    if not judgment_type and prev_awaiting_spec and prev_judgment_type:
        # Check if user just provided a spec code or "all"/"所有"
        spec_patterns = [r"^(120|142|170|213)$", r"^(所有|all)$"]
        for pattern in spec_patterns:
            if re.search(pattern, user_question.strip(), re.IGNORECASE):
                judgment_type = prev_judgment_type
                # Set the spec_code in entities
                spec_code = user_question.strip()
                if spec_code.lower() in ["所有", "all"]:
                    spec_code = "all"
                entities = {**entities, "spec_code": spec_code}
                break

    if judgment_type:
        # Handle judgment rules query
        return await _handle_judgment_rules_query(judgment_type, entities, messages, context)

    # Handle product specs inquiry
    if entities.get("query_type") == "product_specs":
        return await _handle_product_specs_query(context)

    if not metric:
        # 没匹配到指标——可能是闲聊/元问题（"今天几号"、"你是谁"、"帮助"等）。
        # 把今天日期 + 可用能力注入 system prompt，让 LLM 自由回答。
        llm = get_llm(model_name)
        meta_system = (
            "你是实验室质量数据助理，回答用户的中文提问。\n"
            f"今天日期是 {_today_str()}，时间相对词请按此基准回答（"
            f"本周={_format_range(_compute_time_range_absolute({'type': 'this_week'}))}，"
            f"本月={_format_range(_compute_time_range_absolute({'type': 'current_month'}))}，"
            f"本年={_format_range(_compute_time_range_absolute({'type': 'this_year'}))}）。\n\n"
            "你能做的事：\n"
            "1. 回答与今天/日期/时间相关的元问题（直接给出日期，简短自然）。\n"
            "2. 若用户问的是质量数据但没指出指标，列出可用指标：厚度极差(ThicknessRange)、"
            "叠片系数(LaminationFactor)、Ps铁损(PsIronLoss)、矫顽力(Hc)、带钢宽度(StripWidth)。\n"
            '3. 若问"为什么某炉是X级"——提示用户明确给出炉号。\n'
            "4. 闲聊/感谢/打招呼——简短回一两句即可。\n\n"
            "禁止编造数据；不要输出 JSON 或代码块。"
        )
        response = await llm.ainvoke(
            [
                {"role": "system", "content": meta_system},
                *[
                    {"role": "user" if getattr(m, "type", "") == "human" else "assistant",
                     "content": str(getattr(m, "content", ""))}
                    for m in messages
                    if hasattr(m, "content")
                ],
            ]
        )
        return {
            "response": str(response.content),
            "chart_config": None,
            "intent": "query",
            "entities": entities,
            "context": context,
            "calculation_explanation": None,
            "grade_judgment": _build_empty_grade_judgment("非数据查询，直接对话。"),
        }

    # Step 1: Get formula definition
    # Map entity keys to actual database column names (English)
    db_metric_mapping = {
        "thicknessrange": "MaxThickness",  # 最大厚度
        "laminationfactor": "LaminationFactor",  # 叠片系数
        "psironloss": "PsLoss",  # Ps铁损
        "hc": "Hc",  # 矫顽力
        "stripwidth": "StripWidth",  # 带钢宽度
    }
    metric_name_for_query = db_metric_mapping.get(metric, metric)
    formula_result = await get_formula_definition_tool.ainvoke(
        {"metric_name": metric_name_for_query}
    )

    if not formula_result.get("found"):
        return {
            "response": f"未找到指标 '{metric}' 的定义。请检查指标名称是否正确。",
            "chart_config": None,
            "intent": "query",
            "entities": entities,
            "context": context,
            "calculation_explanation": None,
            "grade_judgment": _build_empty_grade_judgment("未找到指标定义，无法进行等级判定。"),
        }

    # Map formula column name to actual table column name
    # Formula table stores names like "PerfHc", but actual table uses "F_PERF_HC"
    raw_column_name = formula_result.get("column_name", "")
    column_name_mapping = {
        "PerfHc": "F_PERF_HC",
        "PerfPsLoss": "F_PERF_PS_LOSS",
        "PerfSsPower": "F_PERF_SS_POWER",
        "AfterHc": "F_AFTER_HC",
        "AfterPsLoss": "F_AFTER_PS_LOSS",
        "AfterSsPower": "F_AFTER_SS_POWER",
        "Width": "F_WIDTH",
        "LaminationFactor": "F_LAM_FACTOR",
        "LaminationFactorRes": "F_LAM_FACTOR_RES",
    }
    column_name = column_name_mapping.get(raw_column_name, raw_column_name)
    if not column_name.startswith("F_") and not column_name.startswith("f_"):
        column_name = f"F_{raw_column_name.upper()}"

    formula_desc = formula_result.get("formula", "无公式说明")
    unit = formula_result.get("unit", "")

    # Step 2: Build time range SQL
    time_range_sql = _build_time_range_sql(time_range)
    if not time_range_sql:
        time_range_sql = (
            "F_DETECTION_DATE >= DATE_SUB(NOW(), INTERVAL 7 DAY)"  # Default to last 7 days
        )

    # Step 3: Query the metric
    query_result = await query_metric_tool.ainvoke(
        {
            "column_name": column_name,
            "aggregation": aggregation,
            "time_range_sql": time_range_sql,
            "shift": shift,
            "group_by_date": False,
        }
    )

    if query_result.get("error"):
        return {
            "response": f"查询出错: {query_result['error']}",
            "chart_config": None,
            "intent": "query",
            "entities": entities,
            "context": context,
            "calculation_explanation": _build_calculation_explanation(
                metric_name=_get_metric_name_cn(metric),
                formula_source=formula_desc,
                data_fields=[column_name],
                aggregation=aggregation,
            ),
            "grade_judgment": _build_empty_grade_judgment("查询失败，暂未生成等级判定。"),
        }

    value = query_result.get("value")
    count = query_result.get("count", 0)

    if value is None:
        return {
            "response": f"未找到符合条件的数据（指标: {metric}，时间范围: {_format_time_range_desc(time_range)}）。",
            "chart_config": None,
            "intent": "query",
            "entities": entities,
            "context": context,
            "calculation_explanation": _build_calculation_explanation(
                metric_name=_get_metric_name_cn(metric),
                formula_source=formula_desc,
                data_fields=[column_name],
                aggregation=aggregation,
            ),
            "grade_judgment": _build_empty_grade_judgment("当前查询无结果，暂未生成等级判定。"),
        }

    # Step 4: Get grade rules (optional)
    grade_info = ""
    grade_result: dict[str, Any] | None = None
    formula_id_raw = formula_result.get("id")
    # Try to convert formula_id to int if it's a string number
    formula_id = None
    if formula_id_raw:
        try:
            formula_id = int(formula_id_raw)
        except (ValueError, TypeError):
            formula_id = None
    if formula_id:
        grade_result = await get_grade_rules_tool.ainvoke(
            {"formula_id": formula_id, "metric_value": value}
        )
        if grade_result.get("found"):
            grade = grade_result.get("grade", "未知")
            quality_status = grade_result.get("quality_status", "")
            grade_info = f"，判定等级为 **{grade}**（{quality_status}）"

    # Step 5: Generate response with LLM
    metric_name_cn = _get_metric_name_cn(metric)
    time_range_desc = _format_time_range_desc(time_range)
    calculation_explanation = _build_calculation_explanation(
        metric_name=metric_name_cn,
        formula_source=formula_desc,
        data_fields=[column_name],
        aggregation=aggregation,
    )
    grade_judgment = _build_grade_judgment(grade_result, value)

    prompt_context = f"""查询结果：
- 指标: {metric_name_cn} ({metric})
- 计算方式: {formula_desc}
- 单位: {unit}
- 时间范围: {time_range_desc}
- 聚合方式: {aggregation}
- 查询结果: {value} {unit}
- 数据条数: {count}
{grade_info}

请根据以上信息，用中文回答用户的问题。"""

    llm = get_llm(model_name)
    response = await llm.ainvoke(
        [
            SystemMessage(content=QUERY_AGENT_SYSTEM_PROMPT),
            *messages,
            AIMessage(content=prompt_context),
        ]
    )

    # Build chart config based on resolved query type
    chart_config = None
    chart_type = _select_chart_type(entities.get("query_type"), user_question)
    if chart_type:
        trend_result = await query_metric_tool.ainvoke(
            {
                "column_name": column_name,
                "aggregation": aggregation,
                "time_range_sql": time_range_sql,
                "shift": shift,
                "group_by_date": True,
            }
        )
        if trend_result.get("values"):
            chart_config = _build_chart_config(
                trend_result["values"],
                metric_name_cn,
                unit,
                aggregation,
                chart_type=chart_type,
            )

    updated_context = _build_query_context(context, entities)

    return {
        "response": str(response.content),
        "chart_config": chart_config,
        "intent": "query",
        "entities": entities,
        "context": updated_context,
        "calculation_explanation": calculation_explanation,
        "grade_judgment": grade_judgment,
    }


def _merge_entities_with_context(
    entities: dict[str, Any],
    context: dict[str, Any] | None,
) -> dict[str, Any]:
    """Fill missing query entities from session context for follow-up turns."""
    merged = dict(context or {})
    merged.update({key: value for key, value in entities.items() if value not in (None, "", {})})
    return merged


def _build_time_range_sql(time_range: dict) -> str | None:
    """Build time range SQL from entity."""
    if not time_range:
        return None

    tr_type = time_range.get("type")

    if tr_type == "recent_days":
        days = time_range.get("days", 7)
        return f"F_DETECTION_DATE >= DATE_SUB(NOW(), INTERVAL {days} DAY)"

    elif tr_type == "recent_weeks":
        weeks = time_range.get("weeks", 1)
        return f"F_DETECTION_DATE >= DATE_SUB(NOW(), INTERVAL {weeks} WEEK)"

    elif tr_type == "recent_months":
        months = time_range.get("months", 1)
        return f"F_DETECTION_DATE >= DATE_SUB(NOW(), INTERVAL {months} MONTH)"

    elif tr_type == "last_month":
        return "F_DETECTION_DATE >= DATE_FORMAT(DATE_SUB(NOW(), INTERVAL 1 MONTH), '%Y-%m-01') AND F_DETECTION_DATE < DATE_FORMAT(NOW(), '%Y-%m-01')"

    elif tr_type == "current_month":
        return "F_DETECTION_DATE >= DATE_FORMAT(NOW(), '%Y-%m-01')"

    elif tr_type == "today":
        return "DATE(F_DETECTION_DATE) = CURDATE()"

    elif tr_type == "yesterday":
        return "DATE(F_DETECTION_DATE) = DATE_SUB(CURDATE(), INTERVAL 1 DAY)"

    elif tr_type == "this_week":
        # 周一为本周起点（YEARWEEK mode 1）。
        return "YEARWEEK(F_DETECTION_DATE, 1) = YEARWEEK(CURDATE(), 1)"

    elif tr_type == "last_week":
        return "YEARWEEK(F_DETECTION_DATE, 1) = YEARWEEK(DATE_SUB(CURDATE(), INTERVAL 1 WEEK), 1)"

    elif tr_type == "this_year":
        return "YEAR(F_DETECTION_DATE) = YEAR(CURDATE())"

    elif tr_type == "last_year":
        return "YEAR(F_DETECTION_DATE) = YEAR(CURDATE()) - 1"

    elif tr_type == "year_month":
        year = time_range.get("year")
        month = time_range.get("month")
        if year and month:
            next_month = month + 1 if month < 12 else 1
            next_year = year if month < 12 else year + 1
            return f"F_DETECTION_DATE >= '{year}-{month:02d}-01' AND F_DETECTION_DATE < '{next_year}-{next_month:02d}-01'"

    elif tr_type == "month":
        month = time_range.get("month")
        if month:
            return f"MONTH(F_DETECTION_DATE) = {month}"

    elif tr_type == "year":
        year = time_range.get("year")
        if year:
            return f"YEAR(F_DETECTION_DATE) = {year}"

    return None


def _format_time_range_desc(time_range: dict) -> str:
    """Format time range for display."""
    if not time_range:
        return "最近7天"

    tr_type = time_range.get("type")

    if tr_type == "recent_days":
        return f"最近{time_range.get('days', 7)}天"
    elif tr_type == "recent_weeks":
        return f"最近{time_range.get('weeks', 1)}周"
    elif tr_type == "recent_months":
        return f"最近{time_range.get('months', 1)}个月"
    elif tr_type == "last_month":
        return "上个月"
    elif tr_type == "current_month":
        return "本月"
    elif tr_type == "today":
        return "今日"
    elif tr_type == "yesterday":
        return "昨日"
    elif tr_type == "this_week":
        return "本周"
    elif tr_type == "last_week":
        return "上周"
    elif tr_type == "this_year":
        return "本年"
    elif tr_type == "last_year":
        return "去年"
    elif tr_type == "year":
        return f"{time_range.get('year')}年"
    elif tr_type == "year_month":
        return f"{time_range.get('year')}年{time_range.get('month')}月"
    elif tr_type == "month":
        return f"{time_range.get('month')}月"

    return "最近7天"


def _get_metric_name_cn(metric: str) -> str:
    """Get Chinese name for metric."""
    mapping = {
        "thicknessrange": "厚度极差",
        "laminationfactor": "叠片系数",
        "psironloss": "Ps铁损",
        "hc": "矫顽力",
        "stripwidth": "带钢宽度",
    }
    return mapping.get(metric.lower(), metric)


def _build_chart_config(
    values: list[dict],
    metric_name: str,
    unit: str,
    aggregation: str,
    chart_type: str,
) -> dict[str, Any]:
    """Build chart configuration for adaptive query visualizations."""
    data = [
        {
            "date": str(value.get("date", "")),
            "value": float(value.get("value", 0)),
        }
        for value in values
    ]
    title_suffix = {
        "line": "趋势图",
        "bar": "分布图",
        "pie": "占比图",
    }
    config: dict[str, Any] = {
        "type": chart_type,
        "title": f"{metric_name} {title_suffix.get(chart_type, '图表')}",
        "data": data,
        "meta": {
            "metricName": metric_name,
            "unit": unit,
            "aggregation": aggregation,
        },
    }

    if chart_type in {"line", "bar"}:
        config["xField"] = "date"
        config["yField"] = "value"

    if chart_type == "pie":
        config["angleField"] = "value"
        config["colorField"] = "date"

    return config


def _select_chart_type(query_type: str | None, user_question: str) -> str | None:
    """Choose chart type from resolved query semantics."""
    query_type_normalized = (query_type or "").lower()
    if query_type_normalized == "trend":
        return "line"
    if query_type_normalized in {"distribution", "comparison", "ranking"}:
        return "bar"
    if query_type_normalized in {"composition", "proportion", "share"}:
        return "pie"

    if any(keyword in user_question for keyword in ["趋势", "变化", "走势"]):
        return "line"
    if any(keyword in user_question for keyword in ["分布", "对比", "排名", "柱状"]):
        return "bar"
    if any(keyword in user_question for keyword in ["占比", "比例", "构成", "饼图"]):
        return "pie"

    return None


def _build_query_context(
    existing_context: dict[str, Any] | None,
    entities: dict[str, Any],
) -> dict[str, Any]:
    """Persist only reusable query context across turns."""
    context = dict(existing_context or {})
    for key in ["metric", "spec_code", "time_range", "aggregation", "shift", "query_type"]:
        value = entities.get(key)
        if value not in (None, "", {}):
            context[key] = value
    return context


def _build_calculation_explanation(
    metric_name: str,
    formula_source: str,
    data_fields: list[str],
    aggregation: str,
) -> dict[str, Any]:
    """Build structured explanation for how the metric result was calculated."""
    fields_text = "、".join(data_fields)
    return {
        "formula_source": formula_source,
        "data_fields": data_fields,
        "natural_language": (
            f"{metric_name}使用{formula_source}，基于字段 {fields_text} 按 {aggregation} 计算。"
        ),
    }


def _build_empty_grade_judgment(summary: str) -> dict[str, Any]:
    """Build empty grade judgment payload for flows without rule matches."""
    return {
        "available": False,
        "grade": None,
        "quality_status": None,
        "color": None,
        "metric_value": None,
        "matched_rule": None,
        "all_rules": [],
        "summary": summary,
    }


def _build_grade_judgment(
    grade_result: dict[str, Any] | None,
    metric_value: float,
) -> dict[str, Any]:
    """Build structured grade judgment payload from tool output."""
    if not grade_result or not grade_result.get("found"):
        return _build_empty_grade_judgment("当前未查询到可用等级判定规则。")

    grade = grade_result.get("grade")
    quality_status = grade_result.get("quality_status")
    if grade and quality_status:
        summary = f"当前结果判定为 {grade}（{quality_status}）。"
    elif grade:
        summary = f"当前结果判定为 {grade}。"
    else:
        summary = "已查询等级规则，但未得到明确等级。"

    return {
        "available": True,
        "grade": grade,
        "quality_status": quality_status,
        "color": grade_result.get("color"),
        "metric_value": metric_value,
        "matched_rule": grade_result.get("matched_rule"),
        "all_rules": grade_result.get("all_rules", []),
        "summary": summary,
    }


def _detect_judgment_inquiry(question: str) -> str | None:
    """Detect if user is asking about judgment rules.

    Args:
        question: User's question

    Returns:
        Judgment type formula_id if detected, None otherwise
    """
    # 判定规则关键词映射
    judgment_keywords = {
        "Labeling": ["贴标", "标签", "Labeling", "label"],
        "MagneticResult": ["磁性能", "磁性能判定", "magnetic", "铁损", "矫顽力"],
        "LaminationResult": ["叠片系数", "叠片系数判定", "叠片", "lamination"],
        "ThicknessResult": ["厚度判定", "厚度", "thickness result"],
        "FirstInspection": ["一次交检", "first inspection"],
    }

    question_lower = question.lower()

    # 检查是否询问判定规则
    rule_keywords = [
        "判定规则",
        "判定规格",
        "规则",
        "规格",
        "标准",
        "等级",
        "grade",
        "rule",
        "judgment",
        "判定",
    ]
    is_asking_rules = any(kw in question_lower for kw in rule_keywords)

    if not is_asking_rules:
        return None

    # 检测具体判定类型
    for formula_id, keywords in judgment_keywords.items():
        if any(kw in question for kw in keywords):
            return formula_id

    return None


async def _handle_judgment_rules_query(
    judgment_type: str, entities: dict, messages: list, context: dict | None = None
) -> dict[str, Any]:
    """Handle judgment rules query by product spec.

    Args:
        judgment_type: Formula ID for judgment type (e.g., "Labeling")
        entities: Extracted entities
        messages: Message history
        context: Previous conversation context for multi-turn support

    Returns:
        Response with judgment rules and updated context
    """
    context = context or {}
    # 中文名称映射
    name_mapping = {
        "Labeling": "贴标",
        "MagneticResult": "磁性能判定",
        "LaminationResult": "叠片系数判定",
        "ThicknessResult": "厚度判定",
        "FirstInspection": "一次交检",
    }
    judgment_name = name_mapping.get(judgment_type, judgment_type)

    # 检查用户是否指定了规格
    spec_code = entities.get("spec_code")

    # 如果没有指定规格，先查询可用规格
    if not spec_code:
        specs_result = await get_product_specs_tool.ainvoke({})

        if not specs_result.get("found"):
            return {
                "response": f"未找到产品规格信息，无法查询{judgment_name}判定规则。",
                "chart_config": None,
                "intent": "query",
                "entities": {"judgment_type": judgment_type, "query_type": "judgment_rules"},
                "context": {
                    **context,
                    "judgment_type": judgment_type,
                    "awaiting_spec_selection": False,
                },
            }

        specs = specs_result.get("specs", [])
        spec_list = "\n".join(
            [
                f"- **{s['code']}**: 带宽 {s['width_min']}-{s['width_max']} mm"
                for s in specs
                if s.get("width_min")
            ]
        )

        # 查询所有规格的判定规则概览
        rules_result = await get_grade_rules_by_spec_tool.ainvoke(
            {"formula_id": judgment_type, "spec_code": "all"}
        )

        total_rules = rules_result.get("total_rules", 0)

        if total_rules == 0:
            return {"response": f"未找到{judgment_name}的判定规则。", "chart_config": None}

        available_specs = rules_result.get("available_specs", [])

        response = f"""## {judgment_name}判定规则

### 可选产品规格
{spec_list}

### 判定规则概览
- **总规则数**: {total_rules} 条
- **适用规格**: {", ".join(available_specs)}

每条判定规则针对不同产品规格有不同的判定标准。请指定您想查看的规格代码（如 **120**、**142**、**170**、**213**），或回复 **"所有"** 查看全部规格的判定规则。"""

        # Set context flag indicating we're awaiting spec selection
        return {
            "response": response,
            "chart_config": None,
            "intent": "query",
            "entities": {"judgment_type": judgment_type, "query_type": "judgment_rules"},
            "context": {**context, "judgment_type": judgment_type, "awaiting_spec_selection": True},
        }

    # 用户指定了规格，查询该规格的详细规则
    if spec_code.lower() == "所有" or spec_code.lower() == "all":
        # 查询所有规格的详细规则
        rules_result = await get_grade_rules_by_spec_tool.ainvoke(
            {"formula_id": judgment_type, "spec_code": "all"}
        )

        total_rules = rules_result.get("total_rules", 0)
        if total_rules == 0:
            return {
                "response": f"未找到{judgment_name}的判定规则。",
                "chart_config": None,
                "intent": "query",
                "entities": {
                    "judgment_type": judgment_type,
                    "query_type": "judgment_rules",
                    "spec_code": "all",
                },
                "context": {
                    **context,
                    "judgment_type": judgment_type,
                    "awaiting_spec_selection": False,
                },
            }

        rules_by_spec = rules_result.get("rules_by_spec", {})
        unmatched_rules = rules_result.get("unmatched_rules", [])

        response = f"""## {judgment_name}判定规则（共{total_rules}条）\n\n"""

        # 如果有按规格分组的规则，按规格显示
        if rules_by_spec:
            for spec in ["120", "142", "170", "213"]:
                if spec in rules_by_spec:
                    rules = rules_by_spec[spec]
                    response += f"\n### {spec} 规格（带宽 {119.5 + int(spec) - 120}-{120.5 + int(spec) - 120} mm）\n\n"

                    # 按等级分组
                    grades = {}
                    for r in rules:
                        grade = r.get("name", "未知")
                        if grade not in grades:
                            grades[grade] = r

                    for grade_name, grade_info in sorted(
                        grades.items(), key=lambda x: -x[1].get("priority", 0)
                    ):
                        quality_status = grade_info.get("quality_status", "")
                        status_text = (
                            "合格"
                            if quality_status == "1"
                            else "不合格"
                            if quality_status == "0"
                            else "未知"
                        )
                        response += f"- **{grade_name}**: {status_text} (优先级: {grade_info.get('priority', 'N/A')})\n"

        # 如果有未匹配的规则，也显示出来
        if unmatched_rules:
            response += "\n### 其他判定规则\n\n"
            grades = {}
            for r in unmatched_rules:
                grade = r.get("name", "未知")
                if grade not in grades:
                    grades[grade] = r

            for grade_name, grade_info in sorted(
                grades.items(), key=lambda x: -x[1].get("priority", 0)
            ):
                quality_status = grade_info.get("quality_status", "")
                status_text = (
                    "合格"
                    if quality_status == "1"
                    else "不合格"
                    if quality_status == "0"
                    else "未知"
                )
                response += f"- **{grade_name}**: {status_text} (优先级: {grade_info.get('priority', 'N/A')})\n"

        return {
            "response": response,
            "chart_config": None,
            "intent": "query",
            "entities": {
                "judgment_type": judgment_type,
                "query_type": "judgment_rules",
                "spec_code": "all",
            },
            "context": {
                **context,
                "judgment_type": judgment_type,
                "awaiting_spec_selection": False,
            },
        }

    # 查询指定规格的详细规则
    rules_result = await get_grade_rules_by_spec_tool.ainvoke(
        {"formula_id": judgment_type, "spec_code": spec_code}
    )

    if not rules_result.get("found"):
        return {
            "response": f"未找到{judgment_name}在 **{spec_code}** 规格下的判定规则。\n\n可用规格：120、142、170、213",
            "chart_config": None,
            "intent": "query",
            "entities": {
                "judgment_type": judgment_type,
                "query_type": "judgment_rules",
                "spec_code": spec_code,
            },
            "context": {
                **context,
                "judgment_type": judgment_type,
                "awaiting_spec_selection": False,
            },
        }

    rules = rules_result.get("rules", [])

    # 按等级分组
    grades = {}
    for r in rules:
        grade = r.get("name", "未知")
        if grade not in grades:
            grades[grade] = r

    response = f"""## {judgment_name}判定规则（{spec_code} 规格）

### 判定等级
"""

    for grade_name, grade_info in sorted(grades.items(), key=lambda x: -x[1].get("priority", 0)):
        quality_status = grade_info.get("quality_status", "")
        status_text = (
            "合格" if quality_status == "1" else "不合格" if quality_status == "0" else "未知"
        )
        response += f"\n- **{grade_name}** (优先级: {grade_info.get('priority', 'N/A')})\n"
        response += f"  - 质量状态: {status_text}\n"

    return {
        "response": response,
        "chart_config": None,
        "intent": "query",
        "entities": {
            "judgment_type": judgment_type,
            "query_type": "judgment_rules",
            "spec_code": spec_code,
        },
        "context": {**context, "judgment_type": judgment_type, "awaiting_spec_selection": False},
    }


async def _handle_product_specs_query(context: dict | None = None) -> dict[str, Any]:
    """Handle product specs inquiry.

    Args:
        context: Previous conversation context

    Returns:
        Response with product specs list
    """
    from app.tools.query_tools import get_product_specs_tool

    specs_result = await get_product_specs_tool.ainvoke({})

    if not specs_result.get("found"):
        return {
            "response": "未找到产品规格信息。",
            "chart_config": None,
            "intent": "query",
            "entities": {"query_type": "product_specs"},
            "context": context or {},
        }

    specs = specs_result.get("specs", [])
    count = specs_result.get("count", 0)

    # Build response
    response = "## 产品规格信息\n\n"
    response += f"**共有 {count} 种产品规格：**\n\n"

    for spec in specs:
        code = spec.get("code", "")
        name = spec.get("name", "")
        width_min = spec.get("width_min")
        width_max = spec.get("width_max")

        if width_min and width_max:
            response += f"- **{code}**：带宽 {width_min}-{width_max} mm"
            if name:
                response += f"（{name}）"
            response += "\n"
        else:
            response += f"- **{code}**"
            if name:
                response += f"（{name}）"
            response += "\n"

    return {
        "response": response,
        "chart_config": None,
        "intent": "query",
        "entities": {"query_type": "product_specs"},
        "context": context or {},
    }


def _detect_first_inspection_rate_query(question: str) -> bool:
    """Detect if user is asking about first inspection pass rate.

    Args:
        question: User's question

    Returns:
        True if asking about first inspection rate, False otherwise
    """
    keywords = [
        "一次交检合格率",
        "一次交检合格",
        "交检合格率",
        "first inspection rate",
        "first inspection pass rate",
        "合格率",
    ]
    question_lower = question.lower()
    return any(kw in question_lower for kw in keywords)


def _today_str() -> str:
    """Return ISO date for today (server clock)."""
    return date.today().isoformat()


def _format_range(rng: tuple[date, date] | None) -> str:
    """Format a (start, end) date range as 'YYYY-MM-DD 至 YYYY-MM-DD'."""
    if not rng:
        return "未知区间"
    start, end = rng
    if start == end:
        return start.isoformat()
    return f"{start.isoformat()} 至 {end.isoformat()}"


def _compute_time_range_absolute(time_range: dict) -> tuple[date, date] | None:
    """Compute (start_date, end_date) inclusive from a time_range entity.

    与 _build_time_range_sql 的语义保持一致——这里用 Python 算一遍以便
    把绝对日期注入到 LLM narrative 上下文，让助理在解释时可以说"4 月份
    （2026-04-01 至 2026-04-27）"而不是只说"本月"。

    日期源：服务器系统时钟 (date.today())。MySQL 的 CURDATE() 与之保持
    时区一致即正确——本地开发可能有几小时偏差但对天级别区间无影响。
    """
    if not time_range:
        return None
    today = date.today()
    tr_type = time_range.get("type")

    if tr_type == "today":
        return today, today
    if tr_type == "yesterday":
        y = today - timedelta(days=1)
        return y, y
    if tr_type == "this_week":
        # 周一为本周起点
        start = today - timedelta(days=today.weekday())
        return start, today
    if tr_type == "last_week":
        this_monday = today - timedelta(days=today.weekday())
        last_monday = this_monday - timedelta(days=7)
        last_sunday = this_monday - timedelta(days=1)
        return last_monday, last_sunday
    if tr_type == "current_month":
        start = today.replace(day=1)
        return start, today
    if tr_type == "last_month":
        first_of_this = today.replace(day=1)
        last_of_last = first_of_this - timedelta(days=1)
        first_of_last = last_of_last.replace(day=1)
        return first_of_last, last_of_last
    if tr_type == "this_year":
        return date(today.year, 1, 1), today
    if tr_type == "last_year":
        return date(today.year - 1, 1, 1), date(today.year - 1, 12, 31)
    if tr_type == "year":
        year = int(time_range.get("year") or today.year)
        return date(year, 1, 1), date(year, 12, 31)
    if tr_type == "year_month":
        year = int(time_range.get("year") or today.year)
        month = int(time_range.get("month") or today.month)
        start = date(year, month, 1)
        # 该月最后一天
        if month == 12:
            end = date(year, 12, 31)
        else:
            end = date(year, month + 1, 1) - timedelta(days=1)
        return start, end
    if tr_type == "month":
        month = int(time_range.get("month") or today.month)
        start = date(today.year, month, 1)
        if month == 12:
            end = date(today.year, 12, 31)
        else:
            end = date(today.year, month + 1, 1) - timedelta(days=1)
        return start, end
    if tr_type == "recent_days":
        days = int(time_range.get("days") or 7)
        return today - timedelta(days=days - 1), today
    if tr_type == "recent_weeks":
        weeks = int(time_range.get("weeks") or 1)
        return today - timedelta(weeks=weeks), today
    if tr_type == "recent_months":
        months = int(time_range.get("months") or 1)
        # 简化处理：30 天 * months
        return today - timedelta(days=30 * months), today
    return None


def user_question_for_narrative(messages: list[Any]) -> str:
    """Extract last human message content for LLM narrative grounding."""
    for msg in reversed(messages or []):
        if hasattr(msg, "content") and getattr(msg, "type", "") == "human":
            return str(msg.content)
        if isinstance(msg, dict) and (msg.get("type") == "human" or msg.get("role") == "user"):
            return str(msg.get("content", ""))
    return ""


async def _handle_first_inspection_rate_query(
    entities: dict,
    context: dict,
    messages: list[Any] | None = None,
    model_name: str | None = None,
) -> dict[str, Any]:
    """Handle first inspection pass rate query.

    Args:
        entities: Extracted entities
        context: Previous conversation context
        messages: Original chat messages (for LLM narrative context)
        model_name: LLM model name (None = .env default)

    Returns:
        Response with first inspection pass rate
    """
    # Get spec code from entities if provided
    spec_code = entities.get("spec_code")
    time_range = entities.get("time_range", {})
    shift = entities.get("shift")
    messages = messages or []

    # Build time range SQL
    time_range_sql = _build_time_range_sql(time_range)

    # Query first inspection rate
    result = await query_first_inspection_rate_tool.ainvoke(
        {
            "spec_code": spec_code,
            "time_range_sql": time_range_sql,
            "shift": shift,
        }
    )

    if result.get("error"):
        return {
            "response": f"查询一次交检合格率出错: {result['error']}",
            "chart_config": None,
            "intent": "query",
            "entities": {**entities, "query_type": "first_inspection_rate"},
            "context": context,
        }

    pass_rate = result.get("pass_rate", 0)
    total_count = result.get("total_count", 0)
    pass_count = result.get("pass_count", 0)
    total_weight = result.get("total_weight_kg", 0)
    pass_weight = result.get("pass_weight_kg", 0)
    pass_grades = result.get("pass_grades", ["A"])

    spec_text = f"{spec_code}规格" if spec_code else "全部规格"
    time_text = _format_time_range_desc(time_range)
    abs_range = _compute_time_range_absolute(time_range)
    abs_text = (
        f"{abs_range[0].isoformat()} 至 {abs_range[1].isoformat()}"
        if abs_range
        else time_text
    )
    grades_text = "、".join(pass_grades)

    fact_block = (
        f"统计范围：{spec_text}，{time_text}（{abs_text}）\n"
        f"合格等级：{grades_text}\n"
        f"合格率（按重量）：{pass_rate}%\n"
        f"合格卷重：{pass_weight} kg / 总卷重：{total_weight} kg\n"
        f"合格件数：{pass_count} 件 / 总件数：{total_count} 件"
    )

    narrative_system = (
        "你是一名实验室质量主管助理。下面是一次交检合格率的结构化统计结果，"
        "请用 2-3 句简明中文向班组长口头汇报：先报核心数字（合格率%、合格/总卷重、件数），"
        "再点出关键观察（如合格率为 0% 时，必须指出'F_FIRST_INSPECTION 列没有一条记录达到合格等级，全部记录都标注了缺陷或为空'）。"
        "禁止再输出 JSON、markdown 表格或代码块；输出纯文本即可。"
        "今天是 {today}。"
    ).format(today=_today_str())
    narrative_user = (
        f"用户问：{user_question_for_narrative(messages)}\n\n"
        f"统计结果：\n{fact_block}\n\n"
        "请用人话向班组长汇报。"
    )

    llm = get_llm(model_name)
    narrative_resp = await llm.ainvoke(
        [
            {"role": "system", "content": narrative_system},
            {"role": "user", "content": narrative_user},
        ]
    )
    narrative_text = str(narrative_resp.content).strip()

    response = (
        f"{narrative_text}\n\n"
        f"---\n\n"
        f"### 数据明细\n\n"
        f"| 指标 | 数值 |\n"
        f"| --- | --- |\n"
        f"| 合格率（按重量） | **{pass_rate}%** |\n"
        f"| 合格卷重 | {pass_weight} kg |\n"
        f"| 总卷重 | {total_weight} kg |\n"
        f"| 合格件数 | {pass_count} 件 |\n"
        f"| 总件数 | {total_count} 件 |\n"
        f"| 统计范围 | {spec_text} · {time_text}（{abs_text}）|\n"
        f"| 合格等级 | {grades_text} |\n\n"
        f"_口径：与 lm/web 月度报表一致——按 F_SINGLE_COIL_WEIGHT 加权（缺失则回落 lab_raw_data）。_"
    )

    return {
        "response": response,
        "chart_config": None,
        "intent": "query",
        "entities": {**entities, "query_type": "first_inspection_rate"},
        "context": context,
    }
