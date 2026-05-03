<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# menu

## Purpose
默认布局的菜单视图与拆分逻辑。基于 `useMenuSetting` 与 `permissionStore` 的菜单数据渲染主菜单组件，并提供 `useSplitMenu` 处理 sidebar/top-menu/mix 拆分场景下的左右联动。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `LayoutMenu`：根据 isHorizontal/menuMode/splitType 渲染 `BasicMenu`，处理点击跳转、当前激活高亮、混合菜单顶部联动 |
| `useLayoutMenu.ts` | `useSplitMenu(splitType)`：监听路由与 split 类型变化，调用 `getCurrentParentPath`/`getChildrenMenus`/`getShallowMenus` 计算左侧菜单数组，节流 `useThrottleFn` |

## For AI Agents

### Working in this directory
- 菜单类型来自 `MenuTypeEnum`（SIDEBAR / MIX / MIX_SIDEBAR / TOP_MENU）和 `MenuSplitTyeEnum`，新增类型须同时扩展 `enums/menuEnum.ts`、`useMenuSetting`、SettingDrawer。
- `getMenus()` 等已基于权限过滤，不要在 view 层再做过滤。
- 切换 split 时菜单数据是异步的（`getChildrenMenus` 返回 Promise），更新时记得 `await`。

### Common patterns
- 通过 `useThrottleFn` 节流 `handleSplitLeftMenu`，避免频繁路由变化抖动。
- watch 双源 `[currentRoute.path, splitType]`，统一在一处计算，避免重复响应。

## Dependencies
### Internal
- `/@/router/menus`、`/@/router/types`、`/@/store/modules/permission`、`/@/hooks/setting/useMenuSetting`、`/@/hooks/web/useAppInject`、`/@/enums/menuEnum`。
### External
- `vue`、`vue-router`、`@vueuse/core` (`useThrottleFn`)。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
