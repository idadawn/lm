<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# user-dropdown

## Purpose
顶部右上角的用户头像下拉菜单。展示当前用户名/头像，提供个人中心、系统切换（多业务系统场景）、消息、退出登录等入口。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `UserDropDown`：基于 `userStore.getUserInfo` 渲染头像与名字，下拉项包含 profile / 多 systemIds 切换子菜单 / feedback / about / logout |
| `DropMenuItem.vue` | 单个下拉项展示原子组件（图标 + 文字 + tooltip） |

## For AI Agents

### Working in this directory
- `getUserInfo.systemIds.length > 1` 才显示「系统切换」子菜单，单系统部署时该入口隐藏。
- 头像 URL 拼装方式：`apiUrl + headIcon`（`headIcon` 是相对路径），更换 CDN/OSS 时注意规范。
- 退出登录调用 `userStore.logout()`，会清缓存并跳转登录页；勿在此组件直接清 token 以免漏掉路由侧守卫。

### Common patterns
- antd `Dropdown` + `Menu` + `SubMenu`，菜单项 click 通过 `key` 走 `handleMenuClick` switch。
- i18n 文案统一从 `layout.header.*` 取（zh-CN/en/zh-TW 三语）。

## Dependencies
### Internal
- `/@/store/modules/user`、`/@/hooks/setting/index`（apiUrl）、`/@/hooks/web/useI18n`、`/@/hooks/web/useDesign`。
### External
- `ant-design-vue` (`Dropdown`/`Menu`/`Avatar`)。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
