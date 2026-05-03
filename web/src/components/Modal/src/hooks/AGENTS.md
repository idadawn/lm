<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
`BasicModal` 的 4 个 Composition API：对外的命令式调用（`useModal` / `useModalInner`）、跨组件上下文（`useModalContext`）、拖拽（`useModalDragMove`）、全屏（`useFullScreen`）。

## Key Files
| File | Description |
|------|-------------|
| `useModal.ts` | 提供 `useModal`（外部使用）与 `useModalInner`（modal 内部使用），通过 `register` 拿到 `ModalMethods` 后暴露 `openModal/closeModal/setModalProps/changeLoading` 等方法；用 `dataTransfer` 传递打开时的 payload。 |
| `useModalContext.ts` | 通过 InjectionKey 提供 `redoModalHeight()`，让 modal 内部组件触发外层重计算。 |
| `useModalDrag.ts` | 监听 `.ant-modal-header` 鼠标事件实现拖拽，含边界处理（防止拖出可视区）。 |
| `useModalFullScreen.ts` | 维护 `fullScreenRef`、追加 `fullscreen-modal` 包裹类名。 |

## For AI Agents

### Working in this directory
- `useModal` 中的 `dataTransfer[uid]` 是按组件 uid 隔离的传参通道，避免跨实例污染；不要改成全局共享对象。
- `tryOnUnmounted` 仅在 `isProdMode()` 下注册以避免 HMR 误清空。
- `useModalDragMove` 通过 `data-drag` 属性去重，确保多实例时不会重复绑定事件。

### Common patterns
- 命令式 API 都返回 `[register, methods]` 的元组形式，与 vben-admin 习惯一致。

## Dependencies
### Internal
- `/@/hooks/core/useContext`、`/@/utils/env`、`/@/utils/is`、`/@/utils/log`、`/@/hooks/core/useTimeout`
### External
- `vue`、`@vueuse/core`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
