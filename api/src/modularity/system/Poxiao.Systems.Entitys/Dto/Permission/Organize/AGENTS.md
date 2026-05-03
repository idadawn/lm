<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Organize (Dto)

## Purpose
机构（公司/部门）DTO。覆盖 CRUD、树形输出、成员列表、选择器、过滤条件等场景，是组织架构页面的主要入参/出参。

## Key Files
| File | Description |
|------|-------------|
| `OrganizeCrInput.cs` / `OrganizeUpInput.cs` | 机构创建/更新 |
| `OrganizeInfoOutput.cs` | 机构详情 |
| `OrganizeListOutput.cs` | 机构列表项 |
| `OrganizeTreeOutput.cs` | 树形结构（含子节点 children） |
| `OrganizeMemberListOutput.cs` | 机构下成员列表 |
| `OrganizeSelectorOutput.cs` | 机构选择器 |
| `OrganizeConditionInput.cs` | 机构查询条件 |

## For AI Agents

### Working in this directory
- 树形输出依赖 `ParentId` 递归；接口返回时 Service 已组装好 children，前端不再二次构建。
- 任何字段调整需同步 `Mapper/PermissionMapper.cs` 与前端机构组件。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
