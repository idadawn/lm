<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# server

## Purpose
uni-app "服务器配置"页面（移动端）。允许用户切换后端 API 基地址（开发/测试/生产环境），实时检测连通性。是离线/多环境调试的关键入口，通常隐藏在"我的-关于"或登录页长按彩蛋中。

## Key Files
| File | Description |
|------|-------------|
| `server.vue` | 服务器地址输入 + 测试连接 + 保存切换；切换后自动登出强制重登 |

## For AI Agents

### Working in this directory
- API 基地址持久化到 `uni.getStorageSync('lm_api_base_url')`，由 `@/utils/storage.js` 中的 `getApiBaseUrl` / `setApiBaseUrl` 统一封装。
- "测试连接"通过 GET `<url>/health` 或类似轻量端点判断在线状态；超时阈值不少于 5 秒以兼容内网慢连接。
- 切换服务器后必须调用 `clearAuth()` 清空 token + userInfo，防止跨环境串号。
- URL 校验需允许 `http://` 与 `https://`、IP+端口形式（如 `http://192.168.x.x:10089`）。
- 切勿把默认服务器地址硬编码在本页，应从 `mobile/utils/http.js` 的 `API_BASE_URL` 兜底。

### Common patterns
- `ref` 状态：`currentUrl` / `serverUrl` / `isOnline` / `testing`。
- `onShow` 拉取当前配置渲染卡片，操作后调用 `uni.showToast` 反馈。

## Dependencies
### Internal
- `@/utils/storage.js`（`getApiBaseUrl` / `setApiBaseUrl` / `clearAuth`）
- `@/utils/http.js`

### External
- `vue` (3.x), `@dcloudio/uni-app`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
