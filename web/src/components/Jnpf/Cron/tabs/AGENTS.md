<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# tabs

## Purpose
`JnpfCron` 各时间维度 Tab 的 UI 组件与公用 `useTabMixin`。每个 Tab 通过 `radio-group` 选择类型（不设置/每…/区间/循环/工作日/最后/指定），并把局部值合并为 cron 段。

## Key Files
| File | Description |
|------|-------------|
| `useTabMixin.ts` | 公用 `TypeEnum`、`useTabProps`/`useTabEmits`/`useTabSetup`：处理类型切换、`computeValue` 回写、跨字段联动 |
| `SecondUI.vue` | 秒（默认 `*`） |
| `MinuteUI.vue` | 分 |
| `HourUI.vue` | 时 |
| `DayUI.vue` | 日（与 周 互斥，多一个 `last`/`work` 选项） |
| `MonthUI.vue` | 月 |
| `WeekUI.vue` | 周（与 日 互斥，下拉选择 1-7） |
| `YearUI.vue` | 年（可隐藏） |

## For AI Agents

### Working in this directory
- 日/周存在互斥（"日和周只能设置其中之一"），改动 `DayUI` 或 `WeekUI` 时同步更新 `useTabMixin.ts` 中跨字段逻辑。
- 新增类型必须扩展 `TypeEnum` 并补齐 `valueRange/valueLoop/valueWeek/valueWork`、`computeValue` 分支。
- `prefixCls` 通过 `inject('prefixCls')` 取得，对应父级 `EasyCronInner`，不要在 Tab 内自行 `useDesign`。

### Common patterns
- 使用 `propTypes`（`/@/utils/propTypes`）声明 `value`/`disabled`，emits 固定为 `['change', 'update:value']`。

## Dependencies
### Internal
- `/@/utils/propTypes`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
