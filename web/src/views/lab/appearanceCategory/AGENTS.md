<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# appearanceCategory

## Purpose
外观特性"大类"管理（如：韧性、脆边、麻点）。简单 CRUD 列表页，作为 `appearance` 页面左侧树的数据来源维护入口。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 标题/搜索/BasicTable + 编辑/删除按钮，"管理特性"按钮跳回 appearance |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 大类编辑弹窗 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- API 命名空间 `/@/api/lab/appearanceCategory`，查询走 `searchKeyword` 关键字模糊匹配。
- 删除前需 `popConfirm`；存在引用时后端会拒绝，前端展示后端错误。

### Common patterns
- Tailwind 工具栏 + 搜索栏 + BasicTable，无树形结构。

## Dependencies
### Internal
- `/@/api/lab/appearanceCategory`
- `/@/components/Table`, `/@/components/Modal`
### External
- `ant-design-vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
