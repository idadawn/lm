<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# api

## Purpose
Next.js Route Handlers 集合。两个反向代理：`chat/route.ts`（POST → `/api/v1/chat/stream`，SSE 透传）、`kg/[...path]/route.ts`（catch-all GET/POST → `/api/v1/kg/<rest>`，JSON 透传）。所有上游地址来自 `NEXT_PUBLIC_AGENT_API_URL`（默认 `http://127.0.0.1:18100`）。

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `chat/` | `chat/route.ts` — SSE 流代理，转发 `Authorization` + `x-user-*` + `x-tenant-id` + `x-user-permissions` + `x-request-origin`（默认 "web"） |
| `kg/` | `kg/[...path]/route.ts` — KG REST 通用透传（保留 query string，原样返回 status code 和 Content-Type） |

## For AI Agents

### Working In This Directory
- 所有 Handler 必须 `export const runtime = "nodejs"` + `export const dynamic = "force-dynamic"`（SSE 不能跑在 edge runtime；动态强制避免 ISR 缓存）。
- 不要在客户端组件直连 18100：跨域 + 暴露内网地址。一切走本目录 Handler。
- 新增 `INTERNAL_API_KEY` 等敏感 header 时只能从 `process.env.*` 读，禁止从前端 cookie/searchParams 取。

### Testing Requirements
- Handler 集成测试归 e2e（`tests/e2e/`），单测一般跳过（mock fetch 价值小）。
- 必跑 `pnpm --filter web type-check`：Next 15 的 Route Handler context 类型 `{ params: Promise<...> }` 容易写错。

### Common Patterns
- SSE 透传：直接 `return new Response(upstream.body, { headers: { Content-Type: "text/event-stream", "Cache-Control": "no-cache", "X-Accel-Buffering": "no" } })`，不要 buffer。
- catch-all 路径：`async function GET(req, context: { params: Promise<{ path: string[] }> })` —— Next 15 起 params 是 Promise。
- 错误回包：`new Response(JSON.stringify({ error: ... }), { status, headers })`，不要 `throw`（会变成 500 + 暴露栈）。

## Dependencies

### Internal
- 上游 agent-api `services/agent-api`（运行时）。

### External
- `next/server` 的 `NextRequest` / `Response`。
