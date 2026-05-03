<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Systems

## Purpose
系统模块业务实现项目。包含 Permission（权限/组织）、System（菜单/字典/日志/调度/数据接口）、Common（文件、测试、定时任务）三大业务域的 Service 实现，每个 Service 即动态 API 控制器。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Systems.csproj` | 引用 Interfaces、VisualDev.Engine、Workflow.Interfaces、Message.Interfaces、TaskScheduler.Interfaces、CollectiveOAuth；输出绑定 Version.txt 与 CHANGELOG.md |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Common/` | 通用服务：文件上传/下载、定时任务桥接、调试 (see `Common/AGENTS.md`) |
| `Permission/` | 用户、机构、角色、岗位、分组、社交账号绑定等权限服务 (see `Permission/AGENTS.md`) |
| `System/` | 菜单/字典/日志/数据接口/打印/省份/调度/系统配置等系统级服务 (see `System/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 所有 Service 必须实现对应 `Poxiao.Systems.Interfaces` 中接口；并使用 `IDynamicApiController, ITransient` + `[ApiDescriptionSettings]` 暴露 API。
- 仓储统一使用 `ISqlSugarRepository<XxxEntity>` 注入；多表组合使用 `Poxiao.DatabaseAccessor` 提供的扩展。
- DTO 映射使用 Mapster（`Adapt<T>()`），映射规则集中在 `Poxiao.Systems.Entitys/Mapper`。

### Common patterns
- API 路由前缀：`api/permission/[controller]`（权限） 或 `api/[controller]`（其他）。
- Order 编号约定 160-220 区间，避免与其他模块冲突。
- 大量服务通过 `App.GetConfig<T>(...)` 读取配置（OAuth、AppOptions 等）。

## Dependencies
### Internal
- `Poxiao.Systems.Interfaces`（接口契约）
- `Poxiao.Systems.Entitys`（实体/DTO 经接口项目传递）
- `engine/Poxiao.VisualDev.Engine`（可视化开发引擎）

### External
- Furion 派生（DynamicApiController、FriendlyException、DataEncryption、DependencyInjection 等命名空间均来自 Poxiao 内嵌框架）
- SqlSugar、Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
