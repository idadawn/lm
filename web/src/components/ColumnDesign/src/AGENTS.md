<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Source for the `BasicColumnDesign` 列表设计器 component. Composes a tabbed canvas (head tabs + main panel + right config panel) with sub-views for column selection, filter rules, conditional formatting, summary, and button events.

## Key Files
| File | Description |
|------|-------------|
| `BasicColumnDesign.vue` | Top-level designer shell (head tabs / left panel / center main / right config). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | Sub-panels: BtnEvent, ConditionModal, FormScript, Main, MainApp, UpLoadTpl (see `components/AGENTS.md`). |
| `helper/` | `config.ts` defaults and jnpfKey lookup helpers (see `helper/AGENTS.md`). |

## For AI Agents

### Working in this directory
- The designer reads/writes a single nested `columnData` object — mutate via the right panel only, never directly from sub-panels.
- Keep `helper/config.ts` as the single source of truth for default flags (`hasPage`, `pageSize`, `hasSuperQuery`, ...).

### Common patterns
- jnpfKey strings drive both rendering and search-type derivation.

## Dependencies
### Internal
- `/@/components/FormGenerator`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
