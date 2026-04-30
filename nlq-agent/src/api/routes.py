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

from fastapi import APIRouter, Depends, Request
from fastapi.responses import PlainTextResponse, StreamingResponse
from prometheus_client import generate_latest

from src.api.dependencies import get_orchestrator, get_services
from src.models.schemas import ChatRequest, HealthResponse
from src.pipelines.orchestrator import PipelineOrchestrator

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


# ── 辅助函数 ─────────────────────────────────────────────────

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
