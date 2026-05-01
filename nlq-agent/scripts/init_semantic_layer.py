"""
语义层初始化脚本

从 MySQL 中读取判定规则、产品规格和公式定义，
向量化后写入 Qdrant，完成语义层的初始构建。

使用方式：
    python -m scripts.init_semantic_layer

注意：需要先启动 Qdrant 和 TEI 服务。
"""

from __future__ import annotations

import asyncio
import logging
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from src.services.resync_service import bulk_resync_all  # noqa: E402

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s | %(levelname)-7s | %(message)s",
)
logger = logging.getLogger(__name__)


async def main() -> None:
    result = await bulk_resync_all()
    logger.info(
        "语义层初始化完成: rules=%d, specs=%d, duration=%dms",
        result["rules"],
        result["specs"],
        result["duration_ms"],
    )


if __name__ == "__main__":
    asyncio.run(main())
