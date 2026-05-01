<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# createModel

## Purpose
KPI「价值链/指标模型」管理：列表（`getIndicatorTreeList`）+ 多个新建/编辑表单 + 价值链图谱编辑器。提供基础新建（Form）、扩展新建（FormMore）、部门关联（DepForm）、成员选择（Member）以及基于 `ZEditor` 的可视化思维导图编辑（chartsTree、editorDemo）。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 价值链列表 + 新建/编辑入口 + 表格行点击进入编辑。 |
| `Form.vue` | 基础信息表单（名称/描述/标签）。 |
| `FormMore.vue` | 扩展表单：更多字段及关联配置。 |
| `DepForm.vue` | 价值链节点编辑：BasicPopup + ZEditor 渲染图谱节点。 |
| `Member.vue` | 成员选择 transfer 模态框。 |
| `chartsTree.vue` | BasicPopup + ZEditor，编辑节点 nodes/edges。 |
| `editorDemo.vue` | ZEditor 内置示例数据演示。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 图表/归因分析展示子组件 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `defineOptions({ name: 'permission-organize' })` 是从权限模块复制后未改名的历史问题；新增页面请使用 `kpi-createModel-xxx`。
- 节点数据格式：`SourceInterface { nodes: Node[]; edges: Edge[]; gotId }`，来自 `/@/components/MindMap/hooks/MindMapSourceType`。保持与 `dimension/` 子树一致。
- `editorDemo.vue` 含硬编码示例 trendData/pieData，仅用于 UI 演示，**不要**在生产分支替换为接口数据。

### Common patterns
- `BasicPopup + usePopupInner(init)` 处理「先拉详情、再渲染编辑器」
- `ZEditor` 接收 `:source` 与 `:status-options`

## Dependencies
### Internal
- `/@/api/createModel/model`, `/@/components/Modal`, `/@/components/Popup`, `/@/components/Table`, `/@/components/ZEditor`, `/@/components/MindMap`, `/@/store/modules/organize`, `/@/api/permission/user`
### External
- `lodash-es`, `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
