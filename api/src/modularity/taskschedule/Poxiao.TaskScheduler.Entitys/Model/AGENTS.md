<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Model

## Purpose
任务调度内部数据模型：执行内容 JSON 结构与本地方法元信息。

## Key Files
| File | Description |
|------|-------------|
| `ContentModel.cs` | 反序列化 `F_EXECUTECONTENT` 的 JSON：cron / interfaceId / interfaceName / parameter[] / localHostTaskId / startTime / endTime（带 `NewtonsoftDateTimeJsonConverter`） |
| `TaskMethodInfo.cs` | 反射本地方法元信息：MethodName、DeclaringType、cron、TimerType (`SpareTimeTypes`)、ExecuteType (`SpareTimeExecuteTypes`)、RequestType (`RequestTypeEnum`) |

## For AI Agents

### Working in this directory
- `ContentModel.parameter` 是接口入参列表，注意区分 `defaultValue` 与 `value`（用户填值优先）。
- `TaskMethodInfo.DoOnce`、`StartNow` 标志影响调度行为；`Interval` 单位为秒。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
