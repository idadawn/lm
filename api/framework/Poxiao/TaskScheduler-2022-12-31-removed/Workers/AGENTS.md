<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Workers

> **DEPRECATED / LEGACY**：旧版 TaskScheduler Worker 标记接口。新代码请实现 `Poxiao.Schedule.IJob`。

## Purpose
`ISpareTimeWorker` 是一个**空标记接口**，用于让 `AddTaskScheduler` 在反射扫描时识别"哪些类承载定时任务方法"。

## Key Files
| File | Description |
|------|-------------|
| `ISpareTimeWorker.cs` | 空 `public interface`（带 `[Obsolete]`），无成员；约定的扫描锚点。 |

## For AI Agents

### Working in this directory
- 这是标记接口，不要往里加方法——任何成员都会破坏现有 Worker 类的兼容性。
- 实现类必须有无参构造函数（被 `Activator.CreateInstance` 直接实例化），不能依赖 DI。

### Common patterns
- 标记接口模式，配合反射扫描注册。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
