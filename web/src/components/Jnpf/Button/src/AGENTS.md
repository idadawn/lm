<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfButton` 的 SFC 与 props 实现。在容器层用 `align` 控制 `text-align`，按钮文本来自 `buttonText`，其它属性透传到底层 `a-button`。

## Key Files
| File | Description |
|------|-------------|
| `Button.vue` | 组件实现：使用 `useDesign('button')`、`useAttrs`、`omit` 过滤掉容器层独有属性后透传 |
| `props.ts` | `buttonProps`：`align`（默认 `left`）、`buttonText`、`onClick: Function` |

## For AI Agents

### Working in this directory
- 透传时务必用 `omit({ ...attrs, ...props }, ['align', 'buttonText'])`，避免把容器属性传到 `a-button` 触发 antd 警告。
- 样式使用 less + `@prefix-cls = ~'@{namespace}-button'`，新增对齐变体时遵循 `&&-{align}` 命名。

### Common patterns
- `defineOptions({ name: 'JnpfButton', inheritAttrs: false })` + `useAttrs` 模式与同级 Jnpf 组件一致。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`、`/@/hooks/web/useDesign`
### External
- `ant-design-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
