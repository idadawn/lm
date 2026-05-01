<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# api

## Purpose
Per-feature API client modules for the LIMS web app. Each subdirectory wraps a backend module's REST endpoints (oauth/system/permission/workflow/lab/...) into typed JS functions calling `defHttp`. New AI / NLQ-agent endpoints (SSE) live here too.

## Key Files
| File | Description |
|------|-------------|
| `nlqAgent.ts` | SSE client for `nlq-agent/services/agent-api`. Streams `reasoning_step` events; final `response_metadata.reasoning_steps` is the canonical override. Base URL via `VITE_NLQ_AGENT_API_BASE`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `basic/` | OAuth (login/logout/userinfo) + common + chart mock APIs (see `basic/AGENTS.md`). |
| `chart/` | Indicator/chart visual-dev mocks (see `chart/AGENTS.md`). |
| `collector/` | 数据采集器 — `/collectServer/collector/*` (see `collector/AGENTS.md`). |
| `createModel/` | KPI 建模接口 (see `createModel/AGENTS.md`). |
| `dataAnalysis/` | 数据分析 / FastGPT 接入 (see `dataAnalysis/AGENTS.md`). |
| `dimension/` | 公共维度 (`metric-dimension`) (see `dimension/AGENTS.md`). |
| `extend/` | 扩展业务模块: bigData / document / email / order / projectGantt / saleOrder / schedule / table (see `extend/AGENTS.md`). |
| `lab/` | 化验室核心域: rawData / appearance / metric / dashboard / monthlyQualityReport / formula 等 (see `lab/AGENTS.md`). |
| `labelManagement/` | KPI 标签 (`metrictag`) (see `labelManagement/AGENTS.md`). |
| `msgCenter/` | 消息中心配置/模板/发送 (see `msgCenter/AGENTS.md`). |
| `onlineDev/` | 在线开发: portal / dataReport / dataV / shortLink / visualDev (see `onlineDev/AGENTS.md`). |
| `permission/` | 用户/角色/岗位/分组/组织/授权 (see `permission/AGENTS.md`). |
| `service/` | 数据采集服务 (`/collectServer/service`) (see `service/AGENTS.md`). |
| `status/` | 价值链状态 (`metric-covstatus`) (see `status/AGENTS.md`). |
| `system/` | 系统管理: 菜单/字典/日志/缓存/版本/打印 (see `system/AGENTS.md`). |
| `systemData/` | 系统数据: 数据源/接口/字典/数据模型 (see `systemData/AGENTS.md`). |
| `targetDefinition/` | 指标定义 (`metrickinship`) (see `targetDefinition/AGENTS.md`). |
| `targetDirectory/` | 指标分类/目录 (`metriccategory`) (see `targetDirectory/AGENTS.md`). |
| `workFlow/` | 工作流引擎 (`/api/workflow/Engine/*`) (see `workFlow/AGENTS.md`). |

## For AI Agents

### Working in this directory
- All HTTP goes through `/@/utils/http/axios` `defHttp.{get,post,put,delete}({ url, data, params, headers }, options?)` — never call `axios` directly except in the SSE client.
- Each module typically declares an inner `enum Api { Prefix = '/api/...', ... }` or a `Url = { ... }` constant block. Keep that pattern when adding endpoints.
- Type definitions for request/response shapes live in adjacent `model/` (sometimes `.ts`) or `typing/` (sometimes `.d.ts`) folders — preserve the existing convention per subdirectory.
- Mock URLs use `import.meta.env.VITE_MOCK_SERVER` — leave the structure intact even when migrating to real APIs.
- 后端是 .NET / SqlSugar 模块化单体；endpoint 命名按后端模块走 (e.g. `/api/permission/Users`, `/api/lab/raw-data`).

### Common patterns
- Functions exported by name (not default), e.g. `getUserList`, `createUser` — call sites import `{ ... }` directly.
- Promise return types are mostly untyped `Promise<any>`; some newer modules (lab/types/*) provide concrete shapes.
- Special options: `{ isTransformResponse: false }` for raw envelope, `{ withToken: false }` for cross-origin (FastGPT).

## Dependencies
### Internal
- `/@/utils/http/axios`, `/@/utils/file/download`, `/@/enums/httpEnum`, `/@/types/reasoning-protocol`.
### External
- `axios` (transitively via `defHttp`), `qs`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
