"""知识图谱 API 模块.

提供知识图谱查询和管理接口.
"""

from typing import Any

from fastapi import APIRouter, HTTPException

from app.knowledge_graph import queries as kg_queries
from app.knowledge_graph.manager import (
    get_knowledge_graph,
    get_knowledge_graph_status,
    init_knowledge_graph,
    is_knowledge_graph_ready,
    refresh_knowledge_graph,
)
from app.models.schemas import (
    ExplainRequest,
    ExplainResponse,
    OntologyGraphDTO,
    OntologyNode,
    OntologyEdge,
    OntologyCombo,
    ReasoningStep,
    ResolveRequest,
    ResolvedEntity,
    SubgraphRequest,
    RibbonSearchResult,
    RibbonSubgraphResponse,
    AskRequest,
    AskResponse,
)
from app.tools.graph_tools import traverse_judgment_path
from app.core.database import AsyncSessionLocal
from sqlalchemy import text

router = APIRouter()


@router.get("/health")
async def knowledge_graph_health() -> dict[str, Any]:
    """检查知识图谱健康状态."""
    return get_knowledge_graph_status()


@router.post("/init")
async def init_graph() -> dict[str, Any]:
    """手动初始化知识图谱."""
    graph = await init_knowledge_graph(force=True)
    if graph is None:
        raise HTTPException(
            status_code=503,
            detail="Knowledge graph initialization failed. Check Neo4j configuration and connectivity.",
        )

    return {"message": "Knowledge graph initialized successfully"}


@router.post("/refresh")
async def refresh_graph(reindex_lightrag: bool = True) -> dict[str, Any]:
    """刷新知识图谱数据。

    流程：
    1. 重建 Neo4j：`MATCH (n) DETACH DELETE n` → 从 MySQL 全量重新插入节点和关系
    2. （可选）联动重建 LightRAG 索引中 Neo4j 来源的 doc 片段

    参数：
    - reindex_lightrag (默认 True)：是否在 Neo4j 刷新成功后增量重建 LightRAG 中 neo4j 来源的片段。
      关掉这个参数适用于：客户只想验证 Neo4j 数据，暂不需要语义检索同步。
    """
    success = await refresh_knowledge_graph()
    if not success:
        raise HTTPException(
            status_code=503,
            detail="Knowledge graph not initialized or refresh failed",
        )

    lightrag_stats: dict[str, Any] | None = None
    if reindex_lightrag:
        try:
            from app.core.config import settings
            if settings.LIGHTRAG_ENABLED:
                from app.knowledge_graph import knowledge_sources, lightrag_index
                # 联动 reindex 的来源：
                # - neo4j：节点/关系镜像（规格、属性等）
                # - judgment_rules：每条判定规则的完整条件树（业务方改了规则当晚自动同步）
                # - report_config：lab_report_config 里的命名指标定义
                merged: list = []
                stats_per_source: dict[str, int] = {}
                for src in ("neo4j", "judgment_rules", "report_config"):
                    try:
                        chunk = await knowledge_sources.collect_one(src)
                    except Exception:
                        chunk = []
                    stats_per_source[src] = len(chunk)
                    merged.extend(chunk)
                if merged:
                    insert_stats = await lightrag_index.index_docs(merged)
                    lightrag_stats = {**insert_stats, "per_source": stats_per_source}
                else:
                    lightrag_stats = {"inserted": 0, "per_source": stats_per_source, "note": "no docs collected"}
        except Exception as e:
            # LightRAG reindex 失败不应让 Neo4j refresh 看起来失败（Neo4j 已成功）
            lightrag_stats = {"error": str(e)}

    return {
        "message": "Knowledge graph refreshed successfully",
        "lightrag_reindex": lightrag_stats,
    }


@router.get("/specs")
async def get_all_specs() -> list[dict[str, Any]]:
    """获取所有产品规格."""
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    return await kg_queries.get_all_specs_with_attributes(graph)


@router.get("/specs/{spec_code}")
async def get_spec_detail(spec_code: str) -> dict[str, Any]:
    """获取产品规格详情."""
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    # 获取规格属性
    attributes = await kg_queries.get_spec_attributes(graph, spec_code)

    # 获取判定类型
    judgment_types = await kg_queries.get_judgment_types_for_spec(graph, spec_code)

    if not attributes and not judgment_types:
        raise HTTPException(status_code=404, detail=f"Spec {spec_code} not found")

    return {
        "code": spec_code,
        "attributes": attributes,
        "judgment_types": judgment_types,
    }


@router.get("/specs/{spec_code}/rules")
async def get_spec_rules(spec_code: str) -> list[dict[str, Any]]:
    """获取产品规格的判定规则."""
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    return await kg_queries.get_spec_judgment_rules(graph, spec_code)


@router.get("/metrics")
async def get_all_metrics() -> list[dict[str, Any]]:
    """获取所有指标."""
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    return await kg_queries.get_metric_formulas(graph)


@router.get("/metrics/{metric_name}")
async def get_metric_detail(metric_name: str) -> dict[str, Any]:
    """获取指标详情."""
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    metrics = await kg_queries.get_metric_formulas(graph, metric_name)
    if not metrics:
        raise HTTPException(status_code=404, detail=f"Metric {metric_name} not found")

    return metrics[0]


@router.get("/first-inspection/config")
async def get_first_inspection_config() -> dict[str, Any]:
    """获取一次交检合格率配置."""
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    return await kg_queries.get_first_inspection_config(graph)


