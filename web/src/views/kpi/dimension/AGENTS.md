<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dimension

## Purpose
KPI「公共维度」管理：树表展示维度（`getDimensionList`）、新建/编辑维度弹窗（基于左侧数据源树 + 维度字段下拉）、字段元数据从 `/@/api/targetDefinition` 的 `getMetricLinkIdSchemaSchemaName` 拉取。是为指标模型提供分析维度的元数据中心。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 维度列表（公共维度名/数据类型/最后更新时间）+ 新建按钮 + 操作列，支持树形（`isTreeTable: true`）。 |
| `DepForm.vue` | BasicModal：左侧 `BasicLeftTree` 数据源树 + 右侧维度字段下拉表单；调 `addDimension/updateDimension`。 |
| `chartsTree.vue` | 与 `createModel/chartsTree.vue` 同源，BasicPopup + ZEditor 编辑维度节点图谱。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 维度图表展示组件（与 createModel/components 镜像） (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `DepForm` 引用了 `views/kpi/indicatorDefine/OrgTree.vue`，跨目录依赖；调整 indicatorDefine 时需保证 OrgTree 接口稳定。
- `defineOptions({ name: 'permission-organize' })` 是历史复制残留；新页面请避免同名。
- 字段下拉的 `fieldNames: { label: 'fieldName', value: 'field' }` 是与后端 `metricLinkId` schema 的契约，改字段名要同步。

### Common patterns
- `useTable({ isTreeTable, useSearchForm, afterFetch })`
- `BasicLeftTree` `:fieldNames="{ key: 'id', title: 'name' }"`

## Dependencies
### Internal
- `/@/api/dimension/model`, `/@/api/targetDefinition`, `/@/components/Modal`, `/@/components/Tree`, `/@/components/Table`, `/@/views/kpi/indicatorDefine/OrgTree.vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
