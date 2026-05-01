<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# visualdev

## Purpose
低代码 / 在线开发（VisualDev）模块根目录：表单/列表设计器、门户设计、首页 Dashboard、外链填单、模型数据运行时、代码生成等。前端通过 `api/visualdev/...` 访问。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.VisualDev/` | 服务实现（VisualDev/Portal/Dashboard/Run/ModelData/ShortLink/ModelApp）(see `Poxiao.VisualDev/AGENTS.md`) |
| `Poxiao.VisualDev.Entitys/` | 实体、DTO、枚举、Mapper (see `Poxiao.VisualDev.Entitys/AGENTS.md`) |
| `Poxiao.VisualDev.Interfaces/` | `IVisualDevService` / `IRunService` 抽象 (see `Poxiao.VisualDev.Interfaces/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 模板（`VisualDevEntity`）+ 发布版本（`VisualDevReleaseEntity`）双表设计：编辑修改在 `VisualDevEntity`；前端运行时按需读 release 版本，由 `IVisualDevService.GetInfoById(id, isGetRelease = true)` 切换。
- 模型数据运行时（`RunService`）支持「无表（NoTbl）」与「有表」两种模式，调用 `IDataBaseManager` 切库执行动态 SQL。

### Common patterns
- 所有控制器：`[ApiDescriptionSettings(Tag = "VisualDev", Name = "...", Order = 17x)]` + `[Route("api/visualdev/[controller]")]`。

## Dependencies
### Internal
- `engine/Poxiao.VisualDev.Engine`（表单引擎核心）、`Poxiao.WorkFlow.Interfaces`（流程联动）
- `Poxiao.Message.Interfaces`、`Poxiao.Systems.Interfaces`、`Poxiao.Extend.Interfaces`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