@router.get("/rules/search")
async def search_rules(keyword: str) -> list[dict[str, Any]]:
    """根据条件关键词搜索判定规则."""
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    return await kg_queries.find_rules_by_condition(graph, keyword)


@router.get("/appearance-features")
async def get_appearance_features() -> list[dict[str, Any]]:
    """获取所有外观特性."""
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    return await kg_queries.get_appearance_features(graph)


@router.get("/appearance-features/categories")
async def get_appearance_feature_categories() -> list[dict[str, Any]]:
    """获取所有外观特性大类."""
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    return await kg_queries.get_appearance_feature_categories(graph)


@router.get("/appearance-features/levels")
async def get_appearance_feature_levels() -> list[dict[str, Any]]:
    """获取所有外观特性等级."""
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    return await kg_queries.get_appearance_feature_levels(graph)


@router.get("/report-configs")
async def get_report_configs() -> list[dict[str, Any]]:
    """获取所有报表配置."""
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    return await kg_queries.get_report_configs(graph)


# --------------------------------------------------------------------------- #
# New endpoints for Phase 2 (Ontology-aware Q&A)
# --------------------------------------------------------------------------- #


@router.post("/explain", response_model=ExplainResponse)
async def explain_question(request: ExplainRequest) -> ExplainResponse:
    """问答解释接口.

    接收自然语言问题，返回结构化答案、推理链、相关子图和证据表。
    MVP 阶段优先支持根因解释类问题（"为什么炉号 X 是 Y 级？"）。
    """
    question = request.question or ""
    context = request.context or {}

    # 简单意图识别：是否包含根因关键词
    import re
    furnace_match = re.search(r"炉号\s*[:：]?\s*([A-Za-z0-9一-鿿-]{4,})", question)
    grade_match = re.search(r"([ABCabc])[级級]", question)

    # 根因解释路径
    if furnace_match or "为什么" in question or "根因" in question or "判定" in question:
        furnace_no = furnace_match.group(1) if furnace_match else context.get("furnace_no", "")
        target_grade = (grade_match.group(1).upper() if grade_match else context.get("grade", ""))

        if not furnace_no:
            return ExplainResponse(
                answer="请提供要归因的炉号或批次号，例如“为什么炉号 1丙20260110-1 是 C 级？”",
                reasoning_steps=[
                    ReasoningStep(
                        id="fallback-1",
                        kind="fallback",
                        title="缺少查询条件",
                        summary="请提供炉号或批次号",
                        status="warning",
                    )
                ],
            )

        steps_raw = await traverse_judgment_path.ainvoke(
            {
                "furnace_no": furnace_no,
                "batch_no": None,
                "target_grade": target_grade,
            }
        )

        # 转换为 Pydantic 模型
        reasoning_steps = []
        for s in steps_raw:
            try:
                reasoning_steps.append(ReasoningStep(**s))
            except Exception:
                # 兼容旧格式：兜底构造
                reasoning_steps.append(
                    ReasoningStep(
                        id=s.get("id", "unknown"),
                        kind=s.get("kind", "fallback"),
                        title=s.get("title", s.get("label", "未知步骤")),
                        summary=s.get("summary", s.get("label", "")),
                        status="success" if s.get("kind") != "fallback" else "warning",
                        meta=s.get("meta"),
                        field=s.get("field"),
                        expected=s.get("expected"),
                        actual=s.get("actual"),
                        satisfied=s.get("satisfied"),
                        label=s.get("label"),
                        detail=s.get("detail"),
                    )
                )

        # 构建子图（从推理步骤中的 ontology_refs 提取节点）
        subgraph = _build_subgraph_from_steps(reasoning_steps)

        # 构建证据表
        evidence_table = []
        for s in reasoning_steps:
            if s.kind == "condition" and s.evidence:
                for ev in s.evidence:
                    evidence_table.append({
                        "step_id": s.id,
                        "field": s.field,
                        "label": ev.label,
                        "expected": s.expected,
                        "actual": ev.value,
                        "unit": ev.unit,
                        "satisfied": s.satisfied,
                    })

        # 取最终答案
        final_answer = ""
        for s in reversed(reasoning_steps):
            if s.kind == "grade":
                final_answer = s.summary
                break
        if not final_answer and reasoning_steps:
            final_answer = reasoning_steps[-1].summary

        # 结构化摘要卡片
        grade = "未知"
        for s in reasoning_steps:
            if s.meta and "grade" in s.meta:
                grade = s.meta["grade"]
                break

        answer_card = {
            "furnace_no": furnace_no,
            "grade": grade,
            "spec_code": context.get("spec_code", ""),
            "rule_count": len([s for s in reasoning_steps if s.kind == "rule"]),
            "condition_count": len([s for s in reasoning_steps if s.kind == "condition"]),
        }

        return ExplainResponse(
            answer=final_answer,
            answer_card=answer_card,
            reasoning_steps=reasoning_steps,
            subgraph=subgraph,
            evidence_table=evidence_table,
            suggested_actions=[
                {"label": "查看规则详情", "action": "open_rule", "params": {}},
                {"label": "模拟阈值", "action": "simulate", "params": {}},
            ],
        )

    # 兜底：直接返回提示
    return ExplainResponse(
        answer="当前只支持根因解释类问题（例如“为什么炉号 X 是 Y 级？”）。其他类型问答正在开发中。",
        reasoning_steps=[
            ReasoningStep(
                id="fallback-intent",
                kind="fallback",
                title="意图不支持",
                summary="当前只支持根因解释类问题",
                status="warning",
            )
        ],
    )


