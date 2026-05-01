<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricNotice

## Purpose
指标通知实体。把价值链节点或规则触发的通知，关联到消息中心 `MessageTemplateEntity` 与可选的调度任务。

## Key Files
| File | Description |
|------|-------------|
| `MetricNoticeEntity.cs` | 表 `metric_notice` (`CUEntityBase`)：`type`(`MetricNoticeType`)/`node_id`/`rule_id?`/`template_id`/`schedule_id?` |

## For AI Agents

### Working in this directory
- `type` 取 `Node`/`Rule`，决定语义上引用 `node_id` 还是 `rule_id`。
- `template_id` 必填，对应 `Poxiao.Message.Entitys.Entity.MessageTemplateEntity.Id`。

### Common patterns
- `schedule_id` 可空，留待对接调度系统时填充。

## Dependencies
### Internal
- `../../Enums/MetricNoticeType`
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
