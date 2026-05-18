"""把全部知识源序列化成 LightRAG 文档片段.

设计原则
--------
1. **每类知识一个 collector**：方便单独 reindex 某一类
2. **任何源失败都不影响其他源**：每个 collector 包 try/except 返回 []
3. **片段内容是给 LLM 看的人话**：不是裸 JSON dump，要可读
4. **source 字段稳定**：用于 citation；同一节点多次 reindex 用同样的 source
5. **id 用稳定 hash**：方便后续做增量更新（id 不变就 dedup）

数据源清单
----------
1. Neo4j 节点（ontology）—— 所有白名单节点类型
2. aliases.json —— 术语同义词词典
3. dimensions_meta.json —— 维度元数据（时间/班次/产线/规格/炉号）
4. sql_templates.json —— 6 个 SQL 模板
5. knowledge_base.json —— 手工 9 条业务定义
6. lab_report_config —— DB 表：指标定义
7. lab_intermediate_data_judgment_level —— DB 表：判定规则
8. docs/*.md —— markdown 文档（按 section 切片）

外部入口：
    await collect_all_sources()  返回 list[LightRAGDoc]
    await collect_one(source_key) 只跑一个 collector，调试用
"""

from __future__ import annotations

import json
import logging
import re
from pathlib import Path
from typing import Any, Callable, Awaitable

from app.knowledge_graph.lightrag_index import LightRAGDoc, build_lightrag_doc

logger = logging.getLogger("nlq-agent.knowledge_sources")

_KG_DIR = Path(__file__).parent
_NLQ_AGENT_ROOT = _KG_DIR.parent.parent.parent.parent  # nlq-agent/
_DOCS_DIR = _NLQ_AGENT_ROOT / "docs"


# --------------------------------------------------------------------------- #
# 公开入口
# --------------------------------------------------------------------------- #

async def collect_all_sources() -> list[LightRAGDoc]:
    """收集全部知识源。任一 collector 失败不影响其他。"""
    collectors: list[tuple[str, Callable[[], Awaitable[list[LightRAGDoc]]]]] = [
        ("knowledge_base.json", _from_knowledge_base_json),
        ("aliases.json", _from_aliases_json),
        ("dimensions_meta.json", _from_dimensions_meta),
        ("sql_templates.json", _from_sql_templates),
        ("Neo4j nodes", _from_neo4j_nodes),
        ("lab_report_config", _from_lab_report_config),
        ("lab_intermediate_data_judgment_level", _from_judgment_rules_table),
        ("docs/*.md", _from_docs_markdown),
    ]
    out: list[LightRAGDoc] = []
    for name, fn in collectors:
        try:
            docs = await fn()
            logger.info("[knowledge_sources] %s → %d docs", name, len(docs))
            out.extend(docs)
        except Exception:
            logger.exception("[knowledge_sources] %s collector failed; skipping", name)
    logger.info("[knowledge_sources] total: %d docs", len(out))
    return out


async def collect_one(source_key: str) -> list[LightRAGDoc]:
    """单独跑一个 collector，调试用。source_key 是上面的 name 字符串。"""
    mapping: dict[str, Callable[[], Awaitable[list[LightRAGDoc]]]] = {
        "knowledge_base": _from_knowledge_base_json,
        "aliases": _from_aliases_json,
        "dimensions": _from_dimensions_meta,
        "sql_templates": _from_sql_templates,
        "neo4j": _from_neo4j_nodes,
        "report_config": _from_lab_report_config,
        "judgment_rules": _from_judgment_rules_table,
        "docs": _from_docs_markdown,
    }
    fn = mapping.get(source_key)
    if not fn:
        raise ValueError(f"unknown source_key={source_key}; valid: {list(mapping.keys())}")
    return await fn()


# --------------------------------------------------------------------------- #
# Collector 1: knowledge_base.json
# --------------------------------------------------------------------------- #

