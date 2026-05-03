<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Link

## Purpose
`JnpfLink` 文本链接组件入口目录。点击文本后根据 `target` 走应用内 `useGo('/externalLink?...')`（base64 加密 href）或 `window.open` 新开。表单设计器中的"超链接"控件即为此组件。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(Link)` 后导出 `JnpfLink`，供全局插件式注册 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件 SFC 与 props 定义（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 新增 props 同时改 `src/props.ts` 与 `src/Link.vue`，保持 `defineProps(linkProps)` 同步。
- 不要在此处写业务跳转逻辑——使用 `useGo`，且外链统一走 `/externalLink` 中转页。

### Common patterns
- `withInstall` 注册全局组件，组件名 `JnpfLink`（PascalCase 与 `defineOptions.name` 一致）。

## Dependencies
### Internal
- `/@/hooks/web/useDesign`、`/@/hooks/web/usePage`、`/@/utils/cipher`
### External
- 无（仅依赖 Vue / 项目内基础工具）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
