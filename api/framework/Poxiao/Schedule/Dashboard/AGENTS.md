<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dashboard

## Purpose
Schedule 模块的可视化看板根目录。包含承载于嵌入资源的前端 SPA（`frontend/`）与 ASP.NET Core 中间件后端（`backend/`），通过 `UseScheduleUI()` 暴露 `/__schedule__` 等静态资源与 `/<RequestPath>/api/*` JSON 接口。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `backend/` | UI 中间件、配置选项与拓展方法 (see `backend/AGENTS.md`) |
| `frontend/` | 预编译的 React 静态资源，作为程序集嵌入资源对外服务 (see `frontend/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 看板前端为构建产物，不要手工编辑 `frontend/static/` 下的 hash 文件名；版本升级需替换整套构建产物并同步 `apiconfig.js` 占位符 `%(RequestPath)`。
- 后端中间件直接路由 `/get-jobs`、`/operate-job`、`/operate-trigger`、`/check-change`（SSE）。新增动作需同时改 `ScheduleUIMiddleware` 与前端 `apiconfig.js`。

### Common patterns
- 前端通过 `EmbeddedFileProvider` 暴露，`Poxiao.Schedule.Dashboard.frontend.*` 资源名严格对应文件路径。
- 中间件在 `IsProduction()` + `DisableOnProduction=true` 时短路返回，避免暴露调试端点。

## Dependencies
### Internal
- 复用根目录 `Schedule` 的 `ISchedulerFactory`、`SchedulerEventArgs`、`TriggerTimeline`。
### External
- `Microsoft.AspNetCore.Builder`、`Microsoft.Extensions.FileProviders.EmbeddedFileProvider`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
