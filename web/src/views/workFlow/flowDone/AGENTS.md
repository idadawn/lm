<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# flowDone

## Purpose
已办 (completed/handled) tasks list — flows the current user has already approved/rejected. Read-only with detail view via `FlowParser`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | List with 通过/退回/前加签 status tags and detail action. |

## For AI Agents

### Working in this directory
- Status mapping here uses 1=通过, 10=前加签, else=退回 — different from the launch/monitor pages; preserve carefully.
