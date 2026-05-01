<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dependencies

## Purpose
TimeCrontab 解析器依赖契约。定义所有 Cron 字段解析器须实现的两个内部接口：判断当前时间是否命中规则，以及在简单字段（秒/分/时/年）上推算下一个发生值与起始值。

## Key Files
| File | Description |
|------|-------------|
| `ICronParser.cs` | 暴露 `Kind` (CrontabFieldKind) 与 `IsMatch(DateTime)`；所有 Parser 必须实现。 |
| `ITimeParser.cs` | `int? Next(int currentValue)` 与 `int First()`，仅 DateTime 主组件（秒/分/时/年）需实现。 |

## For AI Agents

### Working in this directory
- 接口为 `internal`，仅供 `../` 下解析器实现，外部代码请通过 `Crontab` 入口使用。
- Day / Month / DayOfWeek 三种字段不应单独走 ITimeParser 推算（计算耦合复杂），实现里通常抛 `TimeCrontabException`，组合推算在更高层完成。

### Common patterns
- `IsMatch` 内部 `switch (Kind)` 取出 `DateTime` 对应分量并比较。
- `Next/First` 使用 `Constants.Maximum/MinimumDateTimeValues[Kind]` 做边界。

## Dependencies
### Internal
- `CrontabFieldKind` 枚举、`Constants` 字典。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
