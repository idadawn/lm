<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dynamicPortal

## Purpose
可视化门户（VisualPortal）运行时承载页。根据路由 meta 中的 `relationId` 拉取门户配置，分支渲染拖拽布局门户（`PortalLayout`）、自定义页组件或外链 iframe，是首页/工作台之外的多门户多入口入口页。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 门户容器页：依据 `state.type` (0 布局门户 / 1 自定义页) 与 `linkType` 切换渲染，调用 `usePortal` 装载布局并支持运行时锁定与刷新 |

## For AI Agents

### Working in this directory
- 不要在此处实现具体小部件，门户卡片实现位于 `/@/components/VisualPortal/Portal/Layout`。
- 路由必须携带 `meta.relationId`，否则展示空数据占位图（`assets/images/dashboard-nodata.png`）。
- 自动刷新计时器由 `usePortal().clearAutoRefresh` 在 `onUnmounted` 清理，新增轮询逻辑需同样卸载。

### Common patterns
- 通过 `usePortal` 钩子集中管理门户状态（loading / layout / 类型 / URL），页面只做组件分发。
- 样式通过 `@import url('/@/components/VisualPortal/style/index.less')` 复用门户公共样式表。

## Dependencies
### Internal
- `/@/components/VisualPortal/Portal/Layout`
- `/@/views/basic/home/hooks/usePortal`
### External
- `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
