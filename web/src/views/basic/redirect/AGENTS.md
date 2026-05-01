<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# redirect

## Purpose
Internal "bounce" route used to force-refresh KeepAlive-cached pages or to re-route by `name`. The router pushes `/redirect/:path*` and this component immediately replaces the navigation with the real target, optionally restoring `_origin_params` JSON.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Reads `currentRoute.params` (`path`, `_redirect_type`, `_origin_params`) plus `query`; deletes meta keys, joins array path, then `replace({name|path, query, params})` |

## For AI Agents

### Working in this directory
- `path` is decoded from `Array<string>` → joined with `/`; the leading slash is preserved if originally present. Don't change without auditing every call site that builds redirect URLs (search the repo for `'redirect'`).
- `_redirect_type === 'name'` branches expect `_origin_params` to be a JSON string. Callers must `JSON.stringify(params)` before navigation.
- Cloning `params` into `_params` avoids mutating the read-only route object — keep this pattern when extending.

### Common patterns
- Used by `useTabs.refreshPage()` to flush KeepAlive caches.

## Dependencies
### External
- `vue-router` (`useRouter`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
