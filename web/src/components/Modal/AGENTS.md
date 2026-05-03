<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Modal

## Purpose
项目通用对话框组件 `BasicModal`。在 `ant-design-vue` 的 `Modal` 之上叠加全屏切换、拖拽、Wrapper（统一内容高度计算）、loading、`useModal` 命令式 Hook 等能力，是 vben-admin 风格 modal 的本地分支。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 通过 `withInstall` 导出 `BasicModal`，并暴露 `useModal` / `useModalInner` / `useModalContext`。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 主组件、Wrapper、props、样式 (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 公共代码同时兼容 `visible` 与 `open` 两种 ant-design-vue prop 名（`open` 优先级低于 `visible`），改造请保留兼容。
- `BasicModal-fixed.vue` 是历史固定布局变体，请优先维护 `BasicModal.vue` 的主分支。

### Common patterns
- 命令式调用：`const [register, { openModal, closeModal, setModalProps }] = useModal()`，`<BasicModal @register="register">`。

## Dependencies
### Internal
- `/@/utils`、`./src/typing`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
