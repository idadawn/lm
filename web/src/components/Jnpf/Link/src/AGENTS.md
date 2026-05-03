<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfLink` SFC 实现：渲染只读文本链接，点击触发 `onClick`，按 `target` 决定走应用内路由（base64 加密 href）或 `window.open`。

## Key Files
| File | Description |
|------|-------------|
| `Link.vue` | 模板 + setup，`useDesign('link')` 生成前缀类，`useGo` 路由跳转，`encryptByBase64` 编码外链 |
| `props.ts` | `linkProps`：`content`/`href`/`target`/`onClick`/`textStyle`（默认 `text-align: left`）|

## For AI Agents

### Working in this directory
- 文本展示节点是 `<p>`，不要换成 `<a>`——内部跳转通过点击事件，不依赖浏览器默认导航。
- `target='_self'` 的 href 必须经 `encryptByBase64` 编码后传给 `/externalLink`，避免特殊字符破坏路由。
- 样式用 `@prefix-cls`（kebab-case `link`）+ less 嵌套；勿写内联 style。

### Common patterns
- `defineOptions({ name: 'JnpfLink', inheritAttrs: false })` 保证全局组件名一致且不透传 attrs。

## Dependencies
### Internal
- `/@/hooks/web/useDesign`、`/@/hooks/web/usePage`、`/@/utils/cipher`
### External
- 无

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