async def _from_knowledge_base_json() -> list[LightRAGDoc]:
    """从手工维护的 knowledge_base.json 读取 9 条权威定义。"""
    path = _KG_DIR / "knowledge_base.json"
    if not path.exists():
        return []
    data = json.loads(path.read_text(encoding="utf-8"))
    out: list[LightRAGDoc] = []
    for entry in data.get("entries", []):
        # answer 是 markdown，直接拿；title + aliases 拼成首句加强 embedding 召回
        first_line = entry.get("title") or entry.get("topic", "")
        aliases_line = "（也叫：" + "、".join(entry.get("aliases", [])[:5]) + "）" if entry.get("aliases") else ""
        content = f"{first_line}{aliases_line}\n\n{entry.get('answer', '')}"
        out.append(build_lightrag_doc(
            id=f"kb:{entry['id']}",
            content=content,
            source=f"knowledge_base.json#{entry['id']}",
            metadata={
                "category": entry.get("category", "term"),
                "topic": entry.get("topic", ""),
                "kb_entry_id": entry.get("id", ""),
            },
        ))
    return out


# --------------------------------------------------------------------------- #
# Collector 2: aliases.json
# --------------------------------------------------------------------------- #

async def _from_aliases_json() -> list[LightRAGDoc]:
    """术语同义词词典 → 每个分类一份文档。"""
    path = _KG_DIR / "aliases.json"
    if not path.exists():
        return []
    data = json.loads(path.read_text(encoding="utf-8"))
    cat_zh = {
        "formula": "公式 / 指标",
        "metric": "业务术语",
        "appearance": "外观特性",
        "field": "数据库字段",
    }
    out: list[LightRAGDoc] = []
    for category, mapping in data.items():
        if category.startswith("_") or not isinstance(mapping, dict):
            continue
        zh_cat = cat_zh.get(category, category)
        lines = [f"# {zh_cat}同义词词典（来自 aliases.json）", ""]
        for canonical, aliases in mapping.items():
            if not isinstance(aliases, list):
                continue
            lines.append(f"- **{canonical}**：{', '.join(aliases)}")
        out.append(build_lightrag_doc(
            id=f"aliases:{category}",
            content="\n".join(lines),
            source=f"aliases.json#{category}",
            metadata={"category": "term_alias", "alias_group": category},
        ))
    return out


# --------------------------------------------------------------------------- #
# Collector 3: dimensions_meta.json
# --------------------------------------------------------------------------- #

async def _from_dimensions_meta() -> list[LightRAGDoc]:
    """维度元数据：时间 / 班次 / 产线 / 规格 / 炉号。每维度一份文档。"""
    path = _KG_DIR / "dimensions_meta.json"
    if not path.exists():
        return []
    data = json.loads(path.read_text(encoding="utf-8"))
    out: list[LightRAGDoc] = []
    for dim_key, dim in data.items():
        if dim_key.startswith("_") or not isinstance(dim, dict):
            continue
        field = dim.get("field", "")
        aliases = dim.get("aliases", [])
        lines = [
            f"# 维度：{dim_key}",
            f"主字段：`{field}`",
            f"中文别名：{', '.join(aliases)}" if aliases else "",
        ]
        # 时间维度有 common_ranges
        if "common_ranges" in dim:
            lines.append("\n**支持的快速时间区间**：")
            for r in dim["common_ranges"]:
                lines.append(f"- `{r.get('expr')}`（别名：{', '.join(r.get('aliases', []))}）")
        # 班次维度有 values
        if "values" in dim:
            lines.append("\n**取值**：")
            for v in dim["values"]:
                lines.append(f"- `{v.get('code')}`（数字 {v.get('numeric')}，别名：{', '.join(v.get('aliases', []))}）")
        # 产线维度有 value_aliases
        if "value_aliases" in dim:
            lines.append("\n**取值别名**：")
            for k, vs in dim["value_aliases"].items():
                lines.append(f"- `{k}`：{', '.join(vs)}")
        # 炉号维度有 raw_field
        if dim_key == "furnace_no":
            lines.append(f"\n原始字段：`{dim.get('raw_field')}`（格式化前），别名：{', '.join(dim.get('raw_aliases', []))}")
        out.append(build_lightrag_doc(
            id=f"dim:{dim_key}",
            content="\n".join(filter(None, lines)),
            source=f"dimensions_meta.json#{dim_key}",
            metadata={"category": "dimension", "dimension": dim_key, "db_field": field},
        ))
    return out


# --------------------------------------------------------------------------- #
# Collector 4: sql_templates.json
# --------------------------------------------------------------------------- #

