"""SQL safe execution module unit tests."""

from unittest.mock import AsyncMock, MagicMock, patch

import pytest

from app.tools.sql_tools import (
    ALLOWED_AGG_FUNCTIONS,
    ALLOWED_TABLES,
    build_safe_where_clause,
    execute_safe_sql,
    validate_column_name,
    validate_sql,
    validate_time_range_sql,
)


class TestValidateSql:
    """Test SQL validation functionality."""

    def test_valid_select_statement(self) -> None:
        """Test valid SELECT statement passes validation."""
        sql = "SELECT * FROM LAB_INTERMEDIATE_DATA"
        validate_sql(sql)  # Should not raise exception

    def test_valid_select_with_where(self) -> None:
        """Test SELECT with WHERE clause."""
        sql = "SELECT Id, PerfPsLoss FROM LAB_INTERMEDIATE_DATA WHERE Shift = 'A'"
        validate_sql(sql)

    def test_valid_select_with_join(self) -> None:
        """Test SELECT with JOIN clause."""
        sql = """
            SELECT d.*, f.FormulaName
            FROM LAB_INTERMEDIATE_DATA d
            JOIN LAB_INTERMEDIATE_DATA_FORMULA f ON d.FormulaId = f.Id
        """
        validate_sql(sql)

    def test_invalid_not_select(self) -> None:
        """Test non-SELECT statements are rejected."""
        with pytest.raises(ValueError, match="SQL must start with SELECT"):
            validate_sql("DELETE FROM LAB_INTERMEDIATE_DATA")

    def test_invalid_insert_keyword(self) -> None:
        """Test INSERT keyword is rejected."""
        with pytest.raises(ValueError, match="SQL contains forbidden keyword: INSERT"):
            validate_sql("SELECT * FROM LAB_INTERMEDIATE_DATA; INSERT INTO test VALUES (1)")

    def test_invalid_update_keyword(self) -> None:
        """Test UPDATE keyword is rejected."""
        # UPDATE doesn't start with SELECT, so it fails the first check
        with pytest.raises(ValueError, match="SQL must start with SELECT"):
            validate_sql("UPDATE LAB_INTERMEDIATE_DATA SET PerfPsLoss = 0")

    def test_invalid_delete_keyword(self) -> None:
        """Test DELETE keyword is rejected."""
        # DELETE doesn't start with SELECT, so it fails the first check
        with pytest.raises(ValueError, match="SQL must start with SELECT"):
            validate_sql("DELETE FROM LAB_INTERMEDIATE_DATA WHERE Id = 1")

    def test_invalid_drop_keyword(self) -> None:
        """Test DROP keyword is rejected."""
        # DROP doesn't start with SELECT, so it fails the first check
        with pytest.raises(ValueError, match="SQL must start with SELECT"):
            validate_sql("DROP TABLE LAB_INTERMEDIATE_DATA")

    def test_invalid_create_keyword(self) -> None:
        """Test CREATE keyword is rejected."""
        # CREATE doesn't start with SELECT, so it fails the first check
        with pytest.raises(ValueError, match="SQL must start with SELECT"):
            validate_sql("CREATE TABLE test (id INT)")

    def test_invalid_alter_keyword(self) -> None:
        """Test ALTER keyword is rejected."""
        # ALTER doesn't start with SELECT, so it fails the first check
        with pytest.raises(ValueError, match="SQL must start with SELECT"):
            validate_sql("ALTER TABLE LAB_INTERMEDIATE_DATA ADD COLUMN test VARCHAR(10)")

    def test_invalid_truncate_keyword(self) -> None:
        """Test TRUNCATE keyword is rejected."""
        # TRUNCATE doesn't start with SELECT, so it fails the first check
        with pytest.raises(ValueError, match="SQL must start with SELECT"):
            validate_sql("TRUNCATE TABLE LAB_INTERMEDIATE_DATA")

    def test_invalid_union_keyword(self) -> None:
        """Test UNION keyword is rejected."""
        with pytest.raises(ValueError, match="SQL contains forbidden keyword: UNION"):
            validate_sql(
                "SELECT * FROM LAB_INTERMEDIATE_DATA UNION SELECT * FROM LAB_INTERMEDIATE_DATA"
            )

    def test_dangerous_comment_sequence(self) -> None:
        """Test SQL comment sequences are rejected."""
        with pytest.raises(ValueError, match="SQL contains dangerous character sequence"):
            validate_sql("SELECT * FROM LAB_INTERMEDIATE_DATA -- WHERE 1=1")

    def test_dangerous_block_comment(self) -> None:
        """Test block comment sequences are rejected."""
        with pytest.raises(ValueError, match="SQL contains dangerous character sequence"):
            validate_sql("SELECT * FROM LAB_INTERMEDIATE_DATA /* test */ WHERE 1=1")

    def test_dangerous_multiple_statements(self) -> None:
        """Test multiple statements are rejected."""
        # Multiple statements fail at DROP keyword check
        with pytest.raises(ValueError, match="SQL contains forbidden keyword: DROP"):
            validate_sql("SELECT * FROM LAB_INTERMEDIATE_DATA; DROP TABLE test")

    def test_disallowed_table(self) -> None:
        """Test disallowed table names are rejected."""
        with pytest.raises(ValueError, match="Table not allowed: USERS"):
            validate_sql("SELECT * FROM USERS")

    def test_case_insensitive_table_check(self) -> None:
        """Test table name case-insensitive check."""
        validate_sql("SELECT * FROM lab_intermediate_data")  # lowercase should be allowed

    def test_allowed_tables_list(self) -> None:
        """Test allowed table names list is complete."""
        expected_tables = [
            "LAB_INTERMEDIATE_DATA",
            "LAB_INTERMEDIATE_DATA_FORMULA",
            "LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL",
            "LAB_PRODUCT_SPEC",
        ]
        assert set(ALLOWED_TABLES) == set(expected_tables)

    def test_allowed_agg_functions(self) -> None:
        """Test allowed aggregation functions list."""
        expected_functions = ["AVG", "MAX", "MIN", "SUM", "COUNT"]
        assert set(ALLOWED_AGG_FUNCTIONS) == set(expected_functions)


