"""
Unit tests for DatabaseService.validate_sql forbidden-keyword coverage.

Source of truth: src/services/database.py:_FORBIDDEN_PATTERNS

Each keyword in the regex must be rejected with (False, error_message)
where the error message contains the matched keyword. SELECT-prefixed
read queries must be accepted with (True, "").
"""

from __future__ import annotations

import pytest

from src.services.database import DatabaseService

# Mirrors _FORBIDDEN_PATTERNS in src/services/database.py.
# Any change to that regex MUST be reflected here, and vice versa.
SIMPLE_KEYWORDS = [
    "INSERT",
    "UPDATE",
    "DELETE",
    "DROP",
    "ALTER",
    "CREATE",
    "TRUNCATE",
    "GRANT",
    "REVOKE",
    "EXEC",
    "EXECUTE",
    "CALL",
    "SET",
    "LOAD",
]


@pytest.fixture
def db() -> DatabaseService:
    """A fresh DatabaseService instance — no pool init required for validate_sql."""
    return DatabaseService()


# ─────────────────────────────────────────────────────────────────────
# Forbidden simple keywords — each rejected at top level after SELECT
# ─────────────────────────────────────────────────────────────────────
@pytest.mark.parametrize("keyword", SIMPLE_KEYWORDS)
def test_simple_forbidden_keyword_rejected_at_top_level(
    db: DatabaseService, keyword: str
) -> None:
    """Each simple forbidden keyword used as the SQL verb is rejected.

    The SQL fails the SELECT-prefix check first, so we assert the SELECT-prefix
    error rather than the keyword error. This documents the layered defense.
    The fixture SQL is intentionally nonsensical for some keywords (e.g.,
    'GRANT foo SET x=1') — what matters is that the prefix check rejects
    anything not starting with SELECT, regardless of trailing tokens.
    """
    sql = f"{keyword} foo SET x=1"
    is_valid, error = db.validate_sql(sql)
    assert is_valid is False
    assert "仅允许 SELECT 查询" in error


@pytest.mark.parametrize("keyword", SIMPLE_KEYWORDS)
def test_simple_forbidden_keyword_rejected_inside_select(
    db: DatabaseService, keyword: str
) -> None:
    """Each simple forbidden keyword embedded after SELECT is rejected by the keyword filter."""
    sql = f"SELECT 1 FROM t WHERE x=1; {keyword} foo"
    is_valid, error = db.validate_sql(sql)
    assert is_valid is False
    assert "检测到禁止的 SQL 操作" in error


# ─────────────────────────────────────────────────────────────────────
# Case-insensitivity — lowercase / uppercase / mixed all blocked
# ─────────────────────────────────────────────────────────────────────
@pytest.mark.parametrize(
    "variant",
    [
        "select * from t; insert into t values (1)",
        "SELECT * FROM t; INSERT INTO t VALUES (1)",
        "Select * From t; Insert Into t Values (1)",
        "sElEcT * fRoM t; iNsErT iNtO t VaLuEs (1)",
    ],
)
def test_case_insensitivity_for_forbidden_keyword(
    db: DatabaseService, variant: str
) -> None:
    """Forbidden keyword is matched regardless of letter case."""
    is_valid, error = db.validate_sql(variant)
    assert is_valid is False
    assert "检测到禁止的 SQL 操作" in error


# ─────────────────────────────────────────────────────────────────────
# Multi-token forbidden patterns: INTO OUTFILE / INTO DUMPFILE
# Whitespace between INTO and OUTFILE/DUMPFILE may be one or more spaces,
# tabs, or newlines (the regex uses \s+).
# ─────────────────────────────────────────────────────────────────────
@pytest.mark.parametrize(
    "sql",
    [
        "SELECT * FROM users INTO OUTFILE '/tmp/x'",
        "SELECT * FROM users INTO  OUTFILE '/tmp/x'",
        "SELECT * FROM users INTO\tOUTFILE '/tmp/x'",
        "SELECT * FROM users INTO\nOUTFILE '/tmp/x'",
        "SELECT * FROM users into outfile '/tmp/x'",
    ],
)
def test_into_outfile_blocked(db: DatabaseService, sql: str) -> None:
    is_valid, error = db.validate_sql(sql)
    assert is_valid is False
    assert "检测到禁止的 SQL 操作" in error


