"""Unit tests for MysqlRunner.dump_ddl.

Mocks pymysql/sqlalchemy cursor to test CREATE TABLE string structure:
  - Contains field names
  - Contains column types
  - Contains column comments
  - Contains table comment
  - Returns a multi-line string starting with CREATE TABLE
"""

from __future__ import annotations

from unittest.mock import MagicMock, patch, PropertyMock

import pandas as pd
import pytest

from app.adapters.mysql_runner import MysqlRunner


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def _make_runner() -> MysqlRunner:
    """Create a MysqlRunner with a mocked SQLAlchemy engine."""
    with patch("app.adapters.mysql_runner.create_engine") as mock_create_engine:
        mock_engine = MagicMock()
        mock_create_engine.return_value = mock_engine
        runner = MysqlRunner(
            config={
                "mysql_host": "localhost",
                "mysql_port": 3306,
                "mysql_user": "root",
                "mysql_password": "password",
                "mysql_db": "lumei",
            }
        )
        runner._engine = mock_engine
        return runner


def _make_cols_df() -> pd.DataFrame:
    """Return a sample columns DataFrame matching information_schema.COLUMNS."""
    return pd.DataFrame(
        {
            "COLUMN_NAME": ["F_Id", "iron_loss", "spec", "grade"],
            "COLUMN_TYPE": ["varchar(36)", "float", "varchar(50)", "varchar(10)"],
            "IS_NULLABLE": ["NO", "YES", "YES", "YES"],
            "COLUMN_DEFAULT": [None, None, None, None],
            "COLUMN_COMMENT": ["主键", "铁损值", "规格型号", "等级"],
        }
    )


def _make_tbl_df() -> pd.DataFrame:
    """Return a sample table DataFrame matching information_schema.TABLES."""
    return pd.DataFrame({"TABLE_COMMENT": ["检测中间数据表"]})


# ---------------------------------------------------------------------------
# Tests
# ---------------------------------------------------------------------------


