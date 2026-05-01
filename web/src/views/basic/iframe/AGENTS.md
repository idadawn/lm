<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# iframe

## Purpose
Generic iframe page host. Used by routes whose menu config sets a `frameSrc` — embeds external pages inside the SPA chrome, computes its height from layout headers via `useLayoutHeight`, and shows an Ant `Spin` until the frame fires `load`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Main iframe wrapper — calculates height via `useWindowSizeFn` + `headerHeightRef`, hides loader on `load` |
| `FrameBlank.vue` | Empty placeholder used when no `frameSrc` is provided (router stub) |

## For AI Agents

### Working in this directory
- The iframe height is set imperatively via `iframe.style.height` after a window resize — keep `calcHeight` in sync if you change the parent layout's header structure.
- `useLayoutHeight` is shared with the default layout — touching its return shape will affect this page.
- Routes that target this page pass `frameSrc` through router meta or component props (see `web/src/router`).

### Common patterns
- `useDesign('iframe-page')` produces the `prefixCls` Less namespace (`@{namespace}-iframe-page`).
- `propTypes.string.def('')` for the single `frameSrc` prop.

## Dependencies
### Internal
- `/@/hooks/event/useWindowSizeFn`, `/@/hooks/web/useDesign`, `/@/utils/propTypes`, `/@/layouts/default/content/useContentViewHeight`
### External
- `ant-design-vue` (`Spin`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
