<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Qrcode

## Purpose
QR code component package with optional centre-logo overlay and download support. Wraps the `qrcode` npm library and adds canvas-based logo compositing (border, radius, background colour) so a brand mark can sit inside the code without breaking scannability.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `QrCode` (via `withInstall`) and re-exports type definitions from `src/typing`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Implementation: `Qrcode.vue`, `qrcodePlus.ts`, `drawCanvas.ts`, `drawLogo.ts`, `toCanvas.ts`, `typing.ts` (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Component supports two render tags: `canvas` (full features incl. logo) and `img` (no embedded logo).
- Emits `done({ url, ctx })` on successful render and `error(err)` on failure — wire both for downloadable flows.
- `LogoType` carries border/bgColor/borderRadius/logoRadius — tune via the `logo` prop, not by editing internals.

### Common patterns
- Plug-in style API similar to vben-admin: `withInstall` for global registration.

## Dependencies
### Internal
- `/@/utils` (`withInstall`), `/@/utils/file/download` (`downloadByUrl`).
### External
- `qrcode` (`toCanvas`, `toDataURL`, `QRCodeRenderersOptions`, `QRCodeSegment`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
