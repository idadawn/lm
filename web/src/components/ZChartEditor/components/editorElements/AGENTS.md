<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# editorElements

## Purpose
ZChartEditor 右侧属性编辑面板。基于当前选中的 chart/filter 节点,提供 Tab 切换的"图表数据 / 样式 / 筛选条件"配置:图表类型 radio、Y 轴指标多选、X 轴维度单选、筛选条件 (Filter.vue) 增删。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 表单容器,按 `currTab` 切换不同配置面板,绑定 `chartType / yAxisMetrics / dimension / conditions` |
| `Filter.vue` | 单条筛选条件编辑器(字段/操作符/值) |
| `props.ts` | `currentChartId / chartType / metrics / dimensions` 等表单 props |

## For AI Agents

### Working in this directory
- 表单数据通过 `chartStore` 同步到画布;不要在本面板缓存独立副本,否则与画布状态不一致。
- `state.chartTypes` 用于决定渲染图表数据 Tab 的字段(指标/维度);新增图表类型须同步该集合。
- 条件项使用 `addCondition / editCondition / delCondition` 三件套,保持 UX 一致。

### Common patterns
- `a-form-item label="..." class="font-bold"` 风格统一,分隔使用 `<a-divider dashed />`。

## Dependencies
### Internal
- `/@/store/modules/chart`
### External
- Ant Design Vue,`@ant-design/icons-vue` (PlusOutlined / CloseOutlined)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
