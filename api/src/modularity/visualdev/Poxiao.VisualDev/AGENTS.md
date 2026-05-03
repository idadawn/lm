<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.VisualDev

## Purpose
低代码模块实现工程。承载 7 个核心服务/控制器，覆盖功能模板、运行时、门户、首页、模型数据、外链、APP 等。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.VisualDev.csproj` | 工程文件，引用 Engine + Workflow + Message + System + Extend + 自身 Interfaces |
| `VisualDevService.cs` | `IVisualDevService` + 控制器（`api/visualdev/Base`）：功能模板 CRUD、字段查询、菜单导出、模板转有表 |
| `VisualDevModelDataService.cs` | 模型数据 CRUD：在用户业务表/`BASE_VISUALDEV_MODELDATA` 上运行时增删改查 |
| `RunService.cs` | 在线运行核心：动态生成 SQL/表，桥接工作流流程实例 |
| `PortalService.cs` | 门户设计 CRUD（`PortalEntity`/`PortalDataEntity`） |
| `DashboardService.cs` | 首页待办/未读/邮件/通知聚合 |
| `VisualdevShortLinkService.cs` | 外链填单/查询（`VisualDevShortLinkEntity`） |
| `VisualdevModelAppService.cs` | App 端模型适配（与 PC 端共享 RunService） |

## For AI Agents

### Working in this directory
- `VisualDevService` 把 `RunService` 作为字段注入；高阶 CRUD 走 RunService，控制器自身仅维护模板与字典。
- 字段类型映射、动态建表语句来自 `Poxiao.VisualDev.Engine.Core`，不要在 service 中硬编码 DDL。
- `IDataBaseManager` 用于切换业务库（`F_DBLINKID`）；切库后必须及时 `Restore` 回主库。

### Common patterns
- 所有路由 `api/visualdev/[controller]`、Tag = `VisualDev`、Order 171–175。
- 失败抛 `Oops.Oh(...)` 走 FriendlyException 统一返回结构。

## Dependencies
### Internal
- `Poxiao.VisualDev.Engine` / `Poxiao.WorkFlow.Interfaces.{Repository,Service}`
- `Poxiao.Message.Interfaces.IMessageService`、`Poxiao.Systems.Interfaces.IDictionaryDataService`/`IDataBaseManager`
### External
- Mapster、SqlSugar、Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
