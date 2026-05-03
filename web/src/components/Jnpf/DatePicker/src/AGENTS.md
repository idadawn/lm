<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
日期/时间四组件的具体实现，复用 `ant-design-vue` 的 `DatePicker/RangePicker` 与 `dayjs`。统一通过 `getDateTimeUnit` 解析 `format` 来推断 `picker` 类型（year/month/date）和 `disabledDate` 上下界。

## Key Files
| File | Description |
|------|-------------|
| `DatePicker.vue` | 单值日期选择 + `disabledDate` 限制 |
| `DateRange.vue` | 日期区间，使用 `RangePicker` |
| `TimePicker.vue` | 单值时间选择 |
| `TimeRange.vue` | 时间区间 |
| `props.ts` | 4 个 props 集合：`datePickerProps`/`dateRangeProps`/`timePickerProps`/`timeRangeProps`，统一 `format`/`startTime`/`endTime`/`placeholder` |

## For AI Agents

### Working in this directory
- 上下界比较使用 `dayjs(x).startOf(unit) / endOf(unit)`，新增 format 时务必在 `getDateTimeUnit` 注册（`/@/utils/jnpf`）。
- `picker` 仅在 `YYYY` / `YYYY-MM` 时改变，其它默认 `date`。
- 区间组件 `value` 类型为 `string[] | number[]`，避免改成 tuple。

### Common patterns
- 全部使用 `useAttrs({ excludeDefaultKeys: false })` 透传，`format`、`valueFormat` 同步。
- `showToday/showNow` 仅在未传 `startTime/endTime` 时启用。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`、`/@/utils/jnpf`
### External
- `ant-design-vue`、`dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
