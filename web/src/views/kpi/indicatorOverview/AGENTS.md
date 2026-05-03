<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# indicatorOverview

## Purpose
KPI 模块"指标看板/概览"页：以卡片网格展示已配置看板（dashboard），支持新建、编辑、删除、预览、跳转图表编辑器。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 卡片列表主页：搜索 + 新建 + 卡片操作（预览/编辑/删除）|
| `Form.vue` | 看板基本信息表单（名称、描述、标签）|
| `DepForm.vue` | 看板部门/分组表单 |
| `ChartEditor.vue` | 看板图表编辑器入口（拖拽布局/图表绑定）|

## For AI Agents

### Working in this directory
- 数据通过 `/@/api/chart`（`getDashTreeList`/`deleteDash` 等）获取；标签通过 `postMetrictagList`。
- 卡片操作按钮使用 `@click.stop` 阻止冒泡到卡片整体跳转。

### Common patterns
- `usePopup` + `useModal` 管理 Form/DepForm。
- `goChartEdtor('edit'|'preview', item)` 路由跳转携带 mode 参数。

## Dependencies
### Internal
- `/@/api/chart`, `/@/api/labelManagement`
- `/@/components/Popup`, `/@/components/Modal`, `/@/hooks/web/useMessage`
### External
- `ant-design-vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
