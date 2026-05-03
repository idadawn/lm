<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HIframe

## Purpose
Embeds an external page inside the portal via `<iframe>`. The URL comes from `useCommon` (which resolves either a static `value` or an interface call).

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Renders `<iframe :src="value" scrolling="yes" frameborder="0">` when `value` is truthy, otherwise the shared empty state. |

## For AI Agents

### Working in this directory
- No sandboxing is set on the iframe — the portal trusts admin-configured URLs. Do not add a `sandbox` attribute without auditing every existing portal config.
- The iframe sizing is via the surrounding `portal-card-iframe` LESS class; keep that wrapper.
- `value` originates from `activeData.option.urlAddress` (static) or a data-interface response (dynamic).

### Common patterns
- One-liner script — `useCommon` returns both `CardHeader` and `value`.

## Dependencies
### Internal
- `../../Design/hooks/useCommon`

### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
