import json
import logging
import re
from datetime import date, datetime, timedelta
from typing import Any

from langchain_core.callbacks import adispatch_custom_event
from langchain_core.messages import AIMessage, SystemMessage

from app.core.llm_factory import get_llm
from app.knowledge_graph.kb_lookup import fetch_lightrag_context, lookup_kb, lookup_kb_smart
from app.tools.query_tools import (
    get_first_inspection_config_tool,
    get_formula_definition_tool,
    get_grade_rules_by_spec_tool,
    get_grade_rules_tool,
    get_indicator_definition_tool,
    get_product_specs_tool,
    query_first_inspection_rate_tool,
    query_metric_tool,
)

logger = logging.getLogger("nlq-agent")

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

    logger.info(
        "[query_agent] enter | model=%s | entities=%s | messages=%d",
        model_name or "(default)",
        entities,
        len(messages),
    )

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

    # ───────────────────────────────────────────────────────────────────────
    # 路由优先级（从上到下，先命中先返回）：
    #   0. **知识库 KB 静态条目**（炉号格式、字段区别、术语定义）—— 权威答案，绝不让 LLM 自由发挥
    #   1. 判定公式的「判断依据」（贴标/磁性能/叠片/厚度/一次交检）
    #   2. 指标定义（lab_report_config）
    #   3. 一次交检合格率（实际值）
    #   ...
    # ───────────────────────────────────────────────────────────────────────

    # 步骤 0：KB lookup —— 用户问的是元问题（"X 是什么/怎么组成/什么意思"），
    # 优先用 LightRAG 语义检索 + 静态 KB 兜底，避免 LLM 对纯定义类问题幻觉。
    #
    # 实际返回三种情况：
    #   高置信度命中 → 直接返回 answer + citations
    #   中等置信度 → kb_hit 是 None，但下面会把 LightRAG context 注入 state 供下游 agent 用
    #   完全无命中 → 走原有路由
    kb_hit = await lookup_kb_smart(user_question)
    # 清洗 LightRAG 答案：剥离 keyword JSON 漏出 + lab_xxx/F_xxx 等技术词换业务词
    # 若清洗后还含「无法回答 / 知识库中说明 / highlevelkeywords」等"摆烂"标记，整段丢弃，让下游 handler 接手
    if kb_hit and kb_hit.get("answer"):
        try:
            cleaned_answer = _sanitize_user_facing_text(kb_hit.get("answer", ""))
            if _has_uncooperative_fallback(cleaned_answer) or not cleaned_answer.strip():
                logger.info("[query_agent] LightRAG answer rejected by sanitizer; fall through to other handlers")
                kb_hit = None  # 触发下面的"未命中"路径
            else:
                kb_hit["answer"] = cleaned_answer
        except Exception:
            logger.exception("[query_agent] sanitizer failed; fall through")
            kb_hit = None

    if kb_hit and kb_hit.get("answer"):
        is_lightrag = kb_hit.get("lightrag", False)
        title = f"从{'图谱语义检索' if is_lightrag else '业务知识库'}取到答案"
        await adispatch_custom_event(
            "reasoning_step",
            {
                "id": "step-kb",
                "kind": "knowledge_lookup",
                "title": title,
                "summary": f"置信度 {kb_hit.get('confidence', 0):.2f}",
                "status": "success",
                "citations": kb_hit.get("citations", []),
            },
        )
        # 答案保持纯净——citations 通过 SSE response_metadata 单独下发，
        # 前端渲染为「📎 来源 N」可折叠按钮（见 ChatAssistant.vue / chat.vue）。
        # 这样答案区域不再有冗余的 markdown 引用尾行，UI 更整洁。
        answer_md = kb_hit["answer"]
        logger.info("[query_agent] route → knowledge_base (lightrag=%s, conf=%.2f)",
                    is_lightrag, kb_hit.get("confidence", 0))
        return {
            "response": answer_md,
            "chart_config": None,
            "intent": "query",
            "entities": {
                **entities,
                "query_type": "knowledge_base",
                "kb_source": "lightrag" if is_lightrag else "static",
            },
            "context": context or {},
            # ★ citations 顶层透传，response_formatter → SSE 直接取
            "citations": kb_hit.get("citations") or [],
            "kb_confidence": kb_hit.get("confidence", 0.0),
        }

    # 注：query_agent 与 chat2sql_agent 是 LangGraph 中互斥的两个节点（intent 路由后只会到一边），
    # 所以这里不需要给 state 加 lightrag_context；chat2sql_agent_node 内部会自己 fetch。

    # ────────────────────────────────────────────────────────────────────────
    # Grade-specific condition routing：「120 贴标 A 等级条件」
    # 需要 3 个要素（产品规格 + 判定项目 + 等级名），缺任何一个都反问澄清。
    # 同时支持多轮承接：上一轮反问后用户回复"120"/"贴标"/"A"等单值补全。
    # ────────────────────────────────────────────────────────────────────────
    prev_pending_gc: dict = (context.get("pending_grade_condition") or {}) if isinstance(context, dict) else {}
    awaiting_gc = bool(context.get("awaiting_grade_condition")) if isinstance(context, dict) else False

    def _has_grade_letter_phrase(q: str) -> bool:
        """检测 A 级 / A级 / A 等级 / A级别 等"""
        return bool(re.search(r"[A-Da-d]\s*(?:级别|等级|级)", q))

    def _has_condition_word(q: str) -> bool:
        return any(k in q for k in ("条件", "规则", "范围", "标准", "阈值", "判定", "怎么", "如何", "依据"))

    # 多轮承接判断：上一轮已锁定完整三要素 + 本轮提到指标 / 完整规则
    has_grade_ctx_ready = (
        isinstance(context, dict)
        and context.get("judgment_type")
        and context.get("grade")
        and context.get("spec_code")
    )
    is_followup_grade = (
        has_grade_ctx_ready
        and (
            _extract_metric_keyword(user_question) is not None
            or any(w in user_question for w in ("完整", "全部", "所有条件", "整个", "全套"))
        )
    )
    is_grade_topic = (
        bool(entities.get("grade"))
        or (_has_grade_letter_phrase(user_question) and _has_condition_word(user_question))
        or awaiting_gc
        or is_followup_grade
    )
    if is_grade_topic:
        bare = user_question.strip()
        # 提取/继承三要素（基础值来自 intent_classifier 或上一轮 pending）
        grade_now = entities.get("grade") or prev_pending_gc.get("grade_name")
        spec_now = entities.get("spec_code") or prev_pending_gc.get("spec_code") or context.get("spec_code")
        # 解析顺序（关键）：
        # 1. intent_classifier 提取的 judgment_type（slow path 会有）
        # 2. 当前文本显式提到判定项目名（"贴标"/"磁性能判定"等专属名）— 严格匹配
        # 3. 上一轮 pending 槽位（反问澄清流程中的暂存）
        # 4. 上一轮 context 里的 judgment_type（多轮承接：用户上轮聊的是"贴标"，本轮没改话题就沿用）
        # 5. 最后才用宽松检测（"铁损"/"矫顽力"等子指标关键词回退猜测）
        # ★ context 必须先于宽松检测——避免 "贴标 A → A 级铁损范围" 在第二轮被
        #   误识别成 MagneticResult，破坏多轮承接。
        judg_now = (
            entities.get("judgment_type")
            or _detect_judgment_type_from_text(user_question)
            or prev_pending_gc.get("judgment_type")
            or context.get("judgment_type")
            or _detect_judgment_inquiry(user_question)
        )
        # 文本兜底提取（fast-path 绕过 intent_classifier 时 entities 为空，必须自己抽）：
        # - spec_code：在文本中匹配 120/142/170/213
        if not spec_now:
            m_spec = re.search(r"\b(120|142|170|213)\b", user_question)
            if m_spec:
                spec_now = m_spec.group(1)
        # - grade：在文本中匹配 A/B/C/D（要求紧邻"级"以避免误抓）
        if not grade_now:
            m_grade = re.search(r"([A-Da-d])\s*(?:级别|等级|级)", user_question)
            if m_grade:
                grade_now = m_grade.group(1).upper()
        # 多轮承接：用户单独回复了某个补充值（仅一个字符就是补全）
        if awaiting_gc:
            if not spec_now and re.match(r"^(120|142|170|213)$", bare):
                spec_now = bare
            if not judg_now:
                judg_now = _detect_judgment_type_from_text(bare)
            if not grade_now and re.match(r"^[A-Da-d]$", bare):
                grade_now = bare.upper()

        if spec_now and judg_now and grade_now:
            # 抽取指标关键词（"铁损"/"带宽"/"叠片系数"等）—— 命中时只渲染相关条件组
            # 但若用户说"完整条件 / 全部条件"，就不过滤，给完整树。
            metric_filter = None
            if not any(w in user_question for w in ("完整", "全部", "所有条件", "整个", "全套")):
                metric_filter = _extract_metric_keyword(user_question)
            logger.info(
                "[query_agent] route → grade_condition | spec=%s formula=%s grade=%s metric=%s",
                spec_now, judg_now, grade_now, metric_filter,
            )
            return await _handle_single_grade_condition(
                spec_now, judg_now, grade_now, entities, context, model_name=model_name,
                metric_filter=metric_filter,
            )
        # 三要素未齐 → 反问澄清
        logger.info(
            "[query_agent] grade_condition clarification | spec=%s formula=%s grade=%s",
            spec_now, judg_now, grade_now,
        )
        return await _ask_grade_condition_clarification(
            spec_now, judg_now, grade_now, entities, context,
        )

    # Check if this is a first inspection rate query
    # 关键：除了关键词命中，还要尊重 intent_classifier 通过对话承接推断出的
    # query_type=first_inspection_rate（例如用户问"上个月呢"时，意图分类器会
    # 把 query_type 沿用过来，但 user_question 本身不带"合格率"关键词）。
    # 元问题（"X 的判断依据/计算口径/是什么/怎么算/包含哪些等级"）必须先于
    # _detect_first_inspection_rate_query 路由——否则"一次交检合格率的判断依据"
    # 这种问题会被"合格率"关键词抢走，去执行 SQL 取实际值，返回的是"0% 合格率"
    # 而不是"按 F_LABELING='A' 的卷重求和占比"的口径解释。
    #
    # 路由顺序：判定公式（贴标/磁性能/叠片/厚度/一次交检）的"判断依据"问题必须先于
    # _detect_definition_query 命中——因为这些公式的规则在 lab_intermediate_data_judgment_level，
    # 不在 lab_report_config。
    early_judgment_type = _detect_judgment_inquiry(user_question)
    if early_judgment_type:
        logger.info("[query_agent] route → judgment_rules (early) | type=%s", early_judgment_type)
        return await _handle_judgment_rules_query(
            early_judgment_type, entities, messages, context, model_name=model_name
        )

    if _detect_definition_query(user_question):
        logger.info("[query_agent] route → indicator_definition")
        return await _handle_indicator_definition_query(
            user_question, entities, context, model_name=model_name
        )

    if (
        _detect_first_inspection_rate_query(user_question)
        or query_type == "first_inspection_rate"
    ):
        logger.info("[query_agent] route → first_inspection_rate")
        return await _handle_first_inspection_rate_query(
            entities, context, messages=messages, model_name=model_name
        )

    # 等级分布 / 质量分布 / 分类占比 → 出 donut 图（与 /lab/monthly-dashboard 口径一致）
    # 也尊重上一轮的承接：如果上轮跑过 grade_distribution，且本轮是"使用表格展示/换图表/再画一下"
    # 这类纯展示偏好命令，沿用同一个 handler + 同一时间范围。
    is_display_only_followup = _detect_display_preference_followup(user_question)
    prev_query_type = context.get("last_query_type") if isinstance(context, dict) else None
    if (
        _detect_grade_distribution_query(user_question)
        or query_type in ("grade_distribution", "labeling_distribution", "quality_distribution")
        or (is_display_only_followup and prev_query_type == "grade_distribution")
    ):
        logger.info(
            "[query_agent] route → grade_distribution (display_followup=%s, prev=%s)",
            is_display_only_followup, prev_query_type,
        )
        # 承接旧时间范围 / 班次 / 规格 — 用户没在新问题里提就沿用
        if is_display_only_followup and prev_query_type == "grade_distribution":
            last_entities = context.get("last_query_entities") or {}
            entities = {**last_entities, **entities}
        return await _handle_grade_distribution_query(
            user_question, entities, context, messages=messages, model_name=model_name
        )

    # Check if this is a judgment rules inquiry
    # （贴标 / 磁性能 / 叠片系数 / 厚度 / 一次交检 ＋ 规则 / 判断依据 / 判定 / 等级 等关键词）
    # 必须在 _detect_definition_query 之前——因为"贴标的判断依据"里"判断依据"也会被
    # 元问题检测器命中，但 lab_report_config 里没有贴标，应当落到 judgment_rules 路径。
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
        logger.info("[query_agent] route → judgment_rules | type=%s", judgment_type)
        return await _handle_judgment_rules_query(
            judgment_type, entities, messages, context, model_name=model_name
        )

    # Handle product specs inquiry —
    # 也尊重承接：上轮跑过 product_specs 且本轮是"使用表格 / 再列一下"，沿用 handler
    if (
        entities.get("query_type") == "product_specs"
        or (is_display_only_followup and prev_query_type == "product_specs")
    ):
        logger.info(
            "[query_agent] route → product_specs (display_followup=%s, prev=%s)",
            is_display_only_followup, prev_query_type,
        )
        return await _handle_product_specs_query(context, model_name=model_name)

    if not metric:
        # 防御性兜底：意图分类器抽出了一个 query_type 但前面所有分支都没接住——
        # 这种情况绝对不能让 LLM 凭空编数字（之前就出过 "XXXX 公斤、占比 XX.X%" 的事故）。
        # 直接给出确定性的"未支持"回复 + 列出可用查询。
        UNHANDLED_QT = {
            "grade_distribution", "labeling_distribution", "quality_distribution",
            "trend", "comparison", "ranking", "distribution",
            "shift_comparison", "daily_trend", "unqualified_breakdown",
        }
        if query_type in UNHANDLED_QT:
            logger.warning(
                "[query_agent] query_type=%s fell through to meta fallback (no handler) | question=%r",
                query_type, user_question[:80]
            )
            await adispatch_custom_event(
                "reasoning_step",
                {
                    "id": "step-fallback",
                    "kind": "fallback",
                    "title": f"暂不支持「{query_type}」类型的查询",
                    "summary": "未匹配到任何已实现的查询路径",
                    "status": "warning",
                },
            )
            return {
                "response": (
                    f"这个问题我还没接好对应的查询路径（类型：{query_type}）。"
                    f"你可以试着问得更具体一点——比如：\n"
                    f"  · 上个月一次交检合格率是多少？\n"
                    f"  · 本月各等级的卷重对比\n"
                    f"  · 三班这周合格率对比\n"
                    f"  · 上个月不合格原因 Top 5"
                ),
                "chart_config": None,
                "intent": "query",
                "entities": entities,
                "context": context,
                "calculation_explanation": None,
                "grade_judgment": _build_empty_grade_judgment("路径未覆盖。"),
            }

        # 没匹配到指标——可能是闲聊/元问题（"今天几号"、"你是谁"、"帮助"等）。
        # 把今天日期 + 可用能力注入 system prompt，让 LLM 自由回答。
        llm = get_llm(model_name)
        meta_system = (
            "你是【小美】，检测中心智能数据助理。请用日常中文回答，简洁专业。\n"
            f"今天日期是 {_today_str()}，时间相对词请按此基准回答（"
            f"本周={_format_range(_compute_time_range_absolute({'type': 'this_week'}))}，"
            f"本月={_format_range(_compute_time_range_absolute({'type': 'current_month'}))}，"
            f"本年={_format_range(_compute_time_range_absolute({'type': 'this_year'}))}）。\n\n"
            "【系统能力 — 实事求是】\n"
            "系统已经接入了 lab_intermediate_data 等表的实时查询，"
            "可以直接回答：一次交检合格率、A/B/合格率/不合格 占比、指标均值/极值/趋势、"
            "单炉号根因分析、判定规则查询、外观特性查询、产品规格信息。\n"
            "**绝对禁止**说：'我还没有 SQL 执行能力 / 数据查询功能开发中 / 还没接入数据查询' 之类的话——"
            "后台一定真的会去查。\n"
            "当前这一轮你被路由到了对话兜底（说明问题没匹配上具体的指标/合格率/炉号），"
            "应该 **引导用户补充信息**（具体问哪个指标？哪个时间？哪个班次？），"
            "或者用一句话简短回答（日期/打招呼/介绍能力），让用户继续提问。\n\n"
            "【硬性规则 — 禁止英文/驼峰术语】\n"
            "绝不能输出 ThicknessRange、LaminationFactor、PsIronLoss、Hc、StripWidth 这类驼峰英文。\n"
            "必须使用行业中文术语：\n"
            "  - 厚度极差（不是 ThicknessRange）\n"
            "  - 叠片系数（不是 LaminationFactor）\n"
            "  - Ps 铁损 / 铁损（不是 PsIronLoss、不要单写 Ps）\n"
            "  - 矫顽力（不是 Hc；可写「矫顽力（Hc）」但不要单独写 Hc）\n"
            "  - 带宽（不是带钢宽度、不是 StripWidth）\n\n"
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

    # Step 2: Build time range SQL（与业务系统对齐，按生产日期 F_PROD_DATE 过滤）
    time_range_sql = _build_time_range_sql(time_range)
    if not time_range_sql:
        time_range_sql = (
            "F_PROD_DATE >= DATE_SUB(NOW(), INTERVAL 7 DAY)"  # Default to last 7 days
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
    # 与业务系统（lm/api 的 MonthlyQualityReportService + DashboardService）对齐：
    # 统计的时间维度一律使用【生产日期 F_PROD_DATE】，不用 F_DETECTION_DATE
    # （检测日期是抽检入库时间，可能跨天；生产日期才是该卷钢真正"算在哪天"的口径）。
    if tr_type == "recent_days":
        days = time_range.get("days", 7)
        return f"F_PROD_DATE >= DATE_SUB(NOW(), INTERVAL {days} DAY)"

    elif tr_type == "recent_weeks":
        weeks = time_range.get("weeks", 1)
        return f"F_PROD_DATE >= DATE_SUB(NOW(), INTERVAL {weeks} WEEK)"

    elif tr_type == "recent_months":
        months = time_range.get("months", 1)
        return f"F_PROD_DATE >= DATE_SUB(NOW(), INTERVAL {months} MONTH)"

    elif tr_type == "last_month":
        return "F_PROD_DATE >= DATE_FORMAT(DATE_SUB(NOW(), INTERVAL 1 MONTH), '%Y-%m-01') AND F_PROD_DATE < DATE_FORMAT(NOW(), '%Y-%m-01')"

    elif tr_type == "current_month":
        return "F_PROD_DATE >= DATE_FORMAT(NOW(), '%Y-%m-01')"

    elif tr_type == "today":
        return "DATE(F_PROD_DATE) = CURDATE()"

    elif tr_type == "yesterday":
        return "DATE(F_PROD_DATE) = DATE_SUB(CURDATE(), INTERVAL 1 DAY)"

    elif tr_type == "this_week":
        # 周一为本周起点（YEARWEEK mode 1）。
        return "YEARWEEK(F_PROD_DATE, 1) = YEARWEEK(CURDATE(), 1)"

    elif tr_type == "last_week":
        return "YEARWEEK(F_PROD_DATE, 1) = YEARWEEK(DATE_SUB(CURDATE(), INTERVAL 1 WEEK), 1)"

    elif tr_type == "this_year":
        return "YEAR(F_PROD_DATE) = YEAR(CURDATE())"

    elif tr_type == "last_year":
        return "YEAR(F_PROD_DATE) = YEAR(CURDATE()) - 1"

    elif tr_type == "year_month":
        year = time_range.get("year")
        month = time_range.get("month")
        if year and month:
            next_month = month + 1 if month < 12 else 1
            next_year = year if month < 12 else year + 1
            return f"F_PROD_DATE >= '{year}-{month:02d}-01' AND F_PROD_DATE < '{next_year}-{next_month:02d}-01'"

    elif tr_type == "month":
        month = time_range.get("month")
        if month:
            return f"MONTH(F_PROD_DATE) = {month}"

    elif tr_type == "year":
        year = time_range.get("year")
        if year:
            return f"YEAR(F_PROD_DATE) = {year}"

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
    # 判定规则关键词映射（这些是 lab_intermediate_data_judgment_level 里的 5 个公式）
    judgment_keywords = {
        "Labeling": ["贴标", "标签", "Labeling", "label"],
        "MagneticResult": ["磁性能", "磁性能判定", "magnetic", "铁损", "矫顽力"],
        "LaminationResult": ["叠片系数", "叠片系数判定", "叠片", "lamination"],
        "ThicknessResult": ["厚度判定", "厚度", "thickness result"],
        "FirstInspection": ["一次交检", "first inspection"],
    }

    # 提前过滤：如果问题里出现报表配置的指标级修饰词（合格率 / 占比 / 等比例性词），
    # 用户问的是 lab_report_config 里的指标定义，不是 judgment_level 公式本身的规则。
    # 比如"一次交检合格率的判断依据"应该交给 indicator_definition，不是这里。
    indicator_modifiers = ["合格率", "不合格率", "占比"]
    if any(m in question for m in indicator_modifiers):
        return None

    question_lower = question.lower()

    # 检查是否询问判定规则
    rule_keywords = [
        "判定规则",
        "判定规格",
        "判定依据",
        "判断依据",
        "判断条件",
        "判断标准",
        "判定标准",
        "判定方法",
        "怎么判",
        "怎么判定",
        "如何判定",
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


# ============================================================================
# F_CONDITION JSON → 中文描述 解码器
# ============================================================================
# lab_intermediate_data_judgment_level.F_CONDITION 是一棵规则树：
#   { resultValue, groups: [{ logic, conditions: [{leftExpr, operator, rightValue}],
#                              subGroups: [...] }] }
# 把这棵树转成业务人员能看懂的中文描述。

_LEFT_EXPR_CN_MAP = {
    # 外观特性相关
    "AppearanceFeatureCategoryIds": "外观大类",
    "AppearanceFeatureLevelIds": "外观严重等级",
    # 物理量
    "Width": "带宽",
    "Thickness": "厚度",
    "ThicknessRange": "厚度极差",
    "MaxThickness": "最大厚度",
    "MinThickness": "最小厚度",
    "LaminationFactor": "叠片系数",
    "LaminationFactorRes": "叠片判定值",
    # 磁性能（原始 + 退火后 + 刻痕后 三套口径）
    "PerfHc": "矫顽力 Hc",
    "PerfPsLoss": "Ps 铁损",
    "PerfSsPower": "Ss 比功率",
    "AfterHc": "退火后矫顽力",
    "AfterPsLoss": "退火后铁损",
    "AfterSsPower": "退火后比功率",
    # 刻痕后 (After Notching) —— A 等级条件常用这一组
    "PerfAfterPsLoss": "刻痕后 Ps 铁损",
    "PerfAfterHc": "刻痕后矫顽力",
    "PerfAfterSsPower": "刻痕后 Ss 比功率",
    # 卷材形状
    "NewBandLeft": "新带型左侧",
    "NewBandRight": "新带型右侧",
    "BreakHeadCount": "断头数",
    "AvgThickness": "平均厚度",
    "Density": "密度",
    "DensityRes": "密度判定值",
    # 外观（"所有特性" 表示外观所有缺陷标签都为空）
    "AppearanceAll": "所有外观特性",
    "AppearanceFeatures": "外观特性",
    "FeatureSuffix": "外观特性",
    "BreakCount": "断头数",
    "BreakHead": "断头",
    # 判定结果列
    "FirstInspection": "一次交检",
    "Labeling": "等级标签",
    "MagneticResult": "磁性能判定",
    "ThickRes": "厚度判定",
    # 维度
    "Shift": "班次",
    "LineNo": "产线",
    "FurnaceNo": "炉号",
}

_OPERATOR_CN_MAP = {
    "EQ": "等于",
    "EQUALS": "等于",
    "NEQ": "不等于",
    "NE": "不等于",
    "NOT_EQ": "不等于",
    "NOT_EQUALS": "不等于",
    "GT": "大于",
    "GTE": "大于等于",
    "GE": "大于等于",
    "LT": "小于",
    "LTE": "小于等于",
    "LE": "小于等于",
    "IN": "属于",
    "NOT_IN": "不属于",
    "CONTAINS_ANY": "包含任一",
    "CONTAINS_ALL": "全部包含",
    "NOT_CONTAINS": "不包含",
    "BETWEEN": "在区间",
    "NOT_BETWEEN": "不在区间",
    "IS_NULL": "为空",
    "IS_NOT_NULL": "非空",
    "NOT_NULL": "非空",
    "NULL": "为空",
    "LIKE": "匹配",
    "NOT_LIKE": "不匹配",
    "EMPTY": "为空",
    "NOT_EMPTY": "非空",
    # 数据库里有时直接存符号形式（>, <=, !=, ==），兜底映射
    ">": "大于",
    ">=": "大于等于",
    "<": "小于",
    "<=": "小于等于",
    "=": "等于",
    "==": "等于",
    "!=": "不等于",
    "<>": "不等于",
}


def _humanize_left_expr(expr: str) -> str:
    """leftExpr → 中文业务字段名。"""
    if not expr:
        return "?"
    return _LEFT_EXPR_CN_MAP.get(expr, expr)


def _humanize_right_value(
    raw_value: Any,
    left_expr: str,
    category_map: dict[str, str],
    level_map: dict[str, str],
) -> str:
    """右侧值 → 中文值列表。如果是 ID 列表就翻译成中文名。"""
    # 尝试 JSON 解析（rightValue 可能是 '["id1","id2"]' 字符串）
    parsed: Any = raw_value
    if isinstance(raw_value, str):
        try:
            parsed = json.loads(raw_value)
        except Exception:
            parsed = raw_value

    def _id_to_name(items: list) -> list[str]:
        if left_expr == "AppearanceFeatureCategoryIds":
            return [category_map.get(str(x), str(x)) for x in items]
        if left_expr == "AppearanceFeatureLevelIds":
            return [level_map.get(str(x), str(x)) for x in items]
        return [str(x) for x in items]

    if isinstance(parsed, list):
        return "、".join(_id_to_name(parsed)) or "（空集）"
    if isinstance(parsed, (int, float)):
        return str(parsed)
    return str(parsed) if parsed not in (None, "") else "（空）"


def _decode_single_condition(
    cond: dict[str, Any],
    category_map: dict[str, str],
    level_map: dict[str, str],
) -> str:
    """单条 condition → 一行中文描述。例：『外观大类 包含任一 端面不良』。"""
    left = _humanize_left_expr(cond.get("leftExpr") or "")
    op = _OPERATOR_CN_MAP.get(str(cond.get("operator") or "").upper(), cond.get("operator") or "?")
    right = _humanize_right_value(
        cond.get("rightValue"), cond.get("leftExpr") or "", category_map, level_map
    )
    if str(cond.get("operator") or "").upper() in ("IS_NULL", "IS_NOT_NULL"):
        return f"{left}{op}"
    return f"{left} {op}【{right}】"


def _decode_group(
    group: dict[str, Any],
    category_map: dict[str, str],
    level_map: dict[str, str],
    depth: int = 0,
) -> str:
    """一个 group → 中文描述（递归处理 subGroups）。"""
    logic = str(group.get("logic") or "AND").upper()
    joiner = " 且 " if logic == "AND" else " 或 "
    parts: list[str] = []
    for cond in group.get("conditions") or []:
        parts.append(_decode_single_condition(cond, category_map, level_map))
    for sub in group.get("subGroups") or []:
        sub_text = _decode_group(sub, category_map, level_map, depth + 1)
        if sub_text:
            parts.append(f"({sub_text})")
    return joiner.join(parts) if parts else ""


def _decode_condition_to_chinese(
    cond_raw: str | None,
    category_map: dict[str, str],
    level_map: dict[str, str],
) -> tuple[str, str]:
    """F_CONDITION JSON → (resultValue, 业务描述)。

    Returns:
        (resultValue, 中文条件描述) — 解析失败时返回 ('', '（条件解析失败）')
    """
    if not cond_raw:
        return "", "（未配置条件，命中即生效）"
    try:
        data = json.loads(cond_raw)
    except Exception:
        return "", "（条件 JSON 解析失败）"

    result_value = str(data.get("resultValue") or "")
    groups = data.get("groups") or []
    if not groups:
        return result_value, "（未配置任何条件）"

    # 多个顶层 group 用 OR 连接（这是该平台 group 编辑器的默认语义）
    group_texts = []
    for g in groups:
        gt = _decode_group(g, category_map, level_map)
        if gt:
            group_texts.append(gt)
    if not group_texts:
        return result_value, "（未配置任何条件）"
    if len(group_texts) == 1:
        return result_value, group_texts[0]
    return result_value, " 或 ".join(f"({t})" for t in group_texts)


async def _load_formula_var_map() -> dict[str, str]:
    """加载 ``$VAR_<F_Id>`` 类动态变量 → 业务名 映射。

    conditionJson 里出现 ``$VAR_<F_Id>`` 这种引用时，F_Id 可能落在多张表里：
      - lab_intermediate_data_formula（系统级公式："刻痕后铁损" 等）
      - lab_product_spec_attribute（产品规格扩展属性："新带型左侧/右侧" 等）

    统一拉到一个 dict，渲染前替换。
    """
    from app.tools.query_tools import execute_safe_sql as _exec
    combined: dict[str, str] = {}
    # 1. 公式表（业务化命名优先用 F_FORMULA_NAME）
    # 注意：conditionJson 的 leftExpr 形如 ``$VAR_<id>``，这个 id 通常落在
    # F_COLUMN_NAME（去掉 ``$VAR_`` 前缀后）；少数情况 F_Id 也会被引用。
    # 两种都索引，确保都能命中。
    try:
        rows = await _exec(
            "SELECT F_Id, F_FORMULA_NAME, F_COLUMN_NAME FROM lab_intermediate_data_formula "
            "WHERE F_DeleteMark = 0 OR F_DeleteMark IS NULL",
            {},
        )
        for r in rows:
            name = (r.get("F_FORMULA_NAME") or "").strip()
            if not name:
                continue
            combined[str(r["F_Id"])] = name
            col = (r.get("F_COLUMN_NAME") or "").strip()
            if col.startswith("$VAR_"):
                combined[col[len("$VAR_"):]] = name
    except Exception:
        pass
    # 2. 产品规格扩展属性（同一个 F_Id 不可能两表都有，但即使有以公式表为准也合理）
    try:
        rows = await _exec(
            "SELECT F_Id, F_NAME FROM lab_product_spec_attribute "
            "WHERE F_DeleteMark = 0 OR F_DeleteMark IS NULL",
            {},
        )
        for r in rows:
            fid = str(r["F_Id"])
            name = (r.get("F_NAME") or "").strip()
            if name and fid not in combined:
                combined[fid] = name
    except Exception:
        pass
    return combined


_METRIC_KEYWORDS = (
    "平均带厚", "平均厚度", "Ps铁损", "刻痕后铁损", "刻痕后", "PsLoss",
    "矫顽力", "Hc",
    "叠片系数", "叠片",
    "带厚极差", "带厚", "厚度",
    "带宽", "宽度",
    "新带型",
    "断头数", "断头",
    "外观特性", "外观特征", "外观",
    "密度",
    "Ss功率", "比功率",
    "铁损",  # 兜底放最后（避免抢走 Ps铁损 / 刻痕后铁损）
)


def _extract_metric_keyword(question: str) -> str | None:
    """从用户问题里挑一个指标关键词，用于"只渲染相关条件组"。

    例："A 级别的铁损范围" → "铁损" → 渲染时只保留 name 含"铁损"的条件组。
    没命中就返回 None，调用方应展示完整规则。
    """
    if not question:
        return None
    for kw in _METRIC_KEYWORDS:  # 长词优先
        if kw in question:
            return kw
    return None


def _group_matches_metric(group_name: str, metric: str) -> bool:
    """模糊匹配条件组名 vs 用户问的指标——双向 substring 都算。"""
    if not group_name or not metric:
        return False
    g, m = group_name.strip(), metric.strip()
    return m in g or g in m


def render_condition_natural_text(
    cond_raw: str | None,
    category_map: dict[str, str],
    level_map: dict[str, str],
    var_map: dict[str, str] | None = None,
) -> str:
    """F_CONDITION JSON → 扁平自然语言中文文本（喂给 LightRAG 做语义检索用）。

    与 `_render_condition_tree_md` 不同之处：
    - 这个版本输出**扁平的密集自然语言**，不是缩进的 markdown 树
    - 用 `(... 或 ...) 且 ...` 写法，让 LightRAG 的实体抽取器更容易识别"指标 > 阈值"模式
    - 每个条件组用「· **指标名**:」做小节，便于按指标分块召回

    示例：
        · **铁损**: (刻痕后Ps铁损 大于 0 且 刻痕后Ps铁损 小于等于 0.14) 或 (Ps铁损 大于 0 且 ...)
        · **带宽**: 带宽 大于等于 119.5 且 带宽 小于等于 120.5
    """
    if not cond_raw:
        return "（未配置条件）"
    try:
        data = json.loads(cond_raw)
    except Exception:
        return "（条件 JSON 解析失败）"
    groups = data.get("groups") or []
    if not groups:
        return "（未配置任何条件）"

    # 复用 _render_condition_tree_md 里的 $VAR 解析
    def _resolve(node: Any, gn: str = "") -> None:
        if isinstance(node, dict):
            new_gn = node.get("name") or gn
            le = node.get("leftExpr")
            if isinstance(le, str) and le.startswith("$VAR_"):
                fid = le[len("$VAR_"):]
                name = (var_map or {}).get(fid, "")
                node["leftExpr"] = name or (gn or "扩展属性")
            for v in node.values():
                _resolve(v, new_gn)
        elif isinstance(node, list):
            for it in node:
                _resolve(it, gn)
    _resolve(data)

    def _fmt_subgroup(sub: dict) -> str:
        sub_logic = str(sub.get("logic") or "AND").upper()
        joiner = " 且 " if sub_logic == "AND" else " 或 "
        parts = [
            _decode_single_condition(c, category_map, level_map)
            for c in (sub.get("conditions") or [])
        ]
        return joiner.join(p for p in parts if p)

    lines: list[str] = []
    for g in groups:
        gn = (g.get("name") or "").strip() or "条件"
        g_logic = str(g.get("logic") or "AND").upper()
        connector = " 或 " if g_logic == "OR" else " 且 "

        subs = g.get("subGroups") or []
        flat_conds = g.get("conditions") or []
        chunks: list[str] = []
        for s in subs:
            txt = _fmt_subgroup(s)
            if txt:
                chunks.append(f"({txt})" if len(subs) > 1 else txt)
        if flat_conds:
            inner = " 且 " if g_logic == "AND" else " 或 "
            flat_txt = inner.join(
                _decode_single_condition(c, category_map, level_map) for c in flat_conds
            )
            if flat_txt:
                chunks.append(flat_txt)
        body = connector.join(chunks) if chunks else "（空）"
        lines.append(f"- **{gn}**：{body}")
    return "\n".join(lines)


def _render_condition_tree_md(
    cond_raw: str | None,
    category_map: dict[str, str],
    level_map: dict[str, str],
    var_map: dict[str, str] | None = None,
    metric_filter: str | None = None,
) -> tuple[str, int]:
    """F_CONDITION JSON → 嵌套缩进 markdown 条件树。

    返回 ``(markdown, kept_group_count)``：
    - 无 metric_filter：渲染全部条件组，kept = 总组数
    - 有 metric_filter：只渲染 name 含该关键词的条件组（kept 可能为 0；调用方据此决定是否退回完整渲染）
    """
    if not cond_raw:
        return "_未配置条件（命中即生效）_", 0
    try:
        data = json.loads(cond_raw)
    except Exception:
        return "_条件 JSON 解析失败_", 0

    groups = data.get("groups") or []
    if not groups:
        return "_未配置任何条件_", 0

    # 把 $VAR_<F_Id> 类动态变量 leftExpr 替换成业务名。
    # var_map 取自 lab_intermediate_data_formula + lab_product_spec_attribute。
    # 若仍未命中（罕见，可能是 lab_product_spec_version 的引用），用所在条件组的 name 兜底。
    def _resolve_vars(node: Any, group_name: str = "") -> None:
        if isinstance(node, dict):
            # 进入新 group 时把它的 name 作为上下文（用于子节点 fallback 命名）
            new_group_name = node.get("name") or group_name
            le = node.get("leftExpr")
            if isinstance(le, str) and le.startswith("$VAR_"):
                fid = le[len("$VAR_"):]
                name = (var_map or {}).get(fid, "")
                if name:
                    node["leftExpr"] = name
                else:
                    # 兜底：用所在条件组的名字 + 序号占位
                    node["leftExpr"] = group_name or "扩展属性"
            for v in node.values():
                _resolve_vars(v, new_group_name)
        elif isinstance(node, list):
            for it in node:
                _resolve_vars(it, group_name)
    _resolve_vars(data)
    groups = data.get("groups") or []

    # 指标过滤：保留 name 命中 metric_filter 的条件组（保持原 label A/B/C... 的字母顺序）
    if metric_filter:
        filtered = [
            g for g in groups
            if _group_matches_metric((g.get("name") or "").strip(), metric_filter)
        ]
        if filtered:
            groups = filtered
        # 若过滤后为空，groups 保持全集，让调用方知道没命中（kept_group_count 仍按原 groups 数）

    kept_count = len(groups)

    lines: list[str] = []
    for gi, group in enumerate(groups):
        # 顶层 group 之间默认 AND（业务侧实际语义：所有条件组都得满足）
        # 过滤后用「1/2/3...」序号；未过滤用 A/B/C
        group_label = chr(ord("A") + gi) if not metric_filter else str(gi + 1)
        group_name = (group.get("name") or "").strip()
        prefix = "且 " if gi > 0 else ""
        title_extra = f" · {group_name}" if group_name else ""
        if gi > 0:
            lines.append("")  # 空行隔开顶层组
        lines.append(f"**{prefix}条件组 {group_label}{title_extra}**")

        conds = group.get("conditions") or []
        sub_groups = group.get("subGroups") or []
        group_logic = str(group.get("logic") or "AND").upper()

        # 子组（带 OR/AND 语义连接）
        if sub_groups:
            sub_word = "且" if group_logic == "AND" else "或"
            for si, sub in enumerate(sub_groups):
                sub_label = chr(ord("A") + si)
                head_prefix = "" if si == 0 else f"{sub_word} "
                lines.append(f"- {head_prefix}子组 {sub_label}：")
                sub_conds = sub.get("conditions") or []
                sub_logic = str(sub.get("logic") or "AND").upper()
                inner = "且" if sub_logic == "AND" else "或"
                for ci, c in enumerate(sub_conds):
                    line = _decode_single_condition(c, category_map, level_map)
                    if ci == 0:
                        lines.append(f"  - {line}")
                    else:
                        lines.append(f"  - {inner} {line}")

        # 直接挂在组上的扁平条件
        if conds:
            inner = "且" if group_logic == "AND" else "或"
            for ci, c in enumerate(conds):
                line = _decode_single_condition(c, category_map, level_map)
                if ci == 0:
                    lines.append(f"- {line}")
                else:
                    lines.append(f"- {inner} {line}")

    return "\n".join(lines), kept_count


async def _load_feature_id_maps() -> tuple[dict[str, str], dict[str, str]]:
    """加载外观特性大类 + 等级 的 id → 中文名 映射。"""
    from app.tools.query_tools import execute_safe_sql as _exec
    cat_rows = await _exec(
        "SELECT F_Id, F_NAME FROM lab_appearance_feature_category "
        "WHERE F_DeleteMark IS NULL OR F_DeleteMark = 0",
        {},
    )
    lvl_rows = await _exec(
        "SELECT F_Id, F_NAME FROM lab_appearance_feature_level "
        "WHERE F_DeleteMark IS NULL OR F_DeleteMark = 0",
        {},
    )
    return (
        {str(r["F_Id"]): r["F_NAME"] for r in cat_rows},
        {str(r["F_Id"]): r["F_NAME"] for r in lvl_rows},
    )


_JUDGMENT_FORMULA_CN = {
    "Labeling": "贴标",
    "MagneticResult": "磁性能判定",
    "LaminationResult": "叠片系数判定",
    "ThicknessResult": "厚度判定",
    "FirstInspection": "一次交检",
}


def _detect_judgment_type_from_text(text: str) -> str | None:
    """从用户文本里识别**判定项目本身**（贴标 / 磁性能判定 / 叠片系数 / 厚度判定 / 一次交检）。

    严格：只匹配判定项目的明确名称，**不匹配子指标关键词**（"铁损""矫顽力""叠片系数"等）。
    判定项目和指标是不同层级——"贴标 A 级"里的"铁损"只是 A 级条件之一，不能反推说判定项目就是 MagneticResult。

    用于"等级条件"反问后的二次接续（用户只回复"贴标"也得能继续）。
    """
    if not text:
        return None
    table = {
        "Labeling": ["贴标", "标签", "labeling"],
        "MagneticResult": ["磁性能判定", "磁性能"],
        "LaminationResult": ["叠片系数判定", "叠片判定"],
        "ThicknessResult": ["厚度判定"],
        "FirstInspection": ["一次交检"],
    }
    lower = text.lower()
    for fid, kws in table.items():
        if any(k in text or k in lower for k in kws):
            return fid
    return None


async def _ask_grade_condition_clarification(
    spec_code: str | None,
    judgment_type: str | None,
    grade_name: str | None,
    entities: dict,
    context: dict | None = None,
) -> dict[str, Any]:
    """缺少要素（产品规格 / 判定项目 / 等级名称）时反问用户，并把已收集到的信息存进 context 做多轮承接。"""
    context = context or {}
    judgment_zh = _JUDGMENT_FORMULA_CN.get(judgment_type or "", "") if judgment_type else ""

    received: list[str] = []
    if spec_code:
        received.append(f"**产品规格** = {spec_code}")
    if judgment_zh:
        received.append(f"**判定项目** = {judgment_zh}")
    if grade_name:
        received.append(f"**等级** = {grade_name}")

    missing: list[str] = []
    if not spec_code:
        missing.append("- **产品规格**？可选：120 / 142 / 170 / 213")
    if not judgment_type:
        missing.append("- **判定项目**？可选：贴标 / 磁性能判定 / 叠片系数判定 / 厚度判定 / 一次交检")
    if not grade_name:
        missing.append("- **等级名称**？比如 A / B / C")

    intro = f"已收到：{', '.join(received)}\n\n" if received else ""
    response = (
        f"{intro}等级判定条件需要 3 个要素才能精确定位，还差：\n\n"
        + "\n".join(missing)
        + "\n\n直接告诉我即可，例如：「**120 贴标 A 等级**」。"
    )

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-grade-cond-clarify",
            "kind": "answer",
            "title": "等级判定条件 — 需要您再补充信息",
            "summary": f"已收集 {len(received)}/3，还差 {len(missing)} 项",
            "status": "success",
        },
    )

    return {
        "response": response,
        "chart_config": None,
        "intent": "query",
        "entities": {
            **entities,
            "query_type": "judgment_grade_condition",
            "pending_clarification": True,
        },
        "context": {
            **context,
            "awaiting_grade_condition": True,
            "pending_grade_condition": {
                "spec_code": spec_code,
                "judgment_type": judgment_type,
                "grade_name": grade_name,
            },
        },
    }


async def _handle_single_grade_condition(
    spec_code: str,
    judgment_type: str,
    grade_name: str,
    entities: dict,
    context: dict | None = None,
    model_name: str | None = None,
    metric_filter: str | None = None,
) -> dict[str, Any]:
    """精确定位一条判定规则，把 conditionJson 渲染成嵌套缩进的中文条件树。

    Args:
        spec_code: 产品规格代码（120 / 142 / 170 / 213）
        judgment_type: Formula ID（Labeling / MagneticResult / LaminationResult / ThicknessResult / FirstInspection）
        grade_name: 等级名称（A / B / C / …）
    """
    from app.tools.query_tools import execute_safe_sql as _exec

    context = context or {}
    judgment_zh = _JUDGMENT_FORMULA_CN.get(judgment_type, judgment_type)

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-grade-cond-sql",
            "kind": "sql",
            "title": f"正在查找「{spec_code} 规格 · {judgment_zh} · {grade_name} 等级」的条件",
            "summary": "从判定规则配置加载完整条件树",
            "status": "running",
        },
    )

    try:
        category_map, level_map = await _load_feature_id_maps()
    except Exception as exc:  # noqa: BLE001
        logger.warning("[grade_cond] load feature maps failed: %s", exc)
        category_map, level_map = {}, {}
    try:
        var_map = await _load_formula_var_map()
    except Exception as exc:  # noqa: BLE001
        logger.warning("[grade_cond] load formula var map failed: %s", exc)
        var_map = {}

    sql = """
        SELECT
            j.F_NAME AS name,
            j.F_QUALITY_STATUS AS quality_status,
            j.F_PRIORITY AS priority,
            j.F_DESCRIPTION AS description,
            j.F_CONDITION AS condition_json,
            p.F_CODE AS spec_code
        FROM lab_intermediate_data_judgment_level j
        LEFT JOIN lab_product_spec p
          ON j.F_PRODUCT_SPEC_ID COLLATE utf8mb4_unicode_ci = p.F_Id COLLATE utf8mb4_unicode_ci
        WHERE (j.F_DeleteMark IS NULL OR j.F_DeleteMark = 0)
          AND j.F_FORMULA_ID = :formula_id
          AND p.F_CODE = :spec_code
          AND j.F_NAME = :grade_name
        ORDER BY j.F_PRIORITY DESC
        LIMIT 1
    """
    try:
        rows = await _exec(
            sql,
            {"formula_id": judgment_type, "spec_code": spec_code, "grade_name": grade_name},
        )
    except Exception as exc:  # noqa: BLE001
        logger.exception("[grade_cond] SQL failed")
        await adispatch_custom_event(
            "reasoning_step",
            {
                "id": "step-grade-cond-sql",
                "kind": "fallback",
                "title": "查询判定规则数据库出错",
                "summary": str(exc)[:120],
                "status": "error",
            },
        )
        return {
            "response": f"查询判定规则时数据库出错：{str(exc)[:160]}",
            "chart_config": None,
            "intent": "query",
            "entities": entities,
            "context": context,
        }

    if not rows:
        await adispatch_custom_event(
            "reasoning_step",
            {
                "id": "step-grade-cond-sql",
                "kind": "fallback",
                "title": f"没有找到「{spec_code} · {judgment_zh} · {grade_name}」规则",
                "summary": "可能是产品规格或等级名输错，或这个等级在该判定项里没有配置",
                "status": "warning",
            },
        )
        return {
            "response": (
                f"系统里没有找到「{spec_code} 规格 · {judgment_zh} · {grade_name} 等级」的判定条件。\n\n"
                "可能原因：\n"
                f"- 产品规格 **{spec_code}** 不存在（请检查是否是 120 / 142 / 170 / 213）\n"
                f"- 该判定项下没配置 **{grade_name}** 这个等级名\n\n"
                "你可以换成「列出 " + spec_code + " 规格 " + judgment_zh + " 的所有等级」让我把存在的等级都列给你看。"
            ),
            "chart_config": None,
            "intent": "query",
            "entities": entities,
            "context": context,
        }

    rule = rows[0]
    priority = rule.get("priority") or 0
    tree_md, kept_count = _render_condition_tree_md(
        rule.get("condition_json"), category_map, level_map, var_map,
        metric_filter=metric_filter,
    )
    # 过滤后 0 条 → 退回完整渲染并告诉用户
    filter_fallback_note = ""
    if metric_filter and kept_count == 0:
        tree_md, _ = _render_condition_tree_md(
            rule.get("condition_json"), category_map, level_map, var_map,
        )
        filter_fallback_note = (
            f"\n\n> 没找到与「{metric_filter}」直接相关的条件组，下面给出完整规则供参考。\n"
        )

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-grade-cond-sql",
            "kind": "sql",
            "title": f"已读取条件树（优先级 {priority}）",
            "summary": (
                f"判定项：{judgment_zh} · 规格 {spec_code} · 等级 {grade_name}"
                + (f" · 仅显示「{metric_filter}」相关 {kept_count} 条" if metric_filter and kept_count > 0 else "")
            ),
            "status": "success",
        },
    )

    # 根据是否过滤，给不同的标题 + 引导语
    if metric_filter and kept_count > 0:
        intro = (
            f"## {spec_code} 规格 · {judgment_zh} · {grade_name} 等级 — 「{metric_filter}」相关条件\n\n"
            f"判为 **{grade_name}** 时，与「{metric_filter}」相关的条件如下：\n\n"
        )
        suffix = (
            f"\n\n---\n_说明：上面只是 A 等级判定条件里跟「{metric_filter}」相关的部分。_"
            f"\n_想看完整规则可以追问「{spec_code} 贴标 {grade_name} 等级完整条件」。_"
            if judgment_type == "Labeling" else
            f"\n\n---\n_说明：上面只是 {grade_name} 等级判定条件里跟「{metric_filter}」相关的部分，"
            f"完整规则可以追问「完整条件」。_"
        )
    else:
        intro = (
            f"## {spec_code} 规格 · {judgment_zh} · {grade_name} 等级 — 判定条件\n\n"
            f"满足下列**所有条件组**时，会被判为 **{grade_name}**：\n\n"
        )
        suffix = ""

    description_md = ""
    desc = (rule.get("description") or "").strip()
    if desc:
        description_md = f"\n\n_备注：{desc}_"

    response_md = intro + tree_md + filter_fallback_note + suffix + description_md

    return {
        "response": response_md,
        "chart_config": None,
        "intent": "query",
        "entities": {
            **entities,
            "spec_code": spec_code,
            "judgment_type": judgment_type,
            "grade": grade_name,
            "query_type": "judgment_grade_condition",
        },
        "context": {
            **context,
            "judgment_type": judgment_type,
            "spec_code": spec_code,
            "grade": grade_name,
            "awaiting_grade_condition": False,
        },
    }


