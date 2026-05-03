<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# content

## Purpose
默认布局的主内容容器。承载页面切换 (`PageLayout`)、加载遮罩、版权水印背景，并在 setup 阶段计算可用视口高度供子页面消费。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `LayoutContent`：`v-loading` 双开关 (`getOpenPageLoading && getPageLoading`)，注入 `appStore.getSysConfigInfo.copyright` 作为 `::before` 角标；高度 `calc(100vh - @header-height - @multiple-height)` |
| `useContentContext.ts` | `createContentContext`/`useContentContext`：通过 inject 共享内容区状态（高度、loading） |
| `useContentViewHeight.ts` | 计算并 provide 内容区可视高度，供表格/无限滚动等组件使用 |

## For AI Agents

### Working in this directory
- 不要在 `LayoutContent` 内放业务页面：业务页通过 `PageLayout` 渲染，这里只控制容器/loading/水印。
- 高度依赖 `@header-height`/`@multiple-height`（来自 `design/var/index.less`），如调整层级请同步样式变量。
- 背景 `loading-iframe.gif` 是兜底加载图，关闭后业务页面 mounted 即覆盖；自定义页面背景请在子组件内覆盖。

### Common patterns
- 通过 `useDesign('layout-content')` 生成 `jnpf-layout-content` 前缀，配 less 块作用域。
- `useContentViewHeight()` 在 setup 中调用一次，借 provide 让深层组件直接 inject。

## Dependencies
### Internal
- `/@/layouts/page/index.vue`、`/@/hooks/setting/{useRootSetting,useTransitionSetting}`、`/@/store/modules/app`、`/@/hooks/web/useDesign`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
