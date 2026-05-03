<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# user

## Purpose
用户 (User) 管理：左侧组织树 + 右侧用户表，覆盖新建/编辑、批量导入 (.xlsx 三步式)、导出 (字段勾选)、重置密码、第三方账号绑定查看、组织架构图等完整用户运维场景。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`getUserList` + 状态标签（启用/锁定/禁用），管理员行隐藏操作 |
| `Form.vue` | 用户编辑 `BasicPopup`：账号信息分组（默认密码 0000）+ 岗位/角色级联 |
| `ImportModal.vue` | 三步导入向导：上传文件 → 数据预览 → 导入数据 |
| `ExportModal.vue` | 导出弹窗：当前页/全部页 + 字段勾选 |
| `ResetPassword.vue` | 重置密码：`encryptByMd5` 加密提交 |
| `SocialsBind.vue` | 第三方服务绑定列表（解绑） |
| `OrgTree.vue` | 全屏组织架构图（`vue3-tree-org` 渲染） |

## For AI Agents

### Working in this directory
- 状态字段 `enabledMark`：`1=启用 / 2=锁定 / 3=禁用`，渲染颜色 `success/warning/error`。
- 管理员行 (`record.isAdministrator`) 必须隐藏 TableAction，避免误改。
- 重置密码必须经 `encryptByMd5` 才提交后端。

### Common patterns
- `useModal` 管理 6+ 子弹窗。
- 头像 URL = `apiUrl + record.headIcon`。

## Dependencies
### Internal
- `/@/api/permission/user`、`/@/api/permission/socialsUser`、`/@/api/permission/organize`、`/@/utils/cipher`
### External
- `vue3-tree-org`、`ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
