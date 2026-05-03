<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricDash

## Purpose
指标仪表板服务。一个思维图 (`gotId`) 对应一份 `formJson` 表单。`CreateAsync` 行为是 Upsert：若 `gotId` 已存在则转 Update。

## Key Files
| File | Description |
|------|-------------|
| `IMetricDashService.cs` | 仪表板查询/Upsert/Update 接口，`IsMetricCanDragToDashAsync` 校验拖拽可行性 |
| `MetricDashService.cs` | 实现：注入 `IMetricInfoService` 用于校验维度兼容；`GetAsync` 按 `gotId` 取一条记录 |

## For AI Agents

### Working in this directory
- `CreateAsync` 命中已有记录会调用 `UpdateAsync`，注意调用方需保证 `gotId` 唯一。
- `IsMetricCanDragToDashAsync` 当前实现仍在拼装维度，`isFlag` 默认 `false`，待补完业务校验。

### Common patterns
- 仪表板表单整体 JSON 字符串落库，无字段级表结构。

## Dependencies
### Internal
- `../MetricInfo`、`../../../Poxiao.Kpi.Core/Entities/MetricDash`
### External
- SqlSugar, Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
