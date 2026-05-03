<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enum

## Purpose
任务调度专用枚举：作业创建类型、运行时调整动作。

## Key Files
| File | Description |
|------|-------------|
| `RequestTypeEnum.cs` | 作业类型：BuiltIn=0(内置)、Script=1(脚本)、Http=2(HTTP)，写入 `JobDetails.CreateType` |
| `TaskAdjustmentEnum.cs` | 任务变更动作：Update/Suspend/ChangeBuiltInAndPause/ChangeHttpAndPause/Open/OpenAndAdd/ChangeBuiltInAndOpen/ChangeHttpAndOpen/ChangeBuiltIn/ChangeHttp（共 10 种） |

## For AI Agents

### Working in this directory
- `TaskAdjustmentEnum` 决定 `TimeTaskService.PerformJob` 的状态机分支；新增类型务必同时扩展服务侧 switch。
- 枚举均带 `[Description]`，前端可借助 `EnumExtension` 统一渲染中文。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
