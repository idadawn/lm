<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Position (Dto)

## Purpose
岗位 DTO。岗位附属于机构，与角色/用户多对多。提供 CRUD、详情、列表、选择器、缓存列表与查询条件。

## Key Files
| File | Description |
|------|-------------|
| `PositionCrInput.cs` / `PositionUpInput.cs` | 岗位创建/更新 |
| `PositionInfoOutput.cs` | 详情 |
| `PositionListOutput.cs` | 列表 |
| `PositionListQuery.cs` / `PositionConditionInput.cs` | 查询条件 |
| `PositionSelectorOutput.cs` | 选择器 |
| `PositionCacheListOutput.cs` | 缓存全量列表（用于本地下拉、避免反复查询） |

## For AI Agents

### Working in this directory
- 缓存列表 (`PositionCacheListOutput`) 经 `IMemoryCache` 提供，DTO 字段尽量精简以减少缓存体积。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
