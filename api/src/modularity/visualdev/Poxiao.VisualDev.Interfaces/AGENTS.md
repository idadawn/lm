<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.VisualDev.Interfaces

## Purpose
低代码模块对外契约工程，提供给其它模块或工作流引擎注入。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.VisualDev.Interfaces.csproj` | 工程文件 |
| `IVisualDevService.cs` | 功能模板查询：`GetInfoById(id, isGetRelease)`、`GetDataExists(id)` / `(enCode, fullName)`、`CreateImportData`、`NoTblToTable` |
| `IRunService.cs` | 在线开发运行服务：`Create`、`CreateHaveTableSql`、`Update`、动态运行所需公开 API 集合 |

## For AI Agents

### Working in this directory
- 接口工程**仅**引用 `Poxiao.VisualDev.Entitys` + `Poxiao.WorkFlow.Entitys` + `Systems.Entitys`，不要把 Engine 内部类型暴露出来。
- 新增对外能力（如「按 Code 取功能」）应在此扩展，避免其它模块直接依赖具体实现 `RunService`/`VisualDevService`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
