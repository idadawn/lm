<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# utils

## Purpose
mobile 端通用工具模块。封装与 uni-app 平台相关的 HTTP、SSE、存储、版本更新、日期/MD5 等公共能力，给 `pages/`、`components/`、`api/` 提供平台无关的薄封装层。

## Key Files
| File | Description |
|------|-------------|
| `http.js` | 基于 `uni.request` 的 Promise 封装：自动注入 `Authorization` token、`Poxiao-Origin: app` 头、form-urlencoded 序列化、统一错误处理 |
| `sse-client.js` | uni-app 兼容的 SSE 客户端（`enableChunked: true` + 手工 `data:` 分包），暴露 `streamNlqChat`，回调 `onText` / `onReasoningStep` / `onResponseMetadata` |
| `storage.js` | token / userInfo / API base URL 的存储读写 + `clearAuth` 一键登出 |
| `update.js` | 应用内更新入口：根据 `UPDATE_SOURCE` 切换"自建后端" 或 "蒲公英"通道，调用 `plus.runtime.getProperty` 获取当前版本 |
| `update-pgyer.js` | Pgyer 渠道的 `checkPgyerUpdate` 实现，与 `scripts/upload-to-pgyer.ps1` 配套使用 |
| `date.js` | 基础日期格式化（无第三方依赖） |
| `md5.js` | 纯 JS MD5 实现，登录密码加盐时使用 |

## For AI Agents

### Working in this directory
- 所有网络请求必须经过 `http.js`，便于统一注入 token 和拦截 401；不要直接调用 `uni.request`。
- SSE 解析仅识别 `data:` 行，跳过 `[DONE]` 标记；新增事件类型时同步更新 `mobile/types/reasoning-protocol.d.ts`。
- `API_BASE_URL` 常量是开发兜底，运行时优先读 `getApiBaseUrl()`（来自 `server.vue` 切换）。
- `update.js` 中的 `UPDATE_SOURCE` 切换需同时维护 `PGYER_CONFIG` 与后端 `/api/app/version` 协议。
- 微信小程序无原生 `EventSource`，所以 `sse-client.js` 不可被 `EventSource` 替换。

### Common patterns
- 全部使用 ESM `export`，不混用 CommonJS。
- 控制台日志带 `[HTTP Request]` / `[SSE]` 前缀方便过滤。

## Dependencies
### Internal
- `mobile/api/` 全量依赖 `http.js`
- `mobile/components/kg-reasoning-chain/` 依赖 `sse-client.js`

### External
- `@dcloudio/uni-app` 平台 API（`uni.request` / `uni.getStorageSync` / `plus.runtime`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
