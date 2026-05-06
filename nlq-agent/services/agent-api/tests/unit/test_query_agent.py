"""QueryAgent unit tests aligned with the current implementation."""

from types import SimpleNamespace
from unittest.mock import AsyncMock, patch

import pytest
from langchain_core.messages import HumanMessage

from app.agents.query_agent import (
    _build_chart_config,
    _merge_entities_with_context,
    _build_time_range_sql,
    _detect_first_inspection_rate_query,
    _detect_judgment_inquiry,
    _format_time_range_desc,
    query_agent_node,
)


class TestHelpers:
    """Test QueryAgent helper functions."""

    @pytest.mark.parametrize(
        ("time_range", "expected_sql"),
        [
            (
                {"type": "recent_days", "days": 7},
                "F_DETECTION_DATE >= DATE_SUB(NOW(), INTERVAL 7 DAY)",
            ),
            (
                {"type": "last_month"},
                "F_DETECTION_DATE >= DATE_FORMAT(DATE_SUB(NOW(), INTERVAL 1 MONTH), '%Y-%m-01') AND F_DETECTION_DATE < DATE_FORMAT(NOW(), '%Y-%m-01')",
            ),
            (
                {"type": "year_month", "year": 2026, "month": 1},
                "F_DETECTION_DATE >= '2026-01-01' AND F_DETECTION_DATE < '2026-02-01'",
            ),
            ({}, None),
        ],
    )
    def test_build_time_range_sql(
        self, time_range: dict[str, object], expected_sql: str | None
    ) -> None:
        """Build SQL fragments from parsed time ranges."""
        assert _build_time_range_sql(time_range) == expected_sql

    @pytest.mark.parametrize(
        ("time_range", "expected_desc"),
        [
            ({"type": "recent_days", "days": 7}, "最近7天"),
            ({"type": "last_month"}, "上个月"),
            ({"type": "current_month"}, "本月"),
            ({"type": "year_month", "year": 2026, "month": 3}, "2026年3月"),
            ({}, "最近7天"),
        ],
    )
    def test_format_time_range_desc(
        self, time_range: dict[str, object], expected_desc: str
    ) -> None:
        """Format time ranges for natural-language responses."""
        assert _format_time_range_desc(time_range) == expected_desc

    def test_build_chart_config_for_line_chart(self) -> None:
        """Build line chart config for trend queries."""
        chart_config = _build_chart_config(
            values=[
                {"date": "2026-03-01", "value": 1.23},
                {"date": "2026-03-02", "value": 1.25},
            ],
            metric_name="Ps铁损",
            unit="W/kg",
            aggregation="AVG",
            chart_type="line",
        )

        assert chart_config["type"] == "line"
        assert chart_config["title"] == "Ps铁损 趋势图"
        assert chart_config["data"][0]["date"] == "2026-03-01"
        assert chart_config["yField"] == "value"

    def test_build_chart_config_for_bar_chart(self) -> None:
        """Build bar chart config for distribution queries."""
        chart_config = _build_chart_config(
            values=[
                {"date": "2026-03-01", "value": 1.23},
                {"date": "2026-03-02", "value": 1.25},
            ],
            metric_name="Ps铁损",
            unit="W/kg",
            aggregation="AVG",
            chart_type="bar",
        )

        assert chart_config["type"] == "bar"
        assert chart_config["title"] == "Ps铁损 分布图"
        assert chart_config["xField"] == "date"

    def test_build_chart_config_for_pie_chart(self) -> None:
        """Build pie chart config for composition queries."""
        chart_config = _build_chart_config(
            values=[
                {"date": "A", "value": 10},
                {"date": "B", "value": 30},
            ],
            metric_name="Ps铁损",
            unit="W/kg",
            aggregation="AVG",
            chart_type="pie",
        )

        assert chart_config["type"] == "pie"
        assert chart_config["title"] == "Ps铁损 占比图"
        assert chart_config["angleField"] == "value"
        assert chart_config["colorField"] == "date"

    def test_merge_entities_with_context_prefers_new_values(self) -> None:
        """Reuse context for follow-up turns while preserving newly parsed entities."""
        merged = _merge_entities_with_context(
            entities={"time_range": {"type": "current_month"}},
            context={
                "metric": "psironloss",
                "spec_code": "120",
                "time_range": {"type": "last_month"},
                "aggregation": "AVG",
            },
        )

        assert merged["metric"] == "psironloss"
        assert merged["spec_code"] == "120"
        assert merged["aggregation"] == "AVG"
        assert merged["time_range"] == {"type": "current_month"}

    @pytest.mark.parametrize(
        ("question", "expected"),
        [
            ("贴标判定规则有哪些", "Labeling"),
            ("磁性能判定规格是什么", "MagneticResult"),
            ("这个月的Ps铁损平均值", None),
        ],
    )
    def test_detect_judgment_inquiry(self, question: str, expected: str | None) -> None:
        """Detect judgment-rule questions from user input."""
        assert _detect_judgment_inquiry(question) == expected

    @pytest.mark.parametrize(
        ("question", "expected"),
        [
            ("一次交检合格率是多少", True),
            ("first inspection pass rate", True),
            ("最近7天 Ps铁损 平均值", False),
        ],
    )
    def test_detect_first_inspection_rate_query(self, question: str, expected: bool) -> None:
        """Detect first-inspection-pass-rate questions from user input."""
        assert _detect_first_inspection_rate_query(question) is expected


