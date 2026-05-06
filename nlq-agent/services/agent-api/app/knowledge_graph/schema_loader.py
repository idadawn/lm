"""Schema loader for Chat2SQL.

Scans information_schema for all ``lab_*`` tables, captures column comments,
and infers foreign-key edges from naming convention (``F_<ENT>_ID`` →
``lab_<ent>.F_Id``). Persisted in a module-level cache for fast prompt
injection at intent-routing time.

Why not Neo4j here:
    The Neo4j KG already serves rule/spec/grade reasoning. Adding hundreds of
    schema columns to it makes the graph noisy and slow. A pure in-memory
    Python dict is the right primitive—we read it on every Chat2SQL request,
    rebuild on demand via ``refresh_schema_cache()``.
"""

from __future__ import annotations

import logging
from dataclasses import dataclass, field
from typing import Any

from sqlalchemy import text

from app.core.database import AsyncSessionLocal

logger = logging.getLogger("nlq-agent")

# --------------------------------------------------------------------------- #
# Dataclasses
# --------------------------------------------------------------------------- #


@dataclass
class ColumnInfo:
    name: str
    data_type: str
    is_nullable: bool
    comment: str
    is_primary_key: bool = False
    is_foreign_key_candidate: bool = False
    fk_target: str | None = None  # "table.column"


@dataclass
class TableInfo:
    name: str
    comment: str
    columns: list[ColumnInfo] = field(default_factory=list)

    def column_map(self) -> dict[str, ColumnInfo]:
        return {c.name: c for c in self.columns}


@dataclass
class SchemaCache:
    tables: dict[str, TableInfo] = field(default_factory=dict)
    # 中文术语 → 候选 (table, column) 列表
    glossary: dict[str, list[tuple[str, str]]] = field(default_factory=dict)
    loaded: bool = False

    def find_tables(self, keywords: list[str]) -> list[TableInfo]:
        """Return tables whose comment OR name matches any keyword."""
        hits: list[TableInfo] = []
        seen: set[str] = set()
        for kw in keywords:
            kw_lower = kw.lower()
            for tbl in self.tables.values():
                if tbl.name in seen:
                    continue
                if (
                    kw_lower in tbl.name.lower()
                    or kw in tbl.comment
                    or any(kw in col.comment for col in tbl.columns)
                ):
                    hits.append(tbl)
                    seen.add(tbl.name)
        return hits

    def relevant_columns(
        self, table_name: str, keywords: list[str], max_cols: int = 30
    ) -> list[ColumnInfo]:
        """Return columns whose name or comment matches any keyword.

        Always includes PK + FK candidates so JOIN logic stays sound. Caps at
        ``max_cols`` to keep LLM prompt size sane (some tables have 130+ cols).
        """
        tbl = self.tables.get(table_name)
        if not tbl:
            return []
        always = [c for c in tbl.columns if c.is_primary_key or c.is_foreign_key_candidate]
        matched: list[ColumnInfo] = []
        for kw in keywords:
            kw_lower = kw.lower()
            for col in tbl.columns:
                if col in always or col in matched:
                    continue
                if (
                    kw_lower in col.name.lower()
                    or kw in col.comment
                ):
                    matched.append(col)
        # 也总是把常用业务列加进来（如果存在）
        common_priority = [
            "F_DETECTION_DATE",
            "F_FURNACE_NO",
            "F_FURNACE_BATCH_NO",
            "F_SHIFT",
            "F_LABELING",
            "F_FIRST_INSPECTION",
            "F_PRODUCT_SPEC_ID",
            "F_RAW_DATA_ID",
            "F_SINGLE_COIL_WEIGHT",
        ]
        col_index = tbl.column_map()
        priority_existing = [col_index[n] for n in common_priority if n in col_index and col_index[n] not in always and col_index[n] not in matched]

        result = always + matched + priority_existing
        return result[:max_cols]


_CACHE = SchemaCache()


# --------------------------------------------------------------------------- #
# Loader
# --------------------------------------------------------------------------- #


_FK_SUFFIX = "_ID"


def _infer_fk_target(column_name: str, all_table_names: set[str]) -> str | None:
    """Guess FK target by naming convention: F_RAW_DATA_ID → lab_raw_data.F_Id.

    F_<ENTITY>_ID 大写部分作为表名后缀（去掉 lab_ 前缀比较）。
    """
    upper = column_name.upper()
    if not upper.startswith("F_") or not upper.endswith(_FK_SUFFIX):
        return None
    middle = upper[2:-len(_FK_SUFFIX)]  # e.g. RAW_DATA / PRODUCT_SPEC
    if not middle:
        return None
    # 候选表名：lab_<middle 小写>
    candidate = f"lab_{middle.lower()}"
    if candidate in all_table_names:
        return f"{candidate}.F_Id"
    # F_PRODUCT_SPEC_ID → lab_product_spec.F_Id (already covered)
    # F_FORMULA_ID → 不直接对应 lab_formula；返回 None 让 LLM 自己判断
    return None


