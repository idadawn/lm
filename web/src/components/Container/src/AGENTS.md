<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementations of the layout container family: lazy-load gate, scroll wrapper, shared typings, and the `collapse/` subgroup providing collapsible panels with sticky headers.

## Key Files
| File | Description |
|------|-------------|
| `LazyContainer.vue` | IntersectionObserver-based lazy mount; emits `init` once when visible. |
| `ScrollContainer.vue` | Scrollable wrapper with custom-themed scrollbars; exposes `scrollTo`, `getScrollWrap`. |
| `typing.ts` | Shared types for the container components. |
| `collapse/CollapseContainer.vue` | Section-style collapsible panel with title + slot content + toggle. |
| `collapse/CollapseHeader.vue` | Header bar (title + actions) used inside `CollapseContainer`. |

## Subdirectories
None deepinit-tracked under `src/` beyond `collapse/` (covered inline above).

## For AI Agents

### Working in this directory
- Keep `LazyContainer`'s observer cleanup in `onBeforeUnmount` to avoid leaks across route changes.
- `CollapseContainer` should expose its `expand` / `collapse` API via `defineExpose` so parents can drive it.

### Common patterns
- `<script setup>` with `defineExpose` for imperative API.

## Dependencies
### External
- `vue@3.3`, `ant-design-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
