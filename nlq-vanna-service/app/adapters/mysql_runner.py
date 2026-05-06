"""MySQL runner with SELECT whitelist enforcement.

MysqlRunner is a standalone class (not a Vanna mixin).
It provides:
  - run_sql(): execute SELECT-only queries, return pd.DataFrame
  - dump_ddl(): generate a CREATE TABLE string from information_schema
"""

from __future__ import annotations

import re

import pandas as pd
from sqlalchemy import create_engine, text
from sqlalchemy.engine import Engine

# Words that must NOT appear anywhere in the SQL (case-insensitive).
_FORBIDDEN_KEYWORDS: frozenset[str] = frozenset(
    {
        ";",
        "--",
        "/*",
        "UPDATE",
        "DELETE",
        "INSERT",
        "DROP",
        "ALTER",
        "TRUNCATE",
        "CREATE",
    }
)


def _check_sql_allowed(sql: str) -> None:
    """Validate that sql is a safe SELECT statement.

    Rules:
      1. The first non-whitespace word (uppercased) must be SELECT.
      2. None of the forbidden keywords/tokens may appear (case-insensitive).

    Args:
        sql: Raw SQL string supplied by the caller.

    Raises:
        ValueError: If the SQL fails any whitelist check.
    """
    stripped = sql.strip()

    # Rule 1: must start with SELECT
    first_word = stripped.split()[0].upper() if stripped.split() else ""
    if first_word != "SELECT":
        raise ValueError(
            f"SQL not allowed: must begin with SELECT, got '{first_word}'"
        )

    # Rule 2: no forbidden keywords / tokens
    upper_sql = stripped.upper()
    for keyword in _FORBIDDEN_KEYWORDS:
        # For single-char tokens (";", "--", "/*") use plain substring search.
        # For word keywords use word-boundary matching to avoid false positives
        # (e.g. "TRUNCATE" must not fire on a column named "truncate_date").
        if len(keyword) <= 2 or not keyword.isalpha():  # noqa: PLR2004
            if keyword in upper_sql:
                raise ValueError(
                    f"SQL not allowed: forbidden token '{keyword}' found"
                )
        else:
            pattern = rf"\b{re.escape(keyword)}\b"
            if re.search(pattern, upper_sql):
                raise ValueError(
                    f"SQL not allowed: forbidden keyword '{keyword}' found"
                )


class MysqlRunner:
    """Execute SELECT queries and introspect MySQL schema.

    Expected config keys:
        mysql_host (str):     Hostname or IP.
        mysql_port (int):     Port (default 3306).
        mysql_user (str):     Username.
        mysql_password (str): Password.
        mysql_db (str):       Database (schema) name.
    """

    def __init__(self, config: dict) -> None:
        from urllib.parse import quote_plus  # noqa: PLC0415

        host: str = config["mysql_host"]
        port: int = int(config.get("mysql_port", 3306))
        user: str = config["mysql_user"]
        password: str = config["mysql_password"]
        db: str = config["mysql_db"]

        # Security MEDIUM-4: 密码含 @ : / ? # 等 URI 特殊字符时必须 URL-encode，
        # 否则 SQLAlchemy 把 # 当 fragment、@ 当 host 分隔，连接会被指向错误地址。
        dsn = (
            f"mysql+pymysql://{quote_plus(user)}:{quote_plus(password)}"
            f"@{host}:{port}/{quote_plus(db)}?charset=utf8mb4"
        )
        self._engine: Engine = create_engine(dsn, pool_pre_ping=True)
        self._db: str = db

    # ------------------------------------------------------------------
    # Public API
    # ------------------------------------------------------------------

    def run_sql(self, sql: str) -> pd.DataFrame:
        """Execute a SELECT statement and return results as a DataFrame.

        Args:
            sql: A SQL SELECT statement. Semicolons, comments, and
                 DML/DDL keywords are all forbidden.

        Returns:
            pd.DataFrame with query results.

        Raises:
            ValueError: If the SQL fails the whitelist check.
        """
        _check_sql_allowed(sql)
        with self._engine.connect() as conn:
            return pd.read_sql(text(sql), conn)

    def dump_ddl(self, schema: str, table: str) -> str:
        """Generate a human-readable CREATE TABLE string from information_schema.

        Pulls column names, types, nullability, defaults, and comments from
        information_schema.COLUMNS, plus the table-level comment from
        information_schema.TABLES.

        Args:
            schema: Database/schema name (e.g. "lumei").
            table:  Table name (e.g. "LAB_INTERMEDIATE_DATA").

        Returns:
            A multi-line string resembling a CREATE TABLE statement with
            inline column comments.
        """
        col_sql = """
            SELECT
                COLUMN_NAME,
                COLUMN_TYPE,
                IS_NULLABLE,
                COLUMN_DEFAULT,
                COLUMN_COMMENT
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = :schema
              AND TABLE_NAME   = :table
            ORDER BY ORDINAL_POSITION
        """
        tbl_sql = """
            SELECT TABLE_COMMENT
            FROM information_schema.TABLES
            WHERE TABLE_SCHEMA = :schema
              AND TABLE_NAME   = :table
        """
        with self._engine.connect() as conn:
            cols_df: pd.DataFrame = pd.read_sql(
                text(col_sql), conn, params={"schema": schema, "table": table}
            )
            tbl_df: pd.DataFrame = pd.read_sql(
                text(tbl_sql), conn, params={"schema": schema, "table": table}
            )

        table_comment = ""
        if not tbl_df.empty:
            raw = tbl_df.iloc[0]["TABLE_COMMENT"]
            if raw:
                table_comment = f"  -- {raw}"

        lines: list[str] = [f"CREATE TABLE `{table}`({table_comment}"]
        col_defs: list[str] = []
        for _, row in cols_df.iterrows():
            nullable = "NULL" if row["IS_NULLABLE"] == "YES" else "NOT NULL"
            default_clause = ""
            if row["COLUMN_DEFAULT"] is not None:
                default_clause = f" DEFAULT '{row['COLUMN_DEFAULT']}'"
            comment_clause = ""
            if row["COLUMN_COMMENT"]:
                comment_clause = f"  -- {row['COLUMN_COMMENT']}"
            col_defs.append(
                f"    `{row['COLUMN_NAME']}` {row['COLUMN_TYPE']} "
                f"{nullable}{default_clause}{comment_clause}"
            )
        lines.append(",\n".join(col_defs))
        lines.append(");")
        return "\n".join(lines)
