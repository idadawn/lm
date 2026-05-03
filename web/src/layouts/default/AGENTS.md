<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# default

## Purpose
后台管理默认布局根组件。组合 `LayoutFeatures`（锁屏/BackTop/SettingDrawer 注入）、`LayoutHeader`、`LayoutSideBar`、`LayoutMultipleHeader`、`LayoutContent` 与 `XiaoMeiAssistant`（实验室智能问数入口），并在挂载时初始化 WebSocket。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `DefaultLayout`：基于 `getShowSidebar`/`getIsMobile` 计算 `ant-layout-has-sider`，挂接 `useLockPage` 锁屏事件，`onMounted` 调用 `initWebSocket()` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `content/` | 主内容区与可见高度计算 (see `content/AGENTS.md`) |
| `feature/` | BackTop / 锁屏 / 设置抽屉 / 智能问数 (XiaoMei) 入口 (see `feature/AGENTS.md`) |
| `footer/` | 页脚（备案、版权等） (see `footer/AGENTS.md`) |
| `header/` | 顶部栏与 MultipleHeader 容器 (see `header/AGENTS.md`) |
| `menu/` | 侧栏/顶栏菜单组件与拆分逻辑 (see `menu/AGENTS.md`) |
| `setting/` | SettingDrawer 主题与功能开关面板 (see `setting/AGENTS.md`) |
| `sider/` | 侧栏、Mix 侧栏、拖拽条 (see `sider/AGENTS.md`) |
| `tabs/` | 多页签栏与右键菜单/折叠 (see `tabs/AGENTS.md`) |
| `trigger/` | 折叠触发器（header/sider 两形态） (see `trigger/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 这里是布局总装入口，不要写具体业务逻辑；新增横切 UI（FAB、通知）放 `feature/`。
- `useLockPage()` 返回的 `lockEvents` 必须 `v-bind` 在最外层 `<Layout>`，否则锁屏倒计时无法触发。
- WebSocket 在 `default` 挂载时启动；若需要登录页等也连接，需调整调用位置。

### Common patterns
- 用 `createAsyncComponent` 拆分 `LayoutFeatures` 与 `XiaoMeiAssistant` 异步加载，减少首屏体积。
- LESS 中通过 `~'@{namespace}-default-layout'` 动态拼装 BEM 前缀。

## Dependencies
### Internal
- `/@/hooks/setting/{useHeaderSetting,useMenuSetting}`、`/@/hooks/web/{useDesign,useLockPage,useAppInject,useWebSocket}`。
### External
- `ant-design-vue` (`Layout`)、`vue`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
