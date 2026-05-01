<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
多页签栏的细分子组件：折叠菜单按钮、单个页签内容容器、刷新按钮。

## Key Files
| File | Description |
|------|-------------|
| `FoldButton.vue` | 顶部全屏折叠/展开按钮：切换 `useRootSetting().getFullContent` 让内容区占满视口 |
| `TabContent.vue` | 单页签的展示与右键菜单容器，组合 `Dropdown` + `useTabDropdown`，处理 `home` tab 仅图标的特殊形态 |
| `TabRedo.vue` | 重新加载当前页签：刷新动画 + `useTabs().refreshPage()` |

## For AI Agents

### Working in this directory
- `TabContent` 的 `tabItem.name === 'home'` 判定与 `router/routes` 中首页路由 `name: 'home'` 强耦合，重命名首页时同步。
- 右键菜单项与 hover 触发统一通过 `useTabDropdown` 提供，子组件不要自己维护菜单项数组。
- 刷新按钮使用 `useTabs` 内部 `setLoaded(false)` + 触发 `<keep-alive>` 重渲染，不要直接 `location.reload()`。

### Common patterns
- 图标统一 `Icon icon="ion:..."`，配合 `useDesign` 类前缀。
- emit 事件简单，复杂状态走 `useMultipleTabSetting` / store。

## Dependencies
### Internal
- `/@/components/Dropdown`、`/@/components/Icon`、`/@/hooks/web/{useDesign,useI18n,useTabs}`、`/@/hooks/setting/{useRootSetting,useMultipleTabSetting}`、父级 `useTabDropdown`。
### External
- `vue`、`vue-router`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
