"""认证与权限校验模块."""

import time
from typing import Any

import httpx
from fastapi import HTTPException, status

from app.core.config import settings

_auth_cache: dict[str, tuple[float, dict[str, Any]]] = {}


def _normalize_token(auth_context: dict[str, Any]) -> str:
    """Extract access token from auth context."""
    token = auth_context.get("access_token")
    if not isinstance(token, str):
        return ""
    return token.strip()


def _get_required_permission_tokens() -> list[str]:
    """Read required permission tokens from settings."""
    raw_value = settings.REQUIRED_PERMISSION_MODULES.strip()
    if not raw_value:
        return []
    return [item.strip().lower() for item in raw_value.split(",") if item.strip()]


def _match_permission(permission: dict[str, Any], required_tokens: list[str]) -> bool:
    """Check whether one permission entry matches configured module tokens."""
    module_name = str(permission.get("moduleName", "")).lower()
    model_id = str(permission.get("modelId", "")).lower()

    return any(
        token in module_name or token in model_id for token in required_tokens
    )


def _build_upstream_url() -> str:
    base_url = settings.POXIAO_API_BASE_URL.rstrip("/")
    path = settings.POXIAO_CURRENT_USER_PATH
    if not path.startswith("/"):
        path = f"/{path}"
    return f"{base_url}{path}"


async def _fetch_current_user(access_token: str) -> dict[str, Any]:
    """Call upstream current-user endpoint."""
    current_time = time.time()
    cached = _auth_cache.get(access_token)
    if cached and cached[0] > current_time:
      return cached[1]

    headers = {
        "Authorization": f"Bearer {access_token}",
        "Poxiao-Origin": "pc",
    }
    async with httpx.AsyncClient(timeout=15.0) as client:
        response = await client.get(_build_upstream_url(), headers=headers)

    if response.status_code in (401, 403):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="登录态无效或已过期，请重新登录。",
        )

    if response.status_code >= 400:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="上游用户认证服务不可用。",
        )

    payload = response.json()
    data = payload.get("data", payload)
    user_info = data.get("userInfo", {})
    permission_list = data.get("permissionList", [])
    result = {
        "raw": data,
        "userInfo": user_info,
        "permissionList": permission_list,
    }
    _auth_cache[access_token] = (
        current_time + settings.AUTH_CACHE_TTL_SECONDS,
        result,
    )
    return result


def _extract_permission_keys(permission_list: list[dict[str, Any]]) -> list[str]:
    """Flatten permission keys for downstream context."""
    keys: list[str] = []
    for permission in permission_list:
        module_name = permission.get("moduleName")
        model_id = permission.get("modelId")
        if isinstance(module_name, str) and module_name:
            keys.append(module_name)
        if isinstance(model_id, str) and model_id:
            keys.append(model_id)
    return keys


async def validate_chat_auth(auth_context: dict[str, Any]) -> dict[str, Any]:
    """Validate upstream token and enforce module permission for chat."""
    token = _normalize_token(auth_context)

    if not token:
        if settings.AUTH_REQUIRED:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="未登录，无法访问智能问数对话。",
            )
        return auth_context

    if not settings.AUTH_VALIDATE_UPSTREAM:
        return auth_context

    current_user = await _fetch_current_user(token)
    user_info = current_user.get("userInfo") or {}
    permission_list = current_user.get("permissionList") or []

    is_admin = bool(user_info.get("isAdministrator"))
    required_tokens = _get_required_permission_tokens()
    has_permission = is_admin or not required_tokens or any(
        _match_permission(permission, required_tokens)
        for permission in permission_list
        if isinstance(permission, dict)
    )

    if not has_permission:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="当前账号没有实验室问数权限。",
        )

    enriched_context = dict(auth_context)
    enriched_context["user_id"] = str(
        user_info.get("userId") or enriched_context.get("user_id") or ""
    )
    enriched_context["account"] = str(
        user_info.get("userAccount") or enriched_context.get("account") or ""
    )
    enriched_context["tenant_id"] = str(
        user_info.get("organizeId") or enriched_context.get("tenant_id") or ""
    )
    enriched_context["user_name"] = str(user_info.get("userName") or "")
    enriched_context["is_administrator"] = is_admin
    enriched_context["permissions"] = _extract_permission_keys(permission_list)
    return enriched_context
