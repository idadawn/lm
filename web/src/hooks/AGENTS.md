<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
vben-admin 风格的 Composition API 复用钩子库。按职责分目录：组件级辅助、底层 Vue 工具、DOM 事件、项目设置访问、Web 交互。`composables/` 是业务侧封装，本目录侧重通用基础能力。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `component/` | `useFormItem`、`usePageContext` 等组件协作钩子 (see `component/AGENTS.md`) |
| `core/` | `useContext`/`useTimeout`/`useAttrs`/`useLockFn` 等纯 Vue 基础原语 (see `core/AGENTS.md`) |
| `event/` | DOM 事件相关：断点、resize、滚动、IntersectionObserver (see `event/AGENTS.md`) |
| `setting/` | 读写 Pinia `app` store 中的菜单/头部/根/Tabs/过渡/全局环境配置 (see `setting/AGENTS.md`) |
| `web/` | 浏览器/UI 工具：Message、ECharts、I18n、剪贴板、水印、WebSocket 等 (see `web/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 命名统一以 `useXxx` 开头，每个 hook 一个文件；不要在此目录写组件或视图。
- 跨目录调用顺序约定：`web/* → setting/* → core/event/component`，避免 setting/core 反向依赖 web。
- 修改 setting 类钩子的 getter 名称会波及 layouts/components 大量绑定，必须 grep 调用方一并更新。

### Common patterns
- 大多导出形如 `{ getXxx, setXxx }` 或 `[refLike, setter]` 的元组，通过 `computed`+`unref` 暴露只读响应式 ref。
- 通过 `tryOnUnmounted`/`onMountedOrActivated` 等管理副作用生命周期。

## Dependencies
### Internal
- `/@/store/modules/app`、`/@/enums/**`、`/@/settings/**`。
### External
- `vue`、`@vueuse/core`、`vue-i18n`、`echarts`、`ant-design-vue`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
