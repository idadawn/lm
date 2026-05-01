<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# setting

## Purpose
项目设置抽屉 (SettingDrawer)。提供主题色、菜单/头部/Tabs/过渡/内容模式等运行时配置面板，所有变更通过 `baseHandler` 派发到 `appStore` 并落盘到本地缓存 `PROJ_CFG_KEY`。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 入口包装，`<SettingDrawer />` 通过 `BasicDrawer` 弹出 |
| `SettingDrawer.tsx` | TSX 实现的设置面板：组合 `TypePicker`/`ThemeColorPicker`/`SwitchItem`/`SelectItem`/`InputNumberItem` 等，分组展示导航/外观/动画/多页签/系统主题色 |
| `enum.ts` | `HandlerEnum`（变更指令）与各下拉选项 (`contentModeOptions`/`topMenuAlignOptions`/`menuTypeList` 等) |
| `handler.ts` | `baseHandler(event, value)`：基于 `HandlerEnum` 派发到 `useAppStore().setProjectConfig`，集中管理副作用（如重新生成菜单宽度） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 抽屉里的细颗粒度控件（TypePicker/ThemeColorPicker/SwitchItem 等） (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 新增可调字段：①`HandlerEnum` 增枚举；②`SettingDrawer.tsx` 添加 UI；③`handler.ts` 实现写入逻辑；④`/@/settings/projectSetting.ts` 默认值同步。
- 主题预设色取自 `/@/settings/designSetting`（HEADER_PRESET_BG_COLOR_LIST 等），调色板更新走 settings 而非硬编码。
- 该面板默认仅供超级管理员/调试，生产环境可通过 `useRootSetting().getShowSettingButton` 隐藏入口。

### Common patterns
- TSX 渲染，使用 `Divider` 分组；i18n 走 `useI18n().t` 全 layout key。
- 全部修改集中走 `baseHandler`，避免组件直接调 store。

## Dependencies
### Internal
- `/@/components/Drawer`、`/@/store/modules/app`、`/@/hooks/setting/**`、`/@/locales/useLocale`、`/@/settings/designSetting`、`/@/enums/menuEnum`。
### External
- `ant-design-vue` (`Divider`)、`vue`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
