<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Model

## Purpose
工作流引擎使用的「中间模型」目录：包括流程节点 JSON 反序列化模型、审批参数容器、批量审批模型、候选人模型，以及节点属性配置（消息 / 超时 / 函数）和子项（条件 / 指派 / 候选）。这些模型不直接持久化，是 Manager / Repository 之间传递的领域对象。

## Key Files
| File | Description |
|------|-------------|
| `FlowTaskParamter.cs` | 审批参数容器：当前任务 + 当前节点 + 当前节点经办 + 下一节点经办列表 + 抄送 + 异常节点 + 起始节点属性 |
| `FlowHandleModel.cs` | 审批入参（继承 `PageInputBase`）：意见、加签、抄送、签名、候选人、批量 ID、驳回类型等 |
| `FlowTemplateJsonModel.cs` | 流程模板 JSON 顶层模型（节点列表 + 连线） |
| `FlowTaskModel.cs` | 任务摘要模型（精简字段） |
| `FlowTaskNodeModel.cs` | 节点模型（带 prevId/upNodeId 解析） |
| `FlowTaskOperatorModel.cs` / `FlowTaskOperatorRecordModel.cs` | 经办与经办记录视图模型 |
| `TaskNodeModel.cs` | 节点定位模型（含 upNodeId） |
| `FlowTaskCandidateModel.cs` | 候选人解析结果模型（异常节点回填使用） |
| `FlowBeforeRecordListModel.cs` | 审批记录展示模型（按分类） |
| `PortalWaitListModel.cs` | 门户首页待审批快照 |
| `FormOperatesModel.cs` | 表单字段权限模型（按节点设定可见 / 可编辑） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Conifg/` | 节点上的功能/消息/超时配置块（注意目录拼写为 Conifg） (see `Conifg/AGENTS.md`) |
| `Item/` | 节点 JSON 内的列表项模型（条件、候选、指派、明细行） (see `Item/AGENTS.md`) |
| `Properties/` | 节点 JSON 解析模型（开始/审批/条件/定时器/子流程） (see `Properties/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `FlowTaskParamter` 是审批的"上下文包"，新增审批分支时优先把所需上下文塞到这里，避免参数膨胀。
- JSON 反序列化模型属性命名 camelCase（与前端流程设计器一致），不要用 PascalCase。

### Common patterns
- 模型类全部 `[SuppressSniffer]`，并使用可空类型表达可选字段。
- 顶层模型 + 子项分层：Properties 描述节点行为、Item 描述列表项、Conifg 描述附加配置。

## Dependencies
### Internal
- `Poxiao.WorkFlow.Entitys/Entity`（容器引用 Entity）、`framework/Poxiao/Infrastructure.Filter`（`PageInputBase`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