@router.get("/subgraph")
async def get_subgraph(
    anchor_type: str,
    anchor_id: str,
    depth: int = 2,
    relation_filter: str | None = None,
) -> OntologyGraphDTO:
    """局部子图查询.

    以某个锚点对象为中心，查询限定深度内的节点和关系。
    """
    graph = get_knowledge_graph()
    if graph is None:
        raise HTTPException(status_code=503, detail="Knowledge graph not available")

    if depth < 1 or depth > 4:
        raise HTTPException(status_code=400, detail="depth must be between 1 and 4")

    # 构建 Cypher 查询
    rel_clause = f"TYPE(r) = '{relation_filter}'" if relation_filter else "TRUE"

    cypher = f"""
        MATCH path = (anchor)-[r*1..{depth}]-(neighbor)
        WHERE anchor.id = $anchor_id OR anchor.code = $anchor_id
          AND ALL(rel IN r WHERE {rel_clause})
        WITH anchor, neighbor, r
        LIMIT 50
        RETURN DISTINCT
            anchor {{ .id, .code, .name }} as anchor,
            neighbor {{ .id, .code, .name, .formulaId, .qualityStatus, .priority }} as neighbor,
            [rel IN r | {{ type: type(rel), start: startNode(rel).id, end: endNode(rel).id }}] as rels
    """

    results = await graph.query_async(cypher, anchor_id=anchor_id)

    nodes_map: dict[str, OntologyNode] = {}
    edges_list: list[OntologyEdge] = []

    for r in results:
        anchor = r.get("anchor", {})
        neighbor = r.get("neighbor", {})
        rels = r.get("rels", [])

        # 锚点节点
        if anchor and anchor.get("id"):
            aid = str(anchor.get("id"))
            if aid not in nodes_map:
                nodes_map[aid] = OntologyNode(
                    id=aid,
                    type=_infer_node_type(anchor),
                    label=anchor.get("name") or anchor.get("code") or aid,
                    raw=dict(anchor),
                )

        # 邻居节点
        if neighbor and neighbor.get("id"):
            nid = str(neighbor.get("id"))
            if nid not in nodes_map:
                nodes_map[nid] = OntologyNode(
                    id=nid,
                    type=_infer_node_type(neighbor),
                    label=neighbor.get("name") or neighbor.get("code") or nid,
                    raw=dict(neighbor),
                )

        # 关系
        for rel in rels:
            sid = str(rel.get("start", ""))
            tid = str(rel.get("end", ""))
            if sid and tid:
                edges_list.append(
                    OntologyEdge(
                        id=f"{sid}-{rel.get('type', 'REL')}-{tid}",
                        source=sid,
                        target=tid,
                        relation=rel.get("type", "REL"),
                    )
                )

    return OntologyGraphDTO(
        nodes=list(nodes_map.values()),
        edges=edges_list,
    )


@router.post("/resolve", response_model=list[ResolvedEntity])
async def resolve_entities(request: ResolveRequest) -> list[ResolvedEntity]:
    """实体解析接口.

    解析自然语言短语中的本体对象候选。
    """
    phrase = request.phrase or ""
    results: list[ResolvedEntity] = []

    # 炉号解析
    import re
    furnace_pattern = re.compile(r"([A-Za-z0-9一-鿿-]{4,})")
    if furnace_pattern.search(phrase):
        match = furnace_pattern.search(phrase)
        if match:
            results.append(
                ResolvedEntity(
                    type="furnace_no",
                    id=match.group(1),
                    label=f"炉号 {match.group(1)}",
                    confidence=0.85,
                )
            )

    # 等级解析
    grade_pattern = re.compile(r"([ABCabc])[级級]")
    grade_match = grade_pattern.search(phrase)
    if grade_match:
        results.append(
            ResolvedEntity(
                type="grade",
                id=grade_match.group(1).upper(),
                label=f"{grade_match.group(1).upper()} 级",
                confidence=0.95,
            )
        )

    # 规格解析（数字编码）
    spec_pattern = re.compile(r"规格\s*[:：]?\s*(\d+)")
    spec_match = spec_pattern.search(phrase)
    if spec_match:
        results.append(
            ResolvedEntity(
                type="ProductSpec",
                id=spec_match.group(1),
                label=f"规格 {spec_match.group(1)}",
                confidence=0.9,
            )
        )

    return results


# --------------------------------------------------------------------------- #
# Helpers
# --------------------------------------------------------------------------- #


def _infer_node_type(node_data: dict) -> str:
    """根据节点数据推断类型（与前端 OntologyNodeType 对齐）."""
    if "formulaId" in node_data or "qualityStatus" in node_data or "priority" in node_data:
        return "JudgmentRule"
    if "formula" in node_data or "columnName" in node_data or "formulaType" in node_data:
        return "Formula"
    if "attrKey" in node_data or "valueType" in node_data:
        return "SpecAttribute"
    if "keywords" in node_data:
        return "AppearanceFeature"
    if "parentId" in node_data:
        return "AppearanceCategory"
    if "detectionColumns" in node_data or "code" in node_data:
        return "ProductSpec"
    return "ProductSpec"


