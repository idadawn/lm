<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ModuleColumn (Dto)

## Purpose
菜单列字段权限 DTO。控制列表页面用户可见列。提供 CRUD、详情、列表、批量编辑、通用输出。

## Key Files
| File | Description |
|------|-------------|
| `ModuleColumnCrInput.cs` / `ModuleColumnUpInput.cs` | 创建/更新单列 |
| `ModuleColumnActionsBatchInput.cs` | 批量保存（前端在表设计器一次提交全部列） |
| `ModuleColumnInfoOutput.cs` | 详情 |
| `ModuleColumnListOutput.cs` | 列表 |
| `ModuleColumnOutput.cs` | 通用输出（嵌入菜单授权树） |

## For AI Agents

### Working in this directory
- 列字段权限与 `ColumnsPurviewEntity`（角色侧）配合使用：此处定义可控列，那边定义角色具体可见列。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
