"""SQL safe execution module.

Provides SQL execution with whitelist validation to prevent SQL injection attacks.
Only SELECT statements are allowed, all data modification operations are prohibited.
"""

import re
from typing import Any

from sqlalchemy import text

from app.core.database import AsyncSessionLocal

# Whitelist: must start with SELECT
SQL_WHITELIST_PATTERN = re.compile(
    r"^\s*SELECT\s+",
    re.IGNORECASE,
)

# Blacklist keywords (forbidden in SQL)
# Note: Use word boundary check to avoid false positives in valid words
FORBIDDEN_KEYWORDS = [
    "INSERT",
    "UPDATE",
    "DELETE",
    "DROP",
    "CREATE",
    "ALTER",
    "TRUNCATE",
    "EXEC",
    "EXECUTE",
    "UNION",
]

# Allowed aggregation function whitelist
ALLOWED_AGG_FUNCTIONS = ["AVG", "MAX", "MIN", "SUM", "COUNT"]

# Allowed table name whitelist
ALLOWED_TABLES = [
    "LAB_INTERMEDIATE_DATA",
    "LAB_INTERMEDIATE_DATA_FORMULA",
    "LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL",
    "LAB_PRODUCT_SPEC",
    "LAB_PRODUCT_SPEC_ATTRIBUTE",
    "LAB_PRODUCT_SPEC_VERSION",
    "LAB_APPEARANCE_FEATURE",
    "LAB_APPEARANCE_FEATURE_CATEGORY",
    "LAB_APPEARANCE_FEATURE_LEVEL",
    "LAB_REPORT_CONFIG",
    "LAB_RAW_DATA",
    "LAB_MAGNETIC_RAW_DATA",
]


def validate_sql(sql: str) -> None:
    """Validate SQL statement security.

    Args:
        sql: SQL statement

    Raises:
        ValueError: Raised when SQL is invalid
    """
    # Check whitelist: must start with SELECT
    if not SQL_WHITELIST_PATTERN.match(sql):
        raise ValueError(f"SQL must start with SELECT: {sql[:50]}...")

    # Check blacklist keywords (using word boundaries)
    sql_upper = sql.upper()
    for keyword in FORBIDDEN_KEYWORDS:
        pattern = r"\b" + re.escape(keyword) + r"\b"
        if re.search(pattern, sql_upper):
            raise ValueError(f"SQL contains forbidden keyword: {keyword}")

    # Check dangerous character sequences
    dangerous_patterns = [
        (r"--", "SQL comment"),
        (r"/\*", "block comment start"),
        (r"\*/", "block comment end"),
        (r";\s*\w+", "multiple statements"),
    ]

    for pattern, desc in dangerous_patterns:
        if re.search(pattern, sql, re.IGNORECASE):
            raise ValueError(f"SQL contains dangerous character sequence: {desc}")

    # Check table name whitelist
    # Extract table names from SQL (simplified check: check table names after FROM and JOIN)
    from_pattern = r"\bFROM\s+(\w+)"
    join_pattern = r"\bJOIN\s+(\w+)"

    for pattern in [from_pattern, join_pattern]:
        matches = re.findall(pattern, sql_upper)
        for table in matches:
            if table not in [t.upper() for t in ALLOWED_TABLES]:
                raise ValueError(f"Table not allowed: {table}")


async def execute_safe_sql(
    sql: str,
    params: dict[str, Any] | None = None,
) -> list[dict[str, Any]]:
    """Execute SQL query safely.

    Performs whitelist and blacklist validation before execution,
    uses parameterized queries to prevent SQL injection.

    Args:
        sql: SQL statement (must be SELECT)
        params: Query parameters

    Returns:
        List of query results

    Raises:
        ValueError: Raised when SQL validation fails
    """
    # Validate SQL
    validate_sql(sql)

    # Execute query
    params = params or {}
    async with AsyncSessionLocal() as session:
        result = await session.execute(text(sql), params)
        rows = result.mappings().all()
        return [dict(row) for row in rows]


def build_safe_where_clause(conditions: dict[str, Any]) -> tuple[str, dict[str, Any]]:
    """Build safe WHERE clause.

    Uses parameterized queries to avoid SQL injection.

    Args:
        conditions: Condition dict, e.g., {"Shift": "A", "DetectionDate": "2026-01-01"}

    Returns:
        (WHERE clause string, parameter dict)
    """
    if not conditions:
        return "1=1", {}

    clauses = []
    params = {}

    for key, value in conditions.items():
        # Validate column name (only letters, numbers, underscores allowed)
        if not validate_column_name(key):
            raise ValueError(f"Invalid field name: {key}")

        param_name = f"param_{key}"
        clauses.append(f"{key} = :{param_name}")
        params[param_name] = value

    where_clause = " AND ".join(clauses)
    return where_clause, params


def validate_column_name(column_name: str) -> bool:
    """Validate database column name.

    Only allows letters, numbers, underscores, and must start with letter or underscore.

    Args:
        column_name: Column name

    Returns:
        Whether valid
    """
    if not column_name:
        return False
    return bool(re.match(r"^[a-zA-Z_][a-zA-Z0-9_]*$", column_name))


def validate_time_range_sql(time_range_sql: str) -> bool:
    """Validate time range SQL condition.

    Only allows SQL fragments containing time-related functions and conditions.

    Args:
        time_range_sql: Time range SQL condition

    Returns:
        Whether valid
    """
    if not time_range_sql:
        return False

    # Forbidden keywords (blacklist)
    forbidden_patterns = [
        r";\s*\w",  # Multiple statements
        r"--",  # Comment
        r"/\*",  # Block comment start
        r"\*/",  # Block comment end
        r"\bINSERT\b",
        r"\bUPDATE\b",
        r"\bDELETE\b",
        r"\bDROP\b",
        r"\bCREATE\b",
        r"\bALTER\b",
        r"\bUNION\b",
        r"\bSELECT\b",  # Time range condition should not have SELECT
    ]

    sql_upper = time_range_sql.upper()
    for pattern in forbidden_patterns:
        if re.search(pattern, sql_upper):
            return False

    return True
