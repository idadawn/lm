<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfCheckbox` 与 `JnpfCheckboxSingle` 的实现，封装 `ant-design-vue` `CheckboxGroup`/`Checkbox`。提供 `direction`（horizontal/vertical）与 `fieldNames` 自定义。

## Key Files
| File | Description |
|------|-------------|
| `Checkbox.vue` | 多选实现：`CheckboxGroup` + 选项数组；按 `direction` 注入 `jnpf-{horizontal|vertical}-checkbox` class |
| `CheckboxSingle.vue` | 布尔型单选 checkbox 实现 |
| `props.ts` | `checkboxProps` 与 `FieldNames` 接口（`label/value/disabled`） |

## For AI Agents

### Working in this directory
- `value` 类型为 `string[] | number[] | boolean[]`，请勿改成单值（用 `CheckboxSingle` 代替）。
- 增加方向变体时同步更新 `direction` 字符串与对应 less class。
- 通过 `getFieldNames` 合并默认值与用户传入，避免空字段名报错。

### Common patterns
- `useAttrs({ excludeDefaultKeys: false })` 透传 + `getBindValue` 拼接 class。
- `watch(props.value, …, { immediate: true })` 双向同步。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
