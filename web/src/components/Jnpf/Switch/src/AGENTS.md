<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
JnpfSwitch 实现源码。在 ant-design-vue 的 `Switch` 之上提供自定义 `checkedValue` / `unCheckedValue`（支持数字、字符串、布尔），适配 Jnpf 表单引擎对开关字段双向绑定的需求。

## Key Files
| File | Description |
|------|-------------|
| `Switch.vue` | `JnpfSwitch` 组件实现，watch `props.value` 同步内部 `innerValue`，并将 `change` 透传为 `update:value`。 |
| `props.ts` | 定义 `switchProps`：`disabled`、`checkedValue`（默认 `1`）、`unCheckedValue`（默认 `0`）、`value`。 |

## For AI Agents

### Working in this directory
- 仅修改双向绑定与值转换逻辑；样式继承自 ant-design-vue，不要重写。
- 任何新增 prop 必须同步更新 `props.ts` 与父级 `index.ts` 的导出类型。
- 保持 `defineOptions({ name: 'JnpfSwitch' })`，表单引擎依赖该名称匹配组件。

### Common patterns
- 使用 `useAttrs({ excludeDefaultKeys: false })` 透传非声明属性。
- 通过 `Object.keys($slots)` 转发任意具名插槽。
- `omit(props, ['value'])` 防止 `value` 与 ant-design-vue 内部 `checked` 冲突。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`
### External
- `ant-design-vue`（`Switch`）、`lodash-es`、`vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
