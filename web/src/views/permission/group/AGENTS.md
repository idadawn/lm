<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# group

## Purpose
用户组 (Group) 管理：维护跨组织的用户分组（如项目组），支持启用/禁用、全局/局部组、成员管理。成员管理弹窗复用自 `role/GlobalMember.vue`。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`getGroupList`，启用状态 `a-tag`，集成 `Form` 与跨模块 `GlobalMember` 弹窗 |
| `Form.vue` | 新建/编辑表单：字段 `fullName`/`enCode`/`type`/`globalMark`/`organizeIdsTree`/`enabledMark`/`sortCode` |

## For AI Agents

### Working in this directory
- 成员弹窗直接 `import Member from '../role/GlobalMember.vue'`，不要复制重复实现。
- `globalMark` 控制是否为全局组（影响 `organizeIdsTree` 是否必填）。

### Common patterns
- `useOrganizeStore` 提供组织树数据。
- `BasicForm` + `useForm` schemas 驱动表单。

## Dependencies
### Internal
- `/@/api/permission/group`、`/@/store/modules/organize`、`../role/GlobalMember.vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
