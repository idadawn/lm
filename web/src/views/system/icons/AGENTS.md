<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# icons

## Purpose
Icon picker / browser page — lets users browse the bundled `icon-ym` and `ym-custom` icon sets and copy class names to clipboard. Supports a search box and tabs.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tabbed grid of `ymIconJson` and `ymCustomJson` icons with copy-on-click. |

## For AI Agents

### Working in this directory
- Icon data comes from `/@/components/Jnpf/IconPicker/data/{ymIcon,ymCustom}` JSON arrays — update there, not here.
- Page is read-only; no API calls. Keep behavior limited to filter/search and copy.

## Dependencies
### Internal
- `/@/components/Form` (search), `/@/components/Jnpf/IconPicker/data/*`
### External
- `lodash-es` (cloneDeep)
