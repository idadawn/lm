<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.TaskScheduler.Entitys

## Purpose
定时任务实体共享工程：业务任务表 + 调度框架持久化表 + DTO/Mapper/Model/Enum。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.TaskScheduler.Entitys.csproj` | 工程文件 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Dto/` | API 输入/输出 DTO (see `Dto/AGENTS.md`) |
| `Entity/` | 业务任务、日志、JobDetails、JobTriggers (see `Entity/AGENTS.md`) |
| `Enum/` | 请求类型与任务调整动作枚举 (see `Enum/AGENTS.md`) |
| `Mapper/` | Mapster 映射（解析 `ExecuteContent` JSON 取 startTime/endTime）(see `Mapper/AGENTS.md`) |
| `Model/` | 内部数据模型（ContentModel / TaskMethodInfo）(see `Model/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `JobDetails`/`JobTriggers` 是调度框架持久化表，路由到 `[Tenant("Poxiao-Job")]` 数据库；业务表（`BASE_TIMETASK*`）走主库。
- 实体字段沿用 `CLDEntityBase` 全大写约定（详见 `.cursorrules`）。

### Common patterns
- 任务执行参数序列化为 `ContentModel` JSON 存入 `F_EXECUTECONTENT`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
