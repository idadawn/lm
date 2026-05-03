<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricGot

## Purpose
指标思维图实体。统一管理 `Cov`（价值链）与 `Dash`（仪表板）两类思维图条目，并允许挂多个标签。

## Key Files
| File | Description |
|------|-------------|
| `MetricGotEntity.cs` | 表 `metric_got` (`CUEntityBase`)：`type`(`GotType?`)/`sort?`/`name`/`description?`/`img_name`/`metric_tag?` |

## For AI Agents

### Working in this directory
- `metric_tag` 字段是逗号分隔的 tagId 字符串，服务层会按 `,` 分割再联表 `MetricTagsEntity` 取名称。
- `type` 在数据库以字符串存储（`EnumToStringConvert`），与代码中 `GotType.Cov/Dash` 对应。

### Common patterns
- 列表显示时通常 `x.Type?.GetDescription()` 转中文。

## Dependencies
### Internal
- `../../Enums/GotType`
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
