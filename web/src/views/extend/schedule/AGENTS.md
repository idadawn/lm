<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# schedule

## Purpose
日程安排（Schedule）演示页面，使用 `@fullcalendar/vue3` 渲染月 / 周 / 日视图，是 `extend` 模块下日历组件的参考实现。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页面：`FullCalendar` 注册 dayGrid / interaction / timeGrid 三个插件；本地化为中文（today / month / week / day / 全天）；`datesSet` 回调 `datesRender` 拉数据，`eventClick`/`dateClick` 调起表单。 |
| `Form.vue` | 日程新增 / 编辑 Modal 表单，提交后 emit `refresh` 重新拉取。 |

## For AI Agents

### Working in this directory
- API 走 `getSchedule`（`/@/api/extend/schedule`），按 `state.startTime` / `state.endTime` 区间请求；视图切换时由 `datesSet` 自动触发刷新。
- 默认禁用拖拽 / 缩放（`editable/eventStartEditable/eventDurationEditable: false`），如需启用请同步处理 `eventDrop`/`eventResize` 调用后端。
- `eventColor: '#378006'` 是项目约定的默认色；多类别请通过 `event.backgroundColor` 单独设置。

### Common patterns
- `reactive<CalendarOptions>` 配置；`fullCalendarRef.value.getApi()` 取实例做命令式控制。
- 与表单组件通过 `useModal` 解耦。

## Dependencies
### Internal
- `/@/api/extend/schedule`、`/@/components/Modal`
### External
- `@fullcalendar/vue3`、`@fullcalendar/core`、`@fullcalendar/daygrid`、`@fullcalendar/interaction`、`@fullcalendar/timegrid`、`dayjs`
