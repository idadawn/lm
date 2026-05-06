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
