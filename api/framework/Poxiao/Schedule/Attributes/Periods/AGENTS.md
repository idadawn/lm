<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Periods

## Purpose
Unit-converting wrappers over `[Period]`. `PeriodAttribute` takes raw milliseconds; these subclasses accept seconds / minutes / hours and multiply into milliseconds inside the constructor. They make calling-site intent obvious — `[PeriodSeconds(30)]` is clearer than `[Period(30000)]`.

## Key Files
| File | Description |
|------|-------------|
| `PeriodSecondsAttribute.cs` | `[PeriodSeconds(n)]` — multiplies by 1 000. |
| `PeriodMinutesAttribute.cs` | `[PeriodMinutes(n)]` — multiplies by 60 000. |
| `PeriodHoursAttribute.cs` | `[PeriodHours(n)]` — multiplies by 3 600 000. |

## For AI Agents

### Working in this directory
- Don't add `[PeriodDays]` or larger units here — for anything ≥ a day prefer `[Daily]`/`[Workday]`/`[Cron]` so the trigger respects DST and clock skew (a fixed-millisecond interval drifts).
- All three are `sealed`; preserve that to keep the inheritance chain shallow (`PeriodAttribute → TriggerAttribute → Attribute`).

### Common patterns
- Constant multiplier in the constructor; no other state.

## Dependencies
### Internal
- Inherits `Schedule/Attributes/PeriodAttribute.cs` and the runtime `PeriodTrigger`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
