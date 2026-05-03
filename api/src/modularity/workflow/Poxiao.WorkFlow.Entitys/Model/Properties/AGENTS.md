<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Properties

## Purpose
流程节点 JSON 的 C# 强类型映射模型。每种节点类型一个 `*Properties.cs`，描述节点行为：审批人、按钮、超时、消息、事件、抄送、子流程、条件等。

## Key Files
| File | Description |
|------|-------------|
| `StartProperties.cs` | 开始节点：发起人/角色/部门/岗位/分组、抄送、按钮（提交/暂存/撤回/打印/催办）、消息、事件、超时、异常处理规则 |
| `ApproversProperties.cs` | 审批节点：assigneeType（参考 FlowTaskOperatorEnum）、会签/或签、驳回策略、加签、签名、按钮、定时器列表、附加规则、自动同意规则 |
| `ConditionProperties.cs` | 条件分支：title + conditions(`List<ConditionsItem>`) + isDefault |
| `TimerProperties.cs` | 定时器：day/hour/minute/second + upNodeCode |
| `ChildTaskProperties.cs` | 子流程：发起人解析、flowId、字段继承、childTaskId、同步/异步、launchMsgConfig、错误规则 |

## For AI Agents

### Working in this directory
- 这些模型直接由前端流程设计器序列化为 JSON 落库到 `FLOW_TASK.F_FLOWTEMPLATEJSON`，**字段名必须为 camelCase**，新增字段保留默认值避免反序列化失败。
- `ApproversProperties` 是最复杂的节点，包含人员 / 按钮 / 消息 / 事件 / 超时五大块；新增配置项请按现有 region 分组添加。
- `ChildTaskProperties` / `StartProperties` 通过 Mapster 投影到 `ApproversProperties` 复用审批人解析逻辑（见 `Mapper/Mapper.cs`）。

### Common patterns
- 默认值使用 C# 属性初始化器（如 `extraRule = "1"`、`saveBtnText = "暂 存"`）。
- 列表字段全部初始化为 `new List<…>()`，避免 null 引用。

## Dependencies
### Internal
- `Poxiao.WorkFlow.Entitys/Model/Conifg`（MsgConfig / FuncConfig / TimeOutConfig）、`Model/Item`（AssignItem / ConditionsItem）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
