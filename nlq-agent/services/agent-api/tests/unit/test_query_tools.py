"""Query tools module unit tests."""

from unittest.mock import AsyncMock, patch

import pytest

from app.tools.query_tools import (
    _check_conditions,
    get_formula_definition_tool,
    get_grade_rules_tool,
    query_metric_tool,
)


class TestGetFormulaDefinitionTool:
    """Test get_formula_definition_tool."""

    @pytest.mark.asyncio
    async def test_get_formula_found(self) -> None:
        """Test successfully getting formula definition."""
        mock_result = [
            {
                "id": 1,
                "name": "PsIronLoss",
                "column_name": "PerfPsLoss",
                "formula": "P15/50",
                "unit": "W/kg",
                "formula_type": "CALC",
                "description": "Iron loss value",
            }
        ]

        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = mock_result

            result = await get_formula_definition_tool.ainvoke({"metric_name": "PsIronLoss"})

            assert result["found"] is True
            assert result["name"] == "PsIronLoss"
            assert result["column_name"] == "PerfPsLoss"
            assert result["unit"] == "W/kg"

    @pytest.mark.asyncio
    async def test_get_formula_not_found(self) -> None:
        """Test when metric is not found."""
        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.side_effect = [
                [],  # First exact query returns nothing
                [],  # Fuzzy match also returns nothing
                [{"name": "PsIronLoss"}, {"name": "LaminationFactor"}],  # Suggestions
            ]

            result = await get_formula_definition_tool.ainvoke({"metric_name": "NonExistentMetric"})

            assert result["found"] is False
            assert "error" in result
            assert "suggestions" in result

    @pytest.mark.asyncio
    async def test_get_formula_fuzzy_match(self) -> None:
        """Test fuzzy matching metric name returns not found with suggestions."""
        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.side_effect = [
                [],  # Exact query returns nothing
                [
                    {
                        "id": 2,
                        "name": "LaminationFactor",
                        "column_name": "LaminationFactor",
                        "formula": "",
                        "unit": "",
                        "formula_type": "CALC",
                        "description": "",
                    }
                ],  # Fuzzy match results
                [{"name": "LaminationFactor"}],  # Suggestions query
            ]

            result = await get_formula_definition_tool.ainvoke({"metric_name": "Lamina"})

            assert result["found"] is False  # Fuzzy match returns not found
            assert "suggestions" in result

    @pytest.mark.asyncio
    async def test_get_formula_by_column_name(self) -> None:
        """Test querying by column name."""
        mock_result = [
            {
                "id": 1,
                "name": "PsIronLoss",
                "column_name": "PerfPsLoss",
                "formula": "P15/50",
                "unit": "W/kg",
                "formula_type": "CALC",
                "description": "Iron loss value",
            }
        ]

        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = mock_result

            result = await get_formula_definition_tool.ainvoke({"metric_name": "PerfPsLoss"})

            assert result["found"] is True
            assert result["column_name"] == "PerfPsLoss"