async def _handle_judgment_rules_query(
    judgment_type: str,
    entities: dict,
    messages: list,
    context: dict | None = None,
    model_name: str | None = None,
) -> dict[str, Any]:
    """Handle judgment rules query by product spec.

    新版本：
    - 默认不再要求用户先选规格，直接把所有规格的规则按 优先级 倒序列出来
    - 解码 F_CONDITION JSON 成业务化中文（"外观大类 包含 端面不良 且 严重等级 包含 严重"）
    - 把外观特性的 ID 解析成中文名
    - LLM 流式生成一段简介
    - 用 markdown 表格按规格分块展示：优先级 / 等级名 / 命中条件 / 判定结果
    - 用户可以追问"看看 120 规格" → 通过 entities.spec_code 过滤

    Args:
        judgment_type: Formula ID (Labeling / MagneticResult / LaminationResult / ThicknessResult / FirstInspection)
        entities: Extracted entities (可包含 spec_code 用于过滤)
        messages: Message history
        context: Previous conversation context for multi-turn support
        model_name: LLM model override (None = .env default)
    """
    from app.tools.query_tools import execute_safe_sql as _exec
    context = context or {}
    name_mapping = {
        "Labeling": "贴标",
        "MagneticResult": "磁性能判定",
        "LaminationResult": "叠片系数判定",
        "ThicknessResult": "厚度判定",
        "FirstInspection": "一次交检",
    }
    judgment_name = name_mapping.get(judgment_type, judgment_type)
    spec_code = entities.get("spec_code")
    model_name_arg = model_name

    logger.info(
        "[judgment_rules] formula=%s spec=%s",
        judgment_type, spec_code or "(all)"
    )
    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-judge-sql",
            "kind": "sql",
            "title": f"正在查询「{judgment_name}」的判定规则",
            "summary": f"从系统配置加载所有规则" + (f"（{spec_code} 规格）" if spec_code else "（含全部规格）"),
            "status": "running",
        },
    )

    # 1. 加载外观特性的 id → 中文名 映射（贴标规则会用到）
    try:
        category_map, level_map = await _load_feature_id_maps()
    except Exception as exc:  # noqa: BLE001
        logger.warning("[judgment_rules] load feature maps failed: %s", exc)
        category_map, level_map = {}, {}

    # 2. 加载所有规则 JOIN 规格代码
    spec_filter_sql = ""
    params: dict[str, Any] = {"formula_id": judgment_type}
    if spec_code and spec_code.lower() not in ("所有", "all"):
        spec_filter_sql = " AND p.F_CODE = :spec_code"
        params["spec_code"] = spec_code

    sql = f"""
        SELECT
            j.F_Id AS id,
            j.F_NAME AS name,
            j.F_PRIORITY AS priority,
            j.F_QUALITY_STATUS AS quality_status,
            j.F_IS_DEFAULT AS is_default,
            j.F_DESCRIPTION AS description,
            j.F_CONDITION AS condition_json,
            COALESCE(p.F_CODE, '（未关联规格）') AS spec_code
        FROM lab_intermediate_data_judgment_level j
        LEFT JOIN lab_product_spec p
          ON j.F_PRODUCT_SPEC_ID COLLATE utf8mb4_unicode_ci = p.F_Id COLLATE utf8mb4_unicode_ci
        WHERE (j.F_DeleteMark IS NULL OR j.F_DeleteMark = 0)
          AND j.F_FORMULA_ID = :formula_id
          {spec_filter_sql}
        ORDER BY p.F_CODE, j.F_PRIORITY DESC, j.F_NAME
    """
    try:
        rules = await _exec(sql, params)
    except Exception as exc:  # noqa: BLE001
        logger.exception("[judgment_rules] SQL failed")
        await adispatch_custom_event(
            "reasoning_step",
            {
                "id": "step-judge-sql",
                "kind": "fallback",
                "title": "查询判定规则失败",
                "summary": str(exc)[:160],
                "status": "error",
            },
        )
        return {
            "response": f"查询「{judgment_name}」判定规则时数据库报错：{str(exc)[:160]}",
            "chart_config": None,
            "intent": "query",
            "entities": {"judgment_type": judgment_type, "query_type": "judgment_rules"},
            "context": {**context, "judgment_type": judgment_type, "awaiting_spec_selection": False},
        }

    if not rules:
        await adispatch_custom_event(
            "reasoning_step",
            {
                "id": "step-judge-sql",
                "kind": "fallback",
                "title": f"没有找到「{judgment_name}」的判定规则",
                "summary": (f"{spec_code} 规格" if spec_code else "所有规格") + "下都没有配置",
                "status": "warning",
            },
        )
        return {
            "response": f"系统里没有配置「{judgment_name}」的判定规则" + (f"（{spec_code} 规格）" if spec_code else "") + "。",
            "chart_config": None,
            "intent": "query",
            "entities": {"judgment_type": judgment_type, "query_type": "judgment_rules"},
            "context": {**context, "judgment_type": judgment_type, "awaiting_spec_selection": False},
        }

    # 3. 按 spec_code 分组 + 解码每条规则的条件
    from collections import defaultdict
    grouped: dict[str, list[dict[str, Any]]] = defaultdict(list)
    for r in rules:
        result_val, cond_cn = _decode_condition_to_chinese(
            r.get("condition_json"), category_map, level_map
        )
        qs = str(r.get("quality_status") or "")
        if qs in ("1", "True", "true"):
            status_text = "合格"
        elif qs in ("0", "False", "false"):
            status_text = "不合格"
        else:
            status_text = "其他"
        grouped[str(r.get("spec_code") or "（未关联规格）")].append(
            {
                "name": r.get("name") or "",
                "priority": r.get("priority") or 0,
                "status": status_text,
                "is_default": bool(r.get("is_default")),
                "result_value": result_val,
                "condition_cn": cond_cn,
                "description": r.get("description") or "",
            }
        )

    spec_codes_sorted = sorted(grouped.keys(), key=lambda s: (s == "（未关联规格）", s))
    total_rule_count = sum(len(v) for v in grouped.values())

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-judge-sql",
            "kind": "sql",
            "title": f"已读取 {total_rule_count} 条判定规则",
            "summary": "适用规格：" + "、".join(spec_codes_sorted),
            "status": "success",
        },
    )

    # 4. LLM 流式生成一段业务化简介
    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-judge-narrate",
            "kind": "answer",
            "title": "正在整理人话解释",
            "summary": f"把「{judgment_name}」的判定逻辑写成业务描述",
            "status": "running",
        },
    )

    # 给 LLM 的事实块：只用业务名词
    fact_lines = [
        f"判定项：{judgment_name}",
        f"涉及产品规格：{', '.join(spec_codes_sorted)}",
        f"规则总数：{total_rule_count}",
        f"工作原理：每条记录按 优先级 从高到低依次比对规则，命中后赋值给「等级标签 / 一次交检结果」并停止；都没命中走兜底（is_default 标记的那条）。",
    ]
    # 取每个规格里优先级最高的 3 条规则给 LLM 做样本
    samples = []
    for sc in spec_codes_sorted[:2]:
        rs = grouped[sc][:3]
        for r in rs:
            samples.append(f"  · {sc} 规格 · 优先级 {r['priority']} ·「{r['name']}」({r['status']}) ← {r['condition_cn']}")
    fact_block = "\n".join(fact_lines) + "\n样本规则：\n" + "\n".join(samples)

    narrative_system = (
        "你是【小美】，检测中心智能数据助理。下面我会给你一项质量判定规则的业务事实，"
        "请用 2-3 句中文向班组长汇报这项判定是怎么工作的。\n\n"
        "【硬性规则 — 严禁】\n"
        "1. 严禁出现表名/字段名/F_xxx/SQL/JOIN。\n"
        "2. 严禁编造数据。\n"
        "3. 不要使用 markdown 表格 / 项目符号——下面我会自己拼好附在你回复后面。\n\n"
        "【输出要求】\n"
        "1. 第 1 句：解释这项判定的本质（在判什么 / 输入是什么）。\n"
        "2. 第 2 句：解释决策方式（按优先级匹配 / 命中后赋值 / 兜底逻辑）。\n"
        "3. 第 3 句（可选）：提示用户可以问『看看 XX 规格的具体规则』或『哪些规则会判为 X 级』。\n"
        "4. 全文不超过 130 字。"
    )
    narrative_user = (
        f"用户问：「{judgment_name}」的判定依据。\n\n实测事实：\n{fact_block}\n\n请用人话向班组长解释。"
    )

    llm = get_llm(model_name_arg)
    narrative_resp = await llm.ainvoke(
        [
            {"role": "system", "content": narrative_system},
            {"role": "user", "content": narrative_user},
        ]
    )
    narrative_text = _sanitize_user_facing_text(str(narrative_resp.content))

    # 5. 按规格拼 markdown 表格
    body_blocks: list[str] = []
    for sc in spec_codes_sorted:
        rs = grouped[sc]
        body_blocks.append(f"\n### {sc} 规格（共 {len(rs)} 条规则）\n")
        body_blocks.append("| 优先级 | 等级名 | 判定结果 | 命中条件 |")
        body_blocks.append("| --- | --- | --- | --- |")
        for r in rs:
            cond_cn = (r["condition_cn"] or "—").replace("|", "\\|")
            name = r["name"] or "—"
            default_tag = "（兜底）" if r["is_default"] else ""
            body_blocks.append(
                f"| {r['priority']} | **{name}**{default_tag} | {r['status']} | {cond_cn} |"
            )

    full_response = narrative_text + "\n" + "\n".join(body_blocks)

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-judge-narrate",
            "kind": "answer",
            "title": "答案已就绪",
            "summary": f"{judgment_name} · {total_rule_count} 条规则",
            "status": "success",
        },
    )

    return {
        "response": full_response,
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
            "last_query_type": "judgment_rules",
            "last_query_entities": {"judgment_type": judgment_type, "spec_code": spec_code},
        },
    }


