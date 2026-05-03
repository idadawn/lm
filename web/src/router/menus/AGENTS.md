<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# menus

## Purpose
Async menu accessors used by sidebar/breadcrumb components. Reads the dynamic `menuList` produced by `permissionStore.buildRoutesAction` and applies hidden-menu filtering, parent-path lookup, shallow/children slicing.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `getMenus`, `getShallowMenus`, `getChildrenMenus`, `getCurrentParentPath` — each returns the user's permitted menu tree filtered by `meta.hideMenu`. |

## For AI Agents

### Working in this directory
- All accessors are async to keep the shape consistent with role-based mode swapping (back-end / front-end menu source).
- `menuFilter` recurses into `children`; do not flatten before filtering.
- Use `getCurrentParentPath(currentPath)` for breadcrumb head, not direct route matching — this respects custom redirects.

### Common patterns
- Pure consumer of `usePermissionStore`; no side effects.

## Dependencies
### Internal
- `/@/store/modules/permission`, `/@/router/helper/menuHelper`, `/@/router/types`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
