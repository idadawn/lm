<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Constants

## Purpose
Schedule 模块的枚举/常量定义，描述作业调度运行时的状态机与底层 SQL 配置。被触发器、作业信息、持久化器与命名转换器共同引用，是 Schedule 子系统的"词汇表"。

## Key Files
| File | Description |
|------|-------------|
| `TriggerStatus.cs` | 作业触发器状态机（Backlog/Ready/Running/Pause/Blocked/ErrorToReady/Archived/Panic/Overrun/Unoccupied/NotStart/Unknown/Unhandled）。 |
| `ClusterStatus.cs` | 作业集群状态：Crashed / Working / Waiting，配合 `IJobClusterServer` 实现故障转移。 |
| `PersistenceBehavior.cs` | 持久化行为枚举：Appended / Updated / Removed，用于驱动 SQL 与 `IJobPersistence` 通知。 |
| `ScheduleResult.cs` | 调度操作结果（NotFound / NotIdentify / Exists / Succeed / Failed），是工厂/调度器 Try* 系列方法的统一返回值。 |
| `NamingConventions.cs` | 持久化字段命名约定：CamelCase / Pascal / UnderScoreCase，用于生成跨数据库 SQL。 |
| `SqlTypes.cs` | 目标 SQL 方言：Standard / SqlServer / Sqlite / MySql / PostgresSQL / Oracle / Firebird，影响布尔与字符串字面量生成。 |

## For AI Agents

### Working in this directory
- 每个枚举均贴有 `[SuppressSniffer]`；新增枚举请保持该属性以避免被 DI 扫描器误识别为业务类型。
- 不要重新编号已有成员（如 `TriggerStatus`）——持久化层使用整数值作为列值，改动会破坏旧记录。
- 新增 `ScheduleResult` 成员需同步更新所有 Try* 方法的注释与上层处理分支。

### Common patterns
- 所有枚举显式使用 `: uint` 或默认 `int`，配合 `[JsonInclude]` 序列化到看板 API。
- 中文 XML 注释（含 `<remarks>` 解释边界条件，例如 "起始时间大于当前时间" 表示 Backlog）。

## Dependencies
### Internal
- 被 `../Triggers/`、`../Persistences/`、`../Internal/Penetrates.cs` 直接引用。
### External
- 仅依赖 BCL；`SuppressSniffer` 由 Poxiao 框架根命名空间提供。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