# === 旧逻辑（已废弃，保留供参考；新代码上面已重写） ===
async def _handle_judgment_rules_query_OLD(
    judgment_type: str, entities: dict, messages: list, context: dict | None = None
) -> dict[str, Any]:
    """[DEPRECATED] 旧版判定规则查询，按规格分步问询；新版已合并到 _handle_judgment_rules_query。"""
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


def _sanitize_user_facing_text(text: str) -> str:
    """业务展示前的兜底清洗——LLM 有时会无视 system prompt 把技术词漏出来。

    - 剥离开头的 ``{"highlevelkeywords":...,"lowlevelkeywords":...}`` 之类 JSON 关键词块
    - 剥离 ``Code:``/``References:``/``Sources:`` 类英文标记块
    - 把 lab_xxx 表名 / F_XXX 字段名替换成业务词
    - 把"知识库中说明""上下文信息""References"等技术性短语去掉
    """
    import re

    if not text:
        return ""
    s = str(text)

    # 1. 剥离 ALL LightRAG keyword-extraction JSON 泄露（位置任意、数量不限）
    # LightRAG hybrid 模式偶尔会把 prompt 的关键词阶段输出原样吐到 answer 字段。
    # 形如 `{"highlevelkeywords":[...],"lowlevelkeywords":[...]}{"highlevelkeywords":...}`
    s = re.sub(
        r"\{[^{}]*?(?:high(?:_)?level(?:_)?keywords|low(?:_)?level(?:_)?keywords)[^{}]*?\}",
        "",
        s,
        flags=re.IGNORECASE | re.DOTALL,
    )

    # 2. 剥离 ``References`` / ``Sources`` / ``Citations`` 标题块及其后面所有引用条目
    #    匹配 markdown 标题（## References）、加粗标题（**References**）、普通行（References）
    #    冒号可选；后面是任意行 [1] xxx#xxx
    s = re.sub(
        r"\n+\s*(?:\#+\s*|\*\*\s*)?(?:Code|References?|Sources?|Citations?)\s*(?:\*\*)?[:：]?\s*\n[\s\S]*$",
        "",
        s,
        flags=re.IGNORECASE,
    )

    # 3. 技术名词 → 业务词替换
    replacements = [
        # 表名
        (r"\blab_intermediate_data\b", "中间检测数据"),
        (r"\blab_product_spec(_attribute|_version)?\b", "产品规格"),
        (r"\blab_intermediate_data_judgment_level\b", "判定规则"),
        (r"\blab_intermediate_data_formula\b", "指标公式"),
        (r"\blab_report_config\b", "报表配置"),
        (r"\blab_appearance[a-z_]*\b", "外观特性数据"),
        (r"\blab_raw_data\b", "原始叠片数据"),
        (r"\blab_magnetic_raw_data\b", "原始磁性能数据"),
        # 通用 lab_ 兜底
        (r"\blab_[a-z_]+\b", "业务数据"),
        # 字段名
        (r"\bF_FIRST_INSPECTION\b", "一次检验结果"),
        (r"\bF_LABELING\b", "贴标等级"),
        (r"\bF_PROD_DATE\b", "生产日期"),
        (r"\bF_SHIFT\b", "班组"),
        (r"\bF_SINGLE_COIL_WEIGHT\b", "单卷重"),
        (r"\bF_PRODUCT_SPEC_CODE\b", "规格代码"),
        # 炉号双字段：FORMATTED 是物料唯一编码（用于内部匹配/dedup），F_FURNACE_NO 是业务展示炉号
        (r"\bF_FURNACE_NO_FORMATTED\b", "炉号编码"),
        (r"\bF_FURNACE_NO\b", "炉号"),
        (r"\bF_DETECTION_COLUMNS\b", "检测列"),
        # F_xxx 通用兜底（剩下没显式映射的）
        (r"\bF_[A-Z_]+\b", "字段"),
        # 图谱节点类型（业务词替换）
        (r"\bProductSpec\b", "产品规格"),
        (r"\bSpecAttribute\b", "工艺属性"),
        (r"\bFormula\b", "指标"),
        (r"\bJudgmentRule\b", "判定规则"),
        (r"\bFurnaceNoParsed\b", "炉号解析"),
        (r"\bFurnaceNoField\b", "炉号字段"),
        # SQL 关键字（保守起见保留 SQL 本身，因为有时回答是介绍能力）
        (r"\bJOIN\b", "关联"),
    ]
    for pat, repl in replacements:
        s = re.sub(pat, repl, s)

    # 4. 折叠多余空白
    s = re.sub(r"\n{3,}", "\n\n", s)
    s = re.sub(r"[ \t]+\n", "\n", s)
    return s.strip()


