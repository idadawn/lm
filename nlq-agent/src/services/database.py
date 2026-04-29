"""
MySQL 数据库服务

负责：
1. 安全执行只读 SQL（SELECT ONLY）
2. 连接池管理
3. 结果集格式化
"""

from __future__ import annotations

import logging
import re
from typing import Any

import aiomysql

from src.core.settings import get_settings

logger = logging.getLogger(__name__)

# 禁止执行的 SQL 关键词（安全白名单之外的一切）
_FORBIDDEN_PATTERNS = re.compile(
    r"\b(INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|GRANT|REVOKE|"
    r"EXEC|EXECUTE|CALL|SET|LOAD|INTO\s+OUTFILE|INTO\s+DUMPFILE)\b",
    re.IGNORECASE,
)


class DatabaseService:
    """异步 MySQL 数据库服务，仅允许只读查询。"""

    def __init__(self) -> None:
        self._pool: aiomysql.Pool | None = None

    async def init_pool(self) -> None:
        """初始化连接池。"""
        settings = get_settings()
        self._pool = await aiomysql.create_pool(
            host=settings.mysql_host,
            port=settings.mysql_port,
            user=settings.mysql_user,
            password=settings.mysql_password,
            db=settings.mysql_database,
            charset=settings.mysql_charset,
            autocommit=True,
            maxsize=10,
            minsize=2,
        )
        logger.info("MySQL 连接池已初始化")

    async def close(self) -> None:
        """关闭连接池。"""
        if self._pool:
            self._pool.close()
            await self._pool.wait_closed()

    def validate_sql(self, sql: str) -> tuple[bool, str]:
        """
        验证 SQL 安全性。

        Args:
            sql: 待验证的 SQL 语句

        Returns:
            (is_valid, error_message) 元组
        """
        stripped = sql.strip().rstrip(";")

        # 必须以 SELECT 开头
        if not stripped.upper().startswith("SELECT"):
            return False, "仅允许 SELECT 查询"

        # 检查禁止关键词
        match = _FORBIDDEN_PATTERNS.search(stripped)
        if match:
            return False, f"检测到禁止的 SQL 操作: {match.group()}"

        # 禁止子查询中的写操作（额外安全层）
        # 注意：这是基础检查，生产环境建议使用数据库只读账号
        return True, ""

    async def execute_query(
        self,
        sql: str,
        params: tuple | None = None,
    ) -> dict[str, Any]:
        """
        执行只读 SQL 查询。

        Args:
            sql: SELECT 语句
            params: 参数化查询的参数

        Returns:
            {
                "columns": ["col1", "col2", ...],
                "rows": [{"col1": val1, "col2": val2}, ...],
                "row_count": int,
                "truncated": bool,  # 是否因超过 max_rows 被截断
            }
        """
        settings = get_settings()

        # 安全验证
        is_valid, error = self.validate_sql(sql)
        if not is_valid:
            raise ValueError(f"SQL 安全检查失败: {error}")

        # 添加 LIMIT 保护
        stripped = sql.strip().rstrip(";")
        if "LIMIT" not in stripped.upper():
            stripped += f" LIMIT {settings.sql_max_rows + 1}"

        if not self._pool:
            await self.init_pool()

        async with self._pool.acquire() as conn:
            async with conn.cursor(aiomysql.DictCursor) as cursor:
                await cursor.execute(stripped, params)
                rows = await cursor.fetchall()

                columns = [desc[0] for desc in cursor.description] if cursor.description else []

                truncated = len(rows) > settings.sql_max_rows
                if truncated:
                    rows = rows[: settings.sql_max_rows]

                # 将 Decimal / datetime 等类型转为可序列化格式
                serializable_rows = []
                for row in rows:
                    serializable_row = {}
                    for k, v in row.items():
                        if hasattr(v, "__float__"):
                            serializable_row[k] = float(v)
                        elif hasattr(v, "isoformat"):
                            serializable_row[k] = v.isoformat()
                        else:
                            serializable_row[k] = v
                    serializable_rows.append(serializable_row)

                return {
                    "columns": columns,
                    "rows": serializable_rows,
                    "row_count": len(serializable_rows),
                    "truncated": truncated,
                }

    async def health_check(self) -> bool:
        """检查 MySQL 是否可用。"""
        try:
            result = await self.execute_query("SELECT 1 AS ping")
            return result["row_count"] == 1
        except Exception:
            return False
