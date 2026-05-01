<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Schedule

## Purpose
DTOs for the "日程" (personal/team calendar event) feature — covers calendar listing, event detail viewing, and event creation/update with reminders, repetition, and participant assignment. Used by both the web admin page and the mobile (uni-app) "calendar" view.

## Key Files
| File | Description |
|------|-------------|
| `ScheduleListInput.cs` | Calendar-range fetch — `startTime` / `endTime` (`DateTime`). |
| `ScheduleAppListInput.cs` | Mobile-app variant of the list query (uni-app calendar widget). |
| `ScheduleListOutput.cs` | Calendar grid event projection. |
| `ScheduleDetailInput.cs` / `ScheduleDetailOutput.cs` | Single-event detail fetch + projection. |
| `ScheduleInfoOutput.cs` | Edit-form prefill output. |
| `SysScheduleCrInput.cs` | Create — full event payload: `type`, `urgent`, `title`, `content`, `allDay`, `startDay` / `endDay` (DateTime), `startTime` / `endTime` (string), `duration`, `color`, `reminderTime` (-2/-1/5/10/15/30/60/120/1440 minutes), `reminderType`, `send` / `sendName`, `repetition`, `repeatTime`, `toUserIds` (List<string>), `creatorUserId`. |
| `SysScheduleUpInput.cs` | Update payload (adds `id`). |

## For AI Agents

### Working in this directory
- `reminderTime` is an integer with a documented sentinel set (`-2 = 不提醒`, `-1 = 开始时`, otherwise minutes-before). Validate against the documented values; do not silently accept arbitrary integers.
- `startDay` / `endDay` are `DateTime` (date portion) while `startTime` / `endTime` are strings ("HH:mm") — splitting is intentional to match frontend pickers; do not collapse into a single property.
- `creatorUserId` on the create input is filled server-side from `App.User`; if the frontend sends a value, treat it as advisory only.
- Namespace `Poxiao.Systems.Entitys.Dto.Schedule` (no `.System.` segment).
- `[SuppressSniffer]` applied; `using SqlSugar;` in `SysScheduleCrInput.cs` is unnecessary today but kept for consistency with the sibling entity file.
- Mobile vs web list inputs (`ScheduleAppListInput` vs `ScheduleListInput`) intentionally diverge — keep both rather than collapsing.

### Common patterns
- Cr/Up/Info/List/Detail suffix.
- `toUserIds` (`List<string>`) for participant lists; user IDs are framework-wide string GUIDs.

## Dependencies
### Internal
- Consumed by `modularity/system/Poxiao.Systems` schedule service / controllers; mobile app calls go through the same endpoints with `*App*` inputs where shapes differ.

### External
- `Poxiao.DependencyInjection` (`[SuppressSniffer]`), `SqlSugar` (referenced but not used in DTO body).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