def _has_uncooperative_fallback(text: str) -> bool:
    """检测 LLM 是否"摆烂"——明明给了数据还说『无法确定 / 知识库中说明』。

    命中时调用方应该弃用 LLM 输出，改用模板兜底回答。
    """
    if not text:
        return True
    head = text[:200]
    bad_phrases = (
        "无法确定", "无法回答", "我无法回答", "上下文信息", "知识库中说明",
        "knowledgebase.json", "knowledge_base.json", "highlevelkeywords",
        "high_level_keywords", "lowlevelkeywords", "low_level_keywords",
        "References", "Sources",
    )
    return any(p in head for p in bad_phrases)


async def _handle_product_specs_query(
    context: dict | None = None,
    model_name: str | None = None,
) -> dict[str, Any]:
    """Handle product specs inquiry with rich attributes.

    输出：
      - LLM 流式生成的业务化简介（"我们当前有 4 种规格..."）
      - 清晰的中文 markdown 表格（规格 / 带宽 / 检测列 / 工艺属性）

    Args:
        context: Previous conversation context
        model_name: LLM model name (None = .env default)

    Returns:
        Response with product specs list and extension attributes
    """
    from app.tools.query_tools import get_product_specs_tool

    logger.info("[product_specs] start querying lab_product_spec + version + attributes")
    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-spec-sql",
            "kind": "sql",
            "title": "正在查询产品规格清单",
            "summary": "联表读取 lab_product_spec + 版本 + 工艺属性",
            "status": "running",
        },
    )

    try:
        specs_result = await get_product_specs_tool.ainvoke({})
    except Exception as exc:  # noqa: BLE001
        logger.exception("[product_specs] SQL failed: %s", exc)
        await adispatch_custom_event(
            "reasoning_step",
            {
                "id": "step-spec-sql",
                "kind": "fallback",
                "title": "查询产品规格失败",
                "summary": str(exc)[:160],
                "status": "error",
            },
        )
        return {
            "response": f"查询产品规格时数据库报错：{str(exc)[:200]}。请检查 nlq-agent 后台日志。",
            "chart_config": None,
            "intent": "query",
            "entities": {"query_type": "product_specs"},
            "context": context or {},
        }

    if not specs_result.get("found"):
        logger.warning("[product_specs] empty result from get_product_specs_tool")
        await adispatch_custom_event(
            "reasoning_step",
            {
                "id": "step-spec-sql",
                "kind": "fallback",
                "title": "没有找到任何产品规格",
                "summary": "lab_product_spec 启用记录为 0 条",
                "status": "warning",
            },
        )
        return {
            "response": "未找到产品规格信息。",
            "chart_config": None,
            "intent": "query",
            "entities": {"query_type": "product_specs"},
            "context": context or {},
        }

    specs = specs_result.get("specs", [])
    count = specs_result.get("count", 0)
    logger.info("[product_specs] found %d specs", count)
    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-spec-sql",
            "kind": "sql",
            "title": f"产品规格查询完成（{count} 种）",
            "summary": "、".join((s.get("code") or "") for s in specs[:8]),
            "status": "success",
        },
    )
    # 让 LLM 流式输出一段口语化简介（确保用户感受到"打字效果"）
    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-spec-format",
            "kind": "answer",
            "title": "正在整理产品规格说明",
            "summary": "用业务口吻汇报清单",
            "status": "running",
        },
    )

    # 收集所有出现过的工艺属性名作为表格列，避免不同规格属性数量不一致
    attr_name_set: list[str] = []
    for spec in specs:
        for attr in spec.get("attributes") or []:
            n = (attr.get("name") or "").strip()
            if n and n not in attr_name_set:
                attr_name_set.append(n)
    attr_name_set = attr_name_set[:6]  # 最多列出 6 个工艺属性，避免表格太宽

    # 给 LLM 的事实块（纯业务语言，绝不出现 lab_xxx / F_xxx）
    spec_bullets = []
    for spec in specs:
        code = spec.get("code") or ""
        name = spec.get("name") or ""
        wmin, wmax = spec.get("width_min"), spec.get("width_max")
        det = spec.get("detection_columns") or "—"
        bw = f"{wmin}~{wmax} mm" if wmin and wmax else "未配置"
        spec_bullets.append(f"  · {code}{('（' + name + '）') if name and name != code else ''}：带宽 {bw}，检测列 {det}")
    fact_block = (
        f"产品规格总数：{count} 种\n"
        f"各规格基础信息：\n" + "\n".join(spec_bullets) + "\n"
        f"工艺属性维度：{('、'.join(attr_name_set)) if attr_name_set else '无'}"
    )

    narrative_system = (
        "你是【小美】，检测中心智能数据助理，正在向班组长汇报系统里维护的产品规格清单。\n\n"
        "【硬性规则 — 严禁】\n"
        "1. 严禁出现任何表名/字段名：lab_xxx、F_xxx、F_DETECTION_COLUMNS、SQL、JOIN。\n"
        "2. 严禁编造数据——只能引用下面给出的真实统计。\n\n"
        "【输出要求】\n"
        "1. 直接 2 句口语化中文，第一句报总数（"
        "比如『当前系统里维护了 4 种产品规格，分别是 …』），第二句说明它们的差异（带宽 / 检测列 / 工艺属性）。\n"
        "2. 全文不超过 80 字。不要 markdown 标题、不要列表、不要表格——表格我会自己拼好附在下面。\n"
        "3. 不要写'我可以再帮你看 XX'之类的引导句。"
    )
    narrative_user = (
        f"用户问：{('请告诉我系统里有哪些产品规格')}\n\n真实数据：\n{fact_block}\n\n请用人话简短汇报。"
    )

    llm = get_llm(model_name)
    narrative_resp = await llm.ainvoke(
        [
            {"role": "system", "content": narrative_system},
            {"role": "user", "content": narrative_user},
        ]
    )
    raw_narr = str(narrative_resp.content)
    narrative_text = _sanitize_user_facing_text(raw_narr)
    # 若 LLM 明明拿到了 4 种规格，还顽固说"无法确定"或漏 LightRAG 关键词 JSON，
    # 弃用并用模板兜底——业务人员看到的是"系统里有 N 种产品规格，分别是…"。
    if _has_uncooperative_fallback(narrative_text):
        logger.warning(
            "[product_specs] narrative LLM 输出不合作，用模板兜底 | raw=%r",
            raw_narr[:160],
        )
        codes_zh = "、".join((s.get("code") or "") for s in specs if s.get("code"))
        attr_zh = "、".join(attr_name_set) if attr_name_set else ""
        if attr_zh:
            narrative_text = (
                f"当前系统里维护了 {count} 种产品规格，分别是 {codes_zh}。"
                f"它们的差异主要体现在带宽、检测列数以及 {attr_zh} 等工艺属性上。"
            )
        else:
            narrative_text = (
                f"当前系统里维护了 {count} 种产品规格，分别是 {codes_zh}。"
                "它们的差异主要体现在带宽和检测列数上。"
            )

    # 拼中文 markdown 表格：规格代码 / 名称 / 带宽 / 检测列 + 动态工艺属性列
    base_cols = ["规格代码", "规格名称", "带宽 (mm)", "检测列数"]
    headers = base_cols + attr_name_set
    sep = "| --- " * len(headers) + "|"
    table_lines = ["", "### 产品规格清单", "", "| " + " | ".join(headers) + " |", sep]
    for spec in specs:
        code = spec.get("code") or "—"
        name = spec.get("name") or "—"
        wmin, wmax = spec.get("width_min"), spec.get("width_max")
        bw = f"{wmin} ~ {wmax}" if wmin and wmax else "—"
        det = str(spec.get("detection_columns") or "—")
        # 把扩展属性按名字 → 值（含单位）映射
        attr_map: dict[str, str] = {}
        for a in spec.get("attributes") or []:
            n = (a.get("name") or "").strip()
            if not n:
                continue
            v = a.get("value")
            u = a.get("unit") or ""
            attr_map[n] = f"{v}{(' ' + u) if u else ''}" if v not in (None, "") else "—"
        row = [code, name, bw, det] + [attr_map.get(n, "—") for n in attr_name_set]
        table_lines.append("| " + " | ".join(row) + " |")

    full_response = narrative_text + "\n" + "\n".join(table_lines)
    logger.info("[product_specs] response built (%d chars, narrative=%d)", len(full_response), len(narrative_text))

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-spec-format",
            "kind": "answer",
            "title": "答案已就绪",
            "summary": f"产品规格 {count} 种 · 含 {len(attr_name_set)} 项工艺属性",
            "status": "success",
        },
    )

    # 把本轮的查询类型存到 context，下一轮"使用表格 / 详细一点"等承接命令能继续锁定。
    updated_context = dict(context or {})
    updated_context["last_query_type"] = "product_specs"
    updated_context["last_query_entities"] = {}

    return {
        "response": full_response,
        "chart_config": None,
        "intent": "query",
        "entities": {"query_type": "product_specs"},
        "context": updated_context,
    }


