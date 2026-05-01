<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# form

## Purpose
表单短链填报页。通过短链匿名访问发布的在线表单，内置密码解锁、扫码分享、`Parser` 渲染动态字段并提交至 `/@/api/onlineDev/shortLink` 创建数据。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 短链表单容器：密码门 (`formPassUse`) + 二维码气泡 + Parser 渲染 + 提交/重置按钮 |

## For AI Agents

### Working in this directory
- 密码使用 `encryptByMd5` 客户端哈希后 `checkPwd` 校验；不要明文传输。
- 提交调用 `createModel`（携带 `state.encryption` 用于服务端校验签名），成功后清空表单并刷新 `key` 以重置 `Parser`。
- 该页面无登录上下文，不可使用用户 Store；时间默认值通过 `dayjs` + `getDateTimeUnit` 计算。

### Common patterns
- `key` 数字 ref 强制重渲染 `Parser`（避免缓存）。
- `QrCode` 组件直接生成当前页 URL 的二维码作为分享入口。

## Dependencies
### Internal
- `/@/api/onlineDev/shortLink`
- `/@/components/{Popup,Qrcode}`, `/@/utils/{cipher,jnpf,factory/createAsyncComponent}`
- `/@/hooks/web/useMessage`
### External
- `ant-design-vue`, `dayjs`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
