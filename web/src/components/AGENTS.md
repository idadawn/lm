<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
vben-admin-style component library for the Laboratory Data Analysis System web frontend. Each top-level subdirectory is a self-contained component package (PascalCase) exposing a barrel `index.ts`, with `.vue` implementations under `src/`. Used across `web/src/views/` for forms, tables, modals, charts, code editors, and JNPF low-code form/column/print designers.

## Key Files
| File | Description |
|------|-------------|
| `registerGlobComp.ts` | Globally registers Ant Design Vue components on the Vue app instance. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Application/` | App-level shell pieces: provider, logo, locale picker, dark-mode toggle, search modal (see `Application/AGENTS.md`). |
| `Authority/` | Permission-gated render slot wrapper (see `Authority/AGENTS.md`). |
| `Basic/` | Tiny presentational primitives: arrow, title, caption, help tooltip (see `Basic/AGENTS.md`). |
| `Button/` | Enhanced Ant Button with pre/post icon, popconfirm, and modal-confirm variants (see `Button/AGENTS.md`). |
| `CardList/` | Grid-style list view with pagination and slider-controlled column count (see `CardList/AGENTS.md`). |
| `Chart/` | ECharts wrapper component using `useECharts` hook (see `Chart/AGENTS.md`). |
| `ClickOutSide/` | Click-outside directive wrapper component (see `ClickOutSide/AGENTS.md`). |
| `CodeEditor/` | CodeMirror + Monaco editors and JSON preview (see `CodeEditor/AGENTS.md`). |
| `ColumnDesign/` | JNPF list/column designer with filter rules, summary, super-query (see `ColumnDesign/AGENTS.md`). |
| `CommonModal/` | Reusable selection modals: Interface, BillRule, Select, Preview, Export, Import, SuperQuery (see `CommonModal/AGENTS.md`). |
| `Container/` | Layout containers: lazy-load, scroll, collapse (see `Container/AGENTS.md`). |

(Many other component packages exist alongside — `Form/`, `Table/`, `Modal/`, `Drawer/`, `FormGenerator/`, `PrintDesign/`, etc. — covered in their own deepinit chunks.)

## For AI Agents

### Working in this directory
- Each component package follows the pattern: `index.ts` (barrel) + `src/*.vue` (impl) + optional `style/` (LESS) and `data/`.
- Components are PascalCase, exported via `withInstall(...)` from `/@/utils` so they install on `app.use(...)` if needed.
- Do not add inline styles; put scoped LESS in component-local `<style>` or `style/index.less` using `@prefix-cls` patterns.
- New global registrations go in `registerGlobComp.ts`.

### Common patterns
- Barrel re-exports: `export const Foo = withInstall(foo);`.
- Import path alias `/@/` resolves to `web/src/`.
- Hooks live in `/@/hooks/web/...`; utils in `/@/utils/...`.

## Dependencies
### Internal
- `/@/hooks/web/*` (useECharts, usePermission, useDesign), `/@/utils` (withInstall, helpers), `/@/store/modules/app`.
### External
- `vue@3.3`, `ant-design-vue@3.2`, `@ant-design/icons-vue`, `echarts`, `codemirror`, `monaco-editor`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
