<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# collapse

## Purpose
Implements the `CollapseContainer` shell used across detail/form pages — a titled, foldable wrapper with optional help tooltip, loading skeleton, expand action, and footer slot. Used by `Description` and other content components to provide consistent expandable cards.

## Key Files
| File | Description |
|------|-------------|
| `CollapseContainer.vue` | TSX functional shell: `title`, `helpMessage`, `canExpan`, `loading`, `triggerWindowResize`, `lazyTime` props; exposes `handleExpand`; renders header + body via `CollapseTransition` with `Skeleton` loading state. |
| `CollapseHeader.vue` | Title bar rendered inside the container — emits expand toggle, surfaces `title`/`action` slots and the help tooltip. |

## For AI Agents

### Working in this directory
- Component is authored as TSX (`<script lang="tsx">`) — keep JSX-style render functions, do not convert to SFC template.
- Styles use `useDesign('collapse-container')` prefix; LESS lives inline. New CSS classes must follow `@{namespace}-collapse-container__*` pattern.
- When `triggerWindowResize` is true, expand toggle fires `triggerWindowResize` after a 200ms `useTimeoutFn` to let the animation finish — preserve that timing.

### Common patterns
- Slots exposed: `title`, `action`, `default`, `footer` — surface them through `v-slots` when proxying to header.
- Loading state replaces the body with `ant-design-vue` `Skeleton`, never the whole container.

## Dependencies
### Internal
- `/@/components/Transition` (CollapseTransition), `/@/hooks/web/useDesign`, `/@/hooks/core/useTimeout`, `/@/utils/event`.
### External
- `ant-design-vue` (Skeleton), `lodash-es`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
