<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# workFlow

## Purpose
Workflow engine API surface — CRUD over flow templates, before-flow validation hooks, delegation rules, pending/launch tasks, monitoring, form designer, and form lookup. Targets `/api/workflow/Engine/*`.

## Key Files
| File | Description |
|------|-------------|
| `flowEngine.ts` | Flow template CRUD + copy + comments (`FlowComment` prefix). |
| `flowBefore.ts` | Pre-flow rule / validation endpoints. |
| `flowDelegate.ts` | Delegation (代理) configuration. |
| `flowLaunch.ts` | Launch (发起) endpoints — start a process instance. |
| `flowMonitor.ts` | Running-instance monitor. |
| `formDesign.ts` | Form designer schema CRUD. |
| `workFlowForm.ts` | Form lookup helpers used by templates. |

## For AI Agents

### Working in this directory
- `Api.Prefix = '/api/workflow/Engine/flowTemplate'` (PascalCase route segments) — match backend exactly.
- Comments live under a separate prefix (`/api/workflow/Engine/FlowComment`) — keep them in `flowEngine.ts` because they're commented against templates.
- Form designer is shared with online-dev — coordinate type changes between `formDesign.ts` and `/@/api/onlineDev/visualDev.ts`.

### Common patterns
- Each file declares its own `enum Api` block. Standard CRUD + a few resource-specific actions (`/Actions/Copy`).

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
