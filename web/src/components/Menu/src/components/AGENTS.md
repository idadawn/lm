<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
菜单项渲染单元。拆分为「叶子项」「子菜单」「内容（图标 + i18n 标题）」三个最小组件，被 `BasicMenu` 递归组合使用。

## Key Files
| File | Description |
|------|-------------|
| `BasicMenuItem.vue` | 叶子菜单项；包装 `Menu.Item` 并通过 `MenuItemContent` 渲染图标与文本。 |
| `BasicSubMenuItem.vue` | 子菜单容器；负责递归 `children` 与生成 `SubMenu`。 |
| `MenuItemContent.vue` | 渲染单元；通过 `useI18n.t('routes.' + enCode.replace('.', '-'), fullName)` 输出多语言菜单名，并显示 `item.icon`。 |

## For AI Agents

### Working in this directory
- 三个组件共享 `props.ts` 中的 `itemProps` / `contentProps`，新增字段需在父级 props 文件统一声明。
- i18n key 规则固定为 `routes.<enCode 中 . 替换为 ->`，与 `src/locales/lang/**/sys/menu.ts` 保持同步。
- 不要在叶子项里再用 `BasicSubMenuItem`，否则会与递归判定冲突。

### Common patterns
- 使用 `useDesign('basic-menu-item-content')` 等独立 prefix，便于样式覆盖。

## Dependencies
### Internal
- `/@/components/Icon`、`/@/hooks/web/useI18n`、`/@/hooks/web/useDesign`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
