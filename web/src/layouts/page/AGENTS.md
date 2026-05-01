<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# page

## Purpose
页面级布局。提供 `RouterView` + `keep-alive` + 路由过渡动画的最小封装，负责把当前路由组件挂到内容区，并根据「多页签 + keep-alive 开关」动态构造 include 列表。也聚合 `FrameLayout` 用于嵌入 iframe 路由。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `PageLayout`：基于 `getOpenKeepAlive && getShowMultipleTab` 启用 `<keep-alive :include>`，include 取自 `tabStore.getCachedTabList`；异常路由回退到 `EXCEPTION_COMPONENT` |
| `transition.ts` | `getTransitionName({ route, openCache, cacheTabs, enableTransition, def })`：基于 keep-alive 命中和 `route.meta.transitionName` 决定动画类名 (`fade-slide` 等) |

## For AI Agents

### Working in this directory
- 缓存策略与 `tabStore.getCachedTabList` 强绑定；新增页面如不希望缓存，请在 `route.meta` 设置 `ignoreKeepAlive: true`，store 会自动从 cached 中剔除。
- 异常组件 `EXCEPTION_COMPONENT` 来自 `/@/router/constant`，避免业务直接 import 异常页。
- 想要禁用某些路由的过渡动画，调整 `useTransitionSetting` 或在 `meta.transitionName` 显式声明。

### Common patterns
- `<RouterView>` 默认插槽 `{ Component, route }` 解构，配合 `:key="route.name"` 强制重建非缓存场景。
- 过渡名优先级：keep-alive 已加载 > `route.meta.transitionName` > 传入 `def`。

## Dependencies
### Internal
- `/@/layouts/iframe/index.vue`、`/@/router/constant`、`/@/hooks/setting/{useRootSetting,useTransitionSetting,useMultipleTabSetting}`、`/@/store/modules/multipleTab`。
### External
- `vue`、`vue-router`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
