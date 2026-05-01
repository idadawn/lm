<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# position

## Purpose
岗位 (Position) 管理：在所选组织下维护岗位列表，支持启用/禁用、查看岗位成员。布局为左侧组织树 + 右侧岗位表的两栏视图。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 两栏布局：`BasicLeftTree`（按 `getDepartmentSelectorByAuth`）+ `BasicTable`（`getPositionList`） |
| `Form.vue` | 岗位编辑表单 |
| `Member.vue` | 岗位成员弹窗：列出绑定到该岗位的用户 |

## For AI Agents

### Working in this directory
- `searchInfo.organizationId` 由左侧树选择驱动表格刷新。
- 注意：左侧组织树仅展示当前用户授权范围内的部门 (`getDepartmentSelectorByAuth`)。
- `defineOptions({ name: 'permission-position' })` 用于路由缓存。

### Common patterns
- 与 `role/`、`user/` 共享 `BasicLeftTree` + 中间表 + 多个 `useModal` 的相同骨架。

## Dependencies
### Internal
- `/@/api/permission/position`、`/@/api/permission/organize`、`/@/components/Tree`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