class TestDumpDdl:
    def setup_method(self):
        self.runner = _make_runner()

    def _patch_read_sql(self, cols_df: pd.DataFrame, tbl_df: pd.DataFrame):
        """Context manager that patches pd.read_sql for both queries."""
        import unittest.mock as mock

        call_count = {"n": 0}

        def fake_read_sql(query, conn, params=None):
            call_count["n"] += 1
            # First call is columns, second is table comment
            if call_count["n"] == 1:
                return cols_df
            return tbl_df

        return mock.patch("pandas.read_sql", side_effect=fake_read_sql)

    def test_output_starts_with_create_table(self):
        with self._patch_read_sql(_make_cols_df(), _make_tbl_df()):
            with patch.object(self.runner._engine, "connect") as mock_connect:
                mock_conn = MagicMock()
                mock_connect.return_value.__enter__ = lambda s: mock_conn
                mock_connect.return_value.__exit__ = MagicMock(return_value=False)

                result = self.runner.dump_ddl("lumei", "LAB_INTERMEDIATE_DATA")

        assert result.startswith("CREATE TABLE")

    def test_output_contains_table_name(self):
        with self._patch_read_sql(_make_cols_df(), _make_tbl_df()):
            with patch.object(self.runner._engine, "connect") as mock_connect:
                mock_conn = MagicMock()
                mock_connect.return_value.__enter__ = lambda s: mock_conn
                mock_connect.return_value.__exit__ = MagicMock(return_value=False)

                result = self.runner.dump_ddl("lumei", "LAB_INTERMEDIATE_DATA")

        assert "LAB_INTERMEDIATE_DATA" in result

    def test_output_contains_field_names(self):
        with self._patch_read_sql(_make_cols_df(), _make_tbl_df()):
            with patch.object(self.runner._engine, "connect") as mock_connect:
                mock_conn = MagicMock()
                mock_connect.return_value.__enter__ = lambda s: mock_conn
                mock_connect.return_value.__exit__ = MagicMock(return_value=False)

                result = self.runner.dump_ddl("lumei", "LAB_INTERMEDIATE_DATA")

        assert "F_Id" in result
        assert "iron_loss" in result
        assert "spec" in result
        assert "grade" in result

    def test_output_contains_column_types(self):
        with self._patch_read_sql(_make_cols_df(), _make_tbl_df()):
            with patch.object(self.runner._engine, "connect") as mock_connect:
                mock_conn = MagicMock()
                mock_connect.return_value.__enter__ = lambda s: mock_conn
                mock_connect.return_value.__exit__ = MagicMock(return_value=False)

                result = self.runner.dump_ddl("lumei", "LAB_INTERMEDIATE_DATA")

        assert "float" in result
        assert "varchar" in result

    def test_output_contains_column_comments(self):
        with self._patch_read_sql(_make_cols_df(), _make_tbl_df()):
            with patch.object(self.runner._engine, "connect") as mock_connect:
                mock_conn = MagicMock()
                mock_connect.return_value.__enter__ = lambda s: mock_conn
                mock_connect.return_value.__exit__ = MagicMock(return_value=False)

                result = self.runner.dump_ddl("lumei", "LAB_INTERMEDIATE_DATA")

        assert "铁损值" in result
        assert "规格型号" in result

    def test_output_contains_table_comment(self):
        with self._patch_read_sql(_make_cols_df(), _make_tbl_df()):
            with patch.object(self.runner._engine, "connect") as mock_connect:
                mock_conn = MagicMock()
                mock_connect.return_value.__enter__ = lambda s: mock_conn
                mock_connect.return_value.__exit__ = MagicMock(return_value=False)

                result = self.runner.dump_ddl("lumei", "LAB_INTERMEDIATE_DATA")

        assert "检测中间数据表" in result

    def test_output_ends_with_semicolon(self):
        with self._patch_read_sql(_make_cols_df(), _make_tbl_df()):
            with patch.object(self.runner._engine, "connect") as mock_connect:
                mock_conn = MagicMock()
                mock_connect.return_value.__enter__ = lambda s: mock_conn
                mock_connect.return_value.__exit__ = MagicMock(return_value=False)

                result = self.runner.dump_ddl("lumei", "LAB_INTERMEDIATE_DATA")

        assert result.strip().endswith(";")

    def test_output_is_multiline(self):
        with self._patch_read_sql(_make_cols_df(), _make_tbl_df()):
            with patch.object(self.runner._engine, "connect") as mock_connect:
                mock_conn = MagicMock()
                mock_connect.return_value.__enter__ = lambda s: mock_conn
                mock_connect.return_value.__exit__ = MagicMock(return_value=False)

                result = self.runner.dump_ddl("lumei", "LAB_INTERMEDIATE_DATA")

        lines = result.strip().splitlines()
        assert len(lines) >= 3  # at least CREATE TABLE line + columns + closing


class TestCheckSqlAllowed:
    """Tests for the SQL whitelist enforcer (_check_sql_allowed)."""

    def test_select_is_allowed(self):
        from app.adapters.mysql_runner import _check_sql_allowed
        # Should not raise
        _check_sql_allowed("SELECT id FROM lab_data WHERE iron_loss > 1.0")

    def test_delete_raises(self):
        from app.adapters.mysql_runner import _check_sql_allowed
        # DELETE 触发 Rule 1（must begin with SELECT），不是 Rule 2（forbidden keyword）。
        # 任一拒绝路径都可视为白名单生效。
        with pytest.raises(ValueError, match="not allowed"):
            _check_sql_allowed("DELETE FROM lab_data")

    def test_update_raises(self):
        from app.adapters.mysql_runner import _check_sql_allowed
        with pytest.raises(ValueError):
            _check_sql_allowed("UPDATE lab_data SET iron_loss = 0")

    def test_drop_raises(self):
        from app.adapters.mysql_runner import _check_sql_allowed
        with pytest.raises(ValueError):
            _check_sql_allowed("DROP TABLE lab_data")

    def test_non_select_start_raises(self):
        from app.adapters.mysql_runner import _check_sql_allowed
        with pytest.raises(ValueError, match="SELECT"):
            _check_sql_allowed("INSERT INTO t VALUES (1)")
