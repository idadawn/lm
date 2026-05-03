<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Reusable Vue components shared across pages. Currently scoped to knowledge-graph (KG) reasoning visualization for the chat / NLQ-agent flow; other UI primitives (cards, KPI tiles, charts) live inline inside their respective `pages/*` files until they are reused.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `kg-reasoning-chain/` | Collapsible step-by-step renderer for NLQ reasoning traces (see `kg-reasoning-chain/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Components are imported by absolute path (`@/components/<name>/<name>.vue`); keep the folder-name == file-name convention so uni-app's `easycom`-style resolution works on all platforms.
- Use `rpx` units and SCSS scoped styles to stay consistent with the rest of the mobile app.
- Component props that mirror the SSE protocol must keep field names aligned with `types/reasoning-protocol.d.ts` (which is itself synced from `nlq-agent/packages/shared-types`). Renaming a prop here means updating the upstream type as well.
- Prefer the Options API for components that may be consumed by both `<script setup>` pages and legacy Options-API pages — uni-app cross-version compatibility is smoother.

### Common patterns
- Tag-based UI: each step is rendered with kind labels (`record`/`spec`/`rule`/`condition`/`grade`/`fallback`) translated to Chinese via a local map.
- Header tap toggles `expanded` to keep chat scrolling clean by default.

## Dependencies
### Internal
- `@/types/reasoning-protocol.d.ts` for `ReasoningStep` shape.
- Consumers: `pages/chat/chat.vue` and `pages/kg-demo/kg-demo.vue`.

### External
- Vue 3 runtime via uni-app.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
