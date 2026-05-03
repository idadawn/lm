<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfSlider` SFC——14 行薄壳：`<Slider v-bind="getBindValue">` + slot 循环转发。`useAttrs({ excludeDefaultKeys: false })` 让所有 ant slider 原生属性自动透传。

## Key Files
| File | Description |
|------|-------------|
| `Slider.vue` | 仅模板 + useAttrs，无 props 无 emit |

## For AI Agents

### Working in this directory
- 不要在此添加 v-model 或 watch——保持透明壳，业务能力通过 ant `a-slider` 直接驱动。
- 插槽 `Object.keys($slots)` 转发与 `Rate` 同模式，复制时保持一致风格。

### Common patterns
- `defineOptions({ name: 'JnpfSlider', inheritAttrs: false })` + 显式 v-bind 防双重透传。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
