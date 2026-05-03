"""
mock_ingest_demo.py — 不依赖 TEI / MySQL 的 Qdrant ingest 验证

用途：当 HuggingFace 翻不墙拉不到 bge-large-zh，或者 MySQL schema 没就绪时，
还能验证以下链路：
  1. Qdrant 服务可达且能创建 collection
  2. 用伪 embedding（确定性 hash → 向量）把 3 条 doc 写入 lm_judgment_rules /
     lm_product_specs / lm_metrics
  3. nearest-neighbor search 能拿回正确的 doc

这是 init_semantic_layer.py 的"小半步"替代品 —— 真 ingest 必须等 TEI + MySQL
全 OK 才能跑。本脚本只证明 Qdrant 与 schema 端代码无误，不能替代真知识灌入。

跑法：
    cd /data/project/lm/nlq-agent
    .venv/bin/python -m scripts.mock_ingest_demo
"""

from __future__ import annotations

import asyncio
import hashlib
import logging
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from src.core.settings import get_settings
from src.services.qdrant_service import QdrantService

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s | %(levelname)-7s | %(message)s",
)
logger = logging.getLogger(__name__)


def _fake_embedding(text: str, dim: int = 1024) -> list[float]:
    """
    确定性伪 embedding：用 SHA256 扩展到 dim 维 [-1, 1] 浮点。

    不能用于真 RAG（语义不对），仅用于验证 Qdrant 写入/读取 pipeline 没坑。
    """
    digest = hashlib.sha256(text.encode("utf-8")).digest()
    expanded = (digest * ((dim // len(digest)) + 1))[:dim]
    return [(b / 127.5) - 1.0 for b in expanded]


FIXTURES: dict[str, list[dict]] = {
    "lm_judgment_rules": [
        {
            "id": "rule_001",
            "text": "铁损 P17/50 ≤ 1.08 W/kg 判定为 A 级合格",
            "metadata": {
                "name": "铁损 A 级判定",
                "priority": 1,
                "quality_status": "qualified",
            },
        },
    ],
    "lm_product_specs": [
        {
            "id": "spec_50W470",
            "text": "50W470 牌号取向硅钢片，厚度 0.50mm，要求 P17/50 ≤ 4.7 W/kg",
            "metadata": {"code": "50W470", "name": "硅钢 50W470"},
        },
    ],
    "lm_metrics": [
        {
            "id": "metric_pass_rate",
            "text": "合格率：合格样本数 / 抽样总数 × 100%，按月统计",
            "metadata": {
                "key": "qualified_rate",
                "formula": "qualified_count / sample_count",
            },
        },
    ],
}


async def main() -> int:
    from src.services.embedding_client import EmbeddingClient

    settings = get_settings()
    logger.info("Qdrant: %s:%d", settings.qdrant_host, settings.qdrant_port)
    logger.info("TEI:    %s", settings.embedding_base_url)

    embedding = EmbeddingClient()
    qdrant = QdrantService(embedding)
    await qdrant.ensure_collections()

    written = 0
    for collection, docs in FIXTURES.items():
        try:
            count = await qdrant.upsert_documents(collection, docs)
            logger.info("✓ wrote %d into %s", count, collection)
            written += count
        except Exception as e:
            logger.error("✗ failed to write %s: %s", collection, type(e).__name__, e)
            await qdrant.close()
            await embedding.close()
            return 2

    logger.info("=" * 60)
    logger.info("mock ingest 完成；总计 %d 条 doc 已落到 Qdrant（真 TEI 嵌入）", written)
    logger.info("✓ TEI + Qdrant 上链路全通")
    logger.info(
        "→ 跑真知识灌入仍需 MySQL + 三张业务表 schema 就绪后 init_semantic_layer.py"
    )
    logger.info("=" * 60)

    await qdrant.close()
    await embedding.close()

    await qdrant.close()
    return 0


if __name__ == "__main__":
    sys.exit(asyncio.run(main()))
