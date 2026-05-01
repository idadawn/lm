<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# PrintDesign

## Purpose
打印模板设计器 — TinyMCE-based WYSIWYG print template designer with field/system/function trees on the left and a paginated rich-text canvas. Used by the system print template module to author templates that get rendered against form data for browse + actual printing.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Top-level designer: tree panels (form fields / system fields / functions), embedded `PrintDesignTinymce`, page-size modal trigger. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `PrintDesign/` | TinyMCE editor wrapper with full plugin/toolbar config (see `PrintDesign/AGENTS.md`). |
| `PageSize/` | Modal for paper size, direction, margins (A3/A4/A5/B4/B5/custom) (see `PageSize/AGENTS.md`). |
| `printBrowse/` | Print preview modal with batch pagination, download, JsBarcode/QR rendering (see `printBrowse/AGENTS.md`). |
| `printSelect/` | Picker modal listing available print templates (see `printSelect/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Tree node clicks insert tokens (system fields, formulas like `thousands`, `isAmountChinese`) into the TinyMCE editor — see `handleNodeClick` flow before refactoring.
- 系统字段 are hardcoded: 打印人员/打印时间/审批内容/图片/二维码/条形码 — keep IDs (`systemPrinter`, `systemPrintTime`, `img`, `qrCode`, `barCode`) stable; rendering relies on them.

### Common patterns
- Modals are wired via `useModal()` from `/@/components/Modal`.

## Dependencies
### Internal
- `/@/components/Tree` (`BasicLeftTree`), `/@/components/Container` (`ScrollContainer`), `/@/components/Modal`.
### External
- `tinymce` (loaded by child editor).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
