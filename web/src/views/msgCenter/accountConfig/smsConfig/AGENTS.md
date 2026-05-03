<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# smsConfig

## Purpose
短信账号配置页：维护短信服务商（阿里云/腾讯云等）的 AccessKey、签名、模板编号等。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicTable 列表 + 启用/禁用 + 操作列 |
| `Form.vue` | 短信账号表单 |

## For AI Agents

### Working in this directory
- 服务商 type 决定字段集合，前端按 type 切换 schema。
- 路由名 `msgCenter-accountConfig-sms`（按现有约定）。

### Common patterns
- 与 dingConfig 同构，只是 schema 不同。

## Dependencies
### Internal
- `/@/api/msgCenter/accountConfig`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
