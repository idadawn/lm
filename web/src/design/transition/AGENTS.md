<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# transition

## Purpose
Reusable Vue `<transition name="...">` CSS animations used across the SPA — fade, slide, scale, zoom, and scrollable variants — plus a height/padding `collapse-transition` helper. Aggregated via `index.less` and pulled into the global stylesheet by `../index.less`.

## Key Files
| File | Description |
|------|-------------|
| `index.less` | Imports `base`, `fade`, `scale`, `slide`, `scroll`, `zoom`. Defines `.collapse-transition` (200ms easing on height + vertical padding). |
| `base.less` | Shared base styles for all transitions (timing, easing). |
| `fade.less` | `fade`, `fade-bottom`, `fade-right`, etc. transition classes. |
| `scale.less` | `scale` family. |
| `slide.less` | `slide-left/right/up/down`. |
| `scroll.less` | Scroll-related transitions. |
| `zoom.less` | `zoom-in/out` transitions. |

## For AI Agents

### Working in this directory
- These are global Vue transition styles; reference by `<transition name="fade">` in templates — no JS hooks needed.
- Adding a new transition: create a `<name>.less` here and import it from `index.less`. Stick with the Vue convention `name`-enter/leave-{from,active,to} class structure.
- Don't add component-specific animations here — keep this file dedicated to widely-shared transitions.
- The `.collapse-transition` is used by accordion/collapse-style components for height transitions; tweak with care since multiple components depend on the 200ms timing.

### Common patterns
- All durations in the 200–300ms range to match ant-design defaults.
- Class names match Vue 3 transition class convention.

## Dependencies
### Internal
- Loaded via `../index.less`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
