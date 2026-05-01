<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Conifg

## Purpose
节点附加配置块。被 `Properties/*Properties.cs` 内嵌使用：节点上的消息 / 超时 / 事件回调（Func）规则。

> 注意：目录名拼写为 `Conifg`（缺 f）。这是历史遗留，保持现状。

## Key Files
| File | Description |
|------|-------------|
| `MsgConfig.cs` | 消息配置：on(0关闭/1自定义/2同步发起) + msgId + 模板 JSON（List<MessageSendModel>） |
| `FuncConfig.cs` | 事件触发配置：on + interfaceId（数据接口） + 模板 JSON（List<MessageSendParam>） |
| `TimeOutConfig.cs` | 限时/超时/提醒配置：起始时间类型、时长、首次触发、间隔、是否通知/事件/自动审批 + 触发次数 |

## For AI Agents

### Working in this directory
- 字段统一用 `int`/`bool` + 中文枚举注释（如 `0：关闭 1：自定义 2：同步发起配置`），保持与前端流程设计器约定一致。
- `MsgConfig` 引用的 `MessageSendModel` 来自 `Poxiao.Infrastructure.Dtos.Message`；`FuncConfig` 引用 `MessageSendParam`。

### Common patterns
- 这些配置在节点 JSON 中是嵌套对象，反序列化时不能为 null（默认 `new MsgConfig()` / `new TimeOutConfig()`）。

## Dependencies
### Internal
- `framework/Poxiao/Infrastructure.Dtos.Message`、`Poxiao.WorkFlow.Entitys/Model/Item`、`DependencyInjection`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