class TestQueryAgentNode:
    """Test QueryAgent node behavior."""

    @pytest.mark.asyncio
    async def test_query_agent_node_requests_metric_clarification(self) -> None:
        """Ask for clarification when no metric can be identified."""
        llm = SimpleNamespace(
            ainvoke=AsyncMock(return_value=SimpleNamespace(content="请告诉我您要查询的指标。"))
        )

        with patch("app.agents.query_agent.get_llm", return_value=llm) as mock_get_llm:
            result = await query_agent_node(
                {
                    "messages": [HumanMessage(content="帮我查一下")],
                    "session_id": "session-1",
                    "model_name": "qwen2.5-72b",
                    "entities": {},
                    "context": {},
                }
            )

        assert result["response"] == "请告诉我您要查询的指标。"
        assert result["chart_config"] is None
        assert result["intent"] == "query"
        mock_get_llm.assert_called_once_with("qwen2.5-72b")

    @pytest.mark.asyncio
    async def test_query_agent_node_returns_value_response(self) -> None:
        """Run the metric query flow and return the LLM summary."""
        llm = SimpleNamespace(
            ainvoke=AsyncMock(
                return_value=SimpleNamespace(content="2026年1月甲班 Ps铁损平均值为 1.23 W/kg。")
            )
        )

        query_metric = AsyncMock(return_value={"value": 1.23, "count": 12})

        with (
            patch("app.agents.query_agent.get_llm", return_value=llm),
            patch(
                "app.agents.query_agent.get_formula_definition_tool",
                new=SimpleNamespace(
                    ainvoke=AsyncMock(
                        return_value={
                            "found": True,
                            "id": 7,
                            "column_name": "PerfPsLoss",
                            "formula": "铁损计算公式",
                            "unit": "W/kg",
                        }
                    )
                ),
            ),
            patch(
                "app.agents.query_agent.query_metric_tool",
                new=SimpleNamespace(ainvoke=query_metric),
            ),
            patch(
                "app.agents.query_agent.get_grade_rules_tool",
                new=SimpleNamespace(
                    ainvoke=AsyncMock(
                        return_value={
                            "found": True,
                            "grade": "A",
                            "quality_status": "合格",
                        }
                    )
                ),
            ),
        ):
            result = await query_agent_node(
                {
                    "messages": [HumanMessage(content="2026年1月甲班的Ps铁损平均值是多少？")],
                    "session_id": "session-2",
                    "entities": {
                        "metric": "psironloss",
                        "shift": "A",
                        "aggregation": "AVG",
                        "time_range": {"type": "year_month", "year": 2026, "month": 1},
                    },
                    "context": {},
                }
            )

        assert result["response"] == "2026年1月甲班 Ps铁损平均值为 1.23 W/kg。"
        assert result["chart_config"] is None
        assert result["calculation_explanation"] == {
            "formula_source": "铁损计算公式",
            "data_fields": ["F_PERF_PS_LOSS"],
            "natural_language": "Ps铁损使用铁损计算公式，基于字段 F_PERF_PS_LOSS 按 AVG 计算。",
        }
        assert result["grade_judgment"] == {
            "available": True,
            "grade": "A",
            "quality_status": "合格",
            "color": None,
            "metric_value": 1.23,
            "matched_rule": None,
            "all_rules": [],
            "summary": "当前结果判定为 A（合格）。",
        }
        assert query_metric.await_count == 1
        assert query_metric.await_args_list[0].args[0]["column_name"] == "F_PERF_PS_LOSS"
        assert query_metric.await_args_list[0].args[0]["group_by_date"] is False

    @pytest.mark.asyncio
    async def test_query_agent_node_returns_trend_chart(self) -> None:
        """Build chart config for trend queries after value summary generation."""
        llm = SimpleNamespace(
            ainvoke=AsyncMock(return_value=SimpleNamespace(content="最近7天叠片系数趋势如下。"))
        )
        query_metric = AsyncMock(
            side_effect=[
                {"value": 0.98, "count": 7},
                {
                    "values": [
                        {"date": "2026-03-01", "value": 0.97},
                        {"date": "2026-03-02", "value": 0.98},
                    ]
                },
            ]
        )

        with (
            patch("app.agents.query_agent.get_llm", return_value=llm),
            patch(
                "app.agents.query_agent.get_formula_definition_tool",
                new=SimpleNamespace(
                    ainvoke=AsyncMock(
                        return_value={
                            "found": True,
                            "id": 9,
                            "column_name": "LaminationFactor",
                            "formula": "叠片系数公式",
                            "unit": "%",
                        }
                    )
                ),
            ),
            patch(
                "app.agents.query_agent.query_metric_tool",
                new=SimpleNamespace(ainvoke=query_metric),
            ),
            patch(
                "app.agents.query_agent.get_grade_rules_tool",
                new=SimpleNamespace(ainvoke=AsyncMock(return_value={"found": False})),
            ),
        ):
            result = await query_agent_node(
                {
                    "messages": [HumanMessage(content="最近7天叠片系数趋势")],
                    "session_id": "session-3",
                    "entities": {
                        "metric": "laminationfactor",
                        "aggregation": "AVG",
                        "query_type": "trend",
                        "time_range": {"type": "recent_days", "days": 7},
                    },
                    "context": {},
                }
            )

        assert result["response"] == "最近7天叠片系数趋势如下。"
        assert result["chart_config"] is not None
        assert result["chart_config"]["type"] == "line"
        assert query_metric.await_count == 2
        assert query_metric.await_args_list[1].args[0]["group_by_date"] is True

    @pytest.mark.asyncio
    async def test_query_agent_node_selects_bar_chart_for_distribution_query(self) -> None:
        """Return bar chart config for distribution-style queries."""
        llm = SimpleNamespace(
            ainvoke=AsyncMock(return_value=SimpleNamespace(content="上个月 Ps铁损分布如下。"))
        )
        query_metric = AsyncMock(
            side_effect=[
                {"value": 1.15, "count": 2},
                {
                    "values": [
                        {"date": "2026-02-01", "value": 1.10},
                        {"date": "2026-02-02", "value": 1.20},
                    ]
                },
            ]
        )

        with (
            patch("app.agents.query_agent.get_llm", return_value=llm),
            patch(
                "app.agents.query_agent.get_formula_definition_tool",
                new=SimpleNamespace(
                    ainvoke=AsyncMock(
                        return_value={
                            "found": True,
                            "id": 7,
                            "column_name": "PerfPsLoss",
                            "formula": "铁损计算公式",
                            "unit": "W/kg",
                        }
                    )
                ),
            ),
            patch(
                "app.agents.query_agent.query_metric_tool",
                new=SimpleNamespace(ainvoke=query_metric),
            ),
            patch(
                "app.agents.query_agent.get_grade_rules_tool",
                new=SimpleNamespace(ainvoke=AsyncMock(return_value={"found": False})),
            ),
        ):
            result = await query_agent_node(
                {
                    "messages": [HumanMessage(content="上个月 Ps铁损分布")],
                    "session_id": "session-distribution",
                    "entities": {
                        "metric": "psironloss",
                        "aggregation": "AVG",
                        "query_type": "distribution",
                        "time_range": {"type": "last_month"},
                    },
                    "context": {},
                }
            )

        assert result["chart_config"] is not None
        assert result["chart_config"]["type"] == "bar"

    @pytest.mark.asyncio
    async def test_query_agent_node_reuses_context_for_follow_up_query(self) -> None:
        """Resolve missing metric/spec from stored context on follow-up turns."""
        llm = SimpleNamespace(
            ainvoke=AsyncMock(
                return_value=SimpleNamespace(content="本月 Ps铁损平均值为 1.18 W/kg。")
            )
        )

        with (
            patch("app.agents.query_agent.get_llm", return_value=llm),
            patch(
                "app.agents.query_agent.get_formula_definition_tool",
                new=SimpleNamespace(
                    ainvoke=AsyncMock(
                        return_value={
                            "found": True,
                            "id": 7,
                            "column_name": "PerfPsLoss",
                            "formula": "铁损计算公式",
                            "unit": "W/kg",
                        }
                    )
                ),
            ),
            patch(
                "app.agents.query_agent.query_metric_tool",
                new=SimpleNamespace(ainvoke=AsyncMock(return_value={"value": 1.18, "count": 8})),
            ),
            patch(
                "app.agents.query_agent.get_grade_rules_tool",
                new=SimpleNamespace(ainvoke=AsyncMock(return_value={"found": False})),
            ),
        ):
            result = await query_agent_node(
                {
                    "messages": [HumanMessage(content="那这个月呢？")],
                    "session_id": "session-follow-up",
                    "entities": {
                        "time_range": {"type": "current_month"},
                    },
                    "context": {
                        "metric": "psironloss",
                        "spec_code": "120",
                        "aggregation": "AVG",
                        "time_range": {"type": "last_month"},
                    },
                }
            )

        assert result["entities"]["metric"] == "psironloss"
        assert result["entities"]["spec_code"] == "120"
        assert result["entities"]["time_range"] == {"type": "current_month"}
        assert result["context"]["metric"] == "psironloss"
        assert result["context"]["spec_code"] == "120"

    @pytest.mark.asyncio
    async def test_query_agent_node_handles_first_inspection_query(self) -> None:
        """Route first-inspection pass-rate queries without raising context errors."""
        query_first_inspection_rate = AsyncMock(
            return_value={
                "pass_rate": 96.5,
                "pass_count": 193,
                "total_count": 200,
                "pass_grades": ["A", "B"],
            }
        )

        with patch(
            "app.agents.query_agent.query_first_inspection_rate_tool",
            new=SimpleNamespace(ainvoke=query_first_inspection_rate),
        ):
            result = await query_agent_node(
                {
                    "messages": [HumanMessage(content="120规格最近7天一次交检合格率是多少？")],
                    "session_id": "session-4",
                    "entities": {
                        "spec_code": "120",
                        "time_range": {"type": "recent_days", "days": 7},
                    },
                    "context": {"source": "follow-up"},
                }
            )

        assert "96.5%" in result["response"]
        assert result["entities"]["query_type"] == "first_inspection_rate"
        assert result["context"] == {"source": "follow-up"}
