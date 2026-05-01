<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Apps

## Purpose
App 模块的服务实现工程。所有类都是 `IDynamicApiController + ITransient`，挂载在 `api/App/*` 路由下，向移动端/小程序提供常用数据、菜单、用户信息与版本检查能力。

## Key Files
| File | Description |
|------|-------------|
| `AppDataService.cs` | `IAppDataService` 实现：CRUD `AppDataEntity` (`BASE_APPDATA`)，结合 `IFlowTemplateService` 输出常用数据/流程入口 |
| `AppUserService.cs` | App 用户聚合：注入 `IUsersService` / `IDepartmentService` / `IPositionService` 输出当前用户信息 |
| `AppMenuService.cs` | 调用 `IAppDataService.GetAppMenuList` 返回 `ToTree("-1")` 后的菜单 |
| `AppVersion.cs` | 基于 `SysConfigEntity` 读取 App 版本信息，`api/App/Version` |
| `Poxiao.Apps.csproj` | 引用 system / workflow / common 等多个上游工程 |

## For AI Agents

### Working in this directory
- 新增 App 接口先在 `Poxiao.Apps.Interfaces` 定义 `IXxxService`，再在此实现，保持依赖反转。
- 注入跨模块服务时使用接口（如 `IUserManager`），禁止直接 `new` 仓储以外的对象。
- DTO 使用 `Mapster.Adapt`，`AppDataListOutput` 已声明为 `[SuppressSniffer]`，新增 DTO 请保持一致。

### Common patterns
- 头部统一中文 XML 注释（"版 本 / 版 权 / 作 者 / 日 期"），保持代码风格。
- 控制器以 `[ApiDescriptionSettings(Tag = "App", Name = "...", Order = 8xx)]` 编号区分子功能。

## Dependencies
### Internal
- `Poxiao.Apps.Interfaces`、`Poxiao.Apps.Entitys`
- `Poxiao.Systems.Interfaces`、`Poxiao.WorkFlow.Interfaces`
- `framework/Poxiao`（DI、动态 API、Manager）

### External
- `Mapster`、`SqlSugar`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