class TestQueryMetricTool:
    """Test query_metric_tool."""

    @pytest.mark.asyncio
    async def test_query_avg_single_value(self) -> None:
        """Test querying average (single value)."""
        mock_result = [{"value": 1.23, "count": 100}]

        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = mock_result

            result = await query_metric_tool.ainvoke(
                {
                    "column_name": "PerfPsLoss",
                    "aggregation": "AVG",
                    "time_range_sql": "DetectionDate >= DATE_SUB(NOW(), INTERVAL 7 DAY)",
                }
            )

            assert result["value"] == 1.23
            assert result["count"] == 100
            assert result["column_name"] == "PerfPsLoss"
            assert result["aggregation"] == "AVG"

    @pytest.mark.asyncio
    async def test_query_with_shift_filter(self) -> None:
        """Test query with shift filter."""
        mock_result = [{"value": 1.45, "count": 30}]

        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = mock_result

            result = await query_metric_tool.ainvoke(
                {
                    "column_name": "PerfPsLoss",
                    "aggregation": "AVG",
                    "time_range_sql": "DetectionDate >= DATE_SUB(NOW(), INTERVAL 7 DAY)",
                    "shift": "A",
                }
            )

            assert result["value"] == 1.45
            # Verify SQL contains shift parameter
            # execute_safe_sql is called with positional args: (sql, params)
            call_args = mock_execute.call_args
            args = call_args.args if call_args.args else call_args[0]
            params = args[1] if len(args) > 1 else {}
            assert "shift" in params
            assert params["shift"] == "A"

    @pytest.mark.asyncio
    async def test_query_trend_group_by_date(self) -> None:
        """Test trend query (group by date)."""
        mock_result = [
            {"date": "2026-01-01", "value": 1.20, "count": 10},
            {"date": "2026-01-02", "value": 1.25, "count": 12},
            {"date": "2026-01-03", "value": 1.22, "count": 8},
        ]

        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = mock_result

            result = await query_metric_tool.ainvoke(
                {
                    "column_name": "LaminationFactor",
                    "aggregation": "AVG",
                    "time_range_sql": "DetectionDate >= '2026-01-01'",
                    "group_by_date": True,
                }
            )

            assert "values" in result
            assert len(result["values"]) == 3
            assert result["column_name"] == "LaminationFactor"
            assert result["aggregation"] == "AVG"

    @pytest.mark.asyncio
    async def test_query_no_data(self) -> None:
        """Test when no data is returned."""
        mock_result = [{"value": None, "count": 0}]

        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = mock_result

            result = await query_metric_tool.ainvoke(
                {
                    "column_name": "PerfPsLoss",
                    "aggregation": "AVG",
                    "time_range_sql": "DetectionDate >= '2099-01-01'",
                }
            )

            assert result["value"] is None
            assert result["count"] == 0
            assert "message" in result

    @pytest.mark.asyncio
    async def test_query_invalid_aggregation(self) -> None:
        """Test invalid aggregation function is rejected."""
        result = await query_metric_tool.ainvoke(
            {
                "column_name": "PerfPsLoss",
                "aggregation": "INVALID",
                "time_range_sql": "DetectionDate >= NOW()",
            }
        )

        assert "error" in result
        assert "unsupported aggregation" in result["error"].lower()

    @pytest.mark.asyncio
    async def test_query_invalid_column_name(self) -> None:
        """Test invalid column name is rejected."""
        result = await query_metric_tool.ainvoke(
            {
                "column_name": "1invalid; DROP TABLE",
                "aggregation": "AVG",
                "time_range_sql": "DetectionDate >= NOW()",
            }
        )

        assert "error" in result
        assert "invalid" in result["error"].lower()

    @pytest.mark.asyncio
    async def test_query_invalid_time_range_sql(self) -> None:
        """Test invalid time range SQL is rejected."""
        result = await query_metric_tool.ainvoke(
            {
                "column_name": "PerfPsLoss",
                "aggregation": "AVG",
                "time_range_sql": "1=1; DROP TABLE",
            }
        )

        assert "error" in result
        assert "invalid" in result["error"].lower()

    @pytest.mark.asyncio
    async def test_query_all_aggregations(self) -> None:
        """Test all supported aggregation functions."""
        aggregations = ["AVG", "MAX", "MIN", "SUM", "COUNT"]

        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = [{"value": 1.0, "count": 100}]

            for agg in aggregations:
                result = await query_metric_tool.ainvoke(
                    {
                        "column_name": "PerfPsLoss",
                        "aggregation": agg,
                        "time_range_sql": "DetectionDate >= NOW()",
                    }
                )
                assert "error" not in result or "unsupported" not in result.get("error", "").lower()

    @pytest.mark.asyncio
    async def test_query_execution_error(self) -> None:
        """Test execution error handling."""
        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.side_effect = Exception("Database connection failed")

            result = await query_metric_tool.ainvoke(
                {
                    "column_name": "PerfPsLoss",
                    "aggregation": "AVG",
                    "time_range_sql": "DetectionDate >= NOW()",
                }
            )

            assert "error" in result