def _detect_definition_query(question: str) -> bool:
    """Detect meta-question about indicator definition / calculation basis.

    示例：
      - "一次交检合格率的判断依据是什么"
      - "合格率怎么算的"
      - "A 类是按什么算的"
      - "不合格的计算口径"
      - "包含哪些等级"
    这些问题要走元问题分支（解释口径），不要执行 SQL 取实际值。

    Args:
        question: User's question
    Returns:
        True if it's asking about indicator definition rather than value.
    """
    if not question:
        return False
    meta_kw = [
        "判断依据", "判定依据", "计算口径", "计算方法", "怎么算", "怎么计算",
        "如何计算", "如何算", "口径", "判定标准",
        "包含哪些等级", "是按什么", "按什么算", "依据是什么", "怎么定义",
        "什么意思", "什么含义", "定义", "算法",
    ]
    # "X 是什么" 这种短句太宽，容易把"一次交检合格率是什么"判为元问题——这正是我们想要的。
    # 但要排除"X 是 N"这种带数字的陈述。
    q = question.strip()
    if any(kw in q for kw in meta_kw):
        return True
    # "一次交检合格率是什么" / "合格率是什么意思" 这种结尾
    if q.endswith("是什么") or q.endswith("是什么意思") or q.endswith("是什么？") or q.endswith("是什么?"):
        return True
    return False


