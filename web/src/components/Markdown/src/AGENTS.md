<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Markdown 编辑器 / 查看器实现层。基于 Vditor 提供 sv 模式编辑、暗黑模式同步、多语言适配（zh_CN/en_US/ja_JP/ko_KR），并与项目的 `useModalContext` 集成自动调整模态框高度。

## Key Files
| File | Description |
|------|-------------|
| `Markdown.vue` | Vditor 编辑器；watch `getDarkMode` 切主题、`useLocale` 派生 lang，`after` 回调调用 `useModalContext.redoModalHeight`。 |
| `MarkdownViewer.vue` | 仅预览模式，使用 `vditor/dist/method.min` 的 `preview` 方法，主题切换重新渲染。 |
| `getTheme.ts` | 封装 dark/light 到 Vditor 三类主题（`theme/content/code`）的映射函数。 |
| `typing.ts` | `MarkDownActionType`：暴露 `getVditor()` 命令式句柄。 |

## For AI Agents

### Working in this directory
- Vditor 实例必须在 `onBeforeUnmount` / `onDeactivated` 中调用 `destroy()`，避免 keep-alive 场景泄漏。
- `cache.enable=false` 已禁用本地草稿缓存，请勿再次开启（与表单受控逻辑冲突）。
- 编辑器 `mode: 'sv'` 与 `fullscreen.index: 520` 是项目规范，不要随意调整。

### Common patterns
- 通过 `onMountedOrActivated` 兼容 keep-alive 复用。
- `setValue` 仅在外部 `props.value` 与内部缓存不一致时调用，避免光标抖动。

## Dependencies
### Internal
- `/@/locales/useLocale`、`/@/hooks/setting/useRootSetting`、`/@/hooks/core/onMountedOrActivated`、`../../Modal`
### External
- `vditor`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
