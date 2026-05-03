<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MacroAts

## Purpose
"At" cron macros — variants of the `@daily`/`@hourly`/etc. macros that accept positional `params object[] fields` to pin the firing moment (e.g. `[DailyAt(2, 30)]` ≈ "every day at 02:30"). Each derives from `CronAttribute` and forwards the macro string + arg array to `Poxiao.TimeCrontab` for parsing.

## Key Files
| File | Description |
|------|-------------|
| `SecondlyAtAttribute.cs` | `[SecondlyAt(...)]` — pin minute/hour/day fields under `@secondly`. |
| `MinutelyAtAttribute.cs` | `[MinutelyAt(...)]` — pin sub-fields under `@minutely`. |
| `HourlyAtAttribute.cs` | `[HourlyAt(minute)]` — fire at a given minute every hour. |
| `DailyAtAttribute.cs` | `[DailyAt(hour, minute)]` — fire daily at a given time. |
| `WeeklyAtAttribute.cs` | `[WeeklyAt(...)]` — pin time-of-week. |
| `MonthlyAtAttribute.cs` | `[MonthlyAt(...)]` — pin day/time of month. |
| `YearlyAtAttribute.cs` | `[YearlyAt(...)]` — pin month/day/time of year. |

## For AI Agents

### Working in this directory
- All ctors are uniform: `params object[] fields` forwarded to `base("@<macro>", fields)`. The actual semantics of `fields` (which positional slot is hour vs minute, etc.) is decided by `Poxiao.TimeCrontab`'s macro implementation — consult that library when adding a new variant.
- `AllowMultiple = true` and `sealed` on every type — a job can carry several `[DailyAt]` to fire multiple times per day.

### Common patterns
- One-line attribute classes: just call `base("@xxx", fields)`. No additional state.

## Dependencies
### Internal
- Inherits `Schedule/Attributes/CronAttribute.cs` and the runtime `CronTrigger`.
- Macro vocabulary defined in `Poxiao.TimeCrontab`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
