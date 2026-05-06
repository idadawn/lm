"""启动注入主流程：将 DDL / 术语 / 判级规则 / QA 示例写入 Qdrant。

bootstrap_knowledge() 在 main.py lifespan 中调用。
支持 force=True 跳过版本检查，强制重索引（CLI reindex 使用）。
"""

from __future__ import annotations

import logging
from pathlib import Path

from app.config import Settings

logger = logging.getLogger(__name__)

# knowledge/ 内容目录：相对于本文件向上两级到 nlq-vanna-service/，再进 knowledge/
_KNOWLEDGE_DIR = Path(__file__).parent.parent.parent / "knowledge"


async def bootstrap_knowledge(
    vn: object,
    settings: Settings,
    *,
    force: bool = False,
) -> None:
    """将知识库内容注入 Qdrant。

    执行顺序：
      1. TEI dim 校验（vn.verify_dim()）—— 不通过则 fast-fail
      2. 版本检查（可用 force=True 跳过）
      3. 按序注入：DDL → 术语 → 判级规则 → QA 示例
      4. 写入 collection metadata（knowledge_version）

    Args:
        vn:       已初始化的 VannaApp 实例（含 QdrantStoreMixin）。
        settings: 应用配置（含 mysql_* / qdrant_* / knowledge_version）。
        force:    True 时跳过版本检查，直接重索引。
                  用于 CLI `reindex` 子命令。

    Raises:
        RuntimeError: TEI dim 校验失败时抛出（fast-fail）。
        Exception:    任何注入步骤失败时向上传播，让 lifespan 报错。
    """
    import os  # noqa: PLC0415

    # ------------------------------------------------------------------
    # Optional fast-skip — 仅在前置依赖（MySQL/TEI/vLLM）全不可达
    # 时启用，用于本地架构冒烟（仅起 /healthz）。生产请保持未设置。
    # ------------------------------------------------------------------
    if os.environ.get("BOOTSTRAP_SKIP", "").strip() == "1":
        logger.warning(
            "BOOTSTRAP_SKIP=1: 跳过知识注入（开发/冒烟模式）。"
            "生产环境必须取消此标志以确保 G1/P3 prerequisites 验证。"
        )
        return

    # ------------------------------------------------------------------
    # Step 0: TEI dim 校验
    # ------------------------------------------------------------------
    logger.info("bootstrap_knowledge: 开始 TEI dim 校验")
    vn.verify_dim()  # type: ignore[attr-defined]
    logger.info("bootstrap_knowledge: TEI dim 校验通过")

    # ------------------------------------------------------------------
    # Step 1: 版本检查
    # ------------------------------------------------------------------
    from app.knowledge import version as ver  # noqa: PLC0415
    from qdrant_client import QdrantClient  # noqa: PLC0415

    client: QdrantClient = QdrantClient(url=settings.qdrant_url)
    collection: str = settings.qdrant_collection
    target_version: str = settings.knowledge_version

    if not force:
        needs_reindex = ver.should_reindex(client, collection, target_version)
        if not needs_reindex:
            logger.info(
                "bootstrap_knowledge: 跳过索引，版本已是最新 version=%s",
                target_version,
            )
            return
        logger.info(
            "bootstrap_knowledge: 需要重索引 version=%s", target_version
        )
    else:
        logger.info(
            "bootstrap_knowledge: force=True，强制重索引 version=%s", target_version
        )

    # ------------------------------------------------------------------
    # Step 2: 注入 DDL
    # ------------------------------------------------------------------
    from app.adapters.mysql_runner import MysqlRunner  # noqa: PLC0415
    from app.knowledge.ddl_loader import dump_all_lab_tables  # noqa: PLC0415

    runner = MysqlRunner(
        {
            "mysql_host": settings.mysql_host,
            "mysql_port": settings.mysql_port,
            "mysql_user": settings.mysql_user,
            "mysql_password": settings.mysql_password,
            "mysql_db": settings.mysql_database,
        }
    )

    logger.info("bootstrap_knowledge: 开始 dump DDL")
    ddl_map = dump_all_lab_tables(runner)
    for table, ddl in ddl_map.items():
        vn.add_ddl(ddl)  # type: ignore[attr-defined]
        logger.info("bootstrap_knowledge: add_ddl table=%s", table)

    # ------------------------------------------------------------------
    # Step 3: 注入术语
    # ------------------------------------------------------------------
    from app.knowledge.markdown_loader import (  # noqa: PLC0415
        load_judgment_rules,
        load_qa_examples,
        load_terminology,
    )

    logger.info("bootstrap_knowledge: 开始加载术语 from %s", _KNOWLEDGE_DIR)
    terminology_chunks = load_terminology(_KNOWLEDGE_DIR)
    for seg in terminology_chunks:
        vn.add_terminology(seg)  # type: ignore[attr-defined]
    logger.info("bootstrap_knowledge: add_terminology %d 段", len(terminology_chunks))

    # ------------------------------------------------------------------
    # Step 4: 注入判级规则
    # ------------------------------------------------------------------
    logger.info("bootstrap_knowledge: 开始加载判级规则 from %s", _KNOWLEDGE_DIR)
    rule_chunks = load_judgment_rules(_KNOWLEDGE_DIR)
    for seg in rule_chunks:
        vn.add_judgment_rule(seg)  # type: ignore[attr-defined]
    logger.info("bootstrap_knowledge: add_judgment_rule %d 段", len(rule_chunks))

    # ------------------------------------------------------------------
    # Step 5: 注入 QA 示例
    # ------------------------------------------------------------------
    logger.info("bootstrap_knowledge: 开始加载 QA 示例 from %s", _KNOWLEDGE_DIR)
    qa_examples = load_qa_examples(_KNOWLEDGE_DIR)
    for example in qa_examples:
        vn.add_question_sql(  # type: ignore[attr-defined]
            question=example["question"],
            sql=example["sql"],
        )
    logger.info("bootstrap_knowledge: add_question_sql %d 条", len(qa_examples))

    # ------------------------------------------------------------------
    # Step 6: 写入 collection metadata（标记版本）
    # ------------------------------------------------------------------
    ver.set_collection_metadata(
        client, collection, {"knowledge_version": target_version}
    )
    logger.info(
        "bootstrap_knowledge: 完成，已写入 knowledge_version=%s", target_version
    )
