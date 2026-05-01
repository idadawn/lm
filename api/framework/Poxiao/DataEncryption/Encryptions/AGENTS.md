<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Encryptions

## Purpose
Concrete crypto algorithm implementations: AES (CBC + IV-prefixed Base64 output), DES, MD5 (16-/32-bit, upper/lower case, with compare), and RSA (XML-string keys, `RSACryptoServiceProvider`). All four are static, allocation-conscious helpers consumed via the parallel `DataEncryption/Extensions` fluent wrappers.

## Key Files
| File | Description |
|------|-------------|
| `AESEncryption.cs` | `Encrypt(text, skey)` / `Decrypt(hash, skey)` using `Aes.Create()`. Stores IV inline at the front of the cipher buffer and Base64-encodes in place via `Base64.EncodeToUtf8InPlace` + `Unsafe.CopyBlock`. |
| `DESCEncryption.cs` | DES symmetric encrypt/decrypt with caller-supplied key. |
| `MD5Encryption.cs` | `Encrypt`/`Compare` over `string` and `byte[]` with `uppercase` and `is16` (16-char) toggles. |
| `RSAEncryption.cs` | `GenerateSecretKey(keySize)` returning XML public/private key pair, plus `Encrypt(text, publicKey)` / `Decrypt(...)`. Validates `keySize ∈ [2048, 16384]` divisible by 8. |

## For AI Agents

### Working in this directory
- AES output format (IV ‖ ciphertext, Base64) is the implicit on-wire contract; don't switch to a different envelope without a migration plan — historical hashes won't decrypt.
- RSA uses `ToXmlString` / `FromXmlString`, which is .NET-only. If you ever need cross-language interop, add a PEM/PKCS variant rather than swapping the existing one.
- Don't add randomness to MD5 — `Compare` relies on deterministic output.

### Common patterns
- Static-only classes, `[SuppressSniffer]`, parallel `Encrypt`/`Decrypt` signatures with the key as the second arg.

## Dependencies
### External
- `System.Security.Cryptography`, `System.Text`, `System.Buffers.Text`, `System.Runtime.CompilerServices`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
