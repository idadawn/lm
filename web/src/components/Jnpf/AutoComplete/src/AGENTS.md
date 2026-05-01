<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfAutoComplete` 实现，封装 `ant-design-vue` 的 `a-auto-complete`，支持通过远端数据接口（`getDataInterfaceDataSelect`）按关键字检索候选项，并以 `useDebounceFn` 节流。常用于 LIMS 表单生成器中需要联想选择的输入场景。

## Key Files
| File | Description |
|------|-------------|
| `AutoComplete.vue` | 组件实现：组合 `useAttrs`、`Form.useInjectFormItemContext`、debounce 搜索；通过 `interfaceId` + `templateJson` + `relationField` 拉取远端候选 |
| `props.ts` | 导出 `autoCompleteProps`：`interfaceId`、`templateJson`、`relationField`（默认 `fullName`）、`total`、`placeholder`、`disabled`、`rowIndex`、`formData` |

## For AI Agents

### Working in this directory
- 不要把组件 `name` 改掉，需要保持 `JnpfAutoComplete`，表单生成器通过该名字识别动态控件。
- 新增 prop 时同步在 `props.ts` 暴露，避免直接在 `.vue` 中 `defineProps` 内联声明。
- 远程搜索逻辑必须保留 `useDebounceFn` 包装，避免在表单联动场景里频繁请求 `dataInterface`。

### Common patterns
- `useAttrs({ excludeDefaultKeys: false })` 透传剩余属性；`getBindValue` 计算只暴露 `placeholder/allowClear/disabled`。
- 通过 `Form.useInjectFormItemContext().onFieldChange()` 触发外层表单校验。
- `setValue` / `watch(props.value, …, { immediate: true })` 双向绑定外部 value。

## Dependencies
### Internal
- `/@/api/systemData/dataInterface` — `getDataInterfaceDataSelect`
- `/@/hooks/core/useAttrs`
### External
- `ant-design-vue`、`@vueuse/core`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