@pytest.mark.parametrize(
    "sql",
    [
        "SELECT * FROM users INTO DUMPFILE '/tmp/x'",
        "SELECT * FROM users INTO  DUMPFILE '/tmp/x'",
        "SELECT * FROM users into\tdumpfile '/tmp/x'",
    ],
)
def test_into_dumpfile_blocked(db: DatabaseService, sql: str) -> None:
    is_valid, error = db.validate_sql(sql)
    assert is_valid is False
    assert "检测到禁止的 SQL 操作" in error


# ─────────────────────────────────────────────────────────────────────
# Forbidden keyword inside a subquery (parenthesised) is still rejected
# ─────────────────────────────────────────────────────────────────────
@pytest.mark.parametrize(
    "sql",
    [
        "SELECT * FROM (DELETE FROM t) AS x",
        "SELECT id FROM users WHERE id IN (SELECT id FROM logs; DROP TABLE logs)",
        "SELECT col1 FROM t WHERE col1 = (SELECT * FROM (TRUNCATE TABLE u) z)",
    ],
)
def test_forbidden_keyword_in_subquery_rejected(
    db: DatabaseService, sql: str
) -> None:
    is_valid, error = db.validate_sql(sql)
    assert is_valid is False
    assert "检测到禁止的 SQL 操作" in error


# ─────────────────────────────────────────────────────────────────────
# Non-SELECT prefix is rejected by the prefix check before the keyword filter
# ─────────────────────────────────────────────────────────────────────
@pytest.mark.parametrize(
    "sql",
    [
        "UPDATE foo SET x=1",
        "DELETE FROM users",
        "DROP TABLE users",
        "WITH t AS (SELECT 1) SELECT * FROM t",  # CTE not allowed by current policy
        "  COMMIT",
        "EXPLAIN SELECT 1",  # not a SELECT verb at the start
    ],
)
def test_non_select_prefix_rejected(db: DatabaseService, sql: str) -> None:
    is_valid, error = db.validate_sql(sql)
    assert is_valid is False
    assert "仅允许 SELECT 查询" in error


# ─────────────────────────────────────────────────────────────────────
# Benign SELECT statements pass
# ─────────────────────────────────────────────────────────────────────
@pytest.mark.parametrize(
    "sql",
    [
        "SELECT 1",
        "SELECT 1 AS ping",
        "SELECT * FROM users WHERE id = 1",
        "SELECT id, name FROM users WHERE created_at > '2024-01-01' LIMIT 10",
        "select id from users",
        "SELECT\n  id,\n  name\nFROM users\nWHERE id = 1",
        "  SELECT * FROM users  ",  # leading/trailing whitespace tolerated
        "SELECT * FROM users;",  # trailing semicolon stripped
    ],
)
def test_benign_select_accepted(db: DatabaseService, sql: str) -> None:
    is_valid, error = db.validate_sql(sql)
    assert is_valid is True
    assert error == ""


# ─────────────────────────────────────────────────────────────────────
# Word boundary: keyword must be a standalone token, not a substring
# (e.g., "selected" or "delete_at" column names should NOT trigger)
# ─────────────────────────────────────────────────────────────────────
@pytest.mark.parametrize(
    "sql",
    [
        "SELECT updated_at FROM users",  # "updated" contains UPDATE substring
        "SELECT deleted_at FROM users",  # "deleted" contains DELETE substring
        "SELECT created_at FROM users",  # "created" contains CREATE substring
        "SELECT inserted_count FROM stats",  # "inserted" contains INSERT substring
        "SELECT settings_json FROM users",  # "settings" contains SET substring
    ],
)
def test_substring_in_column_name_does_not_trigger(
    db: DatabaseService, sql: str
) -> None:
    """Column names that happen to contain forbidden keyword substrings must pass.

    The regex uses \\b word boundaries to avoid false positives like
    'updated_at', 'deleted_at', 'created_at', 'settings'.
    """
    is_valid, error = db.validate_sql(sql)
    assert is_valid is True, f"Expected pass for {sql!r}, got error: {error}"
    assert error == ""


# ─────────────────────────────────────────────────────────────────────
# Edge cases: empty / whitespace-only input is rejected by the prefix check
# ─────────────────────────────────────────────────────────────────────
@pytest.mark.parametrize("sql", ["", "   ", "\n\t  ", ";"])
def test_empty_or_whitespace_rejected(db: DatabaseService, sql: str) -> None:
    is_valid, error = db.validate_sql(sql)
    assert is_valid is False
    assert "仅允许 SELECT 查询" in error
