<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Sub-components for the 数据接口 form: field configuration row editor and a help/explain modal shown inside `Form.vue`.

## Key Files
| File | Description |
|------|-------------|
| `FieldForm.vue` | Modal/inline editor for a single interface field (name, type, default, required). |
| `PageExplainModal.vue` | Static help modal explaining interface configuration semantics. |

## For AI Agents

### Working in this directory
- These are leaf modals — keep them stateless, accept data via props, emit `register`/`ok`/`change`.
- Do not import from `../Form.vue`; the parent registers and passes config in.

## Dependencies
### Internal
- `/@/components/Modal`, `/@/components/Form`
