"""
FastAPI 路由定义

提供以下端点：
- POST /api/v1/chat/stream — SSE 流式问答（核心端点）
- GET  /health             — 健康检查
- POST /api/v1/sync/rules  — 规则变更同步（.NET 回调）
- POST /api/v1/sync/specs  — 规格变更同步（.NET 回调）
"""

from __future__ import annotations

import logging

from typing import Optional

from fastapi import APIRouter, Depends, Header, HTTPException, Request
from fastapi.responses import PlainTextResponse, StreamingResponse
from prometheus_client import generate_latest

from src.api.dependencies import get_orchestrator, get_services
from src.core.settings import get_settings
from src.models.schemas import ChatRequest, HealthResponse
from src.pipelines.orchestrator import PipelineOrchestrator
from src.services.resync_service import bulk_resync_all

logger = logging.getLogger(__name__)

router = APIRouter()


# ═══════════════════════════════════════════════════════════════
# Prometheus metrics
# ═══════════════════════════════════════════════════════════════


@router.get("/metrics", response_class=PlainTextResponse)
async def metrics():
    """Prometheus scrape endpoint. Exempt from rate limiting."""
    return PlainTextResponse(
        generate_latest(),
        media_type="text/plain; version=0.0.4; charset=utf-8",
    )


# ═══════════════════════════════════════════════════════════════
# 核心端点：SSE 流式问答
# ═══════════════════════════════════════════════════════════════

@router.post("/api/v1/chat/stream")
async def chat_stream(
    request: ChatRequest,
    orchestrator: PipelineOrchestrator = Depends(get_orchestrator),
):
    """
    SSE 流式问答端点。

    请求体：
        {
            "messages": [{"role": "user", "content": "本月合格率是多少？"}],
            "session_id": "optional-session-id",
            "model_name": "optional-model-override"
        }

    响应：
        Content-Type: text/event-stream
        SSE 事件流（reasoning_step / text / response_metadata / done）
    """

    async def event_generator():
        async for event in orchestrator.stream_chat(request):
            yield event

    return StreamingResponse(
        event_generator(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "Connection": "keep-alive",
            "X-Accel-Buffering": "no",  # 禁用 Nginx 缓冲
        },
    )


# ═══════════════════════════════════════════════════════════════
# 健康检查
# ═══════════════════════════════════════════════════════════════

@router.get("/health")
async def health_check(services=Depends(get_services)) -> HealthResponse:
    """
    健康检查端点，检测所有依赖服务的可用性。
    """
    llm, qdrant, db, _ = services

    return HealthResponse(
        status="ok",
        version="0.1.0",
        qdrant_connected=await qdrant.health_check(),
        mysql_connected=await db.health_check(),
        llm_available=await llm.health_check(),
    )


# ═══════════════════════════════════════════════════════════════
# 语义层同步端点（.NET 后端回调）
# ═══════════════════════════════════════════════════════════════

@router.post("/api/v1/sync/rules")
async def sync_rules(request: Request, services=Depends(get_services)):
    """
    规则变更同步。
    当 .NET 后端的判定规则发生变更时，调用此端点触发 Qdrant 增量更新。

    请求体：
        {
            "action": "upsert" | "delete",
            "data": [...]  // 变更的规则数据
        }
    """
    _, qdrant, _, embedding = services
    body = await request.json()
    action = body.get("action", "upsert")
    data = body.get("data", [])

    if action == "upsert" and data:
        from src.core.settings import get_settings
        settings = get_settings()

        documents = [
            {
                "id": item.get("id", ""),
                "text": _format_rule_text(item),
                "metadata": {
                    "spec_code": item.get("product_spec_code", ""),
                    "level_name": item.get("name", ""),
                    "quality_status": item.get("quality_status", ""),
                },
            }
            for item in data
        ]
        count = await qdrant.upsert_documents(
            settings.collection_rules, documents
        )
        return {"status": "ok", "upserted": count}

    return {"status": "ok", "message": "no action taken"}


@router.post("/api/v1/sync/specs")
async def sync_specs(request: Request, services=Depends(get_services)):
    """
    规格变更同步。
    当 .NET 后端的产品规格发生变更时，调用此端点触发 Qdrant 增量更新。
    """
    _, qdrant, _, embedding = services
    body = await request.json()
    action = body.get("action", "upsert")
    data = body.get("data", [])

    if action == "upsert" and data:
        from src.core.settings import get_settings
        settings = get_settings()

        documents = [
            {
                "id": item.get("id", ""),
                "text": _format_spec_text(item),
                "metadata": {
                    "spec_code": item.get("code", ""),
                    "spec_name": item.get("name", ""),
                },
            }
            for item in data
        ]
        count = await qdrant.upsert_documents(
            settings.collection_specs, documents
        )
        return {"status": "ok", "upserted": count}

    return {"status": "ok", "message": "no action taken"}


# ═══════════════════════════════════════════════════════════════
# Bulk resync（admin — 供夜间 cron / 运维手动触发）
# ═══════════════════════════════════════════════════════════════


