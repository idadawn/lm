"""Sync API 模块 — 全量重建等管理接口."""

from typing import Any

from fastapi import APIRouter

from app.core.config import settings
from app.core.database import AsyncSessionLocal
from app.knowledge_graph.manager import refresh_knowledge_graph
from sqlalchemy import text

router = APIRouter()


@router.post("/resync-now")
async def resync_now() -> dict[str, Any]:
    """全量重建知识图谱（兼容前端重建按钮）.

    当 Neo4j 禁用时，返回数据库中的规则/规格数量，不报错。
    """
    refreshed = False
    if settings.NEO4J_ENABLED:
        refreshed = await refresh_knowledge_graph()

    async with AsyncSessionLocal() as session:
        rule_result = await session.execute(
            text("SELECT COUNT(*) FROM lab_intermediate_data_judgment_level WHERE F_DeleteMark = 0 OR F_DeleteMark IS NULL")
        )
        rule_count = rule_result.scalar() or 0

        spec_result = await session.execute(
            text("SELECT COUNT(*) FROM lab_product_spec WHERE F_DeleteMark = 0 OR F_DeleteMark IS NULL")
        )
        spec_count = spec_result.scalar() or 0

    return {
        "rules": int(rule_count),
        "specs": int(spec_count),
        "refreshed": refreshed,
        "message": "重建完成" if refreshed else "Neo4j 未启用，仅返回数据库统计",
    }
