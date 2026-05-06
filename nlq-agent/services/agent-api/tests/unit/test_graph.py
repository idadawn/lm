"""LangGraph graph definition unit tests aligned with the current implementation."""

from types import SimpleNamespace
from unittest.mock import AsyncMock, patch

import pytest
from langchain_core.messages import HumanMessage

from app.agents.graph import (
    create_agent_graph,
    intent_classifier_node,
    response_formatter_node,
    route_by_intent,
)


class TestCreateAgentGraph:
    """Test graph creation."""

    def test_create_agent_graph(self) -> None:
        """Create and compile the graph successfully."""
        graph = create_agent_graph()
        assert graph is not None
        assert hasattr(graph, "ainvoke")
        assert hasattr(graph, "astream_events")


class TestIntentClassifierNode:
    """Test intent classification behavior."""

    @pytest.mark.asyncio
    async def test_intent_classifier_parses_json_response(self) -> None:
        """Parse LLM JSON output into graph state."""
        llm = SimpleNamespace(
            ainvoke=AsyncMock(
                return_value=SimpleNamespace(
                    content="""
{
  "intent": "query",
  "entities": {
    "metric": "psironloss",
    "aggregation": "AVG"
  },
  "context": {
    "metric": "psironloss"
  }
}
"""
                )
            )
        )

        with patch("app.agents.graph.get_llm", return_value=llm):
            result = await intent_classifier_node(
                {
                    "messages": [HumanMessage(content="查询 Ps 铁损平均值")],
                    "session_id": "graph-1",
                    "context": {},
                }
            )

        assert result["intent"] == "query"
        assert result["entities"]["metric"] == "psironloss"
        assert result["entities"]["aggregation"] == "AVG"
        assert result["context"]["metric"] == "psironloss"

    @pytest.mark.asyncio
    async def test_intent_classifier_strips_markdown_code_fence(self) -> None:
        """Support JSON wrapped in markdown fences."""
        llm = SimpleNamespace(
            ainvoke=AsyncMock(
                return_value=SimpleNamespace(
                    content="""```json
{
  "intent": "root_cause",
  "entities": {},
  "context": {}
}
```"""
                )
            )
        )

        with patch("app.agents.graph.get_llm", return_value=llm):
            result = await intent_classifier_node(
                {
                    "messages": [HumanMessage(content="为什么被判为不合格？")],
                    "session_id": "graph-2",
                    "context": {},
                }
            )

        assert result["intent"] == "root_cause"
        assert result["entities"] == {}

    @pytest.mark.asyncio
    async def test_intent_classifier_merges_existing_context(self) -> None:
        """Preserve historical context while merging new context fields."""
        llm = SimpleNamespace(
            ainvoke=AsyncMock(
                return_value=SimpleNamespace(
                    content="""
{
  "intent": "query",
  "entities": {
    "shift": "A"
  },
  "context": {
    "shift": "A"
  }
}
"""
                )
            )
        )

        with patch("app.agents.graph.get_llm", return_value=llm):
            result = await intent_classifier_node(
                {
                    "messages": [HumanMessage(content="甲班数据")],
                    "session_id": "graph-3",
                    "context": {"metric": "psironloss"},
                }
            )

        assert result["context"] == {"metric": "psironloss", "shift": "A"}
        assert result["entities"] == {"shift": "A"}

    @pytest.mark.asyncio
    async def test_intent_classifier_returns_unknown_for_empty_messages(self) -> None:
        """Return unknown when no user message is available."""
        result = await intent_classifier_node(
            {
                "messages": [],
                "session_id": "graph-4",
                "context": {"metric": "psironloss"},
            }
        )

        assert result["intent"] == "unknown"
        assert result["entities"] == {}
        assert result["context"] == {"metric": "psironloss"}

    @pytest.mark.asyncio
    async def test_intent_classifier_falls_back_when_llm_output_is_invalid(self) -> None:
        """Fallback to query intent when the LLM output cannot be parsed."""
        llm = SimpleNamespace(ainvoke=AsyncMock(return_value=SimpleNamespace(content="not json")))

        with patch("app.agents.graph.get_llm", return_value=llm):
            result = await intent_classifier_node(
                {
                    "messages": [HumanMessage(content="查一下")],
                    "session_id": "graph-5",
                    "context": {"metric": "psironloss"},
                }
            )

        assert result["intent"] == "query"
        assert result["entities"] == {}
        assert result["context"] == {"metric": "psironloss"}

    @pytest.mark.asyncio
    async def test_intent_classifier_uses_model_from_state(self) -> None:
        """Select the LLM instance based on the request model_name."""
        llm = SimpleNamespace(
            ainvoke=AsyncMock(
                return_value=SimpleNamespace(
                    content='{"intent":"query","entities":{},"context":{}}'
                )
            )
        )

        with patch("app.agents.graph.get_llm", return_value=llm) as mock_get_llm:
            await intent_classifier_node(
                {
                    "messages": [HumanMessage(content="查一下")],
                    "session_id": "graph-model-1",
                    "model_name": "gemini-2.5-flash",
                    "context": {},
                }
            )

        mock_get_llm.assert_called_once_with("gemini-2.5-flash")


class TestRouteByIntent:
    """Test intent routing."""

    @pytest.mark.parametrize(
        ("state", "expected_route"),
        [
            ({"intent": "query"}, "query"),
            ({"intent": "root_cause"}, "root_cause"),
            ({"intent": "unknown"}, "unknown"),
            ({}, "unknown"),
        ],
    )
    def test_route_by_intent(self, state: dict[str, str], expected_route: str) -> None:
        """Route by current intent field."""
        assert route_by_intent(state) == expected_route


class TestResponseFormatterNode:
    """Test response formatting."""

    @pytest.mark.asyncio
    async def test_response_formatter_handles_unsupported_intent(self) -> None:
        """Return an MVP limitation message for unsupported intents."""
        result = await response_formatter_node({"intent": "insight"})
        assert "does not support insight type questions" in result["response"]

    @pytest.mark.asyncio
    async def test_response_formatter_preserves_root_cause_response(self) -> None:
        """Return RootCauseAgent output unchanged."""
        result = await response_formatter_node(
            {
                "intent": "root_cause",
                "response": "炉号 1丙20260110-1 被判为 C 级。",
                "entities": {"furnace_no": "1丙20260110-1"},
                "context": {"furnace_no": "1丙20260110-1", "grade": "C"},
            }
        )

        assert result["response"] == "炉号 1丙20260110-1 被判为 C 级。"
        assert result["intent"] == "root_cause"
        assert result["context"]["grade"] == "C"

    @pytest.mark.asyncio
    async def test_response_formatter_preserves_query_response(self) -> None:
        """Return query response and chart config unchanged."""
        result = await response_formatter_node(
            {
                "intent": "query",
                "response": "查询完成",
                "chart_config": {"type": "line"},
                "entities": {"metric": "psironloss"},
                "context": {"metric": "psironloss"},
            }
        )

        assert result["response"] == "查询完成"
        assert result["chart_config"] == {"type": "line"}
        assert result["entities"] == {"metric": "psironloss"}

    @pytest.mark.asyncio
    async def test_response_formatter_returns_default_message_when_empty(self) -> None:
        """Return the default processing message when no response exists."""
        result = await response_formatter_node({"intent": "query", "response": ""})
        assert "processing" in result["response"].lower()
