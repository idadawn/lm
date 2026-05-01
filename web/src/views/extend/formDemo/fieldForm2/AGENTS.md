<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# fieldForm2

## Purpose
基础字段示例：演示普通文本框、座机号码、当前登录人、自然数、报销款、产品价格、生产/回款/计划/统计/规划日期等常用业务字段以及日期范围控件 `jnpf-date-range`。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 普通文本 / 数值 / 日期时间组件用法集合 |

## For AI Agents

### Working in this directory
- 日期统一使用 `jnpf-date-picker` / `jnpf-date-range`，禁止直接用 `a-date-picker`，否则丢失项目时区与禁用日期约定。
- 数值字段通过 `addon-after` 描述单位（如 “元”）；动态默认值用 `addonAfter` slot + 点击事件实现（参考“随机设定”）。
- 仅作示例，无后端调用；业务化时把 `dataForm` 改为接口拉取并 `setFieldsValue`。

### Common patterns
- `a-divider orientation="left"` 分组同类字段。
- `:labelCol="{ style: { width: '110px' } }"` 统一标签宽度，与项目通用规范一致。

## Dependencies
### Internal
- `/@/components/Container`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
