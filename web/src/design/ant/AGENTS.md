<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ant

## Purpose
Ant Design Vue 3.2 visual overrides. Tweaks the default look of buttons, inputs, tables, pagination, and a handful of compound components (modal/popover/form/back-top/checkbox/steps) to match the LIMS visual spec while keeping ant theme variables intact.

## Key Files
| File | Description |
|------|-------------|
| `index.less` | Aggregator — imports the per-component files and adds global tweaks: image-preview operations background, popover shadow, back-top position, ant-form item margins, ant-steps-small icon line-height, and modal-icon color helpers (`.modal-icon-warning/-success/-error/-info`) keyed off `@warning-color/@success-color/@error-color/@primary-color`. |
| `btn.less` | Button variant overrides (~7.6KB). |
| `input.less` | Input field tweaks. |
| `pagination.less` | Pagination styling. |
| `table.less` | Ant table overrides (header/cell padding, hover, selection). |

## For AI Agents

### Working in this directory
- Reference Ant variables (e.g. `@warning-color`, `@primary-color`) — they're injected at runtime by `@rys-fe/vite-plugin-theme`. Do not hard-code hex colors.
- Selectors must remain global (no `:scoped`) since these rules target ant-generated DOM.
- Adding a new component override → add a new `<component>.less` here and import it from `index.less`.
- Watch out for `!important` — used sparingly to defeat ant inline styles (e.g. modal-icon colors); avoid spreading further.

### Common patterns
- Each per-component file owns only its component's overrides; cross-component rules go in `index.less`.
- Class prefix `ant-*` for upstream selectors; project-specific compound components use `jnpf-*` (overrides for those live in `../theme.less`/`../common.less`).

## Dependencies
### Internal
- Imported by `../index.less`; depends on Less variables defined in `../var/`.
### External
- `ant-design-vue@^3.2.20`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
