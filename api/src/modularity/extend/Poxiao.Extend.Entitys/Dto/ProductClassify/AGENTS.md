<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ProductClassify

## Purpose
产品分类（树）DTO。提供分类的 CRUD 与树形列表，给前端的下拉/树选择器使用。

## Key Files
| File | Description |
|------|-------------|
| `ProductClassifyCrInput.cs` / `ProductClassifyUpInput.cs` | 分类创建/更新（parentId 自关联） |
| `ProductClassifyInfoOutput.cs` | 分类详情 |
| `ProductClassifyTreeOutput.cs` | 树形结构（含 children） |

## For AI Agents

### Working in this directory
- 分类是无限层级 `parentId` 自关联，新增字段时同步 `ProductClassifyEntity`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
