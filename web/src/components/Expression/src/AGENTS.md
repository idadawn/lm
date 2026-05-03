<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Modal SFC + prop declarations for the expression editor.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `<a-modal width="800px">` two-column layout. Left: search input + scrollable list of metrics from `getMetricAll`. Right: operator toolbar (`+ - * / ( )`), `<a-textarea>` formula buffer, `清空`/`检查` actions. Tracks `cursorPosition` from textarea blur to insert tokens at caret. `inspectCheck` calls `postMetricCompositeFormulaCheck`; `handleExpression` swaps names→ids and emits the result. |
| `props.ts` | Shared props (`visible`, `expressionValue`, etc.). |

## For AI Agents

### Working in this directory
- Caret-aware insertion uses `srcElement.selectionStart` from blur — do not switch to focus-based tracking without revising `expressionListClick`/`expressionBtn`.
- Search uses a per-keystroke regex against the cached `formularOptionsNew`; ignore-case is built in.
- All UI strings are Chinese (`编辑表达式`, `请输入指标`, etc.) — preserve when editing.

### Common patterns
- Submit flow: regex-extract `${name}`s → look up ids in `formularOptionsNew` → `Array.from(new Set(...))` for unique dimensions → emit.

## Dependencies
### Internal
- `/@/api/targetDefinition`, `/@/hooks/web/useMessage`.
### External
- `ant-design-vue`, `@ant-design/icons-vue` (SearchOutlined, InfoCircleFilled), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
