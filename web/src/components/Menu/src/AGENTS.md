<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`BasicMenu` 实现层。串联路由、菜单设置、i18n、open-keys 状态机，渲染水平 / 垂直 / inline 三种模式，支持 mixSider 分屏布局。

## Key Files
| File | Description |
|------|-------------|
| `BasicMenu.vue` | 菜单主体；使用 `useOpenKeys` 管理展开 keys，`handleMenuClick` 透传到 `emit('menuClick')`，`getMenus()` 解析跳转 realKey。 |
| `useOpenKeys.ts` | 维护 `openKeys` / `collapsedOpenKeys`，根据 `accordion` 与 mixSidebar 行为差异化处理。 |
| `props.ts` | `basicProps` / `itemProps` / `contentProps` 三套 prop 集合（见同目录其它 AGENTS.md 引用）。 |
| `types.ts` | `MenuState` 接口：`defaultSelectedKeys`、`openKeys`、`selectedKeys`、`collapsedOpenKeys`。 |
| `index.less` | `@{namespace}-basic-menu` 样式（dark/light、horizontal、collapsed 等）。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 菜单项、子菜单、内容渲染 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `setOpenKeys` 通过 `useTimeoutFn` 16ms 延迟调度（mixSidebar 模式同步），保留该时序避免折叠抖动。
- 修改路由跳转语义时关注 `getMenus().filter(o => o.path === key && o.type === 1)` 这段——`type=1` 代表「菜单」，`type≠1` 视为可跳转节点。
- `currentActiveMenu` 来自 `route.meta`，是手动指定激活菜单的关键路径。

### Common patterns
- `useDesign('basic-menu')` 提供 `prefixCls`，所有样式均通过 BEM 类名挂载。
- `accordion` 模式下保留单一展开链，否则 `uniq(...openKeys, getAllParentPath(...))` 累积展开。

## Dependencies
### Internal
- `/@/enums/menuEnum`、`/@/router/*`、`/@/hooks/setting/useMenuSetting`、`/@/hooks/core/useTimeout`
### External
- `ant-design-vue`、`vue-router`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
