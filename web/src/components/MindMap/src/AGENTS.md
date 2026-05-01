<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
MindMap 组件的入口视图与 props。提供单棵指标树（`index.vue`）和多视图变体（`indexMore.vue`）两种渲染模式，统一通过 `props.ts` 接收数据源与回调。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 默认入口；调用 `useMindMap`，watch `props.source` 深度比较节点字段（`metricId/currentValue/trendData/metricGrade/statusColor`）后增量 `updateItem`，缺省 `onDeleteItem` 时弹 Modal 确认调用 `deleteIndicatorValueChain`。 |
| `indexMore.vue` | 多视图扩展（备用），由 `indexMore` Hook 驱动。 |
| `props.ts` | 定义 `source` / `dragItem` / `authAdd` / `authDelete` 与 `onClick` `onAddItem` `onDeleteItem` `onNodeClick` 回调。 |

## For AI Agents

### Working in this directory
- 节点变更检测使用「字段级 isEqual」短路，新增节点字段需扩展 `isChanged` 与 `updateView`。
- `onAddItem` 必须返回包含 `{id, name, gotId, parentId}` 的对象，否则 `updateChild` 会失败。
- 默认删除流程会对 `is_root && parentId === '-1'` 的根节点重建初始节点（`name='初始节点'`），改造时保持该兜底。

### Common patterns
- 通过 `props.onAddItem` / `props.onDeleteItem` 提供注入点，外部无须 fork 组件。
- `cloneDeep` 应用于初始 `source` 防止 G6 内部修改污染外部 reactive 数据。

## Dependencies
### Internal
- `../hooks/useMindMap`、`/@/api/createModel/model`、`/@/hooks/web/useI18n`
### External
- `ant-design-vue`、`@ant-design/icons-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
