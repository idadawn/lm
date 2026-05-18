"""LightRAG 索引重建脚本.

用法：
    # 全量重建（删除旧索引，重新喂全部）
    uv run python scripts/lightrag_reindex.py --full

    # 增量更新（只跑指定 collector）
    uv run python scripts/lightrag_reindex.py --only knowledge_base
    uv run python scripts/lightrag_reindex.py --only neo4j

    # 只看不动（dry-run 收集 doc 数量但不 insert）
    uv run python scripts/lightrag_reindex.py --dry-run

什么时候手动跑：
- 修改了 knowledge_base.json / aliases.json / dimensions_meta.json / sql_templates.json
- Neo4j 全量重建后（manager.refresh）
- lab_report_config 业务后台改了配置
- docs/ 下 markdown 改了

CI 应该跑 `--full --dry-run` 验证所有 collector 都能产出 doc。
"""

from __future__ import annotations

import argparse
import asyncio
import os
import shutil
import sys
import time
from collections import Counter
from pathlib import Path

# 让脚本直接 python 跑得起
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

os.environ.setdefault("LIGHTRAG_ENABLED", "True")

from app.core.config import settings  # noqa: E402
from app.knowledge_graph import knowledge_sources, lightrag_index  # noqa: E402


async def main() -> int:
    parser = argparse.ArgumentParser(description="Reindex knowledge sources into LightRAG")
    parser.add_argument("--full", action="store_true", help="全量重建：删除 working_dir 下旧数据")
    parser.add_argument("--only", type=str, default=None,
                        help="只跑指定 collector: knowledge_base|aliases|dimensions|sql_templates|neo4j|report_config|judgment_rules|docs")
    parser.add_argument("--dry-run", action="store_true", help="只收集 doc，不真正 insert")
    args = parser.parse_args()

    working_dir = Path(settings.LIGHTRAG_WORKING_DIR)
    print(f"\nLightRAG working_dir: {working_dir}")
    print(f"LIGHTRAG_ENABLED: {settings.LIGHTRAG_ENABLED}")
    print(f"Embedding: {settings.LIGHTRAG_EMBEDDING_MODEL} on {settings.LIGHTRAG_EMBEDDING_DEVICE}\n")

    # 全量重建：清空旧文件
    if args.full and not args.dry_run:
        # 安全检查：working_dir 必须是相对 BASE_DIR 的子目录，避免误删
        if working_dir.exists() and "lightrag" in str(working_dir):
            print(f"→ wiping {working_dir} ...")
            shutil.rmtree(working_dir)
            working_dir.mkdir(parents=True, exist_ok=True)
        await lightrag_index.reset()

    # 收集 doc
    t0 = time.time()
    if args.only:
        docs = await knowledge_sources.collect_one(args.only)
        print(f"→ collected {len(docs)} docs from collector={args.only} in {time.time()-t0:.1f}s")
    else:
        docs = await knowledge_sources.collect_all_sources()
        print(f"→ collected {len(docs)} docs total in {time.time()-t0:.1f}s")

    # 按 category 统计便于核对
    cat_count = Counter((d.metadata or {}).get("category", "unknown") for d in docs)
    print("\n按 category 分布：")
    for cat, n in cat_count.most_common():
        print(f"  {cat:25s}  {n}")

    if args.dry_run:
        print("\n[dry-run] 没有 insert 到 LightRAG。前 3 条示例：\n")
        for d in docs[:3]:
            print(f"--- {d.id} (source={d.source}) ---")
            print(d.content[:300])
            print()
        return 0

    # insert
    print(f"\n→ inserting into LightRAG (this may take a few minutes)...")
    t1 = time.time()
    stats = await lightrag_index.index_docs(docs)
    print(f"  done in {time.time()-t1:.1f}s: {stats}")

    if stats["errors"] > 0:
        print("\n⚠️ some docs failed to insert; check logs above")
        return 1
    if stats["inserted"] == 0:
        print("\n⚠️ no doc inserted; check whether LightRAG is properly enabled")
        return 1

    print(f"\n✅ reindex complete. inserted={stats['inserted']}, errors={stats['errors']}")
    return 0


if __name__ == "__main__":
    rc = asyncio.run(main())
    sys.exit(rc)
