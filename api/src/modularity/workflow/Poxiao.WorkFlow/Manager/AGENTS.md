<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Manager

## Purpose
工作流业务编排核心。`FlowTaskManager` 实现 `IFlowTaskManager`，负责发起、审批（同意/拒绝/撤回/撤销/终止）、转办、加签、催办、变更复活、挂起恢复、超时通知等核心流程动作。其余 `*Util` 类按职责拆分（节点定位、用户解析、消息推送、模板解析、其它工具）。

## Key Files
| File | Description |
|------|-------------|
| `FlowTaskManager.cs` | 主编排器，含 Save/Submit/Audit/Reject/Recall/Revoke/Cancel/Transfer/Change/Suspend 等 |
| `FlowTaskManagerOld.cs` | 旧版编排器（保留供历史流程兼容/参考） |
| `FlowTemplateUtil.cs` | 流程模板（FLOW_TEMPLATE）解析、表单数据装配 |
| `FlowTaskNodeUtil.cs` | 节点定位、上一/下一节点、条件分支判断 |
| `FlowTaskUserUtil.cs` | 候选人/审批人解析（角色、岗位、部门、分组、变量、服务接口） |
| `FlowTaskMsgUtil.cs` | 消息推送（待办、超时、抄送、催办）和事件回调 |
| `FlowTaskOtherUtil.cs` | 表单写回、可视化开发联动、其它通用工具 |

## For AI Agents

### Working in this directory
- 对外仅暴露 `FlowTaskManager`；其余 `*Util` 由 `FlowTaskManager` 在构造时手动 `new`，不要单独注入。
- 修改审批流转顺序前，先理解 `FlowTaskParamter` 容器（任务+节点+经办+下一节点经办列表）。
- 子流程触发使用 `IServiceScopeFactory` 创建独立 scope，避免 SqlSugar `ITenant` 状态污染。

### Common patterns
- 节点属性反序列化为 `ApproversProperties` / `StartProperties` / `ChildTaskProperties` / `ConditionProperties` / `TimerProperties`。
- 通过 `Mapster Adapt` 把 `StartProperties`/`ChildTaskProperties` 投影为 `ApproversProperties` 复用审批人解析。
- 异步审批用 `_runService` / `TaskScheduler` 调度。

## Dependencies
### Internal
- `Poxiao.WorkFlow.Interfaces`、`Poxiao.WorkFlow.Entitys`、`Poxiao.Message.Interfaces`、`Poxiao.VisualDev.Interfaces`、`Poxiao.Systems.Interfaces`、`Poxiao.TaskScheduler`、`Poxiao.RemoteRequest`
### External
- SqlSugar、Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
