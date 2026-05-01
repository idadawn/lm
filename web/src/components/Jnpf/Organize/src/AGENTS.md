<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
组织/部门/岗位/角色/分组/用户选择器 SFC 集合。每个组件以 `a-select` 折叠态显示已选，点击打开 `a-modal` 穿梭面板（左：树或列表 + 搜索，右：已选）。`UserSelect` 还支持 tab 切换"全部/当前组织/我的下属/系统变量"和按 `selectType=custom` 多源筛选。

## Key Files
| File | Description |
|------|-------------|
| `OrganizeSelect.vue` | 组织树穿梭，单/多选；支持 `auth`/`isOnlyOrg`，使用 `organizeStore` 缓存全量树 |
| `DepSelect.vue` | 部门选择器（基于 organizeSelector 子集，按 `ableDepIds` 过滤） |
| `PosSelect.vue` | 岗位选择器，基于部门下钻 |
| `GroupSelect.vue` | 用户分组选择器 |
| `RoleSelect.vue` | 角色选择器 |
| `UserSelect.vue` | 用户选择器（4-tab + 异步分页 + `currentUser` 系统变量），最复杂，~400 行 |
| `UsersSelect.vue` | 多用户选择器变体 |
| `props.ts` | `baseProps` + `organize/dep/pos/role/group/user/usersSelectProps`，按继承链扩展 `ableXxxIds` |

## For AI Agents

### Working in this directory
- 新选择器从 `baseProps` 派生，保留 `multiple`/`buttonType`/`selectType`/`hasSys` 等公共字段。
- `setValue` 必须二次校验 `props.value`（异步回调期间值可能已被清空）—参考 `UserSelect.setValue`。
- 触发表单校验统一调用 `formItemContext.onFieldChange()`；多选时 emit 数组、单选时 emit 标量。
- 系统变量 `currentUser` 仅当 `hasSys=true` 时出现在第 4 个 tab。

### Common patterns
- 树（`BasicTree` + `onLoadData` 异步）与列表（`ScrollContainer` + 滚动加载）双模式；`isAsync.value` 切换。
- 用 `useOrganizeStore` 缓存组织树/列表，避免重复请求。

## Dependencies
### Internal
- `/@/api/permission/{user,organize}`、`/@/store/modules/organize`、`/@/components/Tree`、`/@/components/Container`、`/@/components/Modal/src/components/ModalClose.vue`
### External
- `ant-design-vue`、`@ant-design/icons-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