def _build_subgraph_from_steps(steps: list[ReasoningStep]) -> OntologyGraphDTO:
    """从推理步骤中提取节点和边，构建子图 DTO."""
    nodes_map: dict[str, OntologyNode] = {}
    edges_list: list[OntologyEdge] = []

    for step in steps:
        for ref in step.ontology_refs or []:
            if ref.id not in nodes_map:
                nodes_map[ref.id] = OntologyNode(
                    id=ref.id,
                    type=ref.type,
                    label=ref.label,
                    status=(
                        "ok" if step.status == "success"
                        else "error" if step.status == "failed"
                        else "warning"
                    ),
                )
        for er in step.edge_refs or []:
            edges_list.append(
                OntologyEdge(
                    id=f"{er.source}-{er.relation}-{er.target}",
                    source=er.source,
                    target=er.target,
                    relation=er.relation,
                    status="active" if step.status == "success" else "failed",
                )
            )

    # 高亮所有相关节点和边
    highlights = {
        "nodeIds": list(nodes_map.keys()),
        "edgeIds": [e.id for e in edges_list],
    }

    return OntologyGraphDTO(
        nodes=list(nodes_map.values()),
        edges=edges_list,
        highlights=highlights,
    )


# --------------------------------------------------------------------------- #
# Alias dictionary loader（同义词字典 — 用于智能问数提升召回率）
# --------------------------------------------------------------------------- #


def _load_aliases() -> dict[str, dict[str, list[str]]]:
    """加载知识图谱节点同义词字典.

    路径：app/knowledge_graph/aliases.json
    结构：{ 节点类型(formula/metric/appearance/field): { 节点 key: [同义词...] } }
    文件不存在或解析失败时返回空字典，不阻塞 ontology 返回。
    """
    from pathlib import Path
    import json
    alias_path = Path(__file__).parent.parent / "knowledge_graph" / "aliases.json"
    if not alias_path.exists():
        return {}
    try:
        with open(alias_path, encoding="utf-8") as fp:
            raw = json.load(fp)
            # 过滤掉 _comment 等下划线开头的元数据
            return {k: v for k, v in raw.items() if not k.startswith("_") and isinstance(v, dict)}
    except Exception:
        return {}


def _lookup_aliases(alias_map: dict[str, list[str]], *keys: str) -> list[str]:
    """按多个候选 key 依次查找同义词；返回首个命中的列表（去重保序）."""
    seen: set[str] = set()
    for k in keys:
        if not k:
            continue
        hits = alias_map.get(str(k))
        if hits:
            ordered: list[str] = []
            for h in hits:
                if h not in seen:
                    seen.add(h)
                    ordered.append(h)
            return ordered
    return []


def _load_sql_templates() -> dict[str, Any]:
    """加载 SQL 模板库 (sql_templates.json).

    供 NLQ Agent 在生成 SQL 时复用骨架。返回完整字典含 templates / placeholder_format / table_context。
    """
    from pathlib import Path
    import json
    path = Path(__file__).parent.parent / "knowledge_graph" / "sql_templates.json"
    if not path.exists():
        return {"templates": [], "placeholder_format": {}, "table_context": {}}
    try:
        with open(path, encoding="utf-8") as fp:
            data = json.load(fp)
            # 过滤掉 _comment 元数据，保留其它键
            return {k: v for k, v in data.items() if not k.startswith("_")}
    except Exception:
        return {"templates": [], "placeholder_format": {}, "table_context": {}}


def _load_dimensions_meta() -> dict[str, Any]:
    """加载维度静态元数据 (dimensions_meta.json).

    含字段名 / 别名 / 时间表达式 / 班次产线值的口语化别名等。NLQ Agent 配合这份元数据 + DB 实际取值做语义化解析。
    """
    from pathlib import Path
    import json
    path = Path(__file__).parent.parent / "knowledge_graph" / "dimensions_meta.json"
    if not path.exists():
        return {}
    try:
        with open(path, encoding="utf-8") as fp:
            data = json.load(fp)
            return {k: v for k, v in data.items() if not k.startswith("_")}
    except Exception:
        return {}


