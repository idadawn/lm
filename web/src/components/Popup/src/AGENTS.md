<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`BasicPopup` 主体实现 + 命令式 Hook + 类型定义。提供头部（`PopupHeader`）、`ScrollContainer` 滚动内容区、可选 `insertFooter` 槽，并通过 `usePopup` / `usePopupInner` 在外部 / 内部受控调用。

## Key Files
| File | Description |
|------|-------------|
| `BasicPopup.vue` | 主体；内部使用 `ScrollContainer` 渲染主内容，`v-loading` 指令 + `loadingTip` 显示 loading，`closeFunc` 支持异步关闭拦截。 |
| `usePopup.ts` | 与 `useModal` 同构的命令式 API：`openPopup/closePopup/setPopupProps/changeLoading`，按 uid 隔离 `dataTransferRef`。 |
| `props.ts` | `basicProps` 集合（与 typing 中 `PopupProps` 对齐）。 |
| `typing.ts` | `PopupProps` / `PopupHeaderProps` / `UsePopupReturnType` / `UsePopupInnerReturnType`，包含 OK/Continue/Cancel 三按钮配置。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | `PopupHeader.vue` 等子组件（不在 deepinit 子目录列表内时跳过） |

## For AI Agents

### Working in this directory
- 默认使用 `ScrollContainer`（来自 `/@/components/Container`）实现自滚，不要替换为原生 overflow。
- `changeContinueLoading` / `continueText` / `continueButtonProps` 是项目专属的「继续」按钮通道，保留以兼容业务页面。
- `class="full-popup"` 模式下会隐藏滚动条并让内容子项 100% 高度，新增样式时尊重该规则。

### Common patterns
- `usePopup` 只能在 `setup()` 内调用，且必须搭配 `<BasicPopup @register="register">`；`usePopupInner` 用于 popup 内组件，回调 `dataTransferRef` 接收 open 时携带的 data。

## Dependencies
### Internal
- `/@/components/Container`、`/@/hooks/web/useI18n`、`/@/hooks/web/useDesign`、`/@/hooks/core/useAttrs`、`/@/utils`、`/@/utils/is`、`/@/utils/log`、`/@/utils/env`
### External
- `vue`、`@vueuse/core`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
