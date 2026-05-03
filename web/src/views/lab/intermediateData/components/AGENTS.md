<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
中间数据页面专用组件：进度条、颜色选择器、可编辑单元格容器、层压/厚度子表与数值单元格。

## Key Files
| File | Description |
|------|-------------|
| `CalcProgressBar.vue` | 公式计算进度条 |
| `ColorPicker.vue` | 单元格颜色选择 |
| `EditableCell.vue` | 通用可编辑单元格容器 |
| `LaminationSubTable.vue` | 层压子表（弹层展示某行明细）|
| `ThicknessSubTable.vue` | 厚度子表 |
| `NumericTableCell.vue` | 数值列单元格（带格式化与编辑）|

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `cells/` | 各列类型的具体 Cell 实现 (see `cells/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 单元格组件 emit `change(value)` 给父表，父表再通过 API 落库。
- 子表（Sub Table）使用展开行或独立 Modal 展示，注意 dispose ECharts 等重资源。

### Common patterns
- props 接收 `record`/`column`/`value`，受控更新。

## Dependencies
### Internal
- `/@/api/lab/intermediateData`
- `/@/components/Table`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
