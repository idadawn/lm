<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Expression

## Purpose
"编辑表达式" modal used by the metric/composite-target editor. Lets users build numeric formulas by clicking metric names from a searchable list and combining them with `+ - * / ( )` operators. On submit, names inside `${...}` placeholders are replaced with metric ids, and the formula is validated server-side.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Default-exports `./src/index.vue`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Modal SFC + props (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Backend integration uses `/@/api/targetDefinition` (`getMetricAll`, `postMetricCompositeFormulaCheck`) — only numeric metrics are supported.
- Placeholder syntax is `${metricName}` for the UI and `${metricId}` for emitted payloads; the `replaceNameWithValue` helper handles the swap — preserve when extending.

### Common patterns
- Modal visibility is controlled by `props.visible` and the consumer must listen to `visible_expression` to close.
- Submit emits four events: `expression_id` (formatted with ids), `expression_value` (display formula), `expression_dimensions` (deduped id array), `visible_expression: false`.

## Dependencies
### Internal
- `/@/api/targetDefinition`, `/@/hooks/web/useMessage`.
### External
- `ant-design-vue` (Modal/Input/Textarea/Button/Row/Col), `@ant-design/icons-vue`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
