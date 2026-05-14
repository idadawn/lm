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
async def refresh_graph() -> dict[str, Any]:
    """刷新知识图谱数据."""
    success = await refresh_knowledge_graph()
    if not success:
        raise HTTPException(
            status_code=503,
            detail="Knowledge graph not initialized or refresh failed",
        )
    return {"message": "Knowledge graph refreshed successfully"}


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
    """根据节点数据推断类型."""
    if "formulaId" in node_data or "qualityStatus" in node_data:
        return "JudgmentRule"
    if "formula" in node_data or "columnName" in node_data:
        return "Metric"
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


@router.get("/ontology")
async def get_ontology() -> dict[str, Any]:
    """获取本体数据（规格、规则、公式）——直接从 MySQL 查询，不依赖 Neo4j.

    供前端浏览模式初始化使用。
    """
    async with AsyncSessionLocal() as session:
        # 1. 产品规格
        spec_sql = text("""
            SELECT F_Id as id, F_CODE as code, F_NAME as name,
                   F_DETECTION_COLUMNS as detection_columns
            FROM lab_product_spec
            WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)
              AND F_ENABLEDMARK = 1
        """)
        spec_rows = (await session.execute(spec_sql)).mappings().all()
        specs = [
            {
                "id": str(r["id"]),
                "code": str(r["code"]),
                "name": r["name"],
                "description": str(r["detection_columns"] or ""),
                "attributes": [],
            }
            for r in spec_rows
        ]

        # 2. 判定规则
        rule_sql = text("""
            SELECT j.F_Id as id, j.F_FORMULA_ID as formula_id,
                   j.F_NAME as name, j.F_PRIORITY as priority,
                   j.F_QUALITY_STATUS as quality_status,
                   j.F_COLOR as color, j.F_IS_DEFAULT as is_default,
                   j.F_CONDITION as condition_json,
                   p.F_CODE as spec_code, p.F_Id as spec_id, p.F_NAME as spec_name
            FROM lab_intermediate_data_judgment_level j
            LEFT JOIN lab_product_spec p
              ON j.F_PRODUCT_SPEC_ID COLLATE utf8mb4_unicode_ci = p.F_Id COLLATE utf8mb4_unicode_ci
            WHERE j.F_DeleteMark = 0 OR j.F_DeleteMark IS NULL
        """)
        rule_rows = (await session.execute(rule_sql)).mappings().all()
        rules = [
            {
                "id": str(r["id"]),
                "name": r["name"],
                "code": str(r["formula_id"]) if r["formula_id"] else "",
                "quality_status": r["quality_status"] or "",
                "priority": r["priority"] or 0,
                "description": f"颜色:{r['color']}, 默认:{r['is_default']}",
                "product_spec_id": str(r["spec_id"]) if r["spec_id"] else "",
                "product_spec_name": r["spec_name"] or "",
                "formula_name": str(r["formula_id"]) if r["formula_id"] else "",
                "formula_id": str(r["formula_id"]) if r["formula_id"] else "",
                "conditionJson": r["condition_json"],
            }
            for r in rule_rows
        ]

        # 3. 公式/指标
        formula_sql = text("""
            SELECT F_Id as id, F_FORMULA_NAME as formula_name,
                   F_COLUMN_NAME as column_name, F_FORMULA as formula,
                   F_UNIT_NAME as unit_name, F_FORMULA_TYPE as formula_type
            FROM lab_intermediate_data_formula
            WHERE F_DeleteMark = 0 OR F_DeleteMark IS NULL
        """)
        formula_rows = (await session.execute(formula_sql)).mappings().all()
        formulas = [
            {
                "id": str(r["id"]),
                "formula_name": r["formula_name"],
                "column_name": r["column_name"],
                "formula": r["formula"],
                "formula_type": r["formula_type"] or "",
                "unit_name": r["unit_name"] or "",
            }
            for r in formula_rows
        ]

    return {"specs": specs, "rules": rules, "formulas": formulas}


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
