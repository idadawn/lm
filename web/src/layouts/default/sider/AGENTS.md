<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# sider

## Purpose
默认布局的左侧栏组件集合。包含基础侧栏 (`LayoutSider`)、双列「混合侧栏」(`MixSider`)、可拖拽宽度的 `DragBar`，以及侧栏宽度/折叠/触发器逻辑钩子。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | sider 入口：根据 `getIsMixSidebar` 决定渲染 `MixSider` 还是 `LayoutSider` |
| `LayoutSider.vue` | 标准侧栏：logo + `LayoutMenu` + 折叠 trigger + DragBar；与 `useLayoutSider` 联动响应式宽度/暗模式 |
| `MixSider.vue` | 混合模式：左侧模块条 (一级菜单图标) + 右侧二级菜单浮层；通过 `v-click-outside` 自动收起，支持悬停/点击两种触发 |
| `DragBar.vue` | 拖拽条：监听 mousemove 实时调整 `appStore.menuSetting.menuWidth`，受 `getCanDrag` 控制 |
| `useLayoutSider.ts` | 计算 sider 类名/宽度/触发器位置等响应式 helpers |

## For AI Agents

### Working in this directory
- Mix 模式下二级菜单是浮层（绝对定位），层级使用 `@layout-mix-sider-fixed-z-index`；新增浮层注意叠放。
- 拖拽宽度有上下限，参考 `appStore.getMenuSetting.menuWidth` 与 `SIDE_BAR_MINI_WIDTH/SIDE_BAR_SHOW_TIT_MINI_WIDTH`；不要在视图内自行约束。
- `MixSider` 通过 `mixSideHasChildren` 通知 `useMenuSetting` 调整内容宽度（`getCalcContentWidth` 引用之），改造时保持该信号。

### Common patterns
- 使用 `useDesign('xxx-sider')` 生成 `prefixCls`，BEM 类名 `${prefixCls}-module__item--active` 等。
- `v-click-outside` 指令封装在 `/@/directives/clickOutside.ts`。

## Dependencies
### Internal
- `/@/components/Application`（AppLogo）、`/@/hooks/setting/useMenuSetting`、`/@/store/modules/{app,permission}`、`/@/router/menus`、`/@/directives/clickOutside`。
### External
- `ant-design-vue` (`Layout.Sider`)、`vue-router`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