_INDICATOR_NAME_KEYWORDS = [
    "一次交检合格率",
    "合格率",
    "不合格率",
    "不合格",
    "A 类",
    "A类",
    "B 类",
    "B类",
]


def _guess_indicator_name(question: str) -> str | None:
    """从问题里粗粒度抽取被询问的指标名，用于 lab_report_config 查找。"""
    if not question:
        return None
    q = question.strip()
    for kw in _INDICATOR_NAME_KEYWORDS:
        if kw in q:
            # 归一化："A 类" / "A类" → "A"；"B 类" / "B类" → "B"
            if kw in ("A 类", "A类"):
                return "A"
            if kw in ("B 类", "B类"):
                return "B"
            return kw
    return None


async def _handle_indicator_definition_query(
    user_question: str,
    entities: dict[str, Any],
    context: dict[str, Any],
    model_name: str | None = None,
) -> dict[str, Any]:
    """Handle indicator definition / calculation-basis meta question.

    流程：
      1. 抽取指标名 → 读 lab_report_config + 公式表得到结构化事实
      2. 把"判定列/合格等级/是否占比"等翻译成纯业务语言（一次交检列 / 卷重 / 合格等级）
      3. 交给 LLM 用班组长能听懂的话流式输出
      4. 绝对不在用户面前出现 F_ 列名 / lab_xxx 表名 / SQL
    """
    indicator = _guess_indicator_name(user_question) or entities.get("metric") or "一次交检合格率"
    logger.info("[indicator_definition] indicator=%s | question=%r", indicator, user_question[:80])

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-indicator-def",
            "kind": "sql",
            "title": f"查询「{indicator}」的判断口径",
            "summary": "从系统报表配置读取等级与计算依据",
            "status": "running",
        },
    )

    info = await get_indicator_definition_tool.ainvoke({"indicator_name": indicator})
    if not info.get("found"):
        logger.warning("[indicator_definition] not found: %s", info.get("error"))
        await adispatch_custom_event(
            "reasoning_step",
            {
                "id": "step-indicator-def",
                "kind": "fallback",
                "title": "没找到这个指标的配置",
                "summary": "请确认指标名是否正确",
                "status": "warning",
            },
        )
        # 这种兜底分支不走 LLM 流式 → 直接返回简短业务说明
        return {
            "response": (
                f"没有在系统的报表配置里找到「{indicator}」这个指标。"
                f"当前可用的指标有：一次交检合格率、A、B、合格率、不合格。"
                f"你可以换个说法，或者直接告诉我想看哪一项。"
            ),
            "chart_config": None,
            "intent": "query",
            "entities": {**entities, "query_type": "indicator_definition"},
            "context": context or {},
        }

    name = info["name"]
    desc = info.get("description") or ""
    levels = info.get("level_names") or []
    is_pct = info.get("is_percentage", False)
    raw_column_name = info.get("column_name") or ""

    # 把内部逻辑列名翻译成"一次交检列 / 贴标判定 / ..."这种纯业务话术。
    # 用户看不到 F_FIRST_INSPECTION 这种字段名，纯讲业务语义。
    _COLUMN_TO_BUSINESS = {
        "FirstInspection": "一次交检",
        "Labeling": "贴标判定",
        "MagneticResult": "性能判定",
        "ThickRes": "厚度判定",
        "LaminationFactorRes": "叠片判定",
    }
    column_business = _COLUMN_TO_BUSINESS.get(raw_column_name, "判定列")

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-indicator-def",
            "kind": "sql",
            "title": f"已读取「{name}」的配置",
            "summary": (
                f"合格等级={'、'.join(levels) or '未配置'} · "
                f"判定依据={column_business} · "
                f"{'占比指标' if is_pct else '汇总指标'}"
            ),
            "status": "success",
        },
    )

    # 把结构化事实喂给 LLM 让它用业务话术说，纯人话。
    # 注意：这里给 LLM 的"事实"只用业务名词，不含任何 F_ 列名 / 表名。
    pass_grades_text = "、".join(levels) if levels else "未配置"
    fact_block = (
        f"指标名称：{name}\n"
        f"判定依据：根据【{column_business}】这一项的判定结果\n"
        f"合格等级：{pass_grades_text}\n"
        f"指标类型：{'占比指标（按重量比例展示百分比）' if is_pct else '汇总指标（按重量直接求和）'}\n"
        f"统计口径：按卷重加权（合格记录的卷重总和"
        + ("除以全部记录的卷重总和)" if is_pct else "，按吨/公斤汇总）")
        + "\n"
        f"系统描述：{desc or '（系统未填写额外说明）'}\n"
    )

    narrative_system = (
        "你是【小美】，检测中心智能数据助理，正在向**班组长/质检员**（业务人员）"
        "解释一个统计指标的判断口径。\n\n"
        "【硬性规则 — 绝对禁止】\n"
        "1. 绝对禁止出现任何**数据库表名/列名/字段名**，包括但不限于："
        "  lab_intermediate_data、lab_report_config、lab_product_spec，"
        "  F_FIRST_INSPECTION、F_LABELING、F_SINGLE_COIL_WEIGHT、F_LEVEL_NAMES、F_Id、F_FORMULA_ID 等任何 F_ 开头的字段名。\n"
        "2. 绝对禁止出现 SQL / JOIN / WHERE / SELECT / 表 / 列 / 字段 / JSON 这类技术词。\n"
        "3. 绝对禁止出现 ThicknessRange、LaminationFactor、PsIronLoss、Hc、StripWidth 这类英文驼峰命名。\n"
        "4. 也不要说'在 XX 页面修改'之类的运维指引——业务人员不关心。\n\n"
        "【可用业务词汇】\n"
        "一次交检、贴标判定、性能判定、厚度判定、叠片判定、合格、不合格、"
        "等级（A/B/C）、卷重、班次、产品规格、产线、炉号、占比、汇总。\n\n"
        "【输出要求】\n"
        "1. 直接以一段话开头，告诉用户这个指标是按什么算的、合格的标准是什么。"
        "  例子：『一次交检合格率就是把「一次交检」这一项标为 A 的记录的卷重加起来，再看占总卷重的比例。』\n"
        "2. 然后用 2-3 句补充：合格的等级有哪些、是按重量还是按件数、占比还是汇总。\n"
        "3. 全文不超过 200 字，**不要 markdown 表格、不要代码块、不要项目符号清单**——就是顺畅的口语化中文段落。\n"
        "4. 全文绝不能让人觉得是从数据库导出来的，必须像质检主管亲口讲解。"
    )
    narrative_user = (
        f"用户问：{user_question}\n\n"
        f"系统配置（仅供你参考，绝对不要把字段名/英文名直接复述给用户）：\n"
        f"{fact_block}\n"
        "请用班组长能听懂的话，简洁告诉他这个指标的判断依据。"
    )

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-indicator-narrate",
            "kind": "answer",
            "title": "正在整理人话解释",
            "summary": "把判定口径转写为业务语言",
            "status": "running",
        },
    )

    # 用 LLM 流式输出 → on_chat_model_stream 事件 → 前端 text chunk → 打字机效果
    llm = get_llm(model_name)
    response_msg = await llm.ainvoke(
        [
            {"role": "system", "content": narrative_system},
            {"role": "user", "content": narrative_user},
        ]
    )
    response = str(response_msg.content).strip()
    logger.info("[indicator_definition] response built (%d chars) via LLM", len(response))

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-indicator-narrate",
            "kind": "answer",
            "title": "答案已就绪",
            "summary": f"「{name}」口径已转为业务语言",
            "status": "success",
        },
    )

    return {
        "response": response,
        "chart_config": None,
        "intent": "query",
        "entities": {**entities, "query_type": "indicator_definition", "indicator_name": name},
        "context": context or {},
    }