class TestBuildSafeWhereClause:
    """Test safe WHERE clause building functionality."""

    def test_empty_conditions(self) -> None:
        """Test empty conditions return 1=1."""
        clause, params = build_safe_where_clause({})
        assert clause == "1=1"
        assert params == {}

    def test_single_condition(self) -> None:
        """Test single condition building."""
        clause, params = build_safe_where_clause({"Shift": "A"})
        assert clause == "Shift = :param_Shift"
        assert params == {"param_Shift": "A"}

    def test_multiple_conditions(self) -> None:
        """Test multiple conditions building."""
        conditions = {"Shift": "A", "DetectionDate": "2026-01-01"}
        clause, params = build_safe_where_clause(conditions)
        assert "Shift = :param_Shift" in clause
        assert "DetectionDate = :param_DetectionDate" in clause
        assert " AND " in clause
        assert params == {"param_Shift": "A", "param_DetectionDate": "2026-01-01"}

    def test_invalid_field_name(self) -> None:
        """Test invalid field names are rejected."""
        with pytest.raises(ValueError, match="Invalid field name"):
            build_safe_where_clause({"1invalid": "value"})

    def test_field_name_with_special_chars(self) -> None:
        """Test field names with special characters are rejected."""
        with pytest.raises(ValueError, match="Invalid field name"):
            build_safe_where_clause({"field; DROP": "value"})

    def test_field_name_with_sql_injection(self) -> None:
        """Test SQL injection attempts are rejected."""
        with pytest.raises(ValueError, match="Invalid field name"):
            build_safe_where_clause({"id OR 1=1": "value"})

    def test_numeric_value(self) -> None:
        """Test numeric type parameters."""
        clause, params = build_safe_where_clause({"PerfPsLoss": 1.23})
        assert clause == "PerfPsLoss = :param_PerfPsLoss"
        assert params == {"param_PerfPsLoss": 1.23}


