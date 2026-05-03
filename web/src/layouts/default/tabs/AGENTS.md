<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# tabs

## Purpose
多页签栏。基于路由历史维护打开标签集合，支持 affix 固定页签、拖拽排序、右键菜单（关闭/关闭其他/关闭左/右/重新加载）和最近访问记忆。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `MultipleTabs`：渲染 antd `Tabs`，绑定 `tabStore.getTabList`，处理切换/关闭事件，集成 `FoldButton`/`TabRedo` |
| `useMultipleTabs.ts` | `initAffixTabs`：扫描所有路由 meta.affix 写入 tabStore；`useTabsDrag` 通过 `useSortable` 启用拖拽排序（受 `multiTabsSetting.canDrag` 控制） |
| `useTabDropdown.ts` | 计算每个 tab 的右键菜单项（关闭/关闭其他/关闭左/关闭右/刷新/affix 切换）并派发到 store |
| `types.ts` | `TabContentProps` 等类型 |
| `index.less` | 多页签视觉与暗色/亮色变体 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | tab 内子部件（FoldButton 折叠、TabContent 单页签、TabRedo 刷新） (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 持久化 key `MULTIPLE_TABS_KEY`（见 `enums/cacheEnum.ts`），改名会丢失用户已打开页签状态。
- `initAffixTabs` 模块级 `isAddAffix` 仅在该 hook 模块第一次执行时为 false；切换登录账户时若需要重置，请走 store 的 `resetState`。
- `useTabsDrag` 当前体内有 `return;` 直接短路（拖拽暂禁），如需启用先移除该早返。

### Common patterns
- store 中 tab 唯一性按 `path` + 部分 query；新增页签前通过 `tabStore.addTab` 而非自行 push。
- 通过 `useSortable` (Sortable.js 包装) 实现 DOM 顺序与 store 顺序同步。

## Dependencies
### Internal
- `/@/store/modules/multipleTab`、`/@/hooks/web/{useDesign,useSortable}`、`/@/settings/projectSetting`、`/@/utils/is`。
### External
- `vue`、`vue-router`、`ant-design-vue` (`Tabs`/`Dropdown`)。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
