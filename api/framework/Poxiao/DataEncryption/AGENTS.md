<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DataEncryption

## Purpose
Standalone, dependency-free crypto utility module: AES, DES, MD5 and RSA helpers exposed as static classes plus fluent `string` extensions. Used across the LIMS backend for password hashing, payload encryption and signed-token generation; kept inside the framework so feature modules never need to reach for `System.Security.Cryptography` directly.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Encryptions/` | `AESEncryption`, `DESCEncryption`, `MD5Encryption`, `RSAEncryption` static helpers (see `Encryptions/AGENTS.md`). |
| `Extensions/` | `StringEncryptionExtensions` fluent wrappers (see `Extensions/AGENTS.md`). |

## For AI Agents

### Working in this directory
- These helpers are intentionally framework-internal — feature modules should call them via the `string` extensions (`text.ToMD5Encrypt()`, `.ToAESEncrypt(key)`, etc.), not new up algorithm classes themselves.
- Keep XML doc comments in Chinese to match repo convention; UI / log strings stay Chinese.
- All public types carry `[SuppressSniffer]` so they're hidden from the framework's API scanner — preserve when adding new helpers.

### Common patterns
- Algorithm class with parallel `Encrypt(text, key)` / `Decrypt(hash, key)` static signatures; MD5 also exposes `Compare(text, hash)`.
- `using` over `IDisposable` algorithm objects (`Aes.Create()`, `RSACryptoServiceProvider`).

## Dependencies
### External
- `System.Security.Cryptography`, `System.Text`, `System.Buffers.Text`, `System.Runtime.CompilerServices` (for `Unsafe.CopyBlock`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
