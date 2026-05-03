<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Icon

## Purpose
全站图标组件。提供三个统一入口：通用 `Icon` 组件（同时支持 SVG sprite 与 Iconify 在线图标）、纯 `SvgIcon` 组件、`IconPicker` 图标选择器。是路由菜单、按钮、空状态、文件类型徽标等场景的图标基础设施。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 桶文件：导出 `Icon` (默认)、`SvgIcon`、`IconPicker` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `data/` | Iconify 内置图标元数据 (`icons.data.ts`)（见 `data/AGENTS.md`） |
| `src/` | `Icon.vue`/`SvgIcon.vue`/`IconPicker.vue` 实现（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 使用 `Icon` 时若图标名以 `|svg` 结尾会路由到 `SvgIcon`（本地 sprite），否则走 Iconify (`@purge-icons/generated`)。
- 图标 prefix 默认 `'ant-design'`；切换图标集 (`'ion'`、`'mdi'`) 需要保证 `purge-icons` 构建时已打包。
- 路由菜单字段统一使用字符串图标名，避免直接 import 图标组件。

### Common patterns
- `<Icon icon="ion:apps-outline" />` (Iconify) 与 `<SvgIcon name="logo" />` (本地 sprite) 是两种主要使用方式。

## Dependencies
### Internal
- `/@/utils/is`、`/@/utils/propTypes`、`/@/hooks/web/useDesign`
### External
- `@purge-icons/generated`、`ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
