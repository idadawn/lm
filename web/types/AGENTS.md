<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# types

## Purpose
Top-level (non-`src/`) ambient TypeScript declarations consumed at build time. Declares global helpers (`Recordable`, `Nullable`, `Fn`, JSX intrinsics), Vite env shape (`ViteEnv`), config shape used by `/#/config` virtual import, axios envelope types, store-related types, vue-router augmentation, and module wildcard shims.

## Key Files
| File | Description |
|------|-------------|
| `global.d.ts` | Global types: `Recordable`, `Nullable`, `DeepPartial`, `Fn`, `PromiseFn`, `__APP_INFO__`, `ViteEnv`, JSX namespace, vue JSX bridge. |
| `config.d.ts` | `ProjectConfig`, `MenuSetting`, `MultiTabsSetting`, `HeaderSetting`, `TransitionSetting`, `LocaleType`, `GlobConfig`. |
| `axios.d.ts` | `RequestOptions`, `Result`, `UploadFileParams`, `ErrorMessageMode`. |
| `store.d.ts` | `UserInfo`, `PermissionInfo`, `LockInfo`, `ErrorLogInfo`, `SysConfigInfo`, `ApiAddress`. |
| `vue-router.d.ts` | Augments `RouteMeta` with `title`, `icon`, `hideMenu`, `affix`, `ignoreAuth`, etc. |
| `module.d.ts` | Wildcard module shims (`*.png`, `*.svg`, `vue` JSX). |
| `index.d.ts` | Aggregator re-exports. |
| `utils.d.ts` | Misc utility type declarations. |

## For AI Agents

### Working in this directory
- These files use `declare global { ... }` — do not import from `src/` modules here, or you'll lose ambient behavior.
- `/#/config`, `/#/store`, `/#/axios` virtual paths point to this directory (configured in `tsconfig.json` paths) — never import via relative path.
- When adding a new env var, also extend `ViteEnv` in `global.d.ts` and `.env.*` files.
- Augmenting `RouteMeta` here is required for `meta.affix`, `meta.icon` etc. to typecheck in route module files.

### Common patterns
- Pure `.d.ts` ambient — no runtime emit; safe to reference anywhere.

## Dependencies
### External
- `vue` (typings), `vue-router` (typings).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
