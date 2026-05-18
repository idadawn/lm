"""LangGraph graph definition module.

Defines AgentState state structure and LangGraph workflow graph.
MVP version only implements QueryAgent, other Agents reserved for future.
"""

from typing import Any

from langchain_core.callbacks import adispatch_custom_event
from langchain_core.messages import HumanMessage
from langgraph.graph import END, START, StateGraph

from app.agents.chat2sql_agent import chat2sql_agent_node
from app.agents.query_agent import query_agent_node
from app.agents.root_cause_agent import root_cause_agent_node
from app.core.llm_factory import get_llm

_MONTH_NAMES = [
    "january",
    "february",
    "march",
    "april",
    "may",
    "june",
    "july",
    "august",
    "september",
    "october",
    "november",
    "december",
]


def create_agent_graph() -> Any:
    """Create and compile LangGraph workflow graph.

    工作流：
        START
          ↓
        kb_pre_lookup  ── 命中 KB（含 LightRAG / 静态 KB）→ response_formatter → END
          ↓ 未命中
        intent_classifier
          ↓
        [conditional 路由]
          ├ query / unknown → query_agent
          ├ root_cause      → root_cause_agent
          ├ chat2sql        → chat2sql_agent
          └ insight/hyp     → response_formatter（暂不支持）
                ↓
        response_formatter → END

    Returns:
        Compiled StateGraph instance
    """
    workflow = StateGraph(dict[str, Any])

    # Add nodes
    workflow.add_node("kb_pre_lookup", kb_pre_lookup_node)
    workflow.add_node("intent_classifier", intent_classifier_node)
    workflow.add_node("query_agent", query_agent_node)
    workflow.add_node("root_cause_agent", root_cause_agent_node)
    workflow.add_node("chat2sql_agent", chat2sql_agent_node)
    workflow.add_node("response_formatter", response_formatter_node)

    # Define edges
    workflow.add_edge(START, "kb_pre_lookup")

    # KB 预查找四岔路由：
    # - 命中 KB（含 LightRAG / 静态 KB）→ response_formatter 直接收尾
    # - 命中"数据查询关键词" → fast-path 跳过 intent_classifier，直接进 chat2sql_agent
    # - 命中"等级条件关键词" → fast-path 跳过 intent_classifier，直接进 query_agent
    #   （intent_classifier 容易把「A 级别的铁损范围」误判为 chat2sql，但 query_agent 里有专用条件树 handler）
    # - 都未命中 → intent_classifier 走完整路径
    def _route_after_kb_lookup(state: dict[str, Any]) -> str:
        if state.get("_kb_pre_hit"):
            return "hit"
        if state.get("_query_fast"):
            return "fast_query"
        if state.get("_chat2sql_fast"):
            return "fast_chat2sql"
        return "miss"

    workflow.add_conditional_edges(
        "kb_pre_lookup",
        _route_after_kb_lookup,
        {
            "hit": "response_formatter",
            "fast_query": "query_agent",
            "fast_chat2sql": "chat2sql_agent",
            "miss": "intent_classifier",
        },
    )

    # Conditional routing: dispatch to different Agents based on intent
    workflow.add_conditional_edges(
        "intent_classifier",
        route_by_intent,
        {
            "query": "query_agent",
            "root_cause": "root_cause_agent",
            "chat2sql": "chat2sql_agent",
            "insight": "response_formatter",  # MVP not supported, return hint
            "hypothesis": "response_formatter",  # MVP not supported, return hint
            "unknown": "query_agent",  # Default to query Agent
        },
    )

    # QueryAgent completion leads to response formatting
    workflow.add_edge("query_agent", "response_formatter")
    workflow.add_edge("root_cause_agent", "response_formatter")
    workflow.add_edge("chat2sql_agent", "response_formatter")

    # Response formatting ends
    workflow.add_edge("response_formatter", END)

    return workflow.compile()


