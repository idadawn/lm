<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Menu

## Purpose
后台导航菜单组件。封装 `ant-design-vue` 的 `Menu`，叠加 i18n 路由名、混合侧栏（mix-sider）、分屏（split）、折叠展开同步、`beforeClickFn` 拦截等业务能力，是布局层 `LayoutSideBar` / 顶部菜单的核心控件。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 仅导出 `BasicMenu` 组件实例（无 `withInstall`，需局部引入）。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 菜单主体、props、open keys 管理 (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 菜单项层级数据结构来自 `/@/router/types` 的 `Menu`，字段使用 `path`、`enCode`、`fullName`、`children`、`type`、`icon`，新增字段须同步修改 `props.ts`。
- `enCode` 用于 i18n key（`routes.<enCode 替换 . 为 ->`），后端调整路由编码时需通知前端国际化资源。

### Common patterns
- 路由变化驱动选中态：`listenerRouteChange` 监听后调用 `setOpenKeys` + `selectedKeys`。

## Dependencies
### Internal
- `/@/router/menus`、`/@/router/helper/menuHelper`、`/@/hooks/setting/useMenuSetting`、`/@/logics/mitt/routeChange`
### External
- `ant-design-vue`、`vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
