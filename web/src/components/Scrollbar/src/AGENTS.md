<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation files for the element-ui-derived custom `Scrollbar`. The SFC drives layout and resize tracking, `bar.ts` renders the draggable thumb (vertical/horizontal), and `util.ts` defines the `BAR_MAP` axis lookup that lets thumb code share logic across both directions.

## Key Files
| File | Description |
|------|-------------|
| `Scrollbar.vue` | Wrapper SFC; computes wrap/view styles, listens to scroll + resize, provides context to child `<bar>` instances, conditionally renders X/Y bars. |
| `bar.ts` | Bar component (functional render) — handles thumb drag, `mousemove`/`mouseup` listeners, click-to-jump on track. |
| `util.ts` | `BAR_MAP` (vertical/horizontal axis config), `renderThumbStyle`, `toObject` style merge helper. |
| `types.d.ts` | `BarMap`, `ScrollbarType` interfaces. |

## For AI Agents

### Working in this directory
- `BAR_MAP` is the single source of truth for axis-specific property names (`offsetHeight`/`scrollTop` etc.) — extend here when adding axes, never inline.
- Thumb uses CSS `transform: translateX/Y(...)` with vendor prefixes — preserve `webkitTransform`/`msTransform` for legacy support.

### Common patterns
- `provide`/`inject` used to share scroll wrap ref between `Scrollbar.vue` and `Bar`.

## Dependencies
### Internal
- `/@/utils/event`, `/@/settings/componentSetting`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
