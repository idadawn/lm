<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
ZChartEditor 编辑态业务逻辑组合式封装。集中节点新增/删除、指标维度查询、默认 setting 构造等,供 `editorDashboard` 与外层 `src/index.vue` 复用。

## Key Files
| File | Description |
|------|-------------|
| `useEditor.ts` | 暴露 `addNode/delNode`;按拖入坐标 `e.offsetX/Y` 计算位置,调用 `getMetricsDimensions` 取维度并写入 `chartStore.setDimensionMap` |

## For AI Agents

### Working in this directory
- `addNode` 兼容三种入参形态:从 tree 拖入 (`item.class=='chart'`)、从 filter 列表拖入、对已有节点的覆盖更新(`currentKey`)。
- 默认 `style.width = chart ? 400 : 200`、`height = 300`、`zIndex = 18`,如需修改默认尺寸请同步评估画布密度。
- 节点 `key` 由 `item.type + guid()` 拼接,务必保持唯一性。

### Common patterns
- 与 `chartStore` 紧耦合:状态变更走 store action,组件只调用本 hook。

## Dependencies
### Internal
- `/@/api/chart` (`getMetricsDimensions`)
- `/@/store/modules/chart`
- `/@/utils/helper/toolHelper` (`guid`)
### External
- Vue 3 Composition API

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
