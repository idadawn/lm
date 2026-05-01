<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# cache

## Purpose
Three-layer cache stack: an in-memory LRU-ish `Memory` map, a `WebStorage` wrapper that AES-encrypts values + supports TTL, and a `Persistent` namespace that aggregates the auth/userInfo/permission/multipleTab keys into a single composite local + session blob.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `createLocalStorage` / `createSessionStorage` factories with sensible defaults. |
| `memory.ts` | `Memory<K, V>` class with `get/set/remove/resetCache/clear`. |
| `storageCache.ts` | `WebStorage` impl: encryption (cipher), TTL (`expire`), prefix scoping, JSON safe. |
| `persistent.ts` | `Persistent` static — typed accessors for `TOKEN_KEY`, `USER_INFO_KEY`, `PERMISSIONS_KEY`, `LOCK_INFO_KEY`, `PROJ_CFG_KEY`, `MULTIPLE_TABS_KEY`; emits to a single composite key. |

## For AI Agents

### Working in this directory
- Encryption is via `/@/utils/cipher` AES (key/IV from `settings/encryptionSetting`); these are obfuscation, not security.
- Default TTL = `DEFAULT_CACHE_TIME` (7 days) — pass explicit number or `null` for no expiry.
- Adding a new cache key: extend `BasicStore` interface in `persistent.ts` so the typed getter/setter pair is generated.
- `Persistent` writes lazily; pass `immediate=true` if you need a synchronous flush (e.g. before `window.close`).

### Common patterns
- All `setLocal/Session` accept `immediate?: boolean` final arg controlling commit.
- Keep `omit` / `pick` (lodash-es) usage to maintain shape stability across versions.

## Dependencies
### Internal
- `/@/utils/cipher`, `/@/enums/cacheEnum`, `/@/settings/encryptionSetting`.
### External
- `lodash-es`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
