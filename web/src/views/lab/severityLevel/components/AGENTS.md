<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
特性等级页面专用子组件。

## Key Files
| File | Description |
|------|-------------|
| `SeverityLevelModal.vue` | 等级新增/编辑弹窗（名称、描述、颜色等）|

## For AI Agents

### Working in this directory
- 名称改动需弹窗提示"将影响已绑定的特性"。
- 颜色字段建议预设几种（与等级语义一致：低=绿/中=黄/高=红）。

### Common patterns
- `BasicModal` + `BasicForm` + `useModalInner`。

## Dependencies
### Internal
- `/@/api/lab/severityLevel`, `/@/components/Modal`, `/@/components/Form`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
