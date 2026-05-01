<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# General

## Purpose
常规验证码 — generates a 4-character alphanumeric PNG using SkiaSharp, draws random fonts (`宋体` on Windows, `Cantarell` on Linux) plus 6 interference lines, and caches the answer in Redis under `vercode_{timestamp}` for 5 minutes.

## Key Files
| File | Description |
|------|-------------|
| `IGeneralCaptcha.cs` | Single-method abstraction `CreateCaptchaImage(timestamp, width, height, length=4)`. |
| `GeneralCaptcha.cs` | `ITransient` implementation; uses `ICacheManager` + `Random.NextLetterAndNumberString` extension and writes the answer with `SetCode(timestamp, code, TimeSpan.FromMinutes(5))`. |

## For AI Agents

### Working in this directory
- The verification step lives in the auth/login service — this dir only renders + stores. Don't move comparison logic in here.
- The class is registered automatically via `ITransient`; no explicit `services.Add…` is required.

### Common patterns
- Encoding via `SKImage.Encode(SKEncodedImageFormat.Png, 100)` -> `MemoryStream.ToArray()`. Do not switch to `System.Drawing` — that breaks the Linux Docker image (`SkiaSharp.NativeAssets.Linux.NoDependencies` is the only image-stack package referenced).

## Dependencies
### Internal
- `Poxiao.Infrastructure.Manager.ICacheManager`, `Poxiao.Infrastructure.Const.CommonConst`, `Poxiao.Infrastructure.Extension.RandomExtensions`.

### External
- SkiaSharp.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
