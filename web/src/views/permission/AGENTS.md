<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# permission

## Purpose
权限管理模块：组织机构、岗位、角色、用户、用户组、分级管理员、权限批量授权、在线用户监控。所有子目录均为页面级 SFC，配 `/@/api/permission/*` 接口。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `authorize/` | 权限批量设置入口（步骤式授权弹窗） (see `authorize/AGENTS.md`) |
| `gradeManage/` | 分级管理员管理 (see `gradeManage/AGENTS.md`) |
| `group/` | 用户组管理 (see `group/AGENTS.md`) |
| `organize/` | 组织/部门管理 (see `organize/AGENTS.md`) |
| `position/` | 岗位管理 (see `position/AGENTS.md`) |
| `role/` | 角色管理与成员授权 (see `role/AGENTS.md`) |
| `user/` | 用户管理（含导入/导出/重置密码） (see `user/AGENTS.md`) |
| `userOnline/` | 在线用户监控与强制下线 (see `userOnline/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 各子目录共享 `BasicLeftTree`（左侧组织树）+ 中间 `BasicTable` 的两栏布局。
- 跨模块共用：`role/GlobalMember.vue` 被 `group/index.vue` 复用作为成员管理弹窗。
- `defineOptions({ name: 'permission-xxx' })` 用于 `keep-alive` 缓存。

### Common patterns
- 启用/禁用 `enabledMark` 标签 + `enabledMark==2` 的"锁定"状态（user 模块）。
- `useOrganizeStore` / `useBaseStore` 提供组织树和当前用户信息。

## Dependencies
### Internal
- `/@/api/permission/*`、`/@/store/modules/organize`、`/@/components/Tree`、`/@/components/Table`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