async def kb_pre_lookup_node(state: dict[str, Any]) -> dict[str, Any]:
    """图谱预查找节点：所有问题先在 LightRAG / 静态 KB 里找权威答案。

    命中时（confidence ≥ 阈值）：直接把答案写入 state.response + state.citations，
    并设置 _kb_pre_hit=True 告诉路由跳过 intent_classifier 直接去 response_formatter。

    未命中：返回空 patch，路由继续走 intent_classifier。

    为什么放这里：
    - 「炉号是怎么组成的」「F_LABELING 和 F_FIRST_INSPECTION 区别」等定义类问题
      LLM intent_classifier 容易误分类为 chat2sql，从而跳过 query_agent 顶层的 KB lookup。
    - 把 KB 查找前置，**不管 intent 怎么分**都不会漏掉权威答案。
    - 未命中时只多一次本地向量检索（<100ms），代价可忽略。
    """
    from app.knowledge_graph.kb_lookup import lookup_kb_smart
    from langchain_core.callbacks import adispatch_custom_event
    from langchain_core.messages import HumanMessage

    # 提取最新一条 user message
    messages = state.get("messages", []) or []
    user_question = ""
    for m in reversed(messages):
        if isinstance(m, HumanMessage):
            user_question = str(m.content or "").strip()
            break
        if isinstance(m, dict) and (m.get("type") == "human" or m.get("role") == "user"):
            user_question = str(m.get("content", "")).strip()
            break

    # 关键：LangGraph 用 dict[str, Any] 状态时，node 返回值会覆盖式合并到 state。
    # 我们 miss 路径必须显式带回所有上游字段，否则后续 intent_classifier 收到空 state。
    def _miss():
        return {
            "_kb_pre_hit": False,
            # 透传所有 chat.py initial_state 字段
            "messages": state.get("messages", []),
            "session_id": state.get("session_id"),
            "model_name": state.get("model_name"),
            "auth_context": state.get("auth_context"),
            "intent": state.get("intent", "unknown"),
            "entities": state.get("entities", {}),
            "context": state.get("context", {}),
            "tool_results": state.get("tool_results", {}),
            "chart_config": state.get("chart_config"),
            "response": state.get("response", ""),
        }

    if not user_question:
        return _miss()

    # 启发式：含时间/范围关键词的问题是"数据查询"而非"定义查询"，让 chat2sql 处理
    # 例：「上个月各班组质量分布」要的是真实数据，不是"质量分布怎么算"的定义说明
    DATA_QUERY_HINTS = (
        "今日", "今天", "昨日", "昨天", "本周", "上周", "这周",
        "本月", "上月", "上个月", "这个月", "当月",
        "本年", "去年", "今年",
        "最近", "近 ", "近一", "近2", "近3", "近5", "近7", "近10", "近30",
        "多少", "是多少", "是几", "几条", "几个",
        "趋势", "走势", "对比", "排名", "top", "前几",
    )
    # 等级条件类问题 fast-path → 直通 query_agent 用专用条件树 handler。
    # 触发模式：
    #   1) 「X 级别」+「条件/规则/范围/标准/阈值/判定/依据」（首轮提问）
    #   2) 上一轮已锁定完整三要素（spec+judgment+grade），本轮问指标或"完整规则"（多轮承接）
    import re as _re
    prior_ctx = state.get("context") or {}
    has_grade_ctx = (
        prior_ctx.get("judgment_type")
        and prior_ctx.get("grade")
        and prior_ctx.get("spec_code")
    )
    mentions_metric = any(k in user_question for k in (
        "铁损", "带宽", "厚度", "叠片", "矫顽力", "断头", "外观", "密度", "新带型", "Ps", "Hc",
    ))
    mentions_complete = any(w in user_question for w in (
        "完整", "全部", "所有条件", "整个", "全套",
    ))
    pattern_first_turn = (
        _re.search(r"[A-Da-d]\s*(?:级别|等级|级)", user_question)
        and any(k in user_question for k in ("条件", "规则", "范围", "标准", "阈值", "判定", "依据"))
    )
    pattern_followup = has_grade_ctx and (mentions_metric or mentions_complete)
    if pattern_first_turn or pattern_followup:
        try:
            await adispatch_custom_event(
                "reasoning_step",
                {
                    "id": "step-intent",
                    "kind": "intent",
                    "title": "识别意图：等级判定条件",
                    "summary": "命中等级条件关键词，直接查找判定规则",
                    "status": "success",
                },
            )
        except Exception:
            pass
        fast_q = _miss()
        fast_q["_query_fast"] = True
        fast_q["intent"] = "query"
        return fast_q

    if any(hint in user_question for hint in DATA_QUERY_HINTS):
        # Fast-path：含明确数据查询关键词（时间窗口/聚合/分布等）→ 跳过 intent_classifier
        # 直接进 chat2sql_agent。chat2sql 完全靠 user_question 自己解析，
        # 不依赖 intent entities，所以这一步是纯无损节省（~30s）。
        try:
            await adispatch_custom_event(
                "reasoning_step",
                {
                    "id": "step-intent",
                    "kind": "intent",
                    "title": "识别意图：数据查询（快速通道）",
                    "summary": "命中时间/聚合关键词，直接生成 SQL",
                    "status": "success",
                },
            )
        except Exception:
            pass  # event dispatch 失败不影响主路径
        fast_miss = _miss()
        fast_miss["_chat2sql_fast"] = True
        fast_miss["intent"] = "chat2sql"
        return fast_miss

    try:
        kb_hit = await lookup_kb_smart(user_question)
    except Exception:
        return _miss()

    if not kb_hit or not kb_hit.get("answer"):
        return _miss()

    # 清洗 LightRAG/静态 KB 答案 —— 剥离 keyword JSON 泄露 + 把残留 lab_xxx / F_xxx 等
    # 技术词换成业务词；若清洗后 LLM "摆烂" 说无法回答，干脆当作 miss 让下游 query_agent / chat2sql 处理
    try:
        from app.agents.query_agent import _sanitize_user_facing_text, _has_uncooperative_fallback
        cleaned = _sanitize_user_facing_text(kb_hit.get("answer", ""))
        if _has_uncooperative_fallback(cleaned):
            return _miss()
        kb_hit["answer"] = cleaned
    except Exception:
        pass  # sanitizer 失败不影响主路径

    # 命中！发个 reasoning_step 让前端看到走的是 KB 路径
    try:
        is_lr = kb_hit.get("lightrag", False)
        await adispatch_custom_event(
            "reasoning_step",
            {
                "id": "step-kb-pre",
                "kind": "knowledge_lookup",
                "title": f"从{'图谱语义检索' if is_lr else '业务知识库'}取到答案",
                "summary": f"置信度 {kb_hit.get('confidence', 0):.2f}",
                "status": "success",
                "citations": kb_hit.get("citations", []),
            },
        )
    except Exception:
        pass  # 事件分发失败不影响主路径

    return {
        "_kb_pre_hit": True,
        "response": kb_hit["answer"],
        "intent": "query",
        "chart_config": None,
        "entities": {
            **(state.get("entities") or {}),
            "query_type": "knowledge_base",
            "kb_source": "lightrag" if kb_hit.get("lightrag") else "static",
        },
        "context": state.get("context") or {},
        "citations": kb_hit.get("citations", []),
        "kb_confidence": kb_hit.get("confidence", 0.0),
    }


