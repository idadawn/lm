<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`BasicModal` 的 5 个内部组件：`Modal`（拖拽宿主）、`ModalWrapper`（内容滚动 + 高度计算）、`ModalHeader`、`ModalFooter`、`ModalClose`。

## Key Files
| File | Description |
|------|-------------|
| `Modal.tsx` | TSX 包装的 ant-design-vue `Modal`；调用 `useModalDragMove` 启用拖拽，并使用 `extendSlots` 透传所有具名插槽。 |
| `ModalWrapper.vue` | 内容容器；负责根据 `minHeight/height/fullScreen/modalFooterHeight` 动态计算并设置 wrapper 高度。 |
| `ModalHeader.vue` | 标题渲染；支持 `helpMessage` 帮助提示，配合双击触发全屏。 |
| `ModalFooter.vue` | 默认底部按钮：`okBtn` / `continueBtn` / `cancelBtn`，loading 状态由 `confirmLoading` / `continueLoading` 控制。 |
| `ModalClose.vue` | 关闭 / 全屏切换按钮组合。 |

## For AI Agents

### Working in this directory
- 拖拽逻辑写在 `useModalDragMove`，子组件不要直接操作 DOM 位置。
- `ModalWrapper` 暴露 `setModalHeight` / `scrollTop` 给 `BasicModal.vue` 通过 `ref` 调用，方法名是契约。
- `extendSlots(slots)` 来自 `/@/utils/helper/tsxHelper`，是 TSX 透传插槽的唯一推荐方式。

### Common patterns
- 子组件通过共享 `basicProps` 自动获得全部 prop，新增 prop 不需要改子组件签名。

## Dependencies
### Internal
- `/@/utils/helper/tsxHelper`、`/@/hooks/core/useAttrs`、`../hooks/useModalDrag`
### External
- `ant-design-vue`、`vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
