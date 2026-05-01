<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricNotice

## Purpose
指标消息通知服务。维护 `MetricNoticeEntity`，并联表 `MessageTemplateEntity`/`MessageDataTypeEntity` 输出可用模板供前端选择。

## Key Files
| File | Description |
|------|-------------|
| `IMetricNoticeService.cs` | CRUD + `GetTemplatesAsync` |
| `MetricNoticeService.cs` | 实现：模板查询过滤 `MessageSource = MessageConst.METRICNOTICETYPE && DeleteMark == null`，并子查询消息类型/来源名称 |

## For AI Agents

### Working in this directory
- 通知类型由 `MetricNoticeType.Node/Rule` 决定填 `nodeId` 还是 `ruleId`，DTO 不强制，由调用方保证。
- 模板查询排序：`SortCode` → `CreatorTime DESC` → `LastModifyTime DESC`。

### Common patterns
- 子查询使用 `SqlFunc.Subqueryable<MessageDataTypeEntity>().Where(...).Select(u => u.FullName)`。

## Dependencies
### Internal
- `Poxiao.Message.Entitys`（`MessageTemplateEntity`/`MessageDataTypeEntity`/`MessageConst`）
- `../../../Poxiao.Kpi.Core/Entities/MetricNotice`、`Enums/MetricNoticeType`
### External
- SqlSugar, Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
