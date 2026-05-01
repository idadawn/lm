<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# types

## Purpose
Shared TypeScript declaration files specific to the `src/` runtime — kept separate from `web/types/` (which holds globals + ambient build types). Currently the only resident is the synced reasoning-protocol type used by the NLQ-agent SSE client.

## Key Files
| File | Description |
|------|-------------|
| `reasoning-protocol.d.ts` | Canonical `ReasoningStep` / `ReasoningChainEvent` types; auto-synced from `nlq-agent/packages/shared-types/src/reasoning-protocol.ts` (upstream sha pinned in header). Verify with `pwsh scripts/check-reasoning-protocol-sync.ps1`. |

## For AI Agents

### Working in this directory
- Do **not** hand-edit `reasoning-protocol.d.ts` — modify the upstream `nlq-agent/packages/shared-types` source and re-sync via the script. The `upstream-sha` header is the contract.
- Add new shared runtime types here only if they're consumed by `/@/api/*` or feature views; ambient/global types belong in `web/types/`.

### Common patterns
- Files ending in `.d.ts` are pure type declarations — no runtime emit.

## Dependencies
### Internal
- Synced from `nlq-agent/packages/shared-types/`; consumed by `/@/api/nlqAgent.ts`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
