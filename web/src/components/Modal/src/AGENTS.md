<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`BasicModal` 实现层。组合 `Modal`（拖拽）、`ModalWrapper`（高度计算）、`ModalHeader/Footer/Close` 子组件，提供 `setModalProps`、`redoModalHeight`、全屏、双标题等扩展能力。

## Key Files
| File | Description |
|------|-------------|
| `BasicModal.vue` | 入口；维护 `openRef`、`propsRef`，emit `register/open-change/cancel/ok/update:open/update:visible`。 |
| `BasicModal-fixed.vue` | 固定高度变体（历史保留）。 |
| `props.ts` | `basicProps`：含 `canFullscreen`、`useWrapper`、`draggable`、`closeFunc`、`helpMessage` 等。 |
| `typing.ts` | 对外类型：`ModalProps`、`ModalMethods`、`UseModalReturnType`、`UseModalInnerReturnType`、`ModalWrapperProps`。 |
| `index.less` | 样式入口（fullscreen-modal、drag、wrapper 等）。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 子组件 Modal/Wrapper/Header/Footer/Close (see `components/AGENTS.md`) |
| `hooks/` | useModal、useModalContext、useModalDrag、useModalFullScreen (see `hooks/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `setModalProps` 通过 `deepMerge` 合并历史值，新增字段需保证可被深度合并。
- `redoModalHeight` 由 `useModalContext` 暴露给 `BasicModal` 的内容（如 `Markdown.vue`）调用，谨慎修改 InjectionKey。
- `wrapClassName` 在全屏模式下追加 `fullscreen-modal`，业务自定义 className 需通过该字段传入。

### Common patterns
- 同一个组件实例可同时作为受控（`visible`）与命令式（`useModal`）使用。
- 标题区域双击触发 `handleFullScreen`（`canFullscreen=true` 时）。

## Dependencies
### Internal
- `/@/utils`（`deepMerge`）、`/@/hooks/web/useDesign`、`/@/utils/is`
### External
- `ant-design-vue`、`lodash-es`、`vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
