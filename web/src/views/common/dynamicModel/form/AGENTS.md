<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# form

## Purpose
Single-record form runtime for `dynamicModel`. Decides whether to open a plain form popup or to gate the submission through the workflow engine when `config.enableFlow` is set. Resolves the active flow definition (single → auto, multiple → modal picker) and pipes the user into either `FormPopup` or `FlowParser`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Entry — branches on `props.config.enableFlow`; non-flow opens `FormPopup` immediately, flow case calls `getFlowList` then either auto-`selectFlow` or shows a `BasicModal` flow picker that opens `FlowParser` |
| `FormPopup.vue` | Wraps `Parser` (visual-dev runtime) in `BasicPopup`; submit calls `createModel`; supports `confirmButtonText`, fullscreen width, reset, and `userStore`-driven defaults |

## For AI Agents

### Working in this directory
- `Parser` (`/@/components/VisualDev/Parser` or similar; imported via `createAsyncComponent` in some variants) consumes the formData/layout JSON; never bypass it — it implements all visual-dev field rules and validations.
- Flow case must call `closeFlowListModal()` before opening `FlowParser` to avoid stacked overlays.
- `state.flowItem` caches the user's last pick so back-navigation returns to the same flow without re-prompting.

### Common patterns
- `BasicPopup` + `usePopup` / `usePopupInner` for full-screen form entries.
- `defineEmits(['register'])` so parent's `usePopup` can wire the open/close API.

## Dependencies
### Internal
- `/@/api/workFlow/flowEngine` (`getFlowList`), `/@/api/onlineDev/visualDev` (`createModel`), `/@/components/{Modal,Popup,Container}`, `/@/views/workFlow/components/FlowParser.vue`, `/@/store/modules/user`, `/@/utils/jnpf` (`getDateTimeUnit`)
### External
- `ant-design-vue`, `dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
