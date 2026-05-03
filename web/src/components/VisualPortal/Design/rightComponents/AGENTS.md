<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# rightComponents

## Purpose
Property-panel building blocks rendered inside `RightPanel.vue`'s collapse. Each file is a small `<a-collapse-panel>` form section that mutates `activeData.option` / `activeData.card` / etc. for one aspect of the active widget.

## Key Files
| File | Description |
|------|-------------|
| `RCommon.vue` | `通用属性` — style-type radio plus per-`jnpfKey` shared options (line area/width, pie nightingale, etc.). Drives many other panels via `styleType`. |
| `RCard.vue` | `卡片设置` — card title text/size/weight/color, icon, background. |
| `RBarStyle.vue` | `柱体设置` — bar width and corner radius for `barChart`. |
| `RXAxis.vue` / `RYAxis.vue` | Axis settings for `barChart` / `lineChart`. |
| `RLabel.vue`, `RLegend.vue`, `RTooltip.vue`, `RColor.vue`, `RMargin.vue` | ECharts-style sub-panels, gated on `chartList`. |
| `RMainTitle.vue` / `RSubTitle.vue` | Chart title and subtitle. |
| `RData.vue` | `数据源` — toggle static/dynamic, bind `propsApi` (`getDataInterfaceRes`). |
| `RLink.vue` | Outbound-link config used by data-board/banner/common-func items. |
| `RRefresh.vue` | Auto-refresh interval. |
| `RTextSet.vue` | Text widget settings. |
| `RTabSet.vue` | Tab container setup. |
| `RMapSet.vue` | Map-chart specifics (drill-down, area styling). |
| `RNoticeSet.vue` / `RNoticeColumnModal.vue` | Notice list config + column picker modal. |
| `RScheduleSet.vue` | Schedule widget config. |
| `RTableSet.vue` | Table-list column config. |
| `RCarouselModal.vue`, `RColumnModal.vue`, `RCommonFunModal.vue`, `RDataBoardModal.vue`, `REditorModal.vue`, `RJsonModal.vue`, `RTodoModal.vue` | Modal editors invoked from inline `+` buttons (carousel slides, common-func entries, data-board cards, todo presets, raw JSON, etc.). |

## For AI Agents

### Working in this directory
- All panels share the prop signature `({ activeData, showType, menuList?, appMenuList? })` — keep this shape; `RightPanel.vue` binds them this way.
- Wrap top-level form in `<a-collapse-panel>` with a Chinese `#header` title; keep wording consistent (`柱体设置`, `卡片设置`, ...).
- App-only / PC-only fields are gated with `showType == 'pc'` / `showType == 'app'`. Mirror this when adding fields.
- Use project shared inputs (`jnpf-color-picker`, `jnpf-radio`, `JnpfIconPicker`, `BasicHelp`) rather than raw Ant components when an option exists.

### Common patterns
- `defineProps(['activeData','showType', ...])` with no validation — these are tightly-coupled internals.
- Mutations write directly into `activeData.option.*` to keep two-way binding with `Parser.vue`.

## Dependencies
### Internal
- `/@/components/Modal`, `/@/api/systemData/dataInterface`
- Project `Jnpf*` global components

### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