async def _from_sql_templates() -> list[LightRAGDoc]:
    """6 个 SQL 模板 → 每个模板一份文档。"""
    path = _KG_DIR / "sql_templates.json"
    if not path.exists():
        return []
    data = json.loads(path.read_text(encoding="utf-8"))
    out: list[LightRAGDoc] = []
    for tpl in data.get("templates", []):
        sample = "、".join(tpl.get("sample_questions", []))
        params = ", ".join(
            f"{p.get('name')}({p.get('type', '')})"
            for p in tpl.get("parameters", [])
        )
        content = (
            f"# SQL 模板：{tpl.get('name', '')}\n"
            f"用途：{tpl.get('description', '')}\n"
            f"参数：{params}\n"
            f"典型用户问法：{sample}\n\n"
            f"SQL 骨架（用于 LLM 生成时参考）：\n```sql\n{tpl.get('sql_template', '')}\n```"
        )
        out.append(build_lightrag_doc(
            id=f"sql_tpl:{tpl.get('id')}",
            content=content,
            source=f"sql_templates.json#{tpl.get('id')}",
            metadata={"category": "sql_template", "template_id": tpl.get("id")},
        ))
    return out


# --------------------------------------------------------------------------- #
# Collector 5: Neo4j 节点（ontology）
# --------------------------------------------------------------------------- #

# 白名单节点类型 → 序列化器映射；新增节点类型在这里加 case
_NODE_SERIALIZERS: dict[str, Callable[[dict, list[dict]], tuple[str, str]]] = {}


def _serialize_node(node_type: str, node: dict, related: list[dict]) -> tuple[str, str]:
    """返回 (content, source) 的默认序列化器。"""
    serializer = _NODE_SERIALIZERS.get(node_type, _default_node_serializer)
    return serializer(node, related)


def _default_node_serializer(node: dict, related: list[dict]) -> tuple[str, str]:
    """通用：把节点所有非空属性平铺成 markdown。"""
    label = node.get("name") or node.get("code") or node.get("id") or "(unnamed)"
    lines = [f"# {label}"]
    for k, v in sorted(node.items()):
        if k in ("id", "name") or v in (None, ""):
            continue
        if isinstance(v, (dict, list)):
            v = json.dumps(v, ensure_ascii=False)
        lines.append(f"- **{k}**：{v}")
    if related:
        lines.append("\n**相关节点**：")
        for r in related[:10]:
            r_label = (r.get("name") or r.get("code") or r.get("id") or "")[:60]
            lines.append(f"- {r_label}")
    return "\n".join(lines), str(node.get("id", label))


async def _from_neo4j_nodes() -> list[LightRAGDoc]:
    """遍历 Neo4j 全部白名单节点。Neo4j 不可用时返回空。"""
    from app.knowledge_graph.manager import get_knowledge_graph

    graph = get_knowledge_graph()
    if graph is None:
        logger.info("[knowledge_sources] Neo4j not ready, skip neo4j collector")
        return []

    # 白名单跟 ontology 文档对齐
    node_types = [
        "ProductSpec", "SpecAttribute",
        "Formula", "JudgmentRule",
        "ReportConfig",
        "FurnaceNoInput", "FurnaceNoParsed", "FurnaceNoField",
        "AppearanceFeature", "AppearanceCategory", "AppearanceLevel",
    ]
    out: list[LightRAGDoc] = []
    for nt in node_types:
        try:
            rows = await graph.query_async(
                f"MATCH (n:{nt}) RETURN n LIMIT 500"
            )
        except Exception:
            logger.exception("[knowledge_sources] failed to query %s nodes", nt)
            continue
        for row in rows:
            node = dict(row.get("n") or {})
            content, ident = _serialize_node(nt, node, [])
            node_id = node.get("id") or ident
            out.append(build_lightrag_doc(
                id=f"neo4j:{nt}:{node_id}",
                content=content,
                source=f"Neo4j:{nt}:{node_id}",
                metadata={"category": "ontology_node", "node_type": nt, "node_id": str(node_id)},
            ))
    return out


# --------------------------------------------------------------------------- #
# Collector 6: lab_report_config
# --------------------------------------------------------------------------- #

