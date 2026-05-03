<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# app

## Purpose
移动端/门户 App 后端模块根目录。聚合 App 端常用数据、菜单、用户信息以及版本检查相关接口（路由前缀 `api/App/*`）。底层依托 `system` 模块的用户/部门/岗位服务和 `workflow` 模块的流程模板。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.Apps/` | 服务实现工程（`AppDataService` / `AppUserService` / `AppMenuService` / `AppVersion`） (见 `Poxiao.Apps/AGENTS.md`) |
| `Poxiao.Apps.Interfaces/` | 对外接口契约（`IAppDataService` 等） (见 `Poxiao.Apps.Interfaces/AGENTS.md`) |
| `Poxiao.Apps.Entitys/` | 实体（`AppDataEntity` -> `BASE_APPDATA`）与 DTO (见 `Poxiao.Apps.Entitys/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 路由统一使用 `[Route("api/App/[controller]")]`，Swagger Tag 固定为 `App`。
- App 接口主要服务移动端/小程序，DTO 字段保留 camelCase 与可空标记（`string?`）。
- 与 system 模块共享用户/权限实体（`UserEntity` / `ModuleEntity`），不要在此模块再重新建表。

### Common patterns
- 服务类用 `: IDynamicApiController, ITransient` 标记，注入 `IUserManager`、`IFlowTemplateService` 等跨模块服务。
- 通过 Mapster `Adapt<T>` 在实体与 DTO 间转换。

## Dependencies
### Internal
- `api/src/modularity/system`（用户/模块/部门）
- `api/src/modularity/workflow`（流程模板）
- `framework/Poxiao` 基础设施

### External
- `Mapster`、`SqlSugar`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
