<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Concrete implementations of the seven shared business modals. Each is a self-contained `.vue` SFC combining `a-modal`, `BasicLeftTree`, `BasicTable`, and an internal select/value mode that round-trips selections to the parent through `v-model:value`.

## Key Files
| File | Description |
|------|-------------|
| `InterfaceModal.vue` | 接口选择 — left tree + paginated table with expand-row showing template fields/dataType/required. |
| `BillRuleModal.vue` | 单据规则选择. |
| `SelectModal.vue` | Generic table-based select dialog. |
| `PreviewModal.vue` | Preview / detail viewer. |
| `ExportModal.vue` | Field-pick + format wizard for Excel export. |
| `ImportModal.vue` | Stepper for template download → upload → mapping → confirm. |
| `SuperQueryModal.vue` | 高级查询 builder over column metadata. |

## For AI Agents

### Working in this directory
- Use `<ModalClose :canFullscreen="false" @cancel="handleCancel" />` in the `#closeIcon` slot — this matches the rest of the codebase.
- Layout class names (`page-content-wrapper`, `page-content-wrapper-left`, `page-content-wrapper-center`, `jnpf-sub-table`) are part of a shared CSS contract — don't rename without grepping callers.
- Long lists of fields/options should always be paginated through `BasicTable`.

### Common patterns
- `v-model:visible` + `@ok` / `@cancel` events; inner `innerValue` synced back through `update:value`.

## Dependencies
### Internal
- `/@/components/Table`, `/@/components/Tree`, `/@/components/Modal`.
### External
- `ant-design-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
