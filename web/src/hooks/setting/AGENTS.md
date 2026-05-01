<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# setting

## Purpose
项目级运行时设置访问层。基于 Pinia `useAppStore` 暴露 `菜单 / 头部 / 多页签 / 过渡 / 根设置 / 全局 env` 等读写接口，layouts、SettingDrawer、布局相关组件统一通过这些钩子读取响应式配置。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `useGlobSetting()`：读取 `import.meta.env` 派生的全局变量（标题、apiUrl、shortName、上传地址、WebSocket、本地预览端口、DataV/报表前端路径等） |
| `useMenuSetting.ts` | 菜单全部计算属性 (`getCollapsed`/`getMenuType`/`getRealWidth`/`getCalcContentWidth` 等) 与 `setMenuSetting`/`toggleCollapsed` |
| `useHeaderSetting.ts` | 头部显隐、主题、固定、面包屑/搜索/通知等开关 |
| `useRootSetting.ts` | 根设置：暗黑模式、内容模式、loading、设置按钮位置、错误处理等 |
| `useMultipleTabSetting.ts` | 多页签开关、缓存策略 |
| `useTransitionSetting.ts` | 路由过渡动画配置 |
| `useDefineSetting.ts` | `defineSetting` 工具，便于在 SettingDrawer 内声明式定义可调字段 |

## For AI Agents

### Working in this directory
- 所有 getter 命名 `getXxx`（computed），setter 命名 `setXxxSetting(partial)`，不要直接改 `appStore.getXxxSetting` 字面量。
- `useGlobSetting()` 中带有 `isDevMode()` 分支的硬编码 dev 端口（30090 文件预览、8100 DataV、30000 报表）需在切换部署环境时确认。
- 修改 menu / header 相关 getter 名要全量替换 layouts 下绑定，否则 SettingDrawer 与 layout 行为会脱钩。

### Common patterns
- 通过 `computed(() => appStore.getXxxSetting.field)` 暴露细粒度只读 ref；写操作走 `appStore.setProjectConfig({ menuSetting: {...} })` 单一入口。

## Dependencies
### Internal
- `/@/store/modules/app`、`/@/enums/appEnum`、`/@/enums/menuEnum`、`/@/utils/env`、`/#/config` 类型。
### External
- `vue`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
