"""从 information_schema 拉取 DDL 字符串。

提供两个公共函数：
  - dump_ddl_for_table(): 为单张表生成 CREATE TABLE 字符串
  - dump_all_lab_tables(): 批量 dump 4 张关键实验室表
"""

from __future__ import annotations

import logging

from app.adapters.mysql_runner import MysqlRunner

logger = logging.getLogger(__name__)

# 关键实验室表（硬编码，业务方如需新增请同步更新此列表并 bump KNOWLEDGE_VERSION）
_LAB_TABLES: list[str] = [
    "LAB_INTERMEDIATE_DATA",
    "LAB_PRODUCT_SPEC",
    "LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL",
    "LAB_INTERMEDIATE_DATA_FORMULA",
]


def dump_ddl_for_table(runner: MysqlRunner, schema: str, table: str) -> str:
    """从 information_schema 为指定表生成 CREATE TABLE 字符串。

    查询流程：
      1. 查 information_schema.TABLES 获取 TABLE_COMMENT
      2. 查 information_schema.COLUMNS 获取各列详情（按 ORDINAL_POSITION 排序）
      3. 组装为可读的 CREATE TABLE 语句

    Args:
        runner: 已初始化的 MysqlRunner 实例。
        schema: 数据库/schema 名称（例如 "lumei"）。
        table:  表名（例如 "LAB_INTERMEDIATE_DATA"）。

    Returns:
        多行 CREATE TABLE 字符串，含列注释。
    """
    # 直接复用 MysqlRunner 内置的 dump_ddl 方法
    return runner.dump_ddl(schema=schema, table=table)


def dump_all_lab_tables(runner: MysqlRunner) -> dict[str, str]:
    """批量 dump 4 张关键实验室表的 DDL。

    硬编码表清单：
      - LAB_INTERMEDIATE_DATA
      - LAB_PRODUCT_SPEC
      - LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL
      - LAB_INTERMEDIATE_DATA_FORMULA

    Args:
        runner: 已初始化的 MysqlRunner 实例。
                runner._db 用作 schema 名称。

    Returns:
        dict[str, str]，键为表名，值为 DDL 字符串。
        若某张表 dump 失败，记录 warning 并跳过（返回字典中不含该表）。
    """
    schema: str = runner._db
    result: dict[str, str] = {}

    for table in _LAB_TABLES:
        try:
            ddl = dump_ddl_for_table(runner, schema, table)
            result[table] = ddl
            logger.info("dump_ddl_for_table: OK table=%s (%d bytes)", table, len(ddl))
        except Exception as exc:  # noqa: BLE001
            logger.warning(
                "dump_ddl_for_table: SKIP table=%s error=%s", table, exc
            )

    return result
