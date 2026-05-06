"""
nlq-vanna-service CLI.

Usage:
    python -m app.cli <subcommand> [options]

Subcommands:
    reindex         Rebuild the Qdrant knowledge collection from scratch.
                    Requires typing the collection name to confirm (prevent
                    accidental wipes — ADR-2 requirement).

    dump-ddl        Print DDL for one or all lab tables.
                    (Placeholder — real implementation depends on Wave 2
                    ddl_loader.py)

    list-knowledge  List all points currently stored in the Qdrant collection,
                    grouped by subtype.

    snapshot        Create a Qdrant collection snapshot and copy it to
                    snapshots/demo-YYYYMMDD.snapshot for demo archiving.
"""

from __future__ import annotations

import argparse
import datetime
import logging
import os
import shutil
import sys
import urllib.request
import json


def _get_settings():
    """Import settings lazily to avoid heavy imports at module load."""
    from app.config import get_settings
    return get_settings()


# ---------------------------------------------------------------------------
# reindex
# ---------------------------------------------------------------------------

def cmd_reindex(args: argparse.Namespace) -> int:
    settings = _get_settings()
    collection = settings.qdrant_collection

    print(f"\n[reindex] Target collection: '{collection}'")
    print("WARNING: This will DELETE and RECREATE the entire knowledge collection.")
    print("All existing vectors will be permanently removed.\n")
    confirmation = input(f"Type the collection name to confirm: ").strip()

    if confirmation != collection:
        print(
            f"\nAborted. You entered '{confirmation}' but the collection is '{collection}'."
        )
        return 1

    print(f"\nConfirmed. Starting reindex of '{collection}'...")

    try:
        from app.knowledge.bootstrap import bootstrap_knowledge  # type: ignore[import]
        import asyncio
        from app.deps import get_vanna_app
        from qdrant_client import QdrantClient

        vn = get_vanna_app()
        client = QdrantClient(url=settings.qdrant_url)
        try:
            client.delete_collection(collection_name=collection)
            print(f"Deleted existing collection: {collection}")
        except Exception as exc:  # noqa: BLE001
            print(f"Note: collection may not exist yet: {exc}")
        asyncio.run(bootstrap_knowledge(vn, settings, force=True))
        print("Reindex complete.")
        return 0
    except ImportError:
        print(
            "ERROR: app.knowledge.bootstrap is not yet implemented (Wave 2).\n"
            "Skeleton is in place — reindex will work after Wave 2 lands."
        )
        return 1


# ---------------------------------------------------------------------------
# dump-ddl
# ---------------------------------------------------------------------------

def cmd_dump_ddl(args: argparse.Namespace) -> int:
    """Print DDL for one or all lab tables."""
    settings = _get_settings()
    table = args.table

    from app.adapters.mysql_runner import MysqlRunner
    from app.knowledge.ddl_loader import dump_all_lab_tables, dump_ddl_for_table

    runner = MysqlRunner(
        {
            "mysql_host": settings.mysql_host,
            "mysql_port": settings.mysql_port,
            "mysql_user": settings.mysql_user,
            "mysql_password": settings.mysql_password,
            "mysql_db": settings.mysql_database,
        }
    )

    try:
        if table:
            ddl = dump_ddl_for_table(runner, runner._db, table.upper())
            print(ddl)
        else:
            ddl_map = dump_all_lab_tables(runner)
            for tname, ddl in ddl_map.items():
                print(f"-- Table: {tname}")
                print(ddl)
                print()
        return 0
    except Exception as exc:
        print(f"ERROR: {exc}")
        return 1


# ---------------------------------------------------------------------------
# list-knowledge
# ---------------------------------------------------------------------------

