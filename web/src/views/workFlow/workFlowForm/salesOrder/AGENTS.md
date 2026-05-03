<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# salesOrder

## Purpose
内置销售订单表单 — flow-bound sales-order form. Mirrors `crmOrder` but for outbound sales scenarios.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Sales-order form with line items and customer/salesperson fields. |

## For AI Agents

### Working in this directory
- Use `useFlowForm` for state and node permissions; keep parity with `crmOrder` so changes can be applied symmetrically.
- Number formatting (price, quantity) should defer to `/@/utils/jnpf` helpers rather than ad-hoc `toFixed` calls.
