<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# header

## Purpose
默认布局的顶部栏。组合 Logo、折叠 Trigger、面包屑、横向菜单、搜索、错误日志、全屏、语言切换、用户下拉、消息抽屉、强制改密弹窗等。`MultipleHeader` 提供「固定头部 + 多页签」双层吸顶。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `LayoutHeader`：聚合 `useHeaderSetting`/`useMenuSetting`/`useLocale`，处理强制初始密码修改 (`updatePasswordMessage` + `getSysConfig`)，挂载 `ResetPwdForm` |
| `MultipleHeader.vue` | 多页签 sticky 容器：根据 `getShowMultipleTab` 决定是否吸顶 header + tabs |
| `index.less` | header 主样式：响应式断点、暗/亮主题、action 区间距等 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | header 内的功能小部件（用户下拉、消息、聊天、面包屑、全屏、关于…） (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `LayoutHeader` 在 `onMounted` 调用 `getSysConfig` 检查 `mandatoryModificationOfInitialPassword`，命中则强制弹出 `ResetPwdForm`。修改时不要绕过此校验。
- 横向菜单 (`getShowTopMenu`) 与 split 模式联动 (`MenuSplitTyeEnum.TOP`)；调试时优先确认 `useMenuSetting().getSplit`。
- 不在此处直接 import 业务页面，所有功能按钮应封装为 components/ 内的小组件。

### Common patterns
- props `fixed` 决定是否绝对定位悬停；class 使用 `${prefixCls}--fixed/--mobile/--theme` 变体。
- `createAsyncComponent` 异步加载 `UserDropDown`/`Breadcrumb`/`ErrorAction` 减少首屏体积。

## Dependencies
### Internal
- `/@/components/Application`（AppLogo/AppSearch/AppLocalePicker）、`/@/hooks/setting/**`、`/@/hooks/web/**`、`/@/api/system/sysConfig`、`/@/api/basic/user`、`/@/store/modules/user`。
### External
- `ant-design-vue` (`Layout.Header`)、`vue-i18n`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