@router.get("/ontology")
async def get_ontology() -> dict[str, Any]:
    """获取本体数据（规格、规则、公式）——直接从 MySQL 查询，不依赖 Neo4j.

    供前端浏览模式初始化使用。
    """
    aliases = _load_aliases()
    formula_aliases = aliases.get("formula", {})
    metric_aliases = aliases.get("metric", {})
    appearance_aliases = aliases.get("appearance", {})
    field_aliases = aliases.get("field", {})

    async with AsyncSessionLocal() as session:
        # 1. 产品规格（左连 lab_product_spec_version 取当前版本号）
        # 注意：F_Id 与 F_PRODUCT_SPEC_ID 跨表 collation 可能不一致，必须显式 COLLATE
        spec_sql = text("""
            SELECT s.F_Id as id, s.F_CODE as code, s.F_NAME as name,
                   s.F_DETECTION_COLUMNS as detection_columns,
                   v.F_VERSION as version
            FROM lab_product_spec s
            LEFT JOIN lab_product_spec_version v
              ON s.F_Id COLLATE utf8mb4_unicode_ci = v.F_PRODUCT_SPEC_ID COLLATE utf8mb4_unicode_ci
              AND v.F_IS_CURRENT = 1
              AND (v.F_DELETE_MARK IS NULL OR v.F_DELETE_MARK = 0)
            WHERE (s.F_DeleteMark IS NULL OR s.F_DeleteMark = 0)
              AND s.F_ENABLEDMARK = 1
        """)
        spec_rows = (await session.execute(spec_sql)).mappings().all()
        specs = [
            {
                "id": str(r["id"]),
                "code": str(r["code"]),
                "name": r["name"],
                # description 保留兼容旧调用；detection_columns 是规范字段
                "description": str(r["detection_columns"] or ""),
                "detection_columns": str(r["detection_columns"] or ""),
                "version": str(r["version"]) if r["version"] else "",
                "attributes": [],
            }
            for r in spec_rows
        ]

        # 2. 判定等级（lab_intermediate_data_judgment_level）— 完整字段
        # 每条等级 = (F_FORMULA_ID, F_PRODUCT_SPEC_ID) 二元组下的具象等级
        rule_sql = text("""
            SELECT j.F_Id as id, j.F_CODE as code, j.F_NAME as name,
                   j.F_FORMULA_ID as formula_id, j.F_FORMULA_NAME as formula_name,
                   j.F_PRODUCT_SPEC_ID as raw_spec_id,
                   j.F_PRODUCT_SPEC_NAME as raw_spec_name,
                   j.F_QUALITY_STATUS as quality_status,
                   j.F_PRIORITY as priority,
                   j.F_COLOR as color,
                   j.F_IS_STATISTIC as is_statistic,
                   j.F_IS_DEFAULT as is_default,
                   j.F_DESCRIPTION as description,
                   j.F_CONDITION as condition_json,
                   p.F_CODE as spec_code, p.F_Id as spec_id, p.F_NAME as spec_name
            FROM lab_intermediate_data_judgment_level j
            LEFT JOIN lab_product_spec p
              ON j.F_PRODUCT_SPEC_ID COLLATE utf8mb4_unicode_ci = p.F_Id COLLATE utf8mb4_unicode_ci
            WHERE j.F_DeleteMark = 0 OR j.F_DeleteMark IS NULL
            ORDER BY j.F_PRODUCT_SPEC_ID, j.F_FORMULA_ID, j.F_PRIORITY
        """)
        rule_rows = (await session.execute(rule_sql)).mappings().all()
        rules = [
            {
                "id": str(r["id"]),
                "code": str(r["code"] or ""),
                "name": r["name"] or "",
                "formula_id": str(r["formula_id"]) if r["formula_id"] else "",
                "formula_name": r["formula_name"] or "",
                "product_spec_id": str(r["spec_id"] or r["raw_spec_id"] or ""),
                "product_spec_name": r["spec_name"] or r["raw_spec_name"] or "",
                "product_spec_code": str(r["spec_code"]) if r["spec_code"] else "",
                "quality_status": str(r["quality_status"]) if r["quality_status"] is not None else "",
                "priority": int(r["priority"]) if r["priority"] is not None else 0,
                "color": r["color"] or "",
                "is_statistic": bool(r["is_statistic"]) if r["is_statistic"] is not None else True,
                "is_default": bool(r["is_default"]) if r["is_default"] is not None else False,
                "description": r["description"] or "",
                "conditionJson": r["condition_json"] or "",
            }
            for r in rule_rows
        ]

        # 3. 公式/指标（lab_intermediate_data_formula 完整字段）
        formula_sql = text("""
            SELECT F_Id as id, F_FORMULA_NAME as formula_name,
                   F_COLUMN_NAME as column_name, F_FORMULA as formula,
                   F_UNIT_NAME as unit_name, F_FORMULA_TYPE as formula_type,
                   F_FORMULA_LANGUAGE as formula_language,
                   F_TABLE_NAME as table_name,
                   F_SOURCE_TYPE as source_type,
                   F_PRECISION as precision_val,
                   F_SORT_ORDER as sort_order,
                   F_IS_ENABLED as is_enabled,
                   F_DEFAULT_VALUE as default_value,
                   F_REMARK as remark
            FROM lab_intermediate_data_formula
            WHERE F_DeleteMark = 0 OR F_DeleteMark IS NULL
            ORDER BY F_SORT_ORDER
        """)
        formula_rows = (await session.execute(formula_sql)).mappings().all()
        formulas = []
        for r in formula_rows:
            column_name = r["column_name"] or ""
            formula_name = r["formula_name"] or ""
            formulas.append({
                "id": str(r["id"]),
                "formula_name": formula_name,
                "column_name": column_name,
                "formula": r["formula"] or "",
                # formula_type 归一化为大写字符串 CALC/JUDGE/NO（兼容 DB 可能存 '1'/'2'/'3'）
                "formula_type": (
                    {"1": "CALC", "2": "JUDGE", "3": "NO"}.get(str(r["formula_type"]).strip(), str(r["formula_type"] or "").upper().strip())
                ),
                "unit_name": r["unit_name"] or "",
                "formula_language": r["formula_language"] or "EXCEL",
                "table_name": r["table_name"] or "",
                "source_type": r["source_type"] or "SYSTEM",
                "precision_val": int(r["precision_val"]) if r["precision_val"] is not None else None,
                "sort_order": int(r["sort_order"]) if r["sort_order"] is not None else 0,
                "is_enabled": bool(r["is_enabled"]) if r["is_enabled"] is not None else True,
                "default_value": r["default_value"] or "",
                "remark": r["remark"] or "",
                # 同义词：按 column_name 或 formula_name 匹配
                "aliases": _lookup_aliases(formula_aliases, column_name, formula_name),
            })

        # 4. 产品规格属性
        attr_sql = text("""
            SELECT a.F_Id as id, a.F_PRODUCT_SPEC_ID as spec_id,
                   a.F_ATTRIBUTE_NAME as name, a.F_ATTRIBUTE_KEY as attr_key,
                   a.F_ATTRIBUTE_VALUE as value, a.F_VALUE_TYPE as value_type,
                   a.F_UNIT as unit, a.F_PRECISION as precision_val,
                   a.F_VERSION as version
            FROM lab_product_spec_attribute a
            INNER JOIN lab_product_spec_version v
                ON a.F_PRODUCT_SPEC_ID COLLATE utf8mb4_unicode_ci = v.F_PRODUCT_SPEC_ID COLLATE utf8mb4_unicode_ci
                AND a.F_VERSION COLLATE utf8mb4_unicode_ci = v.F_VERSION COLLATE utf8mb4_unicode_ci
            WHERE v.F_IS_CURRENT = 1
              AND (a.F_DeleteMark IS NULL OR a.F_DeleteMark = 0)
              AND (v.F_DELETE_MARK IS NULL OR v.F_DELETE_MARK = 0)
        """)
        attr_rows = (await session.execute(attr_sql)).mappings().all()
        spec_attributes = [
            {
                "id": str(r["id"]),
                "spec_id": str(r["spec_id"]) if r["spec_id"] else "",
                "name": r["name"],
                "attr_key": r["attr_key"],
                "value": r["value"],
                "value_type": r["value_type"] or "",
                "unit": r["unit"] or "",
                "precision_val": r["precision_val"],
                # 强制 str 化以避免与 spec.version（str）比较时类型不匹配（MySQL F_VERSION 可能是 INT）
                "version": str(r["version"]) if r["version"] is not None else "",
            }
            for r in attr_rows
        ]

        # 5. Excel 导入模板（环样性能 + 叠片数据）
        template_sql = text("""
            SELECT F_Id as id, F_TEMPLATE_NAME as template_name,
                   F_TEMPLATE_CODE as template_code, F_CONFIG_JSON as config_json
            FROM LAB_EXCEL_IMPORT_TEMPLATE
            WHERE F_TEMPLATE_CODE IN ('RawDataImport', 'MagneticDataImport')
              AND (F_DeleteMark IS NULL OR F_DeleteMark = 0)
        """)
        template_rows = (await session.execute(template_sql)).mappings().all()
        excel_templates = []
        for r in template_rows:
            config_json = r["config_json"] or ""
            # 处理双重序列化（字符串被多转义一次）
            if config_json.startswith('"') and config_json.endswith('"'):
                try:
                    import json
                    config_json = json.loads(config_json)
                except Exception:
                    pass
            field_mappings = []
            if config_json:
                try:
                    import json
                    config = json.loads(config_json) if isinstance(config_json, str) else config_json
                    mappings = config.get("fieldMappings", []) if isinstance(config, dict) else []
                    for m in mappings:
                        if isinstance(m, dict) and m.get("field"):
                            field_mappings.append({
                                "field": m["field"],
                                "label": m.get("label", ""),
                                "data_type": m.get("dataType", "string"),
                                "required": m.get("required", False),
                            })
                except Exception:
                    pass
            excel_templates.append({
                "id": str(r["id"]),
                "template_name": r["template_name"],
                "template_code": r["template_code"],
                "field_mappings": field_mappings,
            })

        # 6. 查询目标表字段（lab_magnetic_raw_data / lab_raw_data / lab_intermediate_data）
        base_fields = {
            'F_Id', 'F_TenantId', 'F_CREATORTIME', 'F_CREATORUSERID',
            'F_ENABLEDMARK', 'F_LastModifyTime', 'F_LastModifyUserId',
            'F_DeleteMark', 'F_DeleteTime', 'F_DeleteUserId',
        }
        table_fields = {}
        for table_name in ['lab_magnetic_raw_data', 'lab_raw_data', 'lab_intermediate_data']:
            col_sql = text("""
                SELECT COLUMN_NAME as column_name, COLUMN_COMMENT as column_comment
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = :table_name
                  AND TABLE_SCHEMA = DATABASE()
                ORDER BY ORDINAL_POSITION
            """)
            col_rows = (await session.execute(col_sql, {"table_name": table_name})).mappings().all()
            table_fields[table_name] = [
                {
                    "column_name": str(r["column_name"]),
                    "column_comment": str(r["column_comment"] or ""),
                    "aliases": _lookup_aliases(field_aliases, str(r["column_name"])),
                }
                for r in col_rows
                if str(r["column_name"]) not in base_fields
            ]

        # 7. 外观特性大类（LAB_APPEARANCE_FEATURE_CATEGORY） — 树形结构
        category_sql = text("""
            SELECT F_Id as id, F_NAME as name, F_DESCRIPTION as description,
                   F_PARENTID as parent_id, F_ROOTID as root_id, F_PATH as path,
                   F_SORTCODE as sort_code
            FROM LAB_APPEARANCE_FEATURE_CATEGORY
            WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)
            ORDER BY F_SORTCODE
        """)
        category_rows = (await session.execute(category_sql)).mappings().all()
        appearance_categories = [
            {
                "id": str(r["id"]),
                "name": r["name"] or "",
                "description": r["description"] or "",
                "parent_id": str(r["parent_id"]) if r["parent_id"] else "",
                "root_id": str(r["root_id"]) if r["root_id"] else "",
                "path": r["path"] or "",
                "sort_code": int(r["sort_code"]) if r["sort_code"] is not None else 0,
            }
            for r in category_rows
        ]

        # 8. 外观特性定义（LAB_APPEARANCE_FEATURE）— LEFT JOIN 等级表拿到 level_name
        feature_sql = text("""
            SELECT f.F_Id as id, f.F_NAME as name, f.F_CATEGORY_ID as category_id,
                   f.F_SEVERITY_LEVEL_ID as severity_level_id,
                   f.F_KEYWORDS as keywords, f.F_DESCRIPTION as description,
                   f.F_SORTCODE as sort_code,
                   l.F_NAME as level_name, l.F_ISDEFAULT as level_is_default
            FROM LAB_APPEARANCE_FEATURE f
            LEFT JOIN LAB_APPEARANCE_FEATURE_LEVEL l
              ON f.F_SEVERITY_LEVEL_ID COLLATE utf8mb4_unicode_ci = l.F_Id COLLATE utf8mb4_unicode_ci
              AND (l.F_DeleteMark IS NULL OR l.F_DeleteMark = 0)
            WHERE (f.F_DeleteMark IS NULL OR f.F_DeleteMark = 0)
            ORDER BY f.F_SORTCODE
        """)
        feature_rows = (await session.execute(feature_sql)).mappings().all()
        appearance_features = []
        for r in feature_rows:
            # 解析 keywords JSON（可能是 JSON 数组也可能是字符串）
            kw_raw = r["keywords"] or ""
            keywords: list[str] = []
            if kw_raw:
                try:
                    import json
                    parsed = json.loads(kw_raw)
                    if isinstance(parsed, list):
                        keywords = [str(k) for k in parsed]
                except Exception:
                    # 兜底：按逗号 / 分号切分
                    keywords = [k.strip() for k in str(kw_raw).replace("；", ",").replace(";", ",").split(",") if k.strip()]

            feat_name = r["name"] or ""
            appearance_features.append({
                "id": str(r["id"]),
                "name": feat_name,
                "category_id": str(r["category_id"]) if r["category_id"] else "",
                "severity_level_id": str(r["severity_level_id"]) if r["severity_level_id"] else "",
                "level_name": r["level_name"] or "",
                "level_is_default": bool(r["level_is_default"]) if r["level_is_default"] is not None else False,
                "keywords": keywords,
                "description": r["description"] or "",
                "sort_code": int(r["sort_code"]) if r["sort_code"] is not None else 0,
                # 同义词：补充 F_KEYWORDS 之外的口语化说法
                "aliases": _lookup_aliases(appearance_aliases, feat_name),
            })

        # 9. 外观特性等级（LAB_APPEARANCE_FEATURE_LEVEL） — 独立节点
        level_sql = text("""
            SELECT F_Id as id, F_NAME as name, F_DESCRIPTION as description,
                   F_SORTCODE as sort_code, F_ENABLED as enabled, F_ISDEFAULT as is_default
            FROM LAB_APPEARANCE_FEATURE_LEVEL
            WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)
            ORDER BY F_SORTCODE
        """)
        level_rows = (await session.execute(level_sql)).mappings().all()
        appearance_levels = [
            {
                "id": str(r["id"]),
                "name": r["name"] or "",
                "description": r["description"] or "",
                "sort_code": int(r["sort_code"]) if r["sort_code"] is not None else 0,
                "enabled": bool(r["enabled"]) if r["enabled"] is not None else True,
                "is_default": bool(r["is_default"]) if r["is_default"] is not None else False,
            }
            for r in level_rows
        ]

        # 10. 统计指标（LAB_REPORT_CONFIG）— 报表统计配置
        report_sql = text("""
            SELECT F_Id as id, F_NAME as name, F_FORMULA_ID as formula_id,
                   F_LEVEL_NAMES as level_names, F_DESCRIPTION as description,
                   F_SORT_ORDER as sort_order, F_IS_SYSTEM as is_system,
                   F_IS_HEADER as is_header, F_IS_PERCENTAGE as is_percentage,
                   F_IS_SHOW_IN_REPORT as is_show_in_report,
                   F_IS_SHOW_RATIO as is_show_ratio
            FROM LAB_REPORT_CONFIG
            WHERE F_DeleteMark = 0 OR F_DeleteMark IS NULL
            ORDER BY F_SORT_ORDER
        """)
        report_rows = (await session.execute(report_sql)).mappings().all()
        report_configs = []
        for r in report_rows:
            # 解析 levelNames JSON
            lvl_raw = r["level_names"] or ""
            level_names: list[str] = []
            if lvl_raw:
                try:
                    import json
                    parsed = json.loads(lvl_raw)
                    if isinstance(parsed, list):
                        level_names = [str(x) for x in parsed]
                except Exception:
                    level_names = [s.strip() for s in str(lvl_raw).replace("；", ",").replace(";", ",").split(",") if s.strip()]
            metric_name = r["name"] or ""
            report_configs.append({
                "id": str(r["id"]),
                "name": metric_name,
                "formula_id": str(r["formula_id"]) if r["formula_id"] else "",
                "level_names": level_names,
                "description": r["description"] or "",
                "sort_order": int(r["sort_order"]) if r["sort_order"] is not None else 0,
                "is_system": bool(r["is_system"]) if r["is_system"] is not None else False,
                "is_header": bool(r["is_header"]) if r["is_header"] is not None else False,
                "is_percentage": bool(r["is_percentage"]) if r["is_percentage"] is not None else False,
                "is_show_in_report": bool(r["is_show_in_report"]) if r["is_show_in_report"] is not None else True,
                "is_show_ratio": bool(r["is_show_ratio"]) if r["is_show_ratio"] is not None else True,
                # 同义词：按指标名匹配
                "aliases": _lookup_aliases(metric_aliases, metric_name),
            })

        # 11. 维度元数据（从 lab_intermediate_data 实时去重 + 静态别名合并）
        async def _distinct_values(column: str, cast_str: bool = True):
            sql = text(f"""
                SELECT DISTINCT {column} AS v
                FROM lab_intermediate_data
                WHERE F_DeleteMark IS NULL AND {column} IS NOT NULL
                ORDER BY {column}
            """)
            rows = (await session.execute(sql)).mappings().all()
            if cast_str:
                return [str(r["v"]) for r in rows if r["v"] is not None and str(r["v"]).strip() != ""]
            return [r["v"] for r in rows if r["v"] is not None]

        try:
            distinct_shifts = await _distinct_values("F_SHIFT")
        except Exception:
            distinct_shifts = []
        try:
            distinct_lines = await _distinct_values("F_LINE_NO")
        except Exception:
            distinct_lines = []
        try:
            distinct_spec_codes = await _distinct_values("F_PRODUCT_SPEC_CODE")
        except Exception:
            distinct_spec_codes = []
        try:
            date_range_sql = text("""
                SELECT MIN(F_PROD_DATE) AS min_date, MAX(F_PROD_DATE) AS max_date,
                       COUNT(*) AS row_count
                FROM lab_intermediate_data WHERE F_DeleteMark IS NULL
            """)
            dr = (await session.execute(date_range_sql)).mappings().one_or_none()
        except Exception:
            dr = None

        dim_meta = _load_dimensions_meta()
        time_meta = dict(dim_meta.get("time", {}))
        time_meta["min_date"] = dr["min_date"].isoformat() if dr and dr["min_date"] else ""
        time_meta["max_date"] = dr["max_date"].isoformat() if dr and dr["max_date"] else ""
        time_meta["row_count"] = int(dr["row_count"]) if dr and dr["row_count"] is not None else 0

        shift_meta = dict(dim_meta.get("shift", {}))
        shift_meta["values_from_data"] = distinct_shifts

        line_meta = dict(dim_meta.get("line", {}))
        line_meta["values_from_data"] = distinct_lines

        spec_dim_meta = dict(dim_meta.get("product_spec", {}))
        spec_dim_meta["values_from_data"] = distinct_spec_codes

        furnace_dim_meta = dim_meta.get("furnace_no", {})

        dimensions = {
            "time": time_meta,
            "shift": shift_meta,
            "line": line_meta,
            "product_spec": spec_dim_meta,
            "furnace_no": furnace_dim_meta,
        }

    # 顶层附加：NLQ SQL 模板库（供 LLM 生成 SQL 时复用骨架）
    sql_lib = _load_sql_templates()

    return {
        "specs": specs, "rules": rules, "formulas": formulas,
        "spec_attributes": spec_attributes, "excel_templates": excel_templates,
        "table_fields": table_fields,
        "appearance_categories": appearance_categories,
        "appearance_features": appearance_features,
        "appearance_levels": appearance_levels,
        "report_configs": report_configs,
        "sql_templates": sql_lib.get("templates", []),
        "sql_placeholder_format": sql_lib.get("placeholder_format", {}),
        "sql_table_context": sql_lib.get("table_context", {}),
        "dimensions": dimensions,
    }