INTENT_CLASSIFICATION_PROMPT = """You are an intelligent NLQ (Natural Language Query) system for industrial quality data.

Analyze the user's question and extract the following information:

1. **intent**: The type of query. Choose from:
   - "query" - **快路径**：标准指标值/趋势查询（指标名明确：叠片系数/Ps铁损/矫顽力/厚度极差/带钢宽度），或一次交检合格率/判定规则/产品规格 等已知模板。
   - "root_cause" - 单条记录的根因解释（含"为什么...判为...等级"+ 炉号）。
   - "chat2sql" - **兜底**：用户问的指标名/维度不在 query 快路径里，但需要从 lab_* 表里取数（含 JOIN/聚合/自定义筛选）。例：
       * "甲班本月每个规格的总卷重和缺陷率"
       * "最近一周外观特性出现次数前 5 名"
       * "这个月叠片不合的炉号有哪些"
       * "对比甲乙两班的合格率"
       这类问题需要 LLM 自己读 schema、写 SQL、执行、解释。
   - "insight" - Pattern/anomaly detection (e.g., "find anomalies", "any patterns")
   - "hypothesis" - What-if scenarios (e.g., "what if", "if we relax")
   - "unknown" - 闲聊/打招呼/问日期等非数据问题。会路由到 query_agent 的兜底 LLM 对话路径。

2. **entities**: Extract relevant entities as a JSON object:
   - "metric": The quality metric being asked about (e.g., "PsIronLoss", "LaminationFactor", "ThicknessRange", "Hc"). Use English names.
   - "aggregation": Aggregation function if mentioned (e.g., "AVG", "MAX", "MIN", "SUM", "COUNT")
   - "time_range": Time period. Pick the **most specific** type that matches the user's wording:
       * 今日 / 今天                → {"type": "today"}
       * 昨日 / 昨天                → {"type": "yesterday"}
       * 本周 / 这周 / 这一周        → {"type": "this_week"}
       * 上周 / 上一周               → {"type": "last_week"}
       * 本月 / 这个月 / 当月         → {"type": "current_month"}
       * 上月 / 上个月               → {"type": "last_month"}
       * 本年 / 今年 / 全年          → {"type": "this_year"}
       * 去年                        → {"type": "last_year"}
       * 最近N天                     → {"type": "recent_days", "days": N}
       * 最近N周                     → {"type": "recent_weeks", "weeks": N}
       * 最近N个月                   → {"type": "recent_months", "months": N}
       * 2026年1月 / 一月            → {"type": "year_month", "year": 2026, "month": 1}
       * 1月份（无年份）             → {"type": "month", "month": 1}
       * 2026年（无月份）            → {"type": "year", "year": 2026}
   - "shift": Work shift if mentioned (e.g., "A", "B", "C")
   - "query_type": "value" for single values, "trend" for trends over time, "product_specs" for product specification inquiries
   - "judgment_type": For judgment rule inquiries (e.g., "Labeling", "MagneticResult", "LaminationResult")
   - "spec_code": Product spec code if mentioned (e.g., "120", "142", "170", "213", "all")
   - "furnace_no": Furnace identifier if mentioned
   - "batch_no": Batch identifier if mentioned
   - "grade": Target grade if mentioned (e.g., "A", "B", "C")

3. **context**: Any context that should be preserved for multi-turn conversation.

Respond in JSON format only:
{
  "intent": "query",
  "entities": {
    "metric": "PsIronLoss",
    "aggregation": "AVG",
    "time_range": {"type": "recent_days", "days": 7},
    "query_type": "value"
  },
  "context": {}
}

Examples:
- "最近7天叠片系数的平均值" -> {"intent": "query", "entities": {"metric": "LaminationFactor", "aggregation": "AVG", "time_range": {"type": "recent_days", "days": 7}, "query_type": "value"}}
- "贴标的判定规格有哪些" -> {"intent": "query", "entities": {"judgment_type": "Labeling", "query_type": "judgment_rules"}}
- "现在有多少种产品" -> {"intent": "query", "entities": {"query_type": "product_specs"}}
- "产品类型有哪些" -> {"intent": "query", "entities": {"query_type": "product_specs"}}
- "产品规格的基础信息和扩展信息" -> {"intent": "query", "entities": {"query_type": "product_specs"}}
- "为什么炉号 1丙20260110-1 是 C 级" -> {"intent": "root_cause", "entities": {"furnace_no": "1丙20260110-1", "grade": "C"}}
- "本月一次交检合格率" -> {"intent": "query", "entities": {"time_range": {"type": "current_month"}, "query_type": "first_inspection_rate"}}
- "今日合格率" -> {"intent": "query", "entities": {"time_range": {"type": "today"}, "query_type": "first_inspection_rate"}}
- "本周Ps铁损平均值" -> {"intent": "query", "entities": {"metric": "PsIronLoss", "aggregation": "AVG", "time_range": {"type": "this_week"}, "query_type": "value"}}
- "本年厚度极差最大值" -> {"intent": "query", "entities": {"metric": "ThicknessRange", "aggregation": "MAX", "time_range": {"type": "this_year"}, "query_type": "value"}}
"""


