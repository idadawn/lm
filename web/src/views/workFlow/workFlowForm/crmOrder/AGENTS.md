<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# crmOrder

## Purpose
内置 CRM 客户订单表单 — flow-bound form with customer auto-complete, salesperson select and product line items.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Main order form with `judgeShow`/`judgeWrite` field gating. |
| `GoodsModal.vue` | Modal for picking goods/products into the order. |

## For AI Agents

### Working in this directory
- `judgeShow`/`judgeWrite` come from `useFlowForm`; preserve their use for every field so node-level visibility/writability rules apply.
- Customer auto-complete uses `@select`/`@search` — debounce in the API layer rather than locally.
