<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enum

## Purpose
系统模块共享枚举定义。涵盖菜单类型/类别、系统配置 Key、错误处理策略等业务常量。所有枚举类标注 `[SuppressSniffer]` 防止动态 API 误暴露，并通过 `[Description]` 提供中文友好显示。

## Key Files
| File | Description |
|------|-------------|
| `MenuType.cs` | 菜单/功能类别：Directory(0)、View(1)、Function(2)、Dictionary(3)、Report(4)、Screen(5)、Portal(6)、Link(7) |
| `MenuCategory.cs` | 菜单大类划分（与 `BASE_MODULE.F_CATEGORY` 对应） |
| `SysConfig.cs` | 系统配置 Key 枚举：TokenTimeout、verificationCodeNumber 等 |
| `ErrorStrategy.cs` | 数据接口/任务执行错误处理策略 |

## For AI Agents

### Working in this directory
- 新增菜单类型必须同时更新 `Entity/System/ModuleEntity.cs` 注释、前端菜单类型筛选与 `Mapper/SystemMapper.cs`。
- 添加系统配置 Key 时同步在 `SysConfigService` 中提供取值/默认值；`Description` 文案直接显示给运维。
- 枚举值不允许重排或重用旧编号，会破坏历史数据。

### Common patterns
- `[SuppressSniffer]` + 中文 `[Description]` + XML doc 三元组。

## Dependencies
### Internal
- `Poxiao.DependencyInjection`（SuppressSniffer）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
