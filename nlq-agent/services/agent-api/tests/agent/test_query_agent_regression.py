"""QueryAgent regression baseline tests.

Locks the existing QueryAgent answers shape so future RootCauseAgent / KG
work cannot silently break query_agent_node. Snapshots are intentionally
loose: shape + key fields, not full text.
"""

from __future__ import annotations

from unittest.mock import AsyncMock, MagicMock, patch

import pytest
from langchain_core.messages import HumanMessage


@pytest.mark.asyncio
async def test_query_agent_metric_value_baseline() -> None:
    """Average metric value query returns response + chart_config shape."""
    from app.agents.query_agent import query_agent_node

    formula_result = {
        "found": True,
        "id": "1",
        "column_name": "PerfPsLoss",
        "formula": "PerfPsLoss 平均值",
        "unit": "W/kg",
    }
    query_result_value = {"value": 1.20, "count": 42}
    query_result_trend = {
        "values": [
            {"date": "2026-04-20", "value": 1.18},
            {"date": "2026-04-21", "value": 1.22},
        ]
    }
    grade_result = {"found": True, "grade": "B", "quality_status": "合格"}

    llm = MagicMock()
    llm.ainvoke = AsyncMock(
        return_value=MagicMock(content="最近7天Ps铁损平均值为 1.20 W/kg。")
    )

    with (
        patch("app.agents.query_agent.get_llm", return_value=llm),
        patch(
            "app.agents.query_agent.get_formula_definition_tool"
        ) as formula_tool,
        patch("app.agents.query_agent.query_metric_tool") as metric_tool,
        patch("app.agents.query_agent.get_grade_rules_tool") as grade_tool,
    ):
        formula_tool.ainvoke = AsyncMock(return_value=formula_result)
        metric_tool.ainvoke = AsyncMock(
            side_effect=[query_result_value, query_result_trend]
        )
        grade_tool.ainvoke = AsyncMock(return_value=grade_result)

        result = await query_agent_node(
            {
                "messages": [HumanMessage(content="最近7天Ps铁损的平均值")],
                "context": {},
                "entities": {
                    "metric": "psironloss",
                    "aggregation": "AVG",
                    "time_range": {"type": "recent_days", "days": 7},
                    "query_type": "value",
                },
                "model_name": "gpt-4o",
            }
        )

    assert result["intent"] == "query"
    assert result["response"], "response text must be non-empty"
    assert "calculation_explanation" in result
    assert result["calculation_explanation"] is not None
    assert result["entities"]["metric"] == "psironloss"


@pytest.mark.asyncio
async def test_query_agent_no_metric_baseline() -> None:
    """When no metric extracted, agent prompts for clarification."""
    from app.agents.query_agent import query_agent_node

    llm = MagicMock()
    llm.ainvoke = AsyncMock(
        return_value=MagicMock(
            content="我需要知道您想查询哪个指标。可用的指标包括…"
        )
    )

    with patch("app.agents.query_agent.get_llm", return_value=llm):
        result = await query_agent_node(
            {
                "messages": [HumanMessage(content="给我数据")],
                "context": {},
                "entities": {},
                "model_name": "gpt-4o",
            }
        )

    assert result["intent"] == "query"
    assert "指标" in result["response"]
    assert result["chart_config"] is None
    assert result["grade_judgment"]["available"] is False
