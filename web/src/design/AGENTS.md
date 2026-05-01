<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# design

## Purpose
Global Less stylesheet root. Wires together fonts (icon fonts), CSS transitions, Less variables, public layout helpers, common rules, ant-design overrides, and the dark-mode theme. Imported once from `main.ts` via `/@/design/index.less`.

## Key Files
| File | Description |
|------|-------------|
| `index.less` | Aggregator — imports `fonts`, `transition`, `var`, `public.less`, `common.less`, `ant/index.less`, `theme.less`. Adds autofill fixes and `html`/`body` resets, plus `.color-weak` (invert) and `.gray-mode` (grayscale) page filters. |
| `theme.less` | Light + dark theme overrides — `[data-theme='light']` alert palette and `[data-theme='dark']` block tweaks for ant-design components, JNPF-prefixed modals/drawers, uploader, monitor, dashboard, print template, etc. References `../assets/images/other-login-bg-dark.png`. |
| `common.less` | ~37KB of project-wide utility classes and component overrides. |
| `color.less` | Color palette / functional colors. |
| `public.less` | Layout helpers shared across pages. |
| `config.less` | Tiny shim file. |
| `windi-base.css` | Base CSS shipped alongside WindiCSS virtual modules (`virtual:windi-base.css` in `main.ts` covers utilities/components). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `ant/` | Ant Design Vue overrides (btn, input, table, pagination) (see `ant/AGENTS.md`). |
| `transition/` | Reusable CSS transitions (fade/slide/scale/zoom/scroll) (see `transition/AGENTS.md`). |
| `fonts/` | Iconfonts (`ym`, `ym-custom`, `kpi`) (see `fonts/AGENTS.md`). |
| `var/` | Less variables (palette, spacing, layout). |

## For AI Agents

### Working in this directory
- Edit through `index.less` aggregator only — do not introduce new top-level Less files without adding an `@import` to `index.less`.
- Dark mode rules: target `[data-theme='dark']` (set by the boot script in `web/index.html` from `localStorage['__APP__DARK__MODE__']`). Never use JS to toggle styles.
- Ant variable references like `@component-background`, `@layout-body-background`, `@warning-color`, `@primary-color` come from the theme runtime injected by `@rys-fe/vite-plugin-theme` — keep the references symbolic, do not hard-code colors.
- WindiCSS handles utility classes; this dir is for global rules and overrides not expressible as utility composition.
- No inline styles in `.vue` files (project rule); put shared rules here or scope to `<style scoped>`.

### Common patterns
- Class prefix `jnpf-*` is preserved from the upstream JNPF admin template — keep when overriding existing components.
- Ant component overrides are flat (`.ant-form .ant-form-item`), not BEM.

## Dependencies
### Internal
- Loaded by `../main.ts`; references images in `../assets/images/`.
### External
- Less 4, ant-design-vue, `@rys-fe/vite-plugin-theme` (patched), WindiCSS 1.9.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