# ------------------------------------------------------------------
# Ribbon-centric endpoints (Phase 3 — 以带材为中心的知识图谱)
# ------------------------------------------------------------------


@router.get("/ribbon/search")
async def search_ribbon(q: str = "", limit: int = 20) -> list[RibbonSearchResult]:
    """搜索带材（按炉号、规格名称、规格代码模糊匹配）."""
    from app.tools.graph_tools import search_ribbons as _search
    return await _search(q, limit)


@router.get("/ribbon/{furnace_no}/subgraph")
async def get_ribbon_subgraph(furnace_no: str, depth: int = 2) -> RibbonSubgraphResponse:
    """查询某条带材的局部子图（带材 → 规格/数据/规则/公式）."""
    from app.tools.graph_tools import get_ribbon_subgraph as _subgraph
    result = await _subgraph(furnace_no, depth)
    if result is None:
        raise HTTPException(status_code=404, detail=f"带材 {furnace_no} 未找到")
    return result


@router.post("/ask", response_model=AskResponse)
async def ask_xiaomei(request: AskRequest) -> AskResponse:
    """小美问答（优先走知识图谱路径，再回落到 Chat2SQL）."""
    # TODO: 实现小美问答逻辑（意图识别 → 图谱查询 → 证据链组装）
    return AskResponse(
        answer="小美正在学习中，暂不支持此问题的图谱问答。",
        reasoning_steps=[],
        subgraph=None,
        evidence_table=[],
        suggested_actions=[],
    )
