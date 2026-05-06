/**
 * SSE 流式代理 Route Handler
 *
 * 将前端聊天请求代理到 FastAPI Agent 服务
 */

import { NextRequest } from "next/server";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

const AGENT_API_BASE_URL =
  process.env.NEXT_PUBLIC_AGENT_API_URL ||
  "http://127.0.0.1:18100";

export async function POST(req: NextRequest) {
  try {
    const body = await req.json();
    const authorization = req.headers.get("authorization");
    const userId = req.headers.get("x-user-id");
    const userAccount = req.headers.get("x-user-account");
    const tenantId = req.headers.get("x-tenant-id");
    const permissions = req.headers.get("x-user-permissions");
    const requestOrigin = req.headers.get("x-request-origin") || "web";

    // 转发到 FastAPI Agent 服务
    const upstream = await fetch(
      `${AGENT_API_BASE_URL}/api/v1/chat/stream`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(authorization && {
            Authorization: authorization,
          }),
          ...(userId && {
            "X-User-Id": userId,
          }),
          ...(userAccount && {
            "X-User-Account": userAccount,
          }),
          ...(tenantId && {
            "X-Tenant-Id": tenantId,
          }),
          ...(permissions && {
            "X-User-Permissions": permissions,
          }),
          "X-Request-Origin": requestOrigin,
          // 内部 API Key（如有需要）
          ...(process.env.INTERNAL_API_KEY && {
            "X-Internal-Key": process.env.INTERNAL_API_KEY,
          }),
        },
        body: JSON.stringify(body),
      },
    );

    if (!upstream.ok) {
      const errorText = await upstream.text();
      console.error("Upstream error:", errorText);
      return new Response(
        JSON.stringify({ error: "Agent service error", details: errorText }),
        {
          status: upstream.status,
          headers: { "Content-Type": "application/json" },
        },
      );
    }

    // 直接透传 SSE 流
    return new Response(upstream.body, {
      headers: {
        "Content-Type": "text/event-stream",
        "Cache-Control": "no-cache",
        "X-Accel-Buffering": "no",
      },
    });
  } catch (error) {
    console.error("Chat proxy error:", error);
    return new Response(JSON.stringify({ error: "Internal server error" }), {
      status: 500,
      headers: { "Content-Type": "application/json" },
    });
  }
}
