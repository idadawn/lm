<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricNotice

## Purpose
指标消息通知 DTO。`MetricNoticeService` 将价值链节点/规则触发的告警通过消息模板下发，DTO 定义模板信息与查询入参。

## Key Files
| File | Description |
|------|-------------|
| `MetricNoticeCrInput.cs` | 新建通知（`type`/`nodeId`/`ruleId`/`templateId`/`scheduleId`） |
| `MetricNoticeQryInput.cs` | 列表查询（按节点或规则） |
| `MetricNoticeOutput.cs` | 通知详情 + 审计字段 |
| `MetricNoticeTemplateOutput.cs` | 消息模板：`enCode`、`templateType`、子查询出 `messageType`/`messageSource` 名称 |

## For AI Agents

### Working in this directory
- `Type` 取自 `MetricNoticeType` (`Node`/`Rule`)，决定使用 `nodeId` 还是 `ruleId`。
- 模板查询过滤 `MessageSource = MessageConst.METRICNOTICETYPE` 与 `DeleteMark == null`。

### Common patterns
- 模板的子查询字段 (`messageType`/`messageSource`) 通过 SqlSugar `SqlFunc.Subqueryable` 写入 SQL。
- DTO 在 `Poxiao.Kpi.Application` 命名空间下；外部依赖来自 `Poxiao.Message.Entitys`。

## Dependencies
### Internal
- `Poxiao.Message.Entitys`（`MessageTemplateEntity`、`MessageDataTypeEntity`、`MessageConst`）
- `Core/Enums/MetricNoticeType`
### External
- Newtonsoft.Json, SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
