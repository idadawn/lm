<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# externalLink

## Purpose
菜单外链承载页。从路由 `query.href`（Base64 编码）解码出真实地址后以 `<iframe>` 内嵌打开，并根据 `query.name` 设置标签页标题，用于在系统标签栏内嵌入第三方系统。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 解码 `query.href` 并渲染全屏 iframe；通过 `useTabs.setTitle` 同步标签页名称 |

## For AI Agents

### Working in this directory
- 链接经 `decodeByBase64` 解码，配置端必须用 `encodeByBase64` 编码后写入路由 query；勿直接传明文 URL。
- 不要在此处放业务逻辑，外链 iframe 不应与主应用通信；如需双向通信请改造为自定义页（参见 `dynamicPortal`）。

### Common patterns
- 单文件极简模式：`onMounted` 一次性读取 query 并设置 ref，无状态库依赖。
- 复用统一的 `page-content-wrapper-form` 容器以保持外边距一致。

## Dependencies
### Internal
- `/@/hooks/web/useTabs`
- `/@/utils/cipher` (`decodeByBase64`)
### External
- `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
