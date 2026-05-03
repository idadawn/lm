<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# helper

## Purpose
Static lookup tables and the lunar-calendar helper used by the designer's toolbox, default-data demo content, and the schedule widget.

## Key Files
| File | Description |
|------|-------------|
| `componentMap.ts` | Toolbox catalog: `layoutComponents`, `systemComponents`, `basicComponents`, `chartComponents` arrays. Each entry carries `label`, `icon`, `jnpfKey`, default `option`/`card` config used when dropping a new widget. |
| `dataMap.ts` | Hard-coded demo datasets (`baseBarData`, `mulBarData`, `dataBoardDefault`, etc.) used as `chartData`/`list` placeholders when no real data interface is bound. |
| `calendar.ts` | 1900–2100 公历 ⇄ 农历 conversion library (`calendar.solar2lunar` / `calendar.lunar2solar`) — third-party MIT helper consumed by `HSchedule`. |

## For AI Agents

### Working in this directory
- When adding a new widget, register an entry in the appropriate group in `componentMap.ts` AND add demo data in `dataMap.ts` if charts/tables need a non-empty preview.
- Do not modify `calendar.ts` internals; it's a vendored library — only add wrapper functions if needed.
- Demo data field names (`fullName`, `iconColor`, `linkType`, `urlAddress`, `linkTarget`) match the runtime widget expectations — keep them consistent across files.

### Common patterns
- Plain `export const` arrays; no Vue specifics here.

## Dependencies
### Internal
- (none — pure data)

### External
- (none)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
