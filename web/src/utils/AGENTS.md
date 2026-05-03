<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# utils

## Purpose
Pure-ish utility layer for the web app — no Vue lifecycle, just helpers. Covers object / URL / route operations, type guards, color math, DOM/CSS, env detection, form validation, prop-types, mitt bus, JNPF (legacy code-gen) helpers, lab-specific unit conversion, and concrete subsystems (auth, cache, http, file, factory, helper, event).

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `noop`, `getPopupContainer`, `setObjToUrlParams`, `deepMerge`, `getRawRoute`, `openWindow`, etc. |
| `is.ts` | `isObject` / `isArray` / `isUrl` / `isString` / `isFunction` ... type guards. |
| `env.ts` | `isDevMode`, `getCommonStoragePrefix`, `getStorageShortName`. |
| `cipher.ts` | AES wrapper around `crypto-js` (used by persistent cache). |
| `color.ts` | `lighten` / `darken` / `colorIsDark` / `mixColor` for theme math. |
| `domUtils.ts` | Class / style / event listener helpers. |
| `dateUtil.ts` | dayjs format helpers. |
| `mitt.ts` | Local `mitt` factory wrapper. |
| `jnpf.ts` | Compatibility helpers for the JNPF online-dev stack (`toFileSize`, `toDateText`, etc.). |
| `formValidate.ts` | Common antd form rules. |
| `props.ts`, `propTypes.ts`, `types.ts` | Vue prop-type helpers. |
| `uuid.ts` | UUID v4 generator. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `auth/` | Token storage facade over `cache/persistent` (see `auth/AGENTS.md`). |
| `cache/` | LocalStorage / SessionStorage / in-memory cache layers (see `cache/AGENTS.md`). |
| `event/` | DOM resize / window events (see `event/AGENTS.md`). |
| `factory/` | Async-component factory with retry (see `factory/AGENTS.md`). |
| `file/` | Base64 conversion + browser download helpers (see `file/AGENTS.md`). |
| `helper/` | Tree / theme / tsx helpers (see `helper/AGENTS.md`). |
| `http/` | Axios instance + transforms — primary `defHttp` export (see `http/AGENTS.md`). |
| `lab/` | Laboratory unit conversion utilities (see `lab/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Keep utilities free of Pinia/Router imports where possible — break circular deps that bite at boot.
- The single import surface most code uses is `/@/utils` (re-exported from `index.ts`); add new helpers there only if widely shared.
- `jnpf.ts` is legacy compatibility — prefer dedicated helpers when adding new code.

### Common patterns
- Functions accept loose `any`/`Recordable` and narrow internally; type at call sites.
- Use `lodash-es` (`cloneDeep`, `mergeWith`, `unionWith`) over hand-rolled equivalents.

## Dependencies
### External
- `lodash-es`, `dayjs`, `crypto-js`, `mitt`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
