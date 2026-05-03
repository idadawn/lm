<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# kg-demo

## Purpose
uni-app page that demonstrates the Knowledge-Graph reasoning-chain UI for the LIMS mobile client. Renders a hard-coded fixture set from `nlq-agent/packages/shared-types/fixtures/reasoning-steps.fixture.json` and offers a button to switch over to the real backend via SSE.

## Key Files
| File | Description |
|------|-------------|
| `kg-demo.vue` | Single-file uni-app page. Uses `<kg-reasoning-chain>` to render a `record → spec → rule → condition[] → grade` chain (e.g., 炉号 1丙20260110-1 → C 级 判定); `connectLive` wires `streamNlqChat` to consume reasoning steps from the NLQ agent over SSE. |

## For AI Agents

### Working in this directory
- This is a *demo* page — keep the offline fixture aligned with `nlq-agent/packages/shared-types/fixtures/reasoning-steps.fixture.json`. If that fixture changes, mirror it here.
- The page imports the `kg-reasoning-chain` component from `@/components/kg-reasoning-chain/` and the SSE helper from `@/utils/sse-client.js` — keep those paths.
- Field IDs in conditions (`F_WIDTH`, `F_PERF_PS_LOSS`) reference real LIMS columns; preserve the casing.

### Common patterns
- Reasoning-step kinds: `record`, `spec`, `rule`, `condition`, `grade`. Conditions add `field`, `expected`, `actual`, `satisfied`.
- `streamNlqChat` invokes `onReasoningStep` callback per SSE event — do not buffer; push into `steps` for live render.

## Dependencies
### Internal
- `web/src/components/kg-reasoning-chain` (mobile-shared component), `web/src/utils/sse-client.js`, `nlq-agent` HTTP/SSE API.
### External
- uni-app runtime.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