class TestGetGradeRulesTool:
    """Test get_grade_rules_tool."""

    @pytest.mark.asyncio
    async def test_get_rules_and_grade_found(self) -> None:
        """Test successfully getting rules and grading."""
        mock_rules = [
            {
                "id": 1,
                "formula_id": 1,
                "name": "Premium",
                "priority": 3,
                "quality_status": "Pass",
                "color": "#00FF00",
                "is_default": False,
                "condition_json": '[{"operator": "<=", "value": 1.5}]',
            },
            {
                "id": 2,
                "formula_id": 1,
                "name": "GradeA",
                "priority": 2,
                "quality_status": "Pass",
                "color": "#FFFF00",
                "is_default": False,
                "condition_json": '[{"operator": "<=", "value": 2.0}]',
            },
            {
                "id": 3,
                "formula_id": 1,
                "name": "Pass",
                "priority": 1,
                "quality_status": "Pass",
                "color": "#FFA500",
                "is_default": True,
                "condition_json": None,
            },
        ]

        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = mock_rules

            result = await get_grade_rules_tool.ainvoke(
                {
                    "formula_id": 1,
                    "metric_value": 1.23,
                }
            )

            assert result["found"] is True
            assert result["grade"] == "Premium"
            assert result["metric_value"] == 1.23
            assert len(result["all_rules"]) == 3

    @pytest.mark.asyncio
    async def test_grade_with_multiple_conditions(self) -> None:
        """Test multi-condition grading."""
        mock_rules = [
            {
                "id": 1,
                "formula_id": 1,
                "name": "Premium",
                "priority": 3,
                "quality_status": "Pass",
                "color": "#00FF00",
                "is_default": False,
                "condition_json": (
                    '[{"operator": ">=", "value": 0.5}, {"operator": "<=", "value": 1.5}]'
                ),
            },
        ]

        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = mock_rules

            result = await get_grade_rules_tool.ainvoke(
                {
                    "formula_id": 1,
                    "metric_value": 1.23,
                }
            )

            assert result["found"] is True
            assert result["grade"] == "Premium"

    @pytest.mark.asyncio
    async def test_grade_not_meet_conditions(self) -> None:
        """Test using default grade when conditions not met."""
        mock_rules = [
            {
                "id": 1,
                "formula_id": 1,
                "name": "Premium",
                "priority": 3,
                "quality_status": "Pass",
                "color": "#00FF00",
                "is_default": False,
                "condition_json": '[{"operator": "<=", "value": 1.0}]',
            },
            {
                "id": 2,
                "formula_id": 1,
                "name": "Pass",
                "priority": 1,
                "quality_status": "Pass",
                "color": "#FFA500",
                "is_default": True,
                "condition_json": None,
            },
        ]

        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = mock_rules

            result = await get_grade_rules_tool.ainvoke(
                {
                    "formula_id": 1,
                    "metric_value": 1.5,  # Greater than 1.0, doesn't meet Premium condition
                }
            )

            assert result["found"] is True
            assert result["grade"] == "Pass"

    @pytest.mark.asyncio
    async def test_no_rules_found(self) -> None:
        """Test error when no rules found."""
        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = []

            result = await get_grade_rules_tool.ainvoke(
                {
                    "formula_id": 999,
                    "metric_value": 1.23,
                }
            )

            assert result["found"] is False
            assert "error" in result

    @pytest.mark.asyncio
    async def test_invalid_condition_json(self) -> None:
        """Test invalid condition JSON handling."""
        mock_rules = [
            {
                "id": 1,
                "formula_id": 1,
                "name": "Premium",
                "priority": 3,
                "quality_status": "Pass",
                "color": "#00FF00",
                "is_default": True,
                "condition_json": "invalid json",
            },
        ]

        with patch(
            "app.tools.query_tools.execute_safe_sql", new_callable=AsyncMock
        ) as mock_execute:
            mock_execute.return_value = mock_rules

            result = await get_grade_rules_tool.ainvoke(
                {
                    "formula_id": 1,
                    "metric_value": 1.23,
                }
            )

            assert result["found"] is True
            assert result["grade"] == "Premium"
            assert result["matched_rule"]["conditions"] == []


class TestCheckConditions:
    """Test condition checking function."""

    def test_less_than_or_equal(self) -> None:
        """Test <= condition."""
        assert _check_conditions(1.0, [{"operator": "<=", "value": 1.5}]) is True
        assert _check_conditions(1.5, [{"operator": "<=", "value": 1.5}]) is True
        assert _check_conditions(2.0, [{"operator": "<=", "value": 1.5}]) is False

    def test_less_than(self) -> None:
        """Test < condition."""
        assert _check_conditions(1.0, [{"operator": "<", "value": 1.5}]) is True
        assert _check_conditions(1.5, [{"operator": "<", "value": 1.5}]) is False

    def test_greater_than_or_equal(self) -> None:
        """Test >= condition."""
        assert _check_conditions(1.5, [{"operator": ">=", "value": 1.5}]) is True
        assert _check_conditions(1.0, [{"operator": ">=", "value": 1.5}]) is False

    def test_greater_than(self) -> None:
        """Test > condition."""
        assert _check_conditions(2.0, [{"operator": ">", "value": 1.5}]) is True
        assert _check_conditions(1.5, [{"operator": ">", "value": 1.5}]) is False

    def test_equal(self) -> None:
        """Test == condition."""
        assert _check_conditions(1.5, [{"operator": "==", "value": 1.5}]) is True
        assert _check_conditions(1.0, [{"operator": "==", "value": 1.5}]) is False

    def test_empty_conditions(self) -> None:
        """Test empty condition list."""
        assert _check_conditions(1.0, []) is True
        assert _check_conditions(1.0, None) is True  # type: ignore[arg-type]

    def test_multiple_conditions_all_must_pass(self) -> None:
        """Test multiple conditions must all be satisfied."""
        conditions = [
            {"operator": ">=", "value": 0.5},
            {"operator": "<=", "value": 1.5},
        ]
        assert _check_conditions(1.0, conditions) is True
        assert _check_conditions(0.3, conditions) is False
        assert _check_conditions(2.0, conditions) is False

    def test_invalid_operator(self) -> None:
        """Test invalid operator handling."""
        # Unknown operator should be treated as satisfied
        assert _check_conditions(1.0, [{"operator": "invalid", "value": 1.5}]) is True

    def test_none_value_in_condition(self) -> None:
        """Test condition value is None."""
        assert _check_conditions(1.0, [{"operator": "<=", "value": None}]) is True
