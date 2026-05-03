<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# file

## Purpose
Browser-side file helpers — base64 ↔ blob conversion plus four download flavors (online URL, base64 string, raw data buffer, plain URL). Used by export buttons, image upload preview, report download, etc.

## Key Files
| File | Description |
|------|-------------|
| `download.ts` | `downloadByOnlineUrl`, `downloadByBase64`, `downloadByData`, `downloadByUrl` — orchestrates blob creation + temp `<a>` click. |
| `base64Conver.ts` | `dataURLtoBlob` and `urlToBase64` (canvas-based fetch). |

## For AI Agents

### Working in this directory
- `downloadByOnlineUrl` first round-trips through `urlToBase64` to bypass cross-origin download blocks; only use for same-CDN images.
- Optional `bom` arg lets you prepend `﻿` for Excel-friendly UTF-8 CSVs.
- `openWindow` re-imported from `/@/utils` for fallback opens.

### Common patterns
- Returns `void`; downloads kick off via DOM side effects.
- MIME defaults to `application/octet-stream` when not provided.

## Dependencies
### Internal
- `/@/utils` (`openWindow`), `/@/hooks/setting`, `/@/utils/env`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
