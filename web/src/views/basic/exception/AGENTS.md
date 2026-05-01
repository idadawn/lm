<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# exception

## Purpose
Generic exception page rendered when a route does not exist or the user lacks permission. Shows the project 404 illustration plus a "返回首页" button that pushes `/home`.

## Key Files
| File | Description |
|------|-------------|
| `Exception.vue` | Static 404 layout — image + message "抱歉，你访问的页面不存在或无权访问!" + return-home button; uses `slideUp` keyframes for entrance animation |
| `index.ts` | Re-exports `Exception` as the module default for easy router import |

## For AI Agents

### Working in this directory
- The component currently hard-codes a single 404 message. If different exception statuses (403/500/network) are needed, branch by route meta or props rather than creating new files here — keep this page generic.
- Asset path `../../../assets/images/404.png` is relative — if you move the file, update the path.
- Less variable `@primary-color` is resolved by Ant Design's Less loader; do not inline literal colors.

### Common patterns
- Pages re-exported via `index.ts` so router config can `import { Exception } from '/@/views/basic/exception'`.

## Dependencies
### Internal
- `web/src/assets/images/404.png`
### External
- `ant-design-vue` (`a-button`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
