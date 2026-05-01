<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# composables

## Purpose
Vue 3 composables (the modern `useX()` pattern). Smaller and more focused than the older project hooks under `../hooks/`. Currently houses two utilities used heavily by the lab data table views: per-cell color theming via dynamic CSS rules, and lab formula decimal-precision lookup.

## Key Files
| File | Description |
|------|-------------|
| `useColorStyles.ts` | Manages cell background colors by injecting a single `<style>` element rather than per-cell inline styles. `coloredCells` is a `ShallowRef<Record<string,string>>` keyed `rowId::field`; `watchEffect(flush:'post')` rebuilds the rules from the map and `getCellClass(rowId,field)` returns a deterministic class name (`cc-<rowId>-<field>`, sanitized). The class lookup is non-reactive so template renders create no extra dependencies. Style element is removed `onUnmounted`. |
| `useFormulaPrecision.ts` | Loads decimal-precision config from `/api/lab/intermediate-data-formula` once and caches it in module scope. Exports `getFieldPrecision(fieldName)` with three matching strategies: exact, case-insensitive, and prefix (e.g. `detection1` → `Detection`). Default precision = 4. Provides a `useFormulaPrecision()` composable that triggers `loadFormulaPrecision()` on first mount, with `loading` ref and `reloadFormulaPrecision`. |

## For AI Agents

### Working in this directory
- New composables go here; they should follow the `useX(): { state, action }` shape and clean up on `onUnmounted`.
- Module-level caches (like `precisionCache`, `loadingPrecision`) are intentional — they persist across components and avoid repeated API calls. Do not rewrite as Pinia stores unless cross-store coordination is needed.
- `useColorStyles` solves a perf problem: applying inline styles to thousands of table cells caused recursion. Keep `flush: 'post'` and the non-reactive `getCellClass` to preserve that property.
- HTTP calls use `defHttp` from `/@/utils/http/axios`.
- Comment style is bilingual (Chinese for business semantics, English for implementation notes) — preserve.

### Common patterns
- Cell composite keys use `::` separator (`rowId::field`).
- Composables don't import other composables here; if a dependency emerges, prefer extracting a shared util to `../utils/`.

## Dependencies
### Internal
- `/@/utils/http/axios` (`defHttp`).
### External
- `vue` (`watchEffect`, `onUnmounted`, `ref`, `onMounted`, `ShallowRef`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
