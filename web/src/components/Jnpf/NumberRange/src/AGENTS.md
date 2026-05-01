<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfNumberRange` SFC 实现：两个 `a-input-number` + `-` 分隔符组成 [min, max] 输入。监听 `props.value` 反向写回 `min/max`，`onChange` 时同时 emit `update:value`/`change` 并触发 a-form 校验。

## Key Files
| File | Description |
|------|-------------|
| `NumberRange.vue` | 模板 + setup：`getPrecision` 计算精度；`min/max` 同时为空时 emit `[]`；其余 emit `[min, max]` |

## For AI Agents

### Working in this directory
- `min`/`max` 任一为空（且不为 0）时，整个值视为空 `[]`——保持与表单校验一致，0 是合法值。
- 必须调用 `formItemContext.onFieldChange()`，否则 `a-form` 不会触发 rules 校验。
- 样式 less 中 `.ant-input-number` 设 `flex: 1` 占满父；分隔符 `.separator` 5px margin。

### Common patterns
- `useAttrs({ excludeDefaultKeys: false })` 透传非声明属性（如 disabled、size、placeholder 风格类）。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`、`/@/hooks/web/useDesign`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
