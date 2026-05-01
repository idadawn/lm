<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`appearanceCategory` 页面专用子组件。

## Key Files
| File | Description |
|------|-------------|
| `CategoryModal.vue` | 大类新增/编辑弹窗（fullName + description）|

## For AI Agents

### Working in this directory
- 仅在父页面使用 `useModal` 注册；不要外部引用。
- 字段校验最少两条：名称必填、唯一（前端预校验+后端兜底）。

### Common patterns
- `BasicModal` + `BasicForm` + `useModalInner`。

## Dependencies
### Internal
- `/@/components/Modal`, `/@/components/Form`
- `/@/api/lab/appearanceCategory`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