async def _from_lab_report_config() -> list[LightRAGDoc]:
    """lab_report_config 表：所有命名指标定义。MySQL 不可用时返回空。"""
    try:
        from app.tools.query_tools import execute_safe_sql
    except Exception:
        return []

    sql = """
        SELECT F_Id, F_NAME, F_FORMULA_ID, F_LEVEL_NAMES, F_DESCRIPTION,
               F_COMPUTE_TYPE, F_DISPLAY_ORDER
        FROM lab_report_config
        WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)
        ORDER BY F_DISPLAY_ORDER
    """
    try:
        rows = await execute_safe_sql(sql, {})
    except Exception:
        logger.exception("[knowledge_sources] lab_report_config query failed")
        return []
    out: list[LightRAGDoc] = []
    for r in rows:
        name = (r.get("F_NAME") or "").strip() or f"指标_{r['F_Id']}"
        formula_id = r.get("F_FORMULA_ID") or ""
        levels = r.get("F_LEVEL_NAMES") or ""
        desc = r.get("F_DESCRIPTION") or ""
        compute_type = r.get("F_COMPUTE_TYPE") or ""
        content = (
            f"# 指标：{name}\n"
            f"公式 ID：`{formula_id}`\n"
            f"合格等级列表：`{levels}`\n"
            f"计算类型：{compute_type}\n"
            + (f"\n说明：{desc}" if desc else "")
        )
        out.append(build_lightrag_doc(
            id=f"report_config:{r['F_Id']}",
            content=content,
            source=f"lab_report_config#{r['F_Id']}({name})",
            metadata={
                "category": "metric_definition",
                "indicator_name": name,
                "formula_id": formula_id,
            },
        ))
    return out


# --------------------------------------------------------------------------- #
# Collector 7: lab_intermediate_data_judgment_level（判定规则）
# --------------------------------------------------------------------------- #

async def _from_judgment_rules_table() -> list[LightRAGDoc]:
    """判定规则表 — 每条规则一个 doc（含完整条件树自然语言渲染）。

    输出粒度：
    - 每条规则（spec × formula × grade）单独一个 doc
    - 用业务话术展开 conditionJson 里所有条件组（铁损/带宽/叠片系数/外观/断头数 等）
    - LightRAG 据此能精确召回"120 贴标 A 级"或"什么情况判为 A 级"这类问题

    数据全部来自 DB（lab_intermediate_data_judgment_level + lab_product_spec
    + lab_intermediate_data_formula），3am cron 触发 /kg/refresh 时会重新拉取。
    业务方在 /lab/intermediateDataJudgmentLevel 改了规则，**当晚 3 点自动生效**。
    """
    try:
        from app.tools.query_tools import execute_safe_sql
    except Exception:
        return []
    # 复用 query_agent 的渲染器和数据加载器（避免代码重复）
    try:
        from app.agents.query_agent import (
            render_condition_natural_text,
            _load_formula_var_map,
            _load_feature_id_maps,
        )
    except Exception:
        logger.exception("[knowledge_sources] failed to import condition renderer")
        return []

    sql = """
        SELECT l.F_Id, l.F_FORMULA_ID, l.F_FORMULA_NAME, l.F_PRODUCT_SPEC_ID,
               l.F_NAME AS grade_name, l.F_QUALITY_STATUS,
               l.F_CONDITION, l.F_PRIORITY, l.F_DESCRIPTION,
               p.F_CODE AS spec_code, p.F_NAME AS spec_name
        FROM lab_intermediate_data_judgment_level l
        LEFT JOIN lab_product_spec p
          ON l.F_PRODUCT_SPEC_ID COLLATE utf8mb4_unicode_ci = p.F_Id COLLATE utf8mb4_unicode_ci
        WHERE (l.F_DeleteMark IS NULL OR l.F_DeleteMark = 0)
        ORDER BY l.F_FORMULA_ID, p.F_CODE, l.F_PRIORITY DESC, l.F_NAME
    """
    try:
        rows = await execute_safe_sql(sql, {})
    except Exception:
        logger.exception("[knowledge_sources] judgment_rules query failed")
        return []

    # 一次性加载 $VAR / 外观 ID 映射（每条规则共享）
    try:
        var_map = await _load_formula_var_map()
    except Exception:
        var_map = {}
    try:
        category_map, level_map = await _load_feature_id_maps()
    except Exception:
        category_map, level_map = {}, {}

    formula_zh_map = {
        "Labeling": "贴标", "MagneticResult": "磁性能判定",
        "LaminationResult": "叠片系数判定", "ThicknessResult": "厚度判定",
        "FirstInspection": "一次交检",
    }

    out: list[LightRAGDoc] = []
    for r in rows:
        formula_id = (r.get("F_FORMULA_ID") or "").strip()
        formula_zh = (
            (r.get("F_FORMULA_NAME") or "").strip()
            or formula_zh_map.get(formula_id, formula_id)
        )
        spec_code = (r.get("spec_code") or "").strip() or "（未关联规格）"
        spec_label = (r.get("spec_name") or "").strip() or spec_code
        grade_name = (r.get("grade_name") or "").strip() or "未命名"
        qs = str(r.get("F_QUALITY_STATUS") or "")
        # 用户业务里 0/1 含义可能与代码常识相反，描述用"系统标记为"格式让用户自行解读
        status_text = "系统标记 0（请按业务定义解读）" if qs == "0" else (
            "系统标记 1（请按业务定义解读）" if qs == "1" else f"系统标记 {qs}"
        )
        priority = r.get("F_PRIORITY") or 0
        description = (r.get("F_DESCRIPTION") or "").strip()
        cond_text = render_condition_natural_text(
            r.get("F_CONDITION", ""), category_map, level_map, var_map
        )

        # 标题 + 多种问法的关键词都塞进去，便于 LightRAG 实体抽取
        content = (
            f"# 产品规格 {spec_code} · {formula_zh} · {grade_name} 等级 — 判定条件\n\n"
            f"**适用范围**：产品规格 {spec_label}（代码 {spec_code}）的{formula_zh}判定，"
            f"等级名「{grade_name}」，规则优先级 {priority}，{status_text}。\n\n"
            f"**满足以下所有条件组时被判为 {grade_name}**：\n\n{cond_text}\n"
        )
        if description:
            content += f"\n_备注：{description}_\n"

        out.append(build_lightrag_doc(
            id=f"judgment_rule:{formula_id}:{spec_code}:{grade_name}",
            content=content,
            source=f"lab_intermediate_data_judgment_level/{formula_id}/{spec_code}/{grade_name}",
            metadata={
                "category": "judgment_rule",
                "formula_id": formula_id,
                "formula_name": formula_zh,
                "spec_code": spec_code,
                "grade_name": grade_name,
                "priority": int(priority) if isinstance(priority, (int, str)) and str(priority).isdigit() else 0,
            },
        ))

    logger.info("[knowledge_sources] judgment_rules: built %d detailed docs", len(out))
    return out


