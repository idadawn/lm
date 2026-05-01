<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of the demo `ZEditorForm` component referenced by the parent `componentDemo/index.ts` barrel.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Component definition — `defineOptions({ name: 'ZEditorForm' })` and a single reactive state. |
| `props.ts` | Prop schema consumed by both the component and `ExtractPropTypes`. |

## For AI Agents

### Working in this directory
- Keep `name` in `defineOptions` matching the export name (`ZEditorForm`).
- All public props must live in `props.ts` so the parent `index.ts` can derive types from them.
