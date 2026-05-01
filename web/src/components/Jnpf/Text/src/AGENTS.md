<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfText` 组件实现：一个仅渲染 `<p>` 的展示性控件，根据传入的 `textStyle` 对象动态生成 `font-size` / `line-height` 等行内样式。

## Key Files
| File | Description |
|------|-------------|
| `Text.vue` | 组件主体；`computed` 拼接样式（自动补 `px` 单位），使用 `useDesign('text')` 生成 `jnpf-text` 前缀类。 |

## For AI Agents

### Working in this directory
- 保留 `inheritAttrs: false`，避免外层透传属性污染 `<p>` 标签。
- `textStyle` 中的 `font-size` 与 `line-height` 由 setup 中拼接 `px`，新增数值类样式需同步处理。
- 样式作用域使用 `@prefix-cls: ~'@{namespace}-text'`，不要硬编码类名。

### Common patterns
- 使用 `useDesign` 生成 BEM 风格 prefix。
- 仅暴露 `content` 与 `textStyle` 两个 prop，保持低代码渲染端简单。

## Dependencies
### Internal
- `/@/hooks/web/useDesign`
### External
- `vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