async def refresh_schema_cache() -> SchemaCache:
    """Read information_schema, populate _CACHE, also pull formula glossary."""
    async with AsyncSessionLocal() as session:
        tables_result = await session.execute(
            text(
                "SELECT TABLE_NAME, IFNULL(TABLE_COMMENT,'') AS TABLE_COMMENT "
                "FROM information_schema.TABLES "
                "WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME LIKE 'lab_%' "
                "ORDER BY TABLE_NAME"
            )
        )
        tables: dict[str, TableInfo] = {}
        for row in tables_result.mappings().all():
            name = row["TABLE_NAME"]
            tables[name] = TableInfo(name=name, comment=row["TABLE_COMMENT"] or "")

        all_names = set(tables.keys())

        cols_result = await session.execute(
            text(
                "SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, "
                "       IFNULL(COLUMN_COMMENT,'') AS COLUMN_COMMENT, COLUMN_KEY "
                "FROM information_schema.COLUMNS "
                "WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME LIKE 'lab_%' "
                "ORDER BY TABLE_NAME, ORDINAL_POSITION"
            )
        )
        for row in cols_result.mappings().all():
            tbl_name = row["TABLE_NAME"]
            tbl = tables.get(tbl_name)
            if tbl is None:
                continue
            col_name = row["COLUMN_NAME"]
            fk_target = _infer_fk_target(col_name, all_names)
            tbl.columns.append(
                ColumnInfo(
                    name=col_name,
                    data_type=row["COLUMN_TYPE"],
                    is_nullable=(row["IS_NULLABLE"] == "YES"),
                    comment=row["COLUMN_COMMENT"] or "",
                    is_primary_key=(row["COLUMN_KEY"] == "PRI"),
                    is_foreign_key_candidate=(fk_target is not None),
                    fk_target=fk_target,
                )
            )

        # Glossary: pull formula 中文名 → DB column from lab_intermediate_data_formula
        glossary: dict[str, list[tuple[str, str]]] = {}
        try:
            formulas = await session.execute(
                text(
                    "SELECT F_FORMULA_NAME, F_COLUMN_NAME, F_UNIT_NAME, F_REMARK "
                    "FROM lab_intermediate_data_formula "
                    "WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0) AND F_ENABLEDMARK = 1"
                )
            )
            col_lookup = tables.get("lab_intermediate_data", TableInfo("", "")).column_map()
            for row in formulas.mappings().all():
                cn_term = (row.get("F_FORMULA_NAME") or "").strip()
                col_name = (row.get("F_COLUMN_NAME") or "").strip()
                if not cn_term or not col_name:
                    continue
                # F_COLUMN_NAME 可能是去前缀的英文名（如 PerfPsLoss）；尝试匹配 F_PERF_PS_LOSS 等。
                resolved = _resolve_formula_column(col_name, col_lookup)
                if resolved:
                    glossary.setdefault(cn_term, []).append(("lab_intermediate_data", resolved))
        except Exception as exc:  # noqa: BLE001
            logger.warning(f"[schema_loader] glossary load failed: {exc}")

        _CACHE.tables = tables
        _CACHE.glossary = glossary
        _CACHE.loaded = True
        logger.info(
            f"[schema_loader] loaded {len(tables)} lab_* tables, "
            f"{sum(len(t.columns) for t in tables.values())} columns, "
            f"{len(glossary)} 中文术语"
        )
        return _CACHE


def _resolve_formula_column(raw: str, col_index: dict[str, ColumnInfo]) -> str | None:
    """Map formula table's F_COLUMN_NAME (e.g. 'PerfPsLoss') to actual F_PERF_PS_LOSS."""
    if raw in col_index:
        return raw
    candidates = [
        f"F_{raw.upper()}",
        f"F_{_camel_to_snake(raw).upper()}",
    ]
    for c in candidates:
        if c in col_index:
            return c
    return None


def _camel_to_snake(s: str) -> str:
    out = []
    for i, ch in enumerate(s):
        if ch.isupper() and i > 0:
            out.append("_")
        out.append(ch)
    return "".join(out)


def get_schema_cache() -> SchemaCache:
    """Return the cached SchemaCache. Caller should ensure refresh_schema_cache() ran at startup."""
    return _CACHE
