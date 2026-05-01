<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of the `QrCode` component. Splits responsibilities across a Vue SFC (props/lifecycle/emit), a canvas/image facade (`qrcodePlus`), and helpers that draw the QR matrix and composite the centre logo with rounded corners.

## Key Files
| File | Description |
|------|-------------|
| `Qrcode.vue` | SFC accepting `value` (string\|array), `options`, `width`, `logo`, `tag` ('canvas'\|'img'); emits `done`/`error`. |
| `qrcodePlus.ts` | Re-exports the chosen `toCanvas` strategy + types. |
| `toCanvas.ts` | Thin wrapper selecting the rendering path. |
| `drawCanvas.ts` | Renders QR onto a `<canvas>` and triggers `drawLogo` when a logo is supplied. |
| `drawLogo.ts` | Composites logo image with optional border, bg colour, and rounded corners on the QR canvas. |
| `typing.ts` | `LogoType`, `RenderQrCodeParams`, `ToCanvasFn`, `QrCodeActionType`, `QrcodeDoneEventParams`. |

## For AI Agents

### Working in this directory
- Image (`tag='img'`) path uses `qrcode.toDataURL` and intentionally skips logo overlay.
- Cross-origin logos require `LogoType.crossOrigin` to avoid tainted-canvas errors.

### Common patterns
- Async render is debounced via `watch` on `value`/`options`/`width`/`logo`.

## Dependencies
### External
- `qrcode` (toCanvas, toDataURL).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
