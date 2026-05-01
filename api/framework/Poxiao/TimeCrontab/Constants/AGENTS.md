<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Constants

## Purpose
TimeCrontab 模块的常量、枚举与字段元数据。集中维护字段最值范围、不同 `CronStringFormat` 的字段数量预期、英文缩写映射等，避免硬编码散落到解析器各处。

## Key Files
| File | Description |
|------|-------------|
| `Constants.cs` | 内部静态字典 `MaximumDateTimeValues` / `MinimumDateTimeValues`（按 `CrontabFieldKind` 给出 0-59、1-31 等边界）、`ExpectedFieldCounts`（Default=5/WithYears=6/WithSeconds=6/WithSecondsAndYears=7）、`CronDays`（C# `DayOfWeek` → 0..6）、`Days`（"SUN"..→0..6）、`Months`（"JAN"..→1..12）。 |
| `CronFieldKind.cs` | 内部枚举：`Second=0` / `Minute=1` / `Hour=2` / `Day=3` / `Month=4` / `DayOfWeek=5` / `Year=6`。 |
| `CronStringFormat.cs` | 公共枚举：`Default` / `WithYears` / `WithSeconds` / `WithSecondsAndYears`，对应表达式书写顺序见 XML 注释。 |

## For AI Agents

### Working in this directory
- 修改字段最值会直接影响所有 `Parsers/*Parser.cs` 的越界判断；改动需评估全模块。
- 新增 Cron 格式必须同时更新 `ExpectedFieldCounts` 与 `Crontab.Internal.cs` 的字段拼装逻辑。
- `Days` 起点为 `SUN=0` 与 Quartz/Cronos 兼容，与 .NET `DayOfWeek` 起点不一致——通过 `CronDays` 字典做映射。

### Common patterns
- 全部 `internal static readonly Dictionary<,>`；`internal enum` 仅在模块内可见，公共部分通过 `CronStringFormat` 暴露。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
