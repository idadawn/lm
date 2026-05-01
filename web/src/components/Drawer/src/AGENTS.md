<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of `BasicDrawer` plus the host-side and inner-side composables that drive imperative open/close, prop mutation, loading toggles, and data transfer between parent and drawer body.

## Key Files
| File | Description |
|------|-------------|
| `BasicDrawer.vue` | SFC that composes `Drawer + DrawerHeader + ScrollContainer + DrawerFooter`. Manages `visibleRef`, `getMergeProps` (deep-merged props), `getProps` (handles detail/100%-width/getContainer), async `onClose`, footer height calc. Emits `register/visible-change/ok/close`. |
| `useDrawer.ts` | Host-side composable. Tracks `visibleData[uid]` reactive map and `dataTransferRef[uid]` for `openDrawer(visible, data)`; exposes `setDrawerProps/openDrawer/closeDrawer/getVisible`. `useDrawerInner(callbackFn)` is for the drawer's own setup — receives transferred data via `watchEffect` and exposes `changeLoading/changeOkLoading/changeContinueLoading/closeDrawer/setDrawerProps/getVisible`. |
| `props.ts` | `basicProps` and `footerProps` shared between BasicDrawer and `DrawerFooter`. |
| `typing.ts` | `DrawerProps`, `DrawerInstance`, `ReturnMethods`, `UseDrawerReturnType`, `UseDrawerInnerReturnType`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | Header & footer sub-components (see `components/AGENTS.md`). |

## For AI Agents

### Working in this directory
- The drawer registers itself with a UID via `emit('register', drawerInstance, instance.uid)` — both composables key state by that UID; do not bypass it.
- `setDrawerProps({ visible: true })` synchronizes `visibleRef` — keep that branch when extending `setDrawerProps`.
- LESS prefix is `@{namespace}-basic-drawer`; the `__detail` modifier mounts the drawer absolutely.

### Common patterns
- Footer height is propagated to the scroll container's `height: calc(100% - {height})` — preserve when changing layout.
- `getInstance()` warns via `/@/utils/log.error` when the drawer hasn't registered yet.

## Dependencies
### Internal
- `/@/components/Container` (ScrollContainer), `/@/hooks/web/useDesign`, `/@/hooks/web/useI18n`, `/@/hooks/core/useAttrs`, `/@/utils` (deepMerge), `/@/utils/env` (isProdMode), `/@/utils/log`, `/@/utils/is`.
### External
- `ant-design-vue` (Drawer), `@vueuse/core`, `lodash-es` (isEqual), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
