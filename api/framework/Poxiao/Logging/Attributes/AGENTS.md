<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
日志声明式标记目录。`[OperateLog]` 用于业务操作审计（写入"模块/动作"），`[IgnoreLog]` 用于在受控控制器内部跳过日志（通常配合 `[AllowAnonymous]`）。这两个特性是检测室系统操作日志面板的数据来源。

## Key Files
| File | Description |
|------|-------------|
| `OperateLogAttribute.cs` | `AttributeUsage(Method)`，构造 `(moduleName, action)`，可选 `IsVisualDev` 标识在线开发；通常配合 MVC Filter 抓取并落库。 |
| `IgnoreLogAttribute.cs` | `AttributeUsage(Class \| Method, AllowMultiple=true)`，无字段；用于显式排除某些动作不写日志。 |

## For AI Agents

### Working in this directory
- 切忌把 `[OperateLog]` 加到非 Action 方法；切面只在 MVC 管道反射读取。
- `IsVisualDev` 是后台代码生成器/可视化开发用途，不要在普通业务接口随便置 `true`。
- 两个特性都需要 `[SuppressSniffer]` 防止被框架类型扫描重复注册。

### Common patterns
- 单一职责声明式标记——所有解析/落库逻辑在外部 Filter（业务模块 `system/Service` 或 `LoggingMonitor` 切面）里。

## Dependencies
### Internal
- 由系统模块的操作日志 Filter / `LoggingMonitor` 反射读取。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
