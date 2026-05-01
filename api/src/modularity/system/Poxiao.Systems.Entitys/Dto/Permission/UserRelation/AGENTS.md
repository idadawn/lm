<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# UserRelation (Dto)

## Purpose
用户关系（上下级、关注）DTO。提供创建、查询输入与列表输出，用于人员架构与流程指派。

## Key Files
| File | Description |
|------|-------------|
| `UserRelationCrInput.cs` | 创建关系（设置主管/下属） |
| `UserRelationInput.cs` | 查询输入 |
| `UserRelationListOutput.cs` | 关系列表项 |
| `UserPageListOutput.cs` | 关系下成员分页列表 |

## For AI Agents

### Working in this directory
- `UserPageListOutput` 名称与 `User/UserPageListOutput` 重复但命名空间不同（`...Dto.UserRelation`），引用时注意 using。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
