"""
T7: Unit tests — 合格率_趋势 SQL template validation.

Verifies:
1. Template exists in METRIC_SQL_TEMPLATES
2. Generated SQL passes validate_sql.is_valid
3. SQL contains required structural elements (DATE_FORMAT, GROUP BY, ORDER BY)
4. time_window_months parameter is correctly substituted
"""

from __future__ import annotations

from src.models.ddl import METRIC_SQL_TEMPLATES
from src.services.database import DatabaseService


def _render_template(time_window_months: int = 6, extra_where: str = "") -> str:
    """Render the 合格率_趋势 template with given params."""
    entry = METRIC_SQL_TEMPLATES["合格率_趋势"]
    return entry["sql_template"].format(
        time_window_months=time_window_months,
        extra_where=extra_where,
    ).strip()


class TestTrendSQLTemplate:
    """合格率_趋势 template structure and safety tests."""

    def test_template_exists(self) -> None:
        """合格率_趋势 key present in METRIC_SQL_TEMPLATES."""
        assert "合格率_趋势" in METRIC_SQL_TEMPLATES
        entry = METRIC_SQL_TEMPLATES["合格率_趋势"]
        assert entry["name"] == "合格率_趋势"
        assert "sql_template" in entry

    def test_sql_valid_default_window(self) -> None:
        """Rendered SQL with default 6-month window passes validate_sql."""
        sql = _render_template(time_window_months=6)
        db = DatabaseService()
        is_valid, error = db.validate_sql(sql)
        assert is_valid, f"SQL validation failed: {error}"

    def test_sql_valid_custom_window(self) -> None:
        """Rendered SQL with 12-month window passes validate_sql."""
        sql = _render_template(time_window_months=12)
        db = DatabaseService()
        is_valid, error = db.validate_sql(sql)
        assert is_valid, f"SQL validation failed: {error}"

    def test_sql_contains_date_format(self) -> None:
        """SQL contains DATE_FORMAT for monthly bucketing."""
        sql = _render_template()
        assert "DATE_FORMAT" in sql
        assert "%Y-%m" in sql
        assert "month_bucket" in sql

    def test_sql_contains_order_by(self) -> None:
        """SQL contains ORDER BY month_bucket ASC."""
        sql = _render_template()
        assert "ORDER BY month_bucket ASC" in sql

    def test_sql_contains_group_by_spec_and_month(self) -> None:
        """SQL groups by F_PRODUCT_SPEC_ID and DATE_FORMAT."""
        sql = _render_template()
        assert "GROUP BY" in sql
        assert "F_PRODUCT_SPEC_ID" in sql
        assert "DATE_FORMAT(F_CREATORTIME" in sql

    def test_sql_contains_qualified_rate(self) -> None:
        """SQL computes qualified_rate with proper CASE WHEN logic."""
        sql = _render_template()
        assert "qualified_rate" in sql
        assert "F_MAGNETIC_RES = '合格'" in sql
        assert "F_THICK_RES = '合格'" in sql
        assert "F_LAM_FACTOR_RES = '合格'" in sql

    def test_sql_uses_time_window_parameter(self) -> None:
        """SQL uses DATE_SUB with INTERVAL N MONTH."""
        sql = _render_template(time_window_months=3)
        assert "INTERVAL 3 MONTH" in sql

    def test_sql_contains_sample_count(self) -> None:
        """SQL outputs sample_count."""
        sql = _render_template()
        assert "sample_count" in sql
