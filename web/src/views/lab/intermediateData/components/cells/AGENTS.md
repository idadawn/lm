<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# cells

## Purpose
中间数据表格按列类型拆分的单元格实现：默认/数值/日期/特征/标签/性能/可编辑测量。每列在 BasicTable `bodyCell` 插槽中按 `column.cellType` 渲染。

## Key Files
| File | Description |
|------|-------------|
| `DefaultCell.vue` | 兜底单元格（纯文本 + 颜色背景）|
| `DefaultCellWithNumeric.vue` | 兼容数值的默认单元格 |
| `NumericCell.vue` | 数值显示（千分位/小数位格式化）|
| `EditableMeasurementCell.vue` | 可原位编辑的测量值单元格 |
| `DateCell.vue` | 日期格式化单元格 |
| `FeatureCell.vue` | 外观特性标签展示 |
| `LabelingCell.vue` | 标注/标签单元格 |
| `PerfCell.vue` | 性能/绩效单元格（带配色阈值）|

## For AI Agents

### Working in this directory
- 每个 Cell 接收 `record`/`column`/`value`，emit `update`/`save` 上抛变更。
- 编辑态切换时只允许一个单元格处于编辑；用 `provide/inject` 或父组件 ref 控制全局编辑锁。

### Common patterns
- `<script setup lang="ts">` + Composition API；样式 scoped。

## Dependencies
### Internal
- `../EditableCell.vue`, `/@/utils/format`
### External
- `ant-design-vue`, `dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
