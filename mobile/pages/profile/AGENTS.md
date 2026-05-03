<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# profile

## Purpose
uni-app "个人资料"页面（移动端）。展示账号、组织、岗位、角色、注册时间等只读信息，并支持修改头像、昵称、邮箱、电话等可编辑字段。对应"我的"Tab 中的资料编辑入口。

## Key Files
| File | Description |
|------|-------------|
| `profile.vue` | 个人资料单页：头像选择上传 + 账户/资料分区表单 + 保存提交 |

## For AI Agents

### Working in this directory
- 头像通过 `uni.chooseImage` + `uni.uploadFile` 上传，URL 返回后写回 `userInfo.headIcon`。
- 账户信息区为只读展示，编辑能力仅开放给"个人资料"区块；不要在只读字段上加 v-model。
- 字段名沿用后端响应（`creatorTime` / `prevLoginTime` 等），与 `mobile/api/` 中的接口 DTO 一一对应。
- 头像 URL 通过 `headIconUrl` computed 拼接 baseUrl + 时间戳；改造时保留防缓存策略。
- 修改成功后需 `setUserInfo` 同步本地缓存，避免其他页面读到过期信息。

### Common patterns
- `ref<UserInfo>` 单对象 + `computed` 派生展示字段。
- `onShow` 触发 `loadAccount()`，表单提交后再次拉取保证一致性。

## Dependencies
### Internal
- `@/utils/storage.js`（`getUserInfo` / `setUserInfo`）
- `@/utils/http.js`、`mobile/api/` 用户接口

### External
- `vue` (3.x), `@dcloudio/uni-app`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
