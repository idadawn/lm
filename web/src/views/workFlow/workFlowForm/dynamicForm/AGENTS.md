<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dynamicForm

## Purpose
动态表单 host — renders a `Parser` of any form-design output for flows that don't use a hardcoded business form. Pulls the form layout from the generator store.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Mounts `Parser` (`createAsyncComponent`) with `formConf` from `useGeneratorStore`. |

## For AI Agents

### Working in this directory
- Use `createAsyncComponent` to lazy-load `Parser`; do not import it eagerly — the parser bundle is large.
- Exposes `dataFormSubmit` via `defineExpose` so `FlowParser` can call submit programmatically.

## Dependencies
### Internal
- `/@/store/modules/generator`, `/@/store/modules/user`, `/@/utils/jnpf`, `/@/utils/factory/createAsyncComponent`