def cmd_list_knowledge(args: argparse.Namespace) -> int:
    """List Qdrant points grouped by subtype."""
    settings = _get_settings()

    try:
        from qdrant_client import QdrantClient
    except ImportError:
        print("ERROR: qdrant-client is not installed. Run: uv sync")
        return 1

    client = QdrantClient(url=settings.qdrant_url)
    collection = settings.qdrant_collection

    try:
        result = client.scroll(
            collection_name=collection,
            limit=1000,
            with_payload=True,
            with_vectors=False,
        )
    except Exception as exc:
        print(f"ERROR: Could not connect to Qdrant at {settings.qdrant_url}: {exc}")
        return 1

    points, _ = result
    if not points:
        print(f"Collection '{collection}' is empty.")
        return 0

    by_subtype: dict[str, list] = {}
    for point in points:
        payload = point.payload or {}
        subtype = str(payload.get("subtype", payload.get("type", "unknown")))
        if subtype == "__meta__":
            continue  # exclude metadata sentinel points from listing
        by_subtype.setdefault(subtype, []).append(point.id)

    print(f"\nCollection: {collection}  ({len(points)} points total)")
    for subtype, ids in sorted(by_subtype.items()):
        print(f"  {subtype:20s}: {len(ids)} points")
    return 0


# ---------------------------------------------------------------------------
# snapshot
# ---------------------------------------------------------------------------

def cmd_snapshot(args: argparse.Namespace) -> int:
    """Create a Qdrant collection snapshot and store it in snapshots/."""
    settings = _get_settings()
    collection = settings.qdrant_collection
    qdrant_url = settings.qdrant_url.rstrip("/")

    # POST to Qdrant snapshots API
    snapshot_url = f"{qdrant_url}/collections/{collection}/snapshots"
    print(f"Creating snapshot for collection '{collection}'...")

    req = urllib.request.Request(
        snapshot_url,
        method="POST",
        headers={"Content-Type": "application/json"},
    )
    try:
        with urllib.request.urlopen(req, timeout=60) as resp:
            body = json.loads(resp.read().decode())
    except Exception as exc:
        print(f"ERROR: Qdrant snapshot request failed: {exc}")
        return 1

    snapshot_name: str = body.get("result", {}).get("name", "")
    if not snapshot_name:
        print(f"ERROR: Unexpected response from Qdrant: {body}")
        return 1

    # Download snapshot file
    download_url = f"{qdrant_url}/collections/{collection}/snapshots/{snapshot_name}"
    date_str = datetime.date.today().strftime("%Y%m%d")
    dest_dir = os.path.join(os.path.dirname(os.path.dirname(__file__)), "snapshots")
    os.makedirs(dest_dir, exist_ok=True)
    dest_path = os.path.join(dest_dir, f"demo-{date_str}.snapshot")

    print(f"Downloading snapshot from {download_url} ...")
    try:
        urllib.request.urlretrieve(download_url, dest_path)
    except Exception as exc:
        print(f"ERROR: Failed to download snapshot: {exc}")
        return 1

    print(f"Snapshot saved: {dest_path}")
    return 0


# ---------------------------------------------------------------------------
# Argument parser
# ---------------------------------------------------------------------------

def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="python -m app.cli",
        description="nlq-vanna-service CLI",
    )
    sub = parser.add_subparsers(dest="command", required=True)

    # reindex
    sub.add_parser("reindex", help="Rebuild Qdrant knowledge collection (with confirmation)")

    # dump-ddl
    p_ddl = sub.add_parser("dump-ddl", help="Print DDL for lab tables")
    p_ddl.add_argument(
        "table",
        nargs="?",
        default=None,
        help="Table name (omit for all tables)",
    )

    # list-knowledge
    sub.add_parser("list-knowledge", help="List Qdrant collection points by subtype")

    # snapshot
    sub.add_parser(
        "snapshot",
        help="Create Qdrant snapshot -> snapshots/demo-YYYYMMDD.snapshot",
    )

    return parser


def main() -> None:
    logging.basicConfig(level="INFO", format="%(levelname)s: %(message)s")
    parser = build_parser()
    args = parser.parse_args()

    handlers = {
        "reindex": cmd_reindex,
        "dump-ddl": cmd_dump_ddl,
        "list-knowledge": cmd_list_knowledge,
        "snapshot": cmd_snapshot,
    }

    handler = handlers.get(args.command)
    if handler is None:
        parser.print_help()
        sys.exit(1)

    sys.exit(handler(args))


if __name__ == "__main__":
    main()
