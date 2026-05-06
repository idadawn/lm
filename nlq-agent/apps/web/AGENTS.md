<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# web

## Purpose
NLQ-Agent Web 前端。Next.js 15 App Router，三个用户态界面：
- `/`（默认页）— `NlqChatPanel` 聊天对话，消费 SSE，渲染图表 + 计算说明卡片 + 等级判定卡片 + KG 推理链折叠块
- `/dashboard` — 月度质量驾驶舱（KPI + 趋势 + 分布 + 班次对比 + 散点）
- `/kg` — 知识图谱浏览器（产品规格 / 指标公式 / 判定规则 / 外观特性 / 报表统计 / 系统配置）

`/api/chat` 与 `/api/kg/[...path]` Route Handler 把请求代理到 FastAPI agent-api（默认 `http://127.0.0.1:18100`），SSE 直接透传。

## Key Files

| File | Description |
|------|-------------|
| `package.json` | `@nlq-agent/web@0.1.0`，依赖 `@ai-sdk/react`、`@ant-design/charts`、`ai-elements`、`@nlq-agent/shared-types`（workspace） |
| `next.config.ts` | Next.js 配置 |
| `tailwind.config.ts` | Tailwind 主题（含 shadcn-style 颜色） |
| `vitest.config.ts` | 单元测试配置（jsdom + @testing-library/react） |
| `playwright.config.ts` | E2E 测试配置 |
| `tsconfig.json` | TS 配置（含路径别名 `@/*`） |

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `app/` | Next.js App Router：页面 + Route Handlers（见 `app/AGENTS.md`） |
| `components/` | UI 组件：`charts/`（@ant-design/charts 包装）、`cards/`（计算说明 / 等级判定）、`chat/`（NlqChatPanel + KgReasoningChain）（见 `components/AGENTS.md`） |
| `tests/` | Playwright E2E（`tests/e2e/`）（见 `tests/AGENTS.md`） |

## For AI Agents

### Working In This Directory
- 添加新页面：`app/<route>/page.tsx`，仅当用 hook / state / event 时加 `"use client"`。
- 添加新图表：先在 `components/charts/` 写 `XxxChart.tsx`，再在 `components/charts/index.tsx` 用 `dynamic(..., { ssr: false })` 暴露。
- 调用后端 KG REST：用 `fetch("/api/kg/<path>")`（route 透传），不要直接调 18100。
- SSE 客户端逻辑集中在 `components/chat/NlqChatPanel.tsx`，不要在多处复制 SSE 解析。

### Testing Requirements
- 单元：`pnpm --filter web test`（vitest，jsdom），新组件加 `*.test.tsx`。
- 端到端：`pnpm --filter web test:e2e`（Playwright），需先启动 docker compose（Neo4j + MySQL）+ agent-api（端口 18100）+ `pnpm --filter web dev`。
- 类型检查：`pnpm --filter web type-check`。

### Common Patterns
- Tailwind utility-first，禁止内联 `style={...}`（除非 chart annotation 等三方库要求）。
- 组件 PascalCase；hook / util camelCase；CSS class kebab-case。
- 鉴权：组件优先把上游主系统传来的 `?token=&account=` 和 `x-user-*` header 透传到 `/api/chat`，agent-api 会解析 `auth_context`。

## Dependencies

### Internal
- `@nlq-agent/shared-types` — `ChatRequest/Response`、`StreamEvent`、`ReasoningStep`、`ChartDescriptor`、KG 类型。
- `services/agent-api`（运行时）— 通过 `NEXT_PUBLIC_AGENT_API_URL`（默认 `http://127.0.0.1:18100`）访问。

### External
- `next@^15`、`react@^19`、`@ant-design/charts@^2.2`、`ai-elements@^1.8`、`react-markdown`、`zustand@^5`、`tailwindcss@^3.4`、`@playwright/test@^1.50`、`vitest@^3`。
