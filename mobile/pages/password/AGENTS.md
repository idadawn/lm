<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# password

## Purpose
uni-app 修改密码页面（移动端）。要求用户输入旧密码、新密码、重复密码及图形验证码，调用后端 `/api/system/account/password` 完成密码修改。属于 `mobile/pages/` 中的"我的-账号安全"子流程。

## Key Files
| File | Description |
|------|-------------|
| `password.vue` | 密码修改单页：表单 + 图形验证码刷新 + 提交逻辑（Vue 3 `<script setup>`） |

## For AI Agents

### Working in this directory
- 使用 uni-app 组件（`<view>` / `<input>` / `<image>`）而非纯 HTML，禁止引入仅浏览器可用的 DOM API。
- 验证码 `codeImgUrl` 通过附加时间戳防缓存，刷新逻辑保持点击图片即重置。
- 网络请求统一走 `@/utils/http.js`，token 由 `@/utils/storage.js` 管理；不要在本页直接读写 `uni.getStorageSync`。
- 表单校验在前端做基础非空与"两次密码一致"校验，强度校验交给后端返回的错误信息提示。
- 路由由 `pages.json` 注册，跳转使用 `uni.navigateTo` / `uni.navigateBack`，不要 push 新窗口栈以外的方式。

### Common patterns
- `ref` + `reactive` 维护表单状态，`onShow` 刷新验证码。
- 提交后通过 `uni.showToast` 反馈结果，成功后清空 token 并跳回登录。

## Dependencies
### Internal
- `@/utils/http.js`、`@/utils/storage.js`
- `mobile/api/` 中的账号相关接口

### External
- `vue` (3.x), `@dcloudio/uni-app`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