class TestExecuteSafeSql:
    """Test safe SQL execution functionality."""

    @pytest.mark.asyncio
    async def test_execute_valid_query(self) -> None:
        """Test executing valid query."""
        mock_result = [
            {"Id": 1, "PerfPsLoss": 1.23},
            {"Id": 2, "PerfPsLoss": 1.45},
        ]

        with patch("app.tools.sql_tools.AsyncSessionLocal") as mock_session_class:
            mock_session = AsyncMock()
            mock_session_class.return_value.__aenter__ = AsyncMock(return_value=mock_session)
            mock_session_class.return_value.__aexit__ = AsyncMock(return_value=False)

            # Setup mock for result.mappings().all() chain
            # mappings() returns a sync object, all() is also sync
            mock_mappings = MagicMock()
            mock_mappings.all.return_value = mock_result
            mock_result_obj = MagicMock()
            mock_result_obj.mappings.return_value = mock_mappings
            mock_session.execute = AsyncMock(return_value=mock_result_obj)

            result = await execute_safe_sql(
                "SELECT Id, PerfPsLoss FROM LAB_INTERMEDIATE_DATA WHERE Shift = :shift",
                {"shift": "A"},
            )

            assert result == mock_result
            mock_session.execute.assert_called_once()

    @pytest.mark.asyncio
    async def test_execute_invalid_sql(self) -> None:
        """Test executing invalid SQL is rejected."""
        with pytest.raises(ValueError, match="SQL must start with SELECT"):
            await execute_safe_sql("DELETE FROM LAB_INTERMEDIATE_DATA")

    @pytest.mark.asyncio
    async def test_execute_with_empty_params(self) -> None:
        """Test executing query without parameters."""
        mock_result = [{"count": 100}]

        with patch("app.tools.sql_tools.AsyncSessionLocal") as mock_session_class:
            mock_session = AsyncMock()
            mock_session_class.return_value.__aenter__ = AsyncMock(return_value=mock_session)
            mock_session_class.return_value.__aexit__ = AsyncMock(return_value=False)

            # Setup mock for result.mappings().all() chain
            # mappings() returns a sync object, all() is also sync
            mock_mappings = MagicMock()
            mock_mappings.all.return_value = mock_result
            mock_result_obj = MagicMock()
            mock_result_obj.mappings.return_value = mock_mappings
            mock_session.execute = AsyncMock(return_value=mock_result_obj)

            result = await execute_safe_sql("SELECT COUNT(*) as count FROM LAB_INTERMEDIATE_DATA")

            assert result == mock_result

    @pytest.mark.asyncio
    async def test_sql_injection_attempt_blocked(self) -> None:
        """Test SQL injection attempt is blocked."""
        with pytest.raises(ValueError):
            await execute_safe_sql(
                "SELECT * FROM LAB_INTERMEDIATE_DATA WHERE Id = 1; DROP TABLE test"
            )


class TestValidateColumnName:
    """Test column name validation functionality."""

    def test_valid_column_names(self) -> None:
        """Test valid column names."""
        valid_names = [
            "PerfPsLoss",
            "LaminationFactor",
            "DetectionDate",
            "_id",
            "Shift",
            "Column1",
        ]
        for name in valid_names:
            assert validate_column_name(name) is True, f"Failed for: {name}"

    def test_invalid_column_names(self) -> None:
        """Test invalid column names."""
        invalid_names = [
            "1invalid",  # starts with number
            "column; DROP",  # contains special characters
            "column--",  # contains comment
            "column name",  # contains space
            "column'name",  # contains quote
            "",  # empty string
            "column\nname",  # contains newline
        ]
        for name in invalid_names:
            assert validate_column_name(name) is False, f"Failed for: {name}"

    def test_sql_injection_in_column_name(self) -> None:
        """Test SQL injection attempts in column names."""
        injection_attempts = [
            "id OR 1=1",
            "id; DROP TABLE",
            "id--",
            "id/*",
            "id UNION SELECT",
        ]
        for attempt in injection_attempts:
            assert validate_column_name(attempt) is False, f"Failed for: {attempt}"


class TestValidateTimeRangeSql:
    """Test time range SQL validation functionality."""

    def test_valid_time_range_sql(self) -> None:
        """Test valid time range SQL."""
        valid_sqls = [
            "DetectionDate >= DATE_SUB(NOW(), INTERVAL 7 DAY)",
            "YEAR(DetectionDate) = 2026 AND MONTH(DetectionDate) = 1",
            "DetectionDate BETWEEN '2026-01-01' AND '2026-01-31'",
            "DATE(DetectionDate) >= '2026-01-01'",
        ]
        for sql in valid_sqls:
            assert validate_time_range_sql(sql) is True, f"Failed for: {sql}"

    def test_invalid_time_range_sql(self) -> None:
        """Test invalid time range SQL."""
        invalid_sqls = [
            "1=1; DROP TABLE test",  # multiple statements
            "1=1--",  # comment
            "1=1/* comment */",  # block comment
            "1=1 UNION SELECT * FROM users",  # UNION
            "SELECT * FROM table",  # contains SELECT
        ]
        for sql in invalid_sqls:
            assert validate_time_range_sql(sql) is False, f"Failed for: {sql}"

    def test_empty_time_range_sql(self) -> None:
        """Test empty time range SQL."""
        assert validate_time_range_sql("") is False
        assert validate_time_range_sql(None) is False  # type: ignore[arg-type]