def _detect_grade_distribution_query(question: str) -> bool:
    """Detect quality-grade distribution questions like "上个月质量等级分布"."""
    if not question:
        return False
    kws = [
        "等级分布", "质量分布", "labeling 分布", "标签分布",
        "质量等级分布", "等级占比", "标签占比",
        "等级构成", "质量构成", "等级分类",
        "等级饼图", "质量饼图",
    ]
    q = question.strip()
    return any(k in q for k in kws)


def _detect_display_preference_followup(question: str) -> bool:
    """Detect "display-format-only" follow-up commands like '使用表格展示' / '换成饼图'.

    这类问题没指明新的指标或时间，只是想换种呈现方式 → 应该沿用上一轮的 handler，
    而不是当作新查询去 chat2sql 自由发挥（会扑空因为没有指标关键词）。
    """
    if not question:
        return False
    q = question.strip()
    if len(q) > 30:
        # 太长的问题大概率是新查询，不算纯展示切换
        return False
    display_kws = [
        "使用表格", "用表格", "改表格", "切换表格", "表格展示",
        "改图表", "换图表", "用图表", "切换图表",
        "换成饼图", "用饼图", "饼图展示",
        "换成柱状图", "用柱状图", "柱状图展示",
        "换条形图", "条形图展示",
        "换折线图", "用折线图",
        "再画一下", "再展示一下", "换种方式",
    ]
    return any(k in q for k in display_kws)


