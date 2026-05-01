<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# web

## Purpose
浏览器侧 / UI 集成层 hooks。封装 antd `Modal`/`message`/`notification`、ECharts、I18n、剪贴板、水印、WebSocket、权限、页签操作、内容高度计算等通用能力，业务页面与 layouts 高频复用。

## Key Files
| File | Description |
|------|-------------|
| `useMessage.tsx` | 全局消息工厂：`createMessage`、`createConfirm` (含强制清理残留 mask 的 `forceCleanupConfirmModals`)、`createSuccessModal`/`createErrorModal` 等，统一图标与 i18n |
| `useECharts.ts` | echarts 实例管理：dark/light 主题切换、resize debounce、与 `useMenuSetting().getCollapsed` 联动重绘 |
| `useI18n.ts` | `useI18n` 包装，提供 `t`、动态 path 翻译 |
| `useDesign.ts` | 生成基于 `@namespace` 的类名前缀 (`prefixCls`)，配套 `design/var` 的 `.bem` |
| `useWebSocket.ts` | 长连接初始化与重连，对接消息/聊天/通知模块 |
| `useTabs.ts` | 多页签操作（关闭、刷新、关闭其他、跳转） |
| `useContentHeight.ts` | 计算可用内容区高度，用于表格/容器自适应 |
| `usePermission.ts` | 按钮/路由级权限校验、菜单过滤 |
| `useWatermark.ts` | 文字水印 canvas 绘制与 MutationObserver 防移除 |
| `useCopyToClipboard.ts` | 复制文本兼容封装 |
| `useTitle.ts` / `usePage.ts` / `useLockPage.ts` / `useFullContent.ts` | 文档标题、页面跳转、锁屏、全屏内容切换 |
| `useECharts.ts` / `useCalcProgress.ts` / `useSortable.ts` / `useScript.ts` / `useAppInject.ts` | 图表、进度、拖拽、外部脚本、应用注入 |

## For AI Agents

### Working in this directory
- `useMessage` 的 `forceCleanupConfirmModals` 是为修复 antd Modal.confirm 残留 DOM 的兼容代码，移除前要回归确认弹窗的关闭路径。
- `useECharts` 在暗黑模式或 collapsed 变化时会 dispose 重建，长生命周期组件请用返回的 `getInstance` 而非闭包外存储实例。
- `useDesign('xxx')` 产出 `jnpf-xxx`，请勿手写硬编码前缀。

### Common patterns
- TSX 与 TS 混用：含 JSX 的（如 `useMessage.tsx`）输出 antd 图标节点，纯逻辑使用 `.ts`。
- 钩子返回平面对象 `{ ... }`，便于解构；含资源的钩子提供 `dispose`/`removeXxx` 方法。

## Dependencies
### Internal
- `/@/hooks/setting/**`、`/@/hooks/event/**`、`/@/hooks/core/**`、`/@/store/**`、`/@/enums/**`。
### External
- `ant-design-vue`、`@ant-design/icons-vue`、`echarts`、`@vueuse/core`、`vue-i18n`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
