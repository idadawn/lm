<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfRate` SFC——14 行的极薄包装：`<Rate v-bind="getBindValue">` 透传 attrs，并通过插槽循环把所有外层 slot 转发给 `a-rate`。

## Key Files
| File | Description |
|------|-------------|
| `Rate.vue` | 仅 `useAttrs({ excludeDefaultKeys: false })` + 模板的 slot 转发 |

## For AI Agents

### Working in this directory
- 不要在此添加 v-model/state——保持透明壳，业务能力通过 ant `a-rate` props 直接驱动。
- 插槽透传写法（`v-for "item in Object.keys($slots)"`）是项目内 JnpfXxx 薄包装通用模式（同 `Slider`），保持一致。

### Common patterns
- `defineOptions({ name: 'JnpfRate', inheritAttrs: false })` + useAttrs 显式 v-bind，避免双重 attr 透传。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
