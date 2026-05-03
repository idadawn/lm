<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Department (Dto)

## Purpose
部门管理 DTO。部门是机构（Organize）的子分类，提供 CRUD、选择器、详情、列表等输入/输出。

## Key Files
| File | Description |
|------|-------------|
| `DepartmentCrInput.cs` | 创建部门输入 |
| `DepartmentUpInput.cs` | 更新部门输入 |
| `DepartmentInfoOutput.cs` | 部门详情输出 |
| `DepartmentListOutput.cs` | 部门列表项输出 |
| `DepartmentSelectorOutput.cs` | 选择器（下拉/树）输出 |

## For AI Agents

### Working in this directory
- 部门隶属机构，主键与 `OrganizeEntity` 一致；新增字段时考虑在 `OrganizeEntity` 与 Mapper 对应。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
