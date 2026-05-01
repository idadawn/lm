<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# printBrowse

## Purpose
打印预览 fullscreen modal — renders one or more print templates filled with backend data, supports batch pagination (上一页/下一页), client-side rendering of QR codes (`qrcode.toDataURL`) and barcodes (`JsBarcode`), 下载/打印 actions, and writes a print log via `createPrintLog`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Single-file modal: header with logo + page navigator + 下载/打印 buttons; body renders raw HTML via `v-html` from `batchData`. |

## For AI Agents

### Working in this directory
- Templates are pre-rendered server-side into HTML strings (`batchData`) and injected with `v-html` — sanitization is upstream's responsibility.
- QR/barcode tokens in templates are post-processed client-side: scan `qrList`/`barList` and rewrite `<img>` src after mount.
- `createPrintLog` must be called once per actual print action (not per preview page change).

### Common patterns
- Uses `useModalInner` and `useGlobSetting` for global config; `dayjs` for 打印时间 substitution.
- 千位分隔符 / 大写金额 formatting hooks via `getAmountChinese` from `/@/utils/jnpf`.

## Dependencies
### Internal
- `/@/components/Modal`, `/@/api/system/printDev`, `/@/store/modules/user`, `/@/utils/jnpf`, `/@/utils/uuid`.
### External
- `jsbarcode`, `qrcode`, `dayjs`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
