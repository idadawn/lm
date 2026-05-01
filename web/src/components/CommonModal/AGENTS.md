<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# CommonModal

## Purpose
Reusable selection / utility modals used across business pages: 接口选择 (Interface), 单据规则 (BillRule), generic Select, Preview, Excel-style Export and Import wizards, and the SuperQuery (高级查询) builder.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `InterfaceModal`, `BillRuleModal`, `SelectModal`, `PreviewModal`, `ExportModal`, `ImportModal`, `SuperQueryModal` (all `withInstall`). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Modal implementations (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- These modals are typically embedded as `<XxxModal v-model:visible="visible" @ok="..." />` inside form fields or toolbars.
- Tree + table layout uses the shared `page-content-wrapper` / `BasicLeftTree` / `BasicTable` patterns — keep markup consistent across modals.

### Common patterns
- `withInstall` barrel; closeIcon slot uses `ModalClose`.

## Dependencies
### Internal
- `/@/components/Table` (`BasicTable`), `/@/components/Tree` (`BasicLeftTree`), `/@/components/Modal` (`ModalClose`).
### External
- `ant-design-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
