"""
R7: Unit tests — 合格率_归因 SQL template validation.

Verifies:
1. Template exists in METRIC_SQL_TEMPLATES
2. Generated SQL passes validate_sql.is_valid
3. SQL contains required structural elements (GROUP BY, delta_from_overall, LIMIT 5)
4. dimension parameter is correctly substituted
"""

from __future__ import annotations

from src.models.ddl import METRIC_SQL_TEMPLATES
from src.services.database import DatabaseService


def _render_template(dimension: str = "F_PRODUCT_SPEC_CODE", time_window_months: int = 1, extra_where: str = "") -> str:
    """Render the 合格率_归因 template with given params."""
    entry = METRIC_SQL_TEMPLATES["合格率_归因"]
    return entry["sql_template"].format(
        dimension=dimension,
        time_window_months=time_window_months,
        extra_where=extra_where,
    ).strip()


class TestRootCauseSQLTemplate:
    """合格率_归因 template structure and safety tests."""

    def test_template_exists(self) -> None:
        """合格率_归因 key present in METRIC_SQL_TEMPLATES."""
        assert "合格率_归因" in METRIC_SQL_TEMPLATES
        entry = METRIC_SQL_TEMPLATES["合格率_归因"]
        assert entry["name"] == "合格率_归因"
        assert "sql_template" in entry

    def test_sql_valid_default_dimension(self) -> None:
        """Rendered SQL with default dimension passes validate_sql."""
        sql = _render_template(dimension="F_PRODUCT_SPEC_CODE")
        db = DatabaseService()
        is_valid, error = db.validate_sql(sql)
        assert is_valid, f"SQL validation failed: {error}"

    def test_sql_valid_creator_dimension(self) -> None:
        """Rendered SQL with F_CREATORUSERID dimension passes validate_sql."""
        sql = _render_template(dimension="F_CREATORUSERID")
        db = DatabaseService()
        is_valid, error = db.validate_sql(sql)
        assert is_valid, f"SQL validation failed: {error}"

    def test_sql_contains_group_by(self) -> None:
        """SQL contains GROUP BY with the dimension column."""
        sql = _render_template(dimension="F_PRODUCT_SPEC_CODE")
        assert "GROUP BY" in sql
        assert "t.F_PRODUCT_SPEC_CODE" in sql

    def test_sql_contains_delta_from_overall(self) -> None:
        """SQL computes delta_from_overall against overall rate subquery."""
        sql = _render_template()
        assert "delta_from_overall" in sql
        assert "FROM LAB_INTERMEDIATE_DATA" in sql

    def test_sql_contains_order_by_delta(self) -> None:
        """SQL contains ORDER BY delta_from_overall ASC."""
        sql = _render_template()
        assert "ORDER BY delta_from_overall ASC" in sql

    def test_sql_contains_limit_5(self) -> None:
        """SQL limits results to 5 rows."""
        sql = _render_template()
        assert "LIMIT 5" in sql

    def test_sql_contains_qualified_rate(self) -> None:
        """SQL computes qualified_rate with proper CASE WHEN logic."""
        sql = _render_template()
        assert "qualified_rate" in sql
        assert "F_MAGNETIC_RES = '合格'" in sql
        assert "F_THICK_RES = '合格'" in sql
        assert "F_LAM_FACTOR_RES = '合格'" in sql

    def test_sql_contains_sample_count(self) -> None:
        """SQL outputs sample_count."""
        sql = _render_template()
        assert "sample_count" in sql

    def test_sql_uses_time_window_parameter(self) -> None:
        """SQL uses DATE_SUB with INTERVAL N MONTH."""
        sql = _render_template(time_window_months=3)
        assert "INTERVAL 3 MONTH" in sql

    def test_sql_contains_overall_subquery(self) -> None:
        """SQL uses correlated subquery for overall rate calculation."""
        sql = _render_template()
        assert "SELECT ROUND(" in sql
        assert "FROM LAB_INTERMEDIATE_DATA" in sql
