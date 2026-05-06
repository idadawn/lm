<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# apps

## Purpose
NLQ-Agent 用户端可执行应用集合。当前只有 Web 前端 (`web/`) — Next.js 15 + shadcn/ui + ai-elements + @ant-design/charts。移动端按 `CLAUDE.md` 注释暂未在本仓建目录，KG 推理链 UI 落在父项目 `D:/project/lm/mobile/` uni-app 工程里。

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `web/` | Next.js 前端（聊天 + 知识图谱浏览器 + 月度看板）（见 `web/AGENTS.md`） |

## For AI Agents

### Working In This Directory
- 任何新增 app 必须在根 `pnpm-workspace.yaml` 已通过 `apps/*` 通配捕获，无需修改 workspace 配置。
- 共享类型从 `@nlq-agent/shared-types`（`packages/shared-types`）导入，禁止在 app 内重复定义 `ChatRequest`、`ReasoningStep` 等。
- Turborepo 任务（`dev`/`build`/`lint`/`type-check`/`test`/`test:e2e`）由根 `turbo.json` 编排，单 app 命令用 `pnpm --filter web <task>`。

### Testing Requirements
- 单元测试：vitest（`pnpm --filter web test`）。
- 端到端：Playwright（`pnpm --filter web test:e2e`，依赖真实 agent-api + Neo4j）。

### Common Patterns
- 仅在用 hook / 浏览器 API 时加 `"use client"`；其余优先 Server Components。
- `@ant-design/charts` 全部走 `next/dynamic` + `{ ssr: false }`（Canvas 图表 SSR 报错）。

## Dependencies

### Internal
- `packages/shared-types` — 通过 `workspace:*` 引用，提供 SSE 事件、推理链协议、KG 类型。

### External
- `next@^15`、`react@^19`、`@ant-design/charts@^2.2`、`ai-elements@^1.8`、`@playwright/test`、`vitest@^3`。
