<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Tab-pane components mounted by `profile/index.vue`. Each implements one section of the user-self-service flow.

## Key Files
| File | Description |
|------|-------------|
| `UserInfo.vue` | 个人资料 form — account/manager/position/role readonly tab + editable name/gender/nation/native place/cert type, dictionaries from `genderOptions`/`nationOptions`/`certificatesTypeOptions`, validates with `state.form2Rule` |
| `Password.vue` | 修改密码 form — current/new/confirm password with MD5 hashing before submit |
| `JustAuth.vue` | 第三方账号绑定 / 解绑 (JustAuth) — list bound providers, click to bind/unbind via popup |
| `Authorize.vue` | 系统权限 read-only display of menus / buttons / data permissions for the user |
| `SysLog.vue` | 系统日志 — paged login/operation log table for the current user |

## For AI Agents

### Working in this directory
- `UserInfo.vue` emits `updateInfo` to its parent so the avatar/name in the chrome refreshes after save.
- `Password.vue` MUST hash with `encryptByMd5` before `updatePassword` (never send plaintext).
- These components rely on the parent `profile/index.vue` to inject the `user` prop / fetch user info — they do not call `getUserSettingInfo` themselves except to refresh.
- Use `jnpf-select` (custom global wrapper) for dictionary-backed selects, with `:fieldNames="{ value: 'enCode' }"` per project convention.

### Common patterns
- `<a-form ... :rules="state.form2Rule" ref="form2ElRef">` then `await form2ElRef.value.validate()` before API call.

## Dependencies
### Internal
- `/@/api/permission/userSetting`, `/@/utils/cipher`, `/@/store/modules/user`, `/@/hooks/web/useMessage`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
