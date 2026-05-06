<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# tests

## Purpose
Web 前端 Playwright 端到端测试。组件级单元测试在 `components/<X>/<X>.test.tsx`（vitest），不在本目录。

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `e2e/` | Playwright spec：`knowledge-graph.spec.ts`（/kg 页面渲染 + 健康状态 + Tab 切换）、`root-cause.spec.ts`（KG 推理链 SSE 流 + 折叠块渲染） |

## For AI Agents

### Working In This Directory
- E2E 依赖真实运行的服务栈：`docker compose up -d`（Neo4j + MySQL）→ `cd services/agent-api && uv run uvicorn app.main:app --port 18100` → `cd nlq-agent && pnpm --filter web dev`（端口 3000）。
- spec 文件命名 `kebab-case.spec.ts`，置于 `e2e/`；标签命名 `@feature-name`（如 `@root-cause`），允许 `pnpm test:e2e --grep @root-cause` 选择运行。
- 不要在 spec 里 mock SSE — 走真实流，测试真实组件渲染顺序与累积。

### Testing Requirements
- 运行：`pnpm --filter web test:e2e`；HTML 报告输出到 `apps/web/playwright-report/`（已 gitignore，本仓暂时未 ignore，注意提交前清理）。
- 失败时优先从 `playwright-report/` 看 trace。

### Common Patterns
- 用 `page.on("response", ...)` 监听 `text/event-stream` 响应，逐行 parse `data: ...` 累加到 `reasoningEvents` 数组校验。
- 选择器优先 `data-testid`（如 `[data-testid="kg-reasoning-chain"]`）+ 中文 `getByRole/getByText`，避免依赖样式类名。

## Dependencies

### Internal
- 全栈协作 — 隐式依赖 `services/agent-api`、Neo4j（KG 必须真实初始化）、MySQL（`lab_*` 有数据）。

### External
- `@playwright/test@^1.50`。
