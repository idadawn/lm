<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# organize

## Purpose
组织机构 (Organize) 管理：树形维护公司/部门两级结构，支持新建公司、新建部门、成员浏览、批量删除。下拉新建按钮区分 `company` / `department` 两种实体。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`getOrganizeList` 树形展开，下拉新建公司/部门 |
| `Form.vue` | 公司表单 `BasicPopup`：使用 `jnpfOrganizeSelect` 选父级，支持 `isOnlyOrg` 限制 |
| `DepForm.vue` | 部门表单 `BasicModal`：选所属组织（含部门），独立于 `Form.vue` |
| `Member.vue` | 部门成员浏览弹窗：滚动列表展示用户头像/姓名/账号/组织 |

## For AI Agents

### Working in this directory
- 公司用 `BasicPopup`（全屏抽屉），部门用 `BasicModal`（普通弹窗）—— 不要混用。
- `jnpfOrganizeSelect` 通过 `auth` 属性走授权范围；`isOnlyOrg` 控制只显示组织节点（不显示部门）。
- 头像 URL 拼接 `apiUrl + item.headIcon`。

### Common patterns
- `useOrganizeStore` 同步本地组织缓存。
- `dayjs` 处理时间字段（如成立日期）。

## Dependencies
### Internal
- `/@/api/permission/organize`、`/@/api/permission/user`、`/@/store/modules/organize`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