async def _handle_grade_distribution_query(
    user_question: str,
    entities: dict[str, Any],
    context: dict[str, Any],
    messages: list[Any] | None = None,
    model_name: str | None = None,
) -> dict[str, Any]:
    """Handle quality-grade distribution: SUM(F_SINGLE_COIL_WEIGHT) GROUP BY F_LABELING.

    口径与 /lab/monthly-dashboard 的 QualityDistributionPie 一致：
      - 按 F_LABELING 分组
      - 卷重只取 lab_intermediate_data.F_SINGLE_COIL_WEIGHT
      - 空 F_LABELING 显示为"未标注"
      - 排序按重量倒序
    输出：图表 chart_config (donut) + 顶部业务话术
    """
    from app.tools.query_tools import execute_safe_sql

    time_range = entities.get("time_range", {})
    shift = entities.get("shift")
    spec_code = entities.get("spec_code")
    time_desc = _format_time_range_desc(time_range)

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-grade-sql",
            "kind": "sql",
            "title": f"正在按等级汇总卷重（{time_desc}）",
            "summary": "按 F_LABELING 分组，卷重加权",
            "status": "running",
        },
    )

    conds: list[str] = []
    params: dict[str, Any] = {}
    time_clause = _build_time_range_sql(time_range)
    if time_clause:
        # _build_time_range_sql 已经返回 F_PROD_DATE 形式，这里给它加表别名前缀
        conds.append(time_clause.replace("F_PROD_DATE", "d.F_PROD_DATE"))
    if shift:
        conds.append("d.F_SHIFT = :shift")
        params["shift"] = shift
    if spec_code:
        conds.append(
            "d.F_PRODUCT_SPEC_ID COLLATE utf8mb4_unicode_ci "
            "IN (SELECT F_Id COLLATE utf8mb4_unicode_ci FROM lab_product_spec WHERE F_CODE = :spec_code)"
        )
        params["spec_code"] = spec_code

    where_clause = " AND ".join(conds) if conds else "1=1"
    sql = f"""
        SELECT
            COALESCE(NULLIF(TRIM(d.F_LABELING), ''), '未标注') AS labeling,
            ROUND(SUM(COALESCE(d.F_SINGLE_COIL_WEIGHT, 0)), 1) AS weight_kg,
            COUNT(*) AS row_count
        FROM lab_intermediate_data d
        WHERE {where_clause}
          AND (d.F_DeleteMark IS NULL OR d.F_DeleteMark = 0)
        GROUP BY COALESCE(NULLIF(TRIM(d.F_LABELING), ''), '未标注')
        ORDER BY weight_kg DESC
    """
    rows = await execute_safe_sql(sql, params)
    rows = [r for r in rows if (r.get("weight_kg") or 0) > 0]

    if not rows:
        await adispatch_custom_event(
            "reasoning_step",
            {
                "id": "step-grade-sql",
                "kind": "fallback",
                "title": "该时段没有可统计的记录",
                "summary": time_desc,
                "status": "warning",
            },
        )
        return {
            "response": f"{time_desc}没有可统计的卷重记录，可能是这段时间没有检测数据。",
            "chart_config": None,
            "intent": "query",
            "entities": {**entities, "query_type": "grade_distribution"},
            "context": context or {},
        }

    total_kg = sum(float(r["weight_kg"] or 0) for r in rows)
    total_count = sum(int(r["row_count"] or 0) for r in rows)

    # 拼图表配置（donut，前端 ECharts 渲染）
    pie_data = [
        {
            "name": str(r["labeling"]),
            "value": round(float(r["weight_kg"] or 0), 1),
            "count": int(r["row_count"] or 0),
            "percent": round(float(r["weight_kg"] or 0) / total_kg * 100, 2) if total_kg else 0.0,
        }
        for r in rows
    ]
    chart_config = {
        "type": "donut",
        "title": f"质量等级分布（{time_desc}）",
        "center_label": "总重量 (t)",
        "center_value": round(total_kg / 1000.0, 1),
        "unit": "kg",
        "data": pie_data,
    }

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-grade-sql",
            "kind": "sql",
            "title": f"已汇总 {len(rows)} 类等级",
            "summary": f"总卷重 {round(total_kg/1000, 1)} 吨 · 共 {total_count} 条记录",
            "status": "success",
        },
    )

    # 业务话术：LLM 流式输出，绝对不出现 SQL/字段名
    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-grade-narrate",
            "kind": "answer",
            "title": "正在整理答案",
            "summary": "用业务话术总结分布特征",
            "status": "running",
        },
    )

    # 给 LLM 准备的事实块：纯业务语言
    top_lines = []
    for r in pie_data[:5]:
        top_lines.append(
            f"  · {r['name']}：{round(r['value']/1000, 2)} 吨（占 {r['percent']}%）"
        )
    fact_block = (
        f"统计范围：{time_desc}\n"
        f"总卷重：{round(total_kg/1000, 2)} 吨（{round(total_kg, 0)} 公斤）\n"
        f"参与统计的记录：{total_count} 条\n"
        f"各等级（按卷重倒序）：\n"
        + "\n".join(top_lines)
        + (f"\n  · 其余 {len(pie_data)-5} 类合计：{round(sum(r['percent'] for r in pie_data[5:]), 2)}%" if len(pie_data) > 5 else "")
    )

    narrative_system = (
        "你是【小美】，检测中心智能数据助理，正在向班组长汇报质量等级分布。\n\n"
        "【硬性规则 — 严禁】\n"
        "1. 严禁出现任何表名/字段名/列名：lab_xxx、F_xxx、F_LABELING、F_SINGLE_COIL_WEIGHT、SQL、JOIN、WHERE。\n"
        "2. 严禁编造数据——只根据下面给出的真实统计结果回答。\n"
        "3. 严禁用 XXXX / XX.X% 这种占位符。\n\n"
        "【输出要求】\n"
        "1. 直接 2-3 句口语化中文。先报关键观察（哪个等级最多、合格占比大概多少、不合占比大概多少）。\n"
        "2. 然后 1 句建议或下一步动作（"
        "比如建议查看哪个等级的炉号明细、对比班次差异等）。\n"
        "3. 全文不超过 150 字。不要表格、不要列表、不要 markdown。\n"
        "4. 用'合格等级（A、B）'、'不合等级（带「不合」字样的）'、'其他等级' 来归类。"
    )
    narrative_user = (
        f"用户问：{user_question}\n\n实测统计：\n{fact_block}\n\n请用人话向班组长总结。"
    )

    llm = get_llm(model_name)
    narrative_resp = await llm.ainvoke(
        [
            {"role": "system", "content": narrative_system},
            {"role": "user", "content": narrative_user},
        ]
    )
    narrative_text = _sanitize_user_facing_text(str(narrative_resp.content))

    # 在 narrative 下面追加 markdown 数据表（中文表头），让用户看到精确数字
    table_lines = ["", "### 明细数据", "", "| 质量等级 | 卷重（吨） | 占比 | 记录数 |", "| --- | --- | --- | --- |"]
    for r in pie_data:
        ton = round(r['value'] / 1000.0, 2)
        table_lines.append(
            f"| {r['name']} | {ton:,} | {r['percent']}% | {r['count']} |"
        )
    table_lines.append(
        f"| **合计** | **{round(total_kg / 1000.0, 2):,}** | **100.00%** | **{total_count}** |"
    )
    full_response = narrative_text + "\n" + "\n".join(table_lines)

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-grade-narrate",
            "kind": "answer",
            "title": "答案已就绪",
            "summary": f"{len(rows)} 类等级 · 总卷重 {round(total_kg/1000, 1)} 吨",
            "status": "success",
        },
    )

    # 把本轮的查询类型 + 关键过滤条件存到 context，让下一轮的承接性命令
    # （"使用表格展示"、"换饼图"、"再看下班次拆分"）能继续锁定到 grade_distribution。
    updated_context = dict(context or {})
    updated_context["last_query_type"] = "grade_distribution"
    updated_context["last_query_entities"] = {
        "time_range": time_range,
        "shift": shift,
        "spec_code": spec_code,
    }

    return {
        "response": full_response,
        "chart_config": chart_config,
        "intent": "query",
        "entities": {**entities, "query_type": "grade_distribution"},
        "context": updated_context,
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

    # 🔄 真实推理步骤：开始查询数据库
    time_desc = _format_time_range_desc(time_range)
    spec_desc = f"{spec_code}规格" if spec_code else "全部规格"
    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-sql-fi",
            "kind": "sql",
            "title": "正在查询一次交检合格率",
            "summary": f"{spec_desc} · {time_desc}",
            "status": "running",
        },
    )

    # Query first inspection rate
    result = await query_first_inspection_rate_tool.ainvoke(
        {
            "spec_code": spec_code,
            "time_range_sql": time_range_sql,
            "shift": shift,
        }
    )

    # 🔄 真实推理步骤：数据已到手
    pre_total = result.get("total_count", 0) if isinstance(result, dict) else 0
    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-sql-fi",
            "kind": "sql",
            "title": "已从 lab_intermediate_data 取到数据",
            "summary": f"命中 {pre_total} 条记录",
            "status": "success",
        },
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
        "你是【小美】，检测中心数据助理。下面是一次交检合格率的真实统计结果（已经从数据库查到，不是编造）。"
        "请用 2-3 句简明中文向班组长口头汇报：先报核心数字（合格率%、合格/总卷重、件数），"
        "再点出关键观察。\n\n"
        "【硬性规则 — 禁止英文/列名】\n"
        "绝不能输出 F_FIRST_INSPECTION、F_DETECTION_WEIGHT、F_PROD_DATE 等数据库列名。"
        "如果合格率为 0%，要用业务语言说："
        "「该时段还没有一条记录被判为合格等级（A），所有记录要么标了缺陷，要么一次交检列还没填」，"
        "并提示用户「可以检查这些数据是否已经录入或参与判定」。\n\n"
        "输出纯文本，不要 JSON、不要 markdown 表格、不要代码块。"
        "今天是 {today}。"
    ).format(today=_today_str())
    narrative_user = (
        f"用户问：{user_question_for_narrative(messages)}\n\n"
        f"统计结果：\n{fact_block}\n\n"
        "请用人话向班组长汇报。"
    )

    # 🔄 真实推理步骤：交给 LLM 用业务口径整理
    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-narrative",
            "kind": "answer",
            "title": "正在整理答案",
            "summary": "用业务口径转写为人话",
            "status": "running",
        },
    )

    llm = get_llm(model_name)
    narrative_resp = await llm.ainvoke(
        [
            {"role": "system", "content": narrative_system},
            {"role": "user", "content": narrative_user},
        ]
    )
    narrative_text = _sanitize_user_facing_text(str(narrative_resp.content))

    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-narrative",
            "kind": "answer",
            "title": "答案已就绪",
            "summary": f"{pass_rate}% 合格率",
            "status": "success",
        },
    )

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
        f"_口径：与 lm/web 月度报表一致——只按 lab_intermediate_data.F_SINGLE_COIL_WEIGHT 加权。_"
    )

    return {
        "response": response,
        "chart_config": None,
        "intent": "query",
        "entities": {**entities, "query_type": "first_inspection_rate"},
        "context": context,
    }
