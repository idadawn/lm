<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfSelect` SFC 实现：极薄包装 `a-select`，统一 `fieldNames` 默认值，自动按 `multiple` 切 mode，把 `optionFilterProp` 默认指向 label 字段，并通过 slot 循环转发外部插槽。

## Key Files
| File | Description |
|------|-------------|
| `Select.vue` | setup + 模板；`getBindValue` 合并 attrs/props 与 `getPopupContainer = () => document.body` |
| `props.ts` | `selectProps`：`value`(标量\|数组) / `options` / `fieldNames` / `optionFilterProp` / `multiple` / `placeholder`，`FieldNames` 接口含 `options` |

## For AI Agents

### Working in this directory
- 不要直接渲染 `<a-option>`——保持 options 数组驱动，与表单设计器配置一致。
- `setValue` 把 0 视为合法值（`value || value === 0`）；其它假值统一转 `undefined` 让 placeholder 生效。
- 通过 `defineExpose({ getSelectRef })` 暴露原生 select 实例（如需 focus/blur）；调用前要确保 ref 已挂载，否则抛错。

### Common patterns
- `useAttrs({ excludeDefaultKeys: false })` + `inheritAttrs: false`，所有原生 ant select 属性照常透传。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
