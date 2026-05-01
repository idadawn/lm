<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Entity

## Purpose
工作流引擎的 SqlSugar 实体集合。覆盖流程任务、节点、经办、记录、抄送、候选人、委托、评论、模板、引擎、表单、用户、驳回数据等核心表，全部继承 `CLDEntityBase`，使用 `[SugarColumn(ColumnName = "F_*")]` 显式声明列名（遗留字段命名）。

## Key Files
| File | Description |
|------|-------------|
| `FlowTaskEntity.cs` | `FLOW_TASK`：流程实例主表（流程 / 模板 / 当前节点 / 状态 / 紧急度 / 委托 / 挂起） |
| `FlowTaskNodeEntity.cs` | `FLOW_TASK_NODE`：当前任务展开的节点列表 |
| `FlowTaskOperatorEntity.cs` | `FLOW_TASK_OPERATOR`：当前节点经办（任务+节点+用户） |
| `FlowTaskOperatorUserEntity.cs` | 依次审批经办用户队列 |
| `FlowTaskOperatorRecordEntity.cs` | 经办历史记录（每次审批/驳回/转办都会落历史） |
| `FlowTaskCirculateEntity.cs` | 节点抄送记录 |
| `FlowCandidatesEntity.cs` | 候选人解析后落表（避免反复计算） |
| `FlowCommentEntity.cs` | `FLOW_COMMENT`：流程评论（文本 + 图片 + 附件） |
| `FlowDelegateEntity.cs` | `FLOW_DELEGATE`：委托规则 |
| `FlowEngineEntity.cs` | `FLOW_ENGINE`：流程设计草稿 |
| `FlowEngineVisibleEntity.cs` | 流程引擎可见范围（部门 / 角色 / 用户） |
| `FlowFormEntity.cs` / `FlowFormRelationEntity.cs` | 流程表单元数据与关联 |
| `FlowTemplateEntity.cs` / `FlowTemplateJsonEntity.cs` | `FLOW_TEMPLATE` 主表 + JSON 表（多租户：`[Tenant(ClaimConst.TENANTID)]`） |
| `FlowRejectDataEntity.cs` | 驳回上下文（被驳回的节点 + 数据，供恢复） |
| `FlowUserEntity.cs` | 任务发起人/委托人快照 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `WorkFlowForm/` | 内置业务表单实体（请假、销售订单） (see `WorkFlowForm/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 所有列名使用 `F_*` 全大写或混合大小写，**必须**显式 `[SugarColumn(ColumnName = "...")]`。
- 多租户表（如 `FlowTemplateEntity`）需打 `[Tenant(ClaimConst.TENANTID)]`。
- 状态字段（`F_STATUS`）使用 int + 注释枚举映射，不要在数据库存字符串。
- 不要为 `CLDEntityBase` 已有字段重复声明（`F_Id / F_TenantId / F_CREATORTIME / F_LastModifyTime / F_DeleteMark` 等已基类提供）。

### Common patterns
- 主键字段在 `CLDEntityBase` 提供，业务实体只声明业务列。
- JSON 列（流程模板、表单 JSON）类型为 `string`，存放序列化字符串。

## Dependencies
### Internal
- `framework/Poxiao/Infrastructure.Contracts`（`CLDEntityBase`、`OEntityBase`）、`Infrastructure.Const`（`ClaimConst`）
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
