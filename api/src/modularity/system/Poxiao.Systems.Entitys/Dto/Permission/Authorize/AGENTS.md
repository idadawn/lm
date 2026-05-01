<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Authorize (Dto)

## Purpose
权限分配 API DTO。覆盖角色/用户授权批量保存、按对象/类型查询授权数据、列字段权限编辑、可显示字段输出等场景。

## Key Files
| File | Description |
|------|-------------|
| `AuthorizeDataBatchInput.cs` | 批量授权保存（角色/用户 → 菜单/按钮/列/数据权限） |
| `AuthorizeDataModelOutput.cs` | 授权数据聚合输出（含菜单+按钮+列+数据权限） |
| `AuthorizeDataOutput.cs` | 单条授权记录输出 |
| `AuthorizeDataQuery.cs` / `AuthorizeDataQueryInput.cs` | 授权查询条件 |
| `AuthorizeDataUpInput.cs` | 授权更新输入 |
| `AuthorizeModelInput.cs` | 模型授权输入 |
| `ColumnsPurviewDataUpInput.cs` | 列字段权限保存 |
| `ListDisplayFieldOutput.cs` | 列表展示字段输出（用于动态列显示） |

## For AI Agents

### Working in this directory
- `AuthorizeDataBatchInput` 是关键入参，结构修改直接影响前端"角色权限"页面，需联调。
- 字段命名混用 PascalCase / camelCase，遵循已有命名以避免序列化问题。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
