<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HSchedule

## Purpose
Calendar / schedule widget (日程). Wraps `@fullcalendar/vue3` and provides edit/view modal flows for creating, editing, and deleting schedule events including recurring-event "apply to" dialogs.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Mounts `<FullCalendar>` with `dayGridPlugin`/`interactionPlugin`/`timeGridPlugin`. Watches `activeData.option`, fetches events via `getScheduleList`, registers `Form` and `Detail` modals via `useModal`. |
| `Form.vue` | `BasicModal` + `BasicForm` for create/edit, supports send-recipient `msg-modal` slot, separate confirmation `Modal` for recurring-edit / recurring-delete decisions. |
| `Detail.vue` | Read-only `BasicModal` showing schedule fields (type, urgency, title, content, start/end via dayjs, creator, participants). |

## For AI Agents

### Working in this directory
- `calendar` import is the lunar-calendar helper from `../../Design/helper/calendar` — used for displaying lunar dates next to solar.
- Fullcalendar plugin set is fixed; if you need list view, also add `listPlugin` and update `headerToolbar` config.
- Recurring schedules: editing offers `tableList`, deleting offers `deleteList` — keep both branches (`isEdit` flag) when refactoring `Form.vue`.

### Common patterns
- `useModal` register pattern for both `Form` and `Detail`.
- Date formatting via `dayjs`.

## Dependencies
### Internal
- `/@/components/Modal`, `/@/components/Form`, `/@/store/modules/user`
- `/@/api/onlineDev/portal` (`getScheduleList`), `../../Design/helper/calendar`, `../CardHeader`

### External
- `@fullcalendar/vue3`, `@fullcalendar/{core,daygrid,interaction,timegrid}`, `dayjs`, `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
