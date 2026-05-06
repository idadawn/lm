"""LangGraph graph definition module.

Defines AgentState state structure and LangGraph workflow graph.
MVP version only implements QueryAgent, other Agents reserved for future.
"""

from typing import Any

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

    MVP workflow:
    START -> intent_classifier -> [conditional] -> query_agent -> response_formatter -> END

    Returns:
        Compiled StateGraph instance
    """
    workflow = StateGraph(dict[str, Any])

    # Add nodes
    workflow.add_node("intent_classifier", intent_classifier_node)
    workflow.add_node("query_agent", query_agent_node)
    workflow.add_node("root_cause_agent", root_cause_agent_node)
    workflow.add_node("chat2sql_agent", chat2sql_agent_node)
    workflow.add_node("response_formatter", response_formatter_node)

    # Define edges
    workflow.add_edge(START, "intent_classifier")

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

    user_content = str(
        last_message.content
        if hasattr(last_message, "content")
        else last_message.get("content", "")
    )

    # Build the full prompt with context
    # Use single user message format for better compatibility with local models
    full_prompt = f"""{INTENT_CLASSIFICATION_PROMPT}

---

User question: {user_content}

Previous context: {json.dumps(context, ensure_ascii=False)}

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

        return {
            "intent": intent,
            "entities": entities,
            "context": merged_context,
            "messages": messages,
        }

    except Exception as e:
        print(f"[ERROR] Intent classification failed: {e}")
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
    }
