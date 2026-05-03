<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Macros

## Purpose
Argument-less cron macro attributes — convenience aliases for the most common cron expressions used throughout LIMS background jobs. Each is a `sealed` derivative of `CronAttribute` that simply passes the matching `@<macro>` string up.

## Key Files
| File | Description |
|------|-------------|
| `SecondlyAttribute.cs` | `[Secondly]` → `@secondly` (every second). |
| `MinutelyAttribute.cs` | `[Minutely]` → `@minutely` (top of each minute). |
| `HourlyAttribute.cs` | `[Hourly]` → `@hourly` (top of each hour). |
| `DailyAttribute.cs` | `[Daily]` → `@daily` (midnight every day). |
| `WeeklyAttribute.cs` | `[Weekly]` → `@weekly` (start of each week). |
| `MonthlyAttribute.cs` | `[Monthly]` → `@monthly` (1st of each month). |
| `YearlyAttribute.cs` | `[Yearly]` → `@yearly` (Jan 1). |
| `WorkdayAttribute.cs` | `[Workday]` → `@workday` (midnight Mon–Fri). |

## For AI Agents

### Working in this directory
- These are zero-argument; combine with the base `[Cron]`/`[Trigger]` properties for offset / retry / max-runs behaviour: e.g. `[Daily(StartTime = "2026-01-01", MaxNumberOfRuns = 10)]`.
- `[Workday]` is a Poxiao extension (not a standard cron macro) — it must remain in lockstep with the matching macro implementation in `Poxiao.TimeCrontab`.

### Common patterns
- Identical structure: `sealed class XxxAttribute : CronAttribute { public XxxAttribute() : base("@xxx") {} }`. New macros should follow the template exactly.

## Dependencies
### Internal
- Macro semantics resolved by `Poxiao.TimeCrontab`.
- Inherits `Schedule/Attributes/CronAttribute.cs`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