async def intent_classifier_node(state: dict[str, Any]) -> dict[str, Any]:
    """Intent classification node using LLM.

    Uses LLM to understand user intent and extract entities instead of hardcoded rules.
    Uses single user message format for better compatibility with local models like Qwen.

    Args:
        state: Current state dictionary

    Returns:
        Updated state fields
    """
    import json

    messages = state.get("messages", [])
    context = state.get("context", {})
    model_name = state.get("model_name") or None

    # Get last user message
    last_message = None
    for msg in reversed(messages):
        if isinstance(msg, HumanMessage):
            last_message = msg
            break
        elif isinstance(msg, dict):
            if msg.get("type") == "human" or msg.get("role") == "user":
                last_message = msg
                break

    if last_message is None:
        return {"intent": "unknown", "entities": {}, "context": context, "messages": messages}

    # 🔄 真实推理步骤：理解问题
    await adispatch_custom_event(
        "reasoning_step",
        {
            "id": "step-intent",
            "kind": "intent",
            "title": "正在理解你的问题",
            "summary": "解析意图与关键实体",
            "status": "running",
        },
    )

    user_content = str(
        last_message.content
        if hasattr(last_message, "content")
        else last_message.get("content", "")
    )

    # Build conversation history block — 让 LLM 看到完整对话承接关系
    # 关键：用户问"上个月呢"/"再算一下"时，必须知道上一轮聊的是什么
    history_lines: list[str] = []
    for hist_msg in messages[:-1]:  # 排除最后一条（已在 user_content）
        if isinstance(hist_msg, HumanMessage):
            history_lines.append(f"User: {hist_msg.content}")
        elif hist_msg.__class__.__name__ == "AIMessage":
            # 助理消息截断到 280 字，避免 prompt 爆炸但保留关键上下文
            ai_content = str(getattr(hist_msg, "content", ""))[:280]
            history_lines.append(f"Assistant: {ai_content}")
        elif isinstance(hist_msg, dict):
            role = hist_msg.get("role") or hist_msg.get("type")
            if role in ("user", "human"):
                history_lines.append(f"User: {hist_msg.get('content', '')}")
            elif role in ("assistant", "ai"):
                history_lines.append(f"Assistant: {str(hist_msg.get('content', ''))[:280]}")
    conv_history = "\n".join(history_lines) if history_lines else "(no prior turns)"

    full_prompt = f"""{INTENT_CLASSIFICATION_PROMPT}

---

Conversation history:
{conv_history}

Current user question: {user_content}

Previous extracted context: {json.dumps(context, ensure_ascii=False)}

注意：当 current question 是承接性表达（如"上个月呢"、"那今年呢"、"再算一下"），必须从对话历史中推断出上一轮的 metric / query_type / shift / spec_code 等实体，沿用过来。只有 time_range 之类被新问题明确替换的字段才更新。

Respond with JSON only:"""

    # Use LLM to classify intent and extract entities
    llm = get_llm(model_name)

    try:
        response = await llm.ainvoke([{"role": "user", "content": full_prompt}])

        # Parse JSON response
        result_text = str(response.content).strip()
        # Remove markdown code blocks if present
        if result_text.startswith("```json"):
            result_text = result_text[7:]
        if result_text.startswith("```"):
            result_text = result_text[3:]
        if result_text.endswith("```"):
            result_text = result_text[:-3]
        result_text = result_text.strip()

        # Handle empty response
        if not result_text:
            print("[WARN] LLM returned empty response, using fallback")
            return {
                "intent": "query",
                "entities": {},
                "context": context,
                "messages": messages,
            }

        result = json.loads(result_text)

        intent = result.get("intent", "unknown")
        entities = result.get("entities", {})
        new_context = result.get("context", {})

        # Merge with historical context for multi-turn support
        merged_context = {**context, **new_context}

        # 🔄 真实推理步骤：意图识别完成
        intent_label_zh = {
            "query": "标准指标查询",
            "root_cause": "炉号根因分析",
            "chat2sql": "自定义数据查询（生成 SQL）",
            "insight": "异常检测",
            "hypothesis": "What-if 假设",
            "unknown": "通用对话",
        }.get(intent, intent)
        ent_summary_parts = []
        if entities.get("metric"):
            ent_summary_parts.append(f"指标={entities['metric']}")
        if entities.get("query_type"):
            ent_summary_parts.append(f"类型={entities['query_type']}")
        tr = entities.get("time_range") or {}
        if isinstance(tr, dict) and tr.get("type"):
            ent_summary_parts.append(f"时间={tr['type']}")
        ent_summary = " · ".join(ent_summary_parts) or "无额外实体"
        await adispatch_custom_event(
            "reasoning_step",
            {
                "id": "step-intent",
                "kind": "intent",
                "title": f"识别意图：{intent_label_zh}",
                "summary": ent_summary,
                "status": "success",
            },
        )

        return {
            "intent": intent,
            "entities": entities,
            "context": merged_context,
            "messages": messages,
        }

    except Exception as e:
        print(f"[ERROR] Intent classification failed: {e}")
        # 🔄 真实推理步骤：意图识别失败
        try:
            await adispatch_custom_event(
                "reasoning_step",
                {
                    "id": "step-intent",
                    "kind": "intent",
                    "title": "意图识别失败，按通用查询处理",
                    "summary": str(e)[:140],
                    "status": "warning",
                },
            )
        except Exception:
            pass
        # Fallback to simple query intent
        return {
            "intent": "query",
            "entities": {},
            "context": context,
            "messages": messages,
        }


