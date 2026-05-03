<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfCascader` 的 SFC 实现。复用 `a-cascader`，把后端常用字段名 `id/fullName/children` 默认化，并强制 `showCheckedStrategy = SHOW_CHILD`。

## Key Files
| File | Description |
|------|-------------|
| `Cascader.vue` | 组件实现 + 局部 `FieldNames` 接口；通过 `useAttrs({ excludeDefaultKeys: false })` 透传插槽与属性 |

## For AI Agents

### Working in this directory
- 需要修改 `fieldNames` 默认值时，应同时检查后端列表数据是否仍以 `fullName` 作为可读名。
- 透传 `$slots` 时使用 `Object.keys($slots)` 动态注册插槽，新增子节点请保持该模式。

### Common patterns
- `getFieldNames` 在外部传入时合并默认值，确保未传字段不会丢失。
- `defineOptions({ name: 'JnpfCascader', inheritAttrs: false })`。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
