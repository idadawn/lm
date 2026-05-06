"""Full LangGraph flow test for the root_cause path.

Runs the compiled graph (intent_classifier → root_cause_agent → response_formatter)
against a mocked LLM + mocked KG + mocked SQL to ensure the entire pipeline
produces a non-empty response and at least 3 reasoning steps in state.
"""

from __future__ import annotations

from typing import Any
from unittest.mock import AsyncMock, MagicMock, patch

import pytest


_CLASSIFIER_RESPONSE = (
    '{"intent": "root_cause", '
    '"entities": {"furnace_no": "1丙20260110-1", "grade": "C"}, '
    '"context": {}}'
)

_RECORD = {
    "furnace_no": "1丙20260110-1",
    "batch_no": "BATCH-001",
    "spec_code": "120",
    "grade": "C",
    "F_WIDTH": 119.8,
    "F_PERF_PS_LOSS": 1.46,
}

_RULE = [
    {
        "rule": {
            "id": "r-1",
            "name": "C级",
            "priority": 1,
            "qualityStatus": "不合格",
            "color": "#f97316",
            "isDefault": False,
            "conditionJson": (
                '[{"field": "F_WIDTH", "operator": ">=", "value": 119.5},'
                ' {"field": "F_PERF_PS_LOSS", "operator": "<=", "value": 1.30}]'
            ),
        }
    }
]


@pytest.mark.asyncio
async def test_root_cause_flow_end_to_end() -> None:
    """Drive the full graph and assert response + reasoning_steps populated."""
    from app.agents.graph import create_agent_graph
    from langchain_core.messages import HumanMessage

    classifier_llm = AsyncMock()
    classifier_llm.ainvoke = AsyncMock(
        return_value=MagicMock(content=_CLASSIFIER_RESPONSE)
    )

    kg = MagicMock()
    kg.query_async = AsyncMock(return_value=_RULE)

    with (
        patch("app.agents.graph.get_llm", return_value=classifier_llm),
        patch(
            "app.tools.graph_tools.execute_safe_sql",
            new=AsyncMock(return_value=[_RECORD]),
        ),
        patch("app.tools.graph_tools.get_knowledge_graph", return_value=kg),
    ):
        graph = create_agent_graph()
        result = await graph.ainvoke(
            {
                "messages": [
                    HumanMessage(content="为什么炉号 1丙20260110-1 是 C 级？")
                ],
                "session_id": "flow-1",
                "model_name": "gpt-4o",
                "auth_context": {},
                "intent": "unknown",
                "entities": {},
                "context": {},
                "tool_results": {},
                "chart_config": None,
                "response": "",
            }
        )

    response = result.get("response", "")
    assert response, f"response must be non-empty; got: {response!r}"
    assert "C 级" in response or "C级" in response

    steps: list[dict[str, Any]] = result.get("reasoning_steps") or []
    assert len(steps) >= 3, (
        f"Expected ≥3 reasoning_steps for end-to-end root_cause; got {len(steps)}: "
        f"{steps}"
    )
    assert any(s.get("kind") == "record" for s in steps)
    assert any(s.get("kind") == "rule" for s in steps)