# --------------------------------------------------------------------------- #
# Collector 8: docs/*.md
# --------------------------------------------------------------------------- #

async def _from_docs_markdown() -> list[LightRAGDoc]:
    """按 ## section 切片 markdown 文档；文件不存在返回空。"""
    if not _DOCS_DIR.exists():
        logger.info("[knowledge_sources] docs dir not found at %s", _DOCS_DIR)
        return []
    out: list[LightRAGDoc] = []
    for md_path in _DOCS_DIR.glob("*.md"):
        try:
            text = md_path.read_text(encoding="utf-8")
        except Exception:
            logger.warning("[knowledge_sources] cannot read %s", md_path)
            continue
        # 按 ## 二级标题切（保留 # 一级标题作为整体上下文）
        sections = _split_markdown_sections(text)
        for idx, (title, body) in enumerate(sections):
            content = f"# {md_path.stem} · {title}\n\n{body}"[:2000]
            out.append(build_lightrag_doc(
                id=f"docs:{md_path.stem}:{idx}",
                content=content,
                source=f"docs/{md_path.name}#{title or 'top'}",
                metadata={"category": "documentation", "doc_file": md_path.name, "section_idx": idx},
            ))
    return out


def _split_markdown_sections(text: str) -> list[tuple[str, str]]:
    """简单按 `## ` 切段。返回 [(section_title, body), ...]"""
    parts = re.split(r"\n(?=##\s+)", text)
    sections: list[tuple[str, str]] = []
    for i, part in enumerate(parts):
        first_line, _, rest = part.strip().partition("\n")
        title = first_line.lstrip("#").strip() if first_line.startswith("#") else f"section {i}"
        body = rest.strip()
        if body:
            sections.append((title, body))
    return sections
