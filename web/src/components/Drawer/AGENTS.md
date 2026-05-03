<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Drawer

## Purpose
`BasicDrawer` — the project's standard slide-out panel. Wraps `ant-design-vue` `Drawer` with header back-button, scroll container, custom footer (ok/cancel/continue + slots), detail mode, async `closeFunc`, and the imperative `useDrawer`/`useDrawerInner` API used across detail and edit pages.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Barrel: `BasicDrawer`, `useDrawer`, `useDrawerInner`, props/types. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Component, composables, props, sub-components (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Open/close from a parent: use `useDrawer()` returning `[register, { openDrawer, closeDrawer, setDrawerProps, getVisible }]`; inside the drawer, use `useDrawerInner(callbackFn)` to receive the data passed via `openDrawer(true, data)`.
- Detail mode (`isDetail`) auto-stretches to 100% width and mounts inside `.{prefixVar}-layout-content`; preserve the `getContainer` fallback.
- Footer composition uses `showOkBtn/showCancelBtn/showContinueBtn` and named slots `insertFooter`/`centerFooter`/`appendFooter`; do not hardcode buttons.

### Common patterns
- Async close: pass `closeFunc: () => Promise<boolean>` — returning truthy keeps the drawer open.
- Loading state binds via `v-loading` on the inner `ScrollContainer`.

## Dependencies
### Internal
- `/@/components/Container` (ScrollContainer), `/@/components/Basic` (BasicTitle), `/@/hooks/web/useDesign`, `/@/hooks/web/useI18n`, `/@/hooks/core/useAttrs`, `/@/utils` (deepMerge), `/@/utils/log`.
### External
- `ant-design-vue` (Drawer), `@ant-design/icons-vue`, `@vueuse/core`, `lodash-es`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
