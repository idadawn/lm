<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# app

## Purpose
Next.js 15 App Router 根。三个用户态页面 + 两个后端代理 Route Handler。

## Key Files

| File | Description |
|------|-------------|
| `layout.tsx` | RootLayout — 设 `lang="zh-CN"`，导入 `globals.css`，定义 `<title>` `<description>` |
| `page.tsx` | `/` 默认页 — 透传 `?embed=1&mode=dock\|fullscreen` 到 `<NlqChatPanel>` |
| `globals.css` | Tailwind 入口 + 全局样式 |

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `api/` | Route Handlers：`chat/route.ts`（SSE 代理 `/api/v1/chat/stream`）、`kg/[...path]/route.ts`（catch-all 透传到 `/api/v1/kg/*`）（见 `api/AGENTS.md`） |
| `dashboard/` | `/dashboard` 页面 — 月度质量驾驶舱（KPI / 趋势 / 分布 / 班次对比 / 散点 / Top5）静态样例数据 |
| `kg/` | `/kg` 页面 — 知识图谱浏览器（7 个 Tab：概览 / 产品规格 / 指标公式 / 判定规则 / 外观特性 / 报表统计 / 系统配置） |

## For AI Agents

### Working In This Directory
- 默认 Server Component；只有真正需要 hook（`useState`、`useEffect`、`useSearchParams`）时才 `"use client"`。
- 与后端通信优先走本目录 Route Handler（不要在客户端组件直连 18100，防止暴露内网地址 + 跨域）。
- Route Handler 必须 `export const runtime = "nodejs"` + `export const dynamic = "force-dynamic"`（SSE 必须）。

### Testing Requirements
- 端到端：`tests/e2e/*.spec.ts`（Playwright），覆盖 `/kg` 与 `/`（KG reasoning chain 渲染）。
- 路由 mocking：vitest 单测在 `components/`，本目录通过 e2e 验证。

### Common Patterns
- 静态样例数据（如 `dashboard/page.tsx` 的 `dashboardPayload`）显式标记类型为 `MonthlyDashboardPayload`，确保字段对齐 shared-types。
- 鉴权透传：`/api/chat` Handler 把 `authorization`、`x-user-id`、`x-user-account`、`x-tenant-id`、`x-user-permissions`、`x-request-origin` 头部原样转发给 agent-api。

## Dependencies

### Internal
- `@/components/chat`（`NlqChatPanel`）、`@/components/charts`（dashboard 图表）。
- `@nlq-agent/shared-types`（`MonthlyDashboardPayload`、`ProductSpec`、`MetricFormula`、`JudgmentRule`、`ReportConfig` 等）。
- 环境变量 `NEXT_PUBLIC_AGENT_API_URL`（默认 `http://127.0.0.1:18100`）。

### External
- `next/server`、`next/navigation`、`react@^19`。
