<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# mine

## Purpose
uni-app "我的" (mine / profile) page for the LIMS mobile client. Shows the logged-in user card and a menu of account utilities — 个人资料 / 修改密码 / 接口配置 / 清除缓存 / 检查更新.

## Key Files
| File | Description |
|------|-------------|
| `mine.vue` | User card (avatar, `userName`, `organizeName`) plus menu items routed via `goProfile`, `goPassword`, `configApi`, `clearCache`, `checkVersion`; displays current `appVersion`. |

## For AI Agents

### Working in this directory
- All labels are Chinese (项目语言要求); keep the colored-icon pattern (`menu-icon` background + initial 资/密/接/清/更) for visual consistency.
- "接口配置" lets users override the backend host at runtime — keep this menu item gated to non-production builds if needed.
- Avatar fallback shows the first character of `userInfo.userName`; preserve that behavior when refactoring.

### Common patterns
- Menu rows use `<view class="menu-item" @click="...">`; arrows are `›` text characters (deliberate for cross-platform render).
- User data is read from a Pinia store (`userInfo.userName` / `userInfo.organizeName`); avoid duplicating fetch logic here.

## Dependencies
### Internal
- User Pinia store, app-config / version utility, uni storage.
### External
- uni-app runtime.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