@router.post("/api/v1/sync/resync-now", status_code=202)
async def resync_now(authorization: Optional[str] = Header(None)) -> dict:
    """
    全量重建 Qdrant 索引。需要 Bearer token 鉴权。
    """
    settings = get_settings()
    expected = settings.sync_admin_token
    if expected and authorization != f"Bearer {expected}":
        raise HTTPException(status_code=401, detail="invalid_admin_token")
    result = await bulk_resync_all()
    return {"status": "ok", **result}


# ═══════════════════════════════════════════════════════════════
# 知识图谱管理端点（前端 UI）
# ═══════════════════════════════════════════════════════════════


@router.get("/api/v1/kg/collections")
async def list_collections(services=Depends(get_services)):
    """列出所有 Qdrant 业务 Collection 及文档数量。"""
    _, qdrant, _, _ = services
    collections = await qdrant.list_collections_info()
    return {"collections": collections}


@router.get("/api/v1/kg/collections/{collection_name}/documents")
async def list_documents(
    collection_name: str,
    limit: int = 20,
    offset: int = 0,
    services=Depends(get_services),
):
    """分页浏览指定 Collection 中的文档。"""
    _, qdrant, _, _ = services
    result = await qdrant.scroll_documents(collection_name, limit=limit, offset=offset)
    return result


@router.post("/api/v1/kg/search")
async def search_knowledge(request: Request, services=Depends(get_services)):
    """语义搜索知识图谱。"""
    _, qdrant, _, _ = services
    body = await request.json()
    query = body.get("query", "")
    collection = body.get("collection")
    top_k = body.get("top_k", 10)

    if not query:
        raise HTTPException(status_code=400, detail="query is required")

    if collection:
        results = await qdrant.search(collection, query, top_k=top_k)
        return {"results": results, "collection": collection}
    else:
        results = await qdrant.search_multi_collection(query, top_k=top_k)
        all_results = []
        for coll_name, hits in results.items():
            for hit in hits:
                hit["collection"] = coll_name
                all_results.append(hit)
        return {"results": all_results}


@router.get("/api/v1/kg/ontology")
async def get_ontology(services=Depends(get_services)):
    """
    返回本体论结构化数据：产品规格（含属性）、判定规则、指标公式及其关联关系。
    用于前端知识图谱可视化。
    """
    _, _, db, _ = services
    from src.services.resync_service import (
        load_formulas,
        load_judgment_rules,
        load_product_specs,
    )

    specs = await load_product_specs(db)
    rules = await load_judgment_rules(db)
    formulas = await load_formulas(db)

    # Build formula lookup by name
    formula_map = {f["formula_name"]: f["id"] for f in formulas}

    quality_map = {0: "合格", 1: "不合格", 2: "其他"}

    # Serialize
    specs_out = []
    for s in specs:
        attrs = []
        for a in s.get("attributes", []):
            attrs.append({
                "key": a.get("key", ""),
                "name": a.get("name", ""),
                "value": a.get("value", ""),
                "unit": a.get("unit", ""),
            })
        specs_out.append({
            "id": s["id"],
            "code": s.get("code", ""),
            "name": s.get("name", ""),
            "description": s.get("description", ""),
            "attributes": attrs,
        })

    rules_out = []
    for r in rules:
        rules_out.append({
            "id": r["id"],
            "name": r.get("name", ""),
            "code": r.get("code", ""),
            "quality_status": quality_map.get(r.get("quality_status"), "未知"),
            "priority": r.get("priority", 0),
            "description": r.get("description", ""),
            "product_spec_id": r.get("product_spec_id", ""),
            "product_spec_name": r.get("product_spec_name", ""),
            "formula_name": r.get("formula_name", ""),
            "formula_id": formula_map.get(r.get("formula_name", ""), ""),
        })

    formulas_out = []
    for f in formulas:
        formulas_out.append({
            "id": f["id"],
            "formula_name": f.get("formula_name", ""),
            "column_name": f.get("column_name", ""),
            "formula": f.get("formula", ""),
            "formula_type": f.get("formula_type", ""),
            "unit_name": f.get("unit_name", ""),
        })

    return {
        "specs": specs_out,
        "rules": rules_out,
        "formulas": formulas_out,
    }


def _format_rule_text(rule: dict) -> str:
    """将判定规则格式化为可向量化的文本。"""
    parts = [
        f"判定等级: {rule.get('name', '')}",
        f"产品规格: {rule.get('product_spec_name', '')}",
        f"质量状态: {rule.get('quality_status', '')}",
        f"判定条件: {rule.get('condition', '')}",
        f"业务说明: {rule.get('description', '')}",
    ]
    return "\n".join(parts)


def _format_spec_text(spec: dict) -> str:
    """将产品规格格式化为可向量化的文本。"""
    parts = [
        f"产品规格: {spec.get('name', '')} (代码: {spec.get('code', '')})",
        f"有效检测列数: {spec.get('detection_columns', '')}",
        f"描述: {spec.get('description', '')}",
    ]
    # 附加属性
    for attr in spec.get("attributes", []):
        parts.append(
            f"  - {attr.get('name', '')}: {attr.get('value', '')} {attr.get('unit', '')}"
        )
    return "\n".join(parts)
