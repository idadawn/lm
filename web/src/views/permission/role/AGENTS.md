<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# role

## Purpose
角色 (Role) 管理：左侧组织树 + 中间角色表，集成普通成员、全局成员、组织架构图、批量授权 (`AuthorizePopup`) 等弹窗。`GlobalMember.vue` 被 `group/` 模块复用。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`getRoleList`，注册 5 个弹窗（Form/OrgTree/Member/GlobalMember/AuthorizePopup） |
| `Member.vue` | 局部成员弹窗：树/异步滚动列表选择用户加入角色 |
| `GlobalMember.vue` | 全局成员弹窗：含"全部数据/当前组织/我的下属"三个 tab，被 `group/index.vue` 复用 |

## For AI Agents

### Working in this directory
- `AuthorizePopup` 与 `permission/authorize/` 同源（共用组件），通过 `objectType='Role'` 区分批量场景。
- `GlobalMember` 的修改会影响 `group/index.vue`，谨慎重命名/移动。
- `apiUrl + item.headIcon` 拼接头像。

### Common patterns
- `BasicLeftTree` + `searchInfo` 联动表格搜索。
- `BasicTree` 异步加载 `onLoadData`。

## Dependencies
### Internal
- `/@/api/permission/role`、`/@/api/permission/organize`、`/@/components/Tree`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
