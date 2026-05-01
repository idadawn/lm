<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
Fluent `string` and `byte[]` extension wrappers around the algorithm classes in `DataEncryption/Encryptions`. Lets feature code write `text.ToMD5Encrypt(uppercase, is16)` / `text.ToAESEncrypt(key)` / `text.ToAESCompare(hash, key)` instead of touching the underlying static classes directly.

## Key Files
| File | Description |
|------|-------------|
| `StringEncryptionExtensions.cs` | `[SuppressSniffer]` static class. MD5 family: `ToMD5Encrypt(uppercase, is16)` / `ToMD5Compare(hash, ...)` for both `string` and `byte[]`. AES family: `ToAESEncrypt(key)` / `ToAESDecrypt(key)`. (DES, RSA wrappers follow the same shape.) Each method delegates to the matching `*.Encryption` helper. |

## For AI Agents

### Working in this directory
- Keep extension names prefixed with `To` — convention is repo-wide and feature code greps for `.ToMD5Encrypt(`, `.ToAESEncrypt(`, etc.
- Default parameter values matter: `uppercase=false`, `is16=false` are baked into existing call sites' expectations; do not change defaults.
- New algorithm wrappers should mirror the existing pair shape (Encrypt + Compare/Decrypt) so call sites can swap implementations.

### Common patterns
- One static class fronting all algorithms; per-algorithm implementations live in `Encryptions/`.

## Dependencies
### Internal
- `DataEncryption/Encryptions/*.cs`.
### External
- None directly (BCL only via `Encryptions/`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
