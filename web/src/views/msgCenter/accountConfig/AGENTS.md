<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# accountConfig

## Purpose
消息中心"账号配置"模块：每个子目录对应一种发送渠道的账号配置（钉钉、邮件、短信、Webhook、微信公众号、企业微信）。所有子页面共用 `/@/api/msgCenter/accountConfig` API（按类型区分）。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `dingConfig/` | 钉钉账号 (see `dingConfig/AGENTS.md`) |
| `emailConfig/` | 邮件账号（含 Test 发送）(see `emailConfig/AGENTS.md`) |
| `smsConfig/` | 短信账号 (see `smsConfig/AGENTS.md`) |
| `webhookConfig/` | Webhook 账号 (see `webhookConfig/AGENTS.md`) |
| `wxWananchiConfig/` | 微信公众号 (see `wxWananchiConfig/AGENTS.md`) |
| `wxWorkConfig/` | 企业微信账号 (see `wxWorkConfig/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 各子页面布局结构高度相似（BasicTable + enabledMark + 操作列 + Form），新增渠道时复制现有目录改 API/字段即可。
- 删除/复制接口共享 `delConfig`/`copy`，调用时传 `type` 区分。

### Common patterns
- `defineOptions({ name: 'msgCenter-accountConfig-<type>' })`。

## Dependencies
### Internal
- `/@/api/msgCenter/accountConfig`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