def route_by_intent(state: dict[str, Any]) -> str:
    """Route to different nodes based on intent.

    Args:
        state: Current state dictionary

    Returns:
        Target node name
    """
    return state.get("intent", "unknown")


async def response_formatter_node(state: dict[str, Any]) -> dict[str, Any]:
    """Response formatter node.

    Format Agent execution results into final response.

    Args:
        state: Current state dictionary

    Returns:
        Updated state fields
    """
    intent = state.get("intent", "unknown")

    # Handle unsupported intent types
    if intent in ["insight", "hypothesis"]:
        return {
            "response": (
                f"Sorry, the current MVP version does not support {intent} type questions.\n"
                f"You can try the following query types:\n"
                f"- What is the average LaminationFactor in the last 7 days?\n"
                f"- What is the PsIronLoss trend for shift A in January 2026?\n"
                f"- What is the maximum ThicknessRange last month?"
            ),
            "intent": intent,
            "entities": state.get("entities"),
            "context": state.get("context"),
            "calculation_explanation": state.get("calculation_explanation"),
            "grade_judgment": state.get("grade_judgment"),
            "reasoning_steps": state.get("reasoning_steps", []),
        }

    # QueryAgent already generated response, return directly
    response = state.get("response", "")
    if response:
        return {
            "response": response,
            "intent": state.get("intent"),
            "entities": state.get("entities"),
            "context": state.get("context"),
            "chart_config": state.get("chart_config"),
            "calculation_explanation": state.get("calculation_explanation"),
            "grade_judgment": state.get("grade_judgment"),
            "reasoning_steps": state.get("reasoning_steps", []),
            # ★ citations 透传给 SSE / 非流式返回
            "citations": state.get("citations", []),
            "kb_confidence": state.get("kb_confidence"),
        }

    # Default response
    return {
        "response": "I have received your question and am processing it...",
        "intent": state.get("intent"),
        "entities": state.get("entities"),
        "context": state.get("context"),
        "calculation_explanation": state.get("calculation_explanation"),
        "grade_judgment": state.get("grade_judgment"),
        "reasoning_steps": state.get("reasoning_steps", []),
        "citations": state.get("citations", []),
    }
