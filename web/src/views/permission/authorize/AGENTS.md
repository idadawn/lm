<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# authorize

## Purpose
权限批量设置页：作为路由直接挂载，挂载即自动打开 `AuthorizePopup`，用于一次性给多个角色配置应用/菜单/按钮/列表/表单/数据等 6~7 步权限。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 路由壳，`onMounted` 即调用 `openAuthorizePopup(true, { id:'0', fullName:'权限批量设置', type:'Batch' })` |
| `AuthorizePopup.vue` | 全屏 `BasicPopup`，7 步 `a-steps`（应用/菜单/按钮/列表/表单/数据/选择角色），含 `BasicTree` 全勾选/全展开 |

## For AI Agents

### Working in this directory
- `objectType === 'Batch'` 时隐藏返回/取消按钮，强制完成所有步骤。
- 步骤导航通过 `activeStep` + `maxStep` 控制，`disabled` 防止跳步。
- 与 `role/index.vue` 共用同一 `AuthorizePopup`，但 `role` 走 `objectType='Role'` 路径。

### Common patterns
- `BasicTree` + 操作下拉菜单（全部勾选/取消全选/展开/折叠）。

## Dependencies
### Internal
- `/@/components/Popup`、`/@/components/Tree`、`/@/api/permission/authorize`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
