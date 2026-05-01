<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
报告配置子组件。

## Key Files
| File | Description |
|------|-------------|
| `ReportConfigDrawer.vue` | 报告字段配置抽屉表单（合格/不合格列、分组配置）|

## For AI Agents

### Working in this directory
- 抽屉而非弹窗，便于横向放下多列字段。
- emit `success` 让父页 reload。

### Common patterns
- `useDrawerInner` + `useForm`。

## Dependencies
### Internal
- `/@/api/lab/reportConfig`, `/@/components/Drawer`, `/@/components/Form`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
