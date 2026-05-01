<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Apps.Interfaces

## Purpose
App 模块对外契约。供其它模块或 Web.Core 通过接口注入使用，避免直接依赖 `Poxiao.Apps` 实现。

## Key Files
| File | Description |
|------|-------------|
| `IAppDataService.cs` | App 常用数据/菜单接口：`GetAppMenuList(keyword)` 返回 `List<ModuleEntity>`，`Delete(objectId)` 软删除 |
| `Poxiao.Apps.Interfaces.csproj` | 仅引用基础公共工程，避免循环依赖 |

## For AI Agents

### Working in this directory
- 此程序集禁止放入实现，只允许接口、抽象方法或共享枚举。
- 接口签名引用的实体类型（如 `ModuleEntity`）必须来自 `system` 模块的 Entitys 工程，不要新增。
- 新增接口请使用同一命名空间 `Poxiao.Apps.Interfaces`。

### Common patterns
- 接口方法保持中文 XML 注释，列出所有参数说明。

## Dependencies
### Internal
- `Poxiao.Systems.Entitys`（`ModuleEntity`）

### External
- 无

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
