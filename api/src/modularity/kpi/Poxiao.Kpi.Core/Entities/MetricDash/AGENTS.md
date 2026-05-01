<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricDash

## Purpose
指标仪表板实体。每个 `got_id` 对应一份 `form_json` 表单数据，承载整张仪表板的拖拽配置。

## Key Files
| File | Description |
|------|-------------|
| `MetricDashEntity.cs` | 表 `metric_dash` (`CUEntityBase`)：`got_id`/`got_type`(`GotType`)/`form_json?` |

## For AI Agents

### Working in this directory
- `form_json` 是整张仪表板 JSON，字段语义由前端决定，后端不做 schema 解析。
- `got_type` 一般是 `Dash`，与 `MetricGotEntity` 对应。

### Common patterns
- 服务 `MetricDashService.CreateAsync` 在 `got_id` 已存在时改为 Update。

## Dependencies
### Internal
- `../../Enums/GotType`
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
