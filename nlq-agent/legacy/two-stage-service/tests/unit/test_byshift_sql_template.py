"""
S7: Unit tests — 合格率_班次 SQL template validation.

Verifies:
1. Template exists in METRIC_SQL_TEMPLATES
2. Generated SQL passes validate_sql.is_valid
3. SQL contains required structural elements (CASE WHEN HOUR, GROUP BY shift, ORDER BY)
4. time_window_months parameter is correctly substituted
"""

from __future__ import annotations

from src.models.ddl import METRIC_SQL_TEMPLATES
from src.services.database import DatabaseService


def _render_template(time_window_months: int = 1, extra_where: str = "") -> str:
    """Render the 合格率_班次 template with given params."""
    entry = METRIC_SQL_TEMPLATES["合格率_班次"]
    return entry["sql_template"].format(
        time_window_months=time_window_months,
        extra_where=extra_where,
    ).strip()


class TestByShiftSQLTemplate:
    """合格率_班次 template structure and safety tests."""

    def test_template_exists(self) -> None:
        """合格率_班次 key present in METRIC_SQL_TEMPLATES."""
        assert "合格率_班次" in METRIC_SQL_TEMPLATES
        entry = METRIC_SQL_TEMPLATES["合格率_班次"]
        assert entry["name"] == "合格率_班次"
        assert "sql_template" in entry

    def test_sql_valid_default_window(self) -> None:
        """Rendered SQL with default 1-month window passes validate_sql."""
        sql = _render_template(time_window_months=1)
        db = DatabaseService()
        is_valid, error = db.validate_sql(sql)
        assert is_valid, f"SQL validation failed: {error}"

    def test_sql_valid_custom_window(self) -> None:
        """Rendered SQL with 3-month window passes validate_sql."""
        sql = _render_template(time_window_months=3)
        db = DatabaseService()
        is_valid, error = db.validate_sql(sql)
        assert is_valid, f"SQL validation failed: {error}"

    def test_sql_contains_hour_case_when(self) -> None:
        """SQL contains CASE WHEN HOUR for shift mapping."""
        sql = _render_template()
        assert "CASE" in sql
        assert "WHEN HOUR(F_CREATORTIME)" in sql
        assert "早班" in sql
        assert "中班" in sql
        assert "晚班" in sql

    def test_sql_contains_group_by_shift(self) -> None:
        """SQL groups by shift alias."""
        sql = _render_template()
        assert "GROUP BY shift" in sql

    def test_sql_contains_order_by_shift(self) -> None:
        """SQL orders by shift."""
        sql = _render_template()
        assert "ORDER BY shift" in sql

    def test_sql_contains_qualified_rate(self) -> None:
        """SQL computes qualified_rate with proper CASE WHEN logic."""
        sql = _render_template()
        assert "qualified_rate" in sql
        assert "F_MAGNETIC_RES = '合格'" in sql
        assert "F_THICK_RES = '合格'" in sql
        assert "F_LAM_FACTOR_RES = '合格'" in sql

    def test_sql_uses_time_window_parameter(self) -> None:
        """SQL uses DATE_SUB with INTERVAL N MONTH."""
        sql = _render_template(time_window_months=6)
        assert "INTERVAL 6 MONTH" in sql

    def test_sql_contains_sample_count(self) -> None:
        """SQL outputs sample_count."""
        sql = _render_template()
        assert "sample_count" in sql

    def test_sql_shift_boundaries(self) -> None:
        """Shift hour boundaries: 早班 6-14, 中班 14-22, 晚班 ELSE."""
        sql = _render_template()
        assert ">= 6" in sql
        assert "< 14" in sql
        assert ">= 14" in sql
        assert "< 22" in sql
