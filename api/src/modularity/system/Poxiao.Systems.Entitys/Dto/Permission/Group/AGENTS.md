<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Group (Dto)

## Purpose
用户分组 DTO。分组用于跨机构/角色将一组用户聚合（典型场景如群发、流程会签）。提供 CRUD 与选择器。

## Key Files
| File | Description |
|------|-------------|
| `GroupCrInput.cs` | 创建分组输入 |
| `GroupUpInput.cs` | 更新分组输入 |
| `GroupListOutput.cs` | 分组列表输出 |
| `GroupSelectorOutput.cs` | 选择器输出 |

## For AI Agents

### Working in this directory
- 分组与角色是不同概念：角色 → 权限；分组 → 业务聚合。不要在 DTO 中混用字段语义。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
