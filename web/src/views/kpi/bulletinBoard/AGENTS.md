<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# bulletinBoard

## Purpose
KPI/价值链「公告看板」页：顶部一组标签切换不同价值链分组（`getTagSelectorList`），下方 Tabs 展示该分组下多张思维导图（`MindMap`）。每个节点按需轮询/批量查询度量数据（`getMetricData`/`postMetricData`）。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 标签 + Tabs + MindMap 组合；`getTagInfo` 切换标签触发数据加载，`loopMetric` 遍历节点拉取度量值。 |

## For AI Agents

### Working in this directory
- 命名 `kpi-bulletinBoard`。
- MindMap 节点数据结构来自 `/@/components/MindMap` 的 `SourceInterface`；后端返回字段需对齐节点 `nodes/edges` 或 children 树。
- 页面尚有未完成的 `init` 占位逻辑；新增初始化逻辑时统一进入 `init`，避免散落到 `getTagSelectorList().then` 链中。

### Common patterns
- `Spin` 包裹异步图渲染
- `lodash-es.isEmpty` 防御空数据

## Dependencies
### Internal
- `/@/api/createModel/model`, `/@/components/MindMap`, `/@/hooks/web/useMessage`, `/@/enums/publicEnum`, `/@/enums/httpEnum`
### External
- `ant-design-vue`, `lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
