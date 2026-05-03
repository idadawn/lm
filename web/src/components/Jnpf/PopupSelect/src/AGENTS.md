<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfPopupSelect` SFC 实现：折叠态 `a-select` + 弹出态 dialog/drawer/popover 三种容器；内置搜索表单 + `a-table`（行选择 radio/checkbox）+ 分页。选中确认后写回 `update:value` 并将该行通过 `emitter.setRelationData` 广播给 `JnpfPopupAttr`。

## Key Files
| File | Description |
|------|-------------|
| `PopupSelect.vue` | 主组件，~470 行；`getParamList` 处理子表（`relationField` 含 `-`）/主表参数模板；`updateSelectRow` 跨页累计多选 |
| `props.ts` | `popupSelectProps`：`interfaceId`/`templateJson`/`relationField`/`propsValue`/`columnOptions`/`popupType`/`popupTitle`/`popupWidth`/`hasPage`/`rowIndex`/`formData` |

## For AI Agents

### Working in this directory
- 三种 `popupType` 分支共享同一段搜索表单与表格 JSX——抽组件前先评估 popover 特殊样式 `popup-select-popover` 是否兼容。
- 子表场景下 `rowIndex` 必传，`getParamList` 从 `formData[tableVModel][rowIndex][childVModel]` 取行内字段值；新增子表参数解析路径要保持向后兼容。
- 表体高度通过 `getScrollY` 在 `openSelectModal` 内手动写到 `.ant-table-body` 上——勿删除该 nextTick，否则 popover/drawer 高度计算失效。
- 单选/多选回填规则不同：单选直接拿 `selectedRows[0]`，多选用 `updateSelectRow` 累加。

### Common patterns
- 列定义自动加序号列 `indexColumn`；`fieldNames` 用 `relationField` 作 label、`propsValue` 作 value。
- `formItemContext.onFieldChange()` 在每次值变化后调用以触发表单校验。

## Dependencies
### Internal
- `/@/api/systemData/dataInterface`、`/@/store/modules/generator`、`/@/components/Modal/src/components/ModalClose.vue`、`/@/hooks/web/useDesign`、`/@/hooks/web/useI18n`
### External
- `ant-design-vue`、`@ant-design/icons-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
