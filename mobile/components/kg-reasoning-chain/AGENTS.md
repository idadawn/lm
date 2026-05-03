<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# kg-reasoning-chain

## Purpose
Single-component implementation of the **知识图谱推理过程** display: an expandable, numbered list that walks a user through how the NLQ agent arrived at its answer (匹配的检测记录 → 适用规格 → 判定规则 → 条件评估 → 最终结论 / 降级)。Used inline in chat bubbles and as the standalone KG demo page. Renders both fixture data and live SSE-streamed steps from `nlq-agent`.

## Key Files
| File | Description |
|------|-------------|
| `kg-reasoning-chain.vue` | The `<KgReasoningChain>` component. Props: `steps: ReasoningStep[]`, `defaultOpen: boolean`. Maps `step.kind` to Chinese labels (`record→命中记录`, `spec→产品规格`, `rule→判定规则`, `condition→条件评估`, `grade→最终结论`, `fallback→降级`); for `condition` steps adds a 满足/不满足 badge plus 期望/实际 metadata; otherwise renders `step.detail`. SCSS uses `rpx` and a slate/gray palette consistent with chat bubbles. |

## For AI Agents

### Working in this directory
- This is the **canonical** rendering of the reasoning protocol on mobile — keep it field-compatible with `web/`'s equivalent so backend changes can land in one place.
- The component uses Options API on purpose (compat with all uni-app build targets); do not rewrite to `<script setup>` without confirming MP-WeiXin still compiles.
- New `kind` values must be added in three places: this component's `KIND_LABEL`, `types/reasoning-protocol.d.ts`, and `nlq-agent/packages/shared-types/src/reasoning-protocol.ts` (the upstream source).
- Do **not** add network calls here — the component is a pure renderer; the parent page owns the SSE subscription.

### Common patterns
- `v-if="steps && steps.length > 0"` guard so empty arrays render nothing.
- Tag colors use BEM-style modifiers: `kg-reasoning-chain__tag--record`, `--ok`, `--fail`, etc.

## Dependencies
### Internal
- Type contract: `mobile/types/reasoning-protocol.d.ts` (auto-synced from `nlq-agent/packages/shared-types`).
- Consumers: `pages/chat/chat.vue`, `pages/kg-demo/kg-demo.vue`.

### External
- Vue 3 (uni-app runtime).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
