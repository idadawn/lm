<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# auth

## Purpose
Token + auth-cache facade. Routes get/set/clear of auth-related cache keys (`TOKEN_KEY`, `USER_INFO_KEY`, etc.) to either `Persistent.local*` or `Persistent.session*` based on `projectSetting.permissionCacheType`.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `getToken`, `getAuthCache`, `setAuthCache`, `clearAuthCache`. Storage destination decided once at module load via `CacheTypeEnum.LOCAL`. |

## For AI Agents

### Working in this directory
- Don't read tokens directly from `localStorage` elsewhere — always go through this module so the `LOCAL`/`SESSION` switch in project settings is honored.
- `BasicKeys` is the union of `cacheEnum` keys; use those constants instead of string literals.
- Consumed by axios interceptor (`utils/http/axios/index.ts`) and `store/modules/user`.

### Common patterns
- Thin pass-through; encryption/expiry behaviour is handled inside `cache/persistent`.

## Dependencies
### Internal
- `/@/utils/cache/persistent`, `/@/enums/cacheEnum`, `/@/settings/projectSetting`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
