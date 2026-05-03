<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Captcha

## Purpose
Login captcha pipeline. The implementation is currently only one variant — "general" (字符 + 干扰线 image captcha) — but the directory is structured to allow additional captcha types (slider/click etc.) without touching call sites.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `General/` | SkiaSharp-rendered char captcha plus 5-minute Redis cache of the answer (see `General/AGENTS.md`) |

## For AI Agents

### Working in this directory
- New captcha kinds should keep their own subfolder and expose an interface following the `IGeneralCaptcha` shape so DI can register them as `ITransient`.
- Cache keys live under `CommonConst.CACHEKEYCODE` ("vercode_") — reuse, do not invent a parallel prefix.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
