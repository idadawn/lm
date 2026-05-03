<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# layouts

## Purpose
应用整体布局壳子。根据路由 meta 选择 `default`（含侧栏/头部/页签/内容/特性 drawer）、`iframe`（外链页面 keep-alive 容器）、或 `page`（最简 RouterView 包装）。承载主题、锁屏、登录态超时、智能问数入口等横切 UI。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `default/` | 主框架：sider + header + content + tabs + footer + setting drawer + features (see `default/AGENTS.md`) |
| `iframe/` | 嵌入第三方/外链页面的 keep-alive 容器 (see `iframe/AGENTS.md`) |
| `page/` | RouterView + keep-alive + 过渡动画的页面级布局 (see `page/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 路由 `meta.frameSrc` 触发 `iframe/`；普通业务页走 `page/`；`default/` 是默认外壳，三者互斥，新增布局类型需在 `router/` 中显式声明。
- 全局浮层、SettingDrawer、SessionTimeoutLogin、XiaoMeiAssistant（智能问数入口）都注入自 `default/feature/`，不要散落在业务页内。
- 修改样式前先看 `web/src/design/var/index.less` 的 `@header-height`/`@multiple-height` 等变量，避免硬编码 px。

### Common patterns
- 组件大多通过 `createAsyncComponent(() => import(...))` 异步加载以减小首屏。
- 使用 `useDesign('xxx')` 计算 `prefixCls`，配合 `<style lang="less">` 中 `@prefix-cls: ~'@{namespace}-xxx';`。

## Dependencies
### Internal
- `/@/hooks/setting/**`、`/@/hooks/web/**`、`/@/store/**`、`/@/router/**`、`/@/enums/**`、`/@/utils/factory/createAsyncComponent`。
### External
- `ant-design-vue` (`Layout`/`BackTop`/`Dropdown` 等)、`vue-router`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
