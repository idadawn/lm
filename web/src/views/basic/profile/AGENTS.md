<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# profile

## Purpose
个人中心 — left-tabs page exposing 个人资料 / 修改密码 / 我的组织 / 我的岗位 / 我的下属 / 绑定设置 / 系统权限 / 系统日志. Pulls user info from `getUserSettingInfo` and supports avatar upload, primary organize / position switching (`setMajor`), and lazy-loaded subordinate tree.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tab host + avatar upload via `/userAvatar` endpoint with `Authorization` header, `changeMajor` reloads page after switching, subordinate `BasicTree` with `loadData` async children |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | Per-tab pane components (UserInfo, Password, Organize, Subordinate, JustAuth, Authorize, SysLog) (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Avatar upload uses `uploadUrl + '/userAvatar'`; on success calls `updateAvatar(file.response.data.name)` then `userStore.setUserInfo({ headIcon })`. Both the local UI and the global header avatar must be updated together.
- Tab "绑定设置" (`justAuth`) only shows when `localStorage.useSocials` is truthy — set during login config load.
- `changeMajor` triggers `location.reload()` after switching org/position — necessary because permission menu is fetched at boot.

### Common patterns
- Tabs lazy-render via `destroyInactiveTabPane`. Each pane is a separate component file under `components/`.
- `Empty` from Ant Design with `simpleImage` placeholder for empty organize/position lists.

## Dependencies
### Internal
- `/@/api/permission/userSetting` (`getUserSettingInfo`, `getSubordinate`, `updateAvatar`, `getUserOrganizes`, `getUserPositions`, `setMajor`), `/@/store/modules/user`, `/@/components/Tree`, `/@/utils/{auth,cache}`, `/@/hooks/{setting,web/useMessage}`
### External
- `ant-design-vue`, `@ant-design/icons-vue` (`CheckOutlined`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
