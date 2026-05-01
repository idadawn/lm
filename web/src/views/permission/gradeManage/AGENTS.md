<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# gradeManage

## Purpose
分级管理员 (Grade Manager) 管理：为某用户授予对若干组织的"本层/下层/查看/管理"权限，实现非全局管理员的细粒度组织管理。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`getGradeManageList`，列含账号/姓名/性别等，支持新建/删除 |
| `Form.vue` | 编辑弹窗 (`width=900`)：选择管理员 + 组织树表，按行勾选 `thisLayerSelect` 等权限位 |
| `GradeUserSelect.vue` | 自研用户选择器：树/异步滚动列表，根据当前 admin 状态切换 `jnpf-user-select` |

## For AI Agents

### Working in this directory
- 是否使用 `jnpf-user-select` vs `grade-user-select` 由 `getIsAdmin` 决定（管理员看全量，非管理员看授权范围）。
- 组织表数据 `list` 以 `organizeId` 为 rowKey，`defaultExpandAllRows` 默认展开。
- `thisLayerSelect` 等字段值 `0/1/2/3` 分别表示未选/已选/强制选中/禁止。

### Common patterns
- `JnpfCheckboxSingle` 单复选 + `disabled` 状态展示继承权限。

## Dependencies
### Internal
- `/@/api/permission/gradeManage`、`/@/components/Modal`、`/@/components/Tree`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
