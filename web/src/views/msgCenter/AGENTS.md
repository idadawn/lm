<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# msgCenter

## Purpose
"消息中心"模块：账号配置（钉钉/邮件/短信/Webhook/微信公众号/企业微信）、消息监控、消息模板、发送配置（含模板与测试发送）。对应后端 message 模块。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `accountConfig/` | 各类消息渠道账号配置 (see `accountConfig/AGENTS.md`) |
| `msgMonitor/` | 消息发送监控（一键清空/详情）(see `msgMonitor/AGENTS.md`) |
| `msgTemplate/` | 消息模板管理（含复制）(see `msgTemplate/AGENTS.md`) |
| `sendConfig/` | 发送配置（关联模板 + 测试发送）(see `sendConfig/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 子页面命名规范：`msgCenter-<feature>`。
- API 命名空间：`/@/api/msgCenter/<feature>`；多渠道账号共用 `/@/api/msgCenter/accountConfig`，按 type 区分。

### Common patterns
- BasicTable + Form + Detail 的标准三件套。
- `enabledMark` 字段统一：`1` 启用 / `0` 禁用，`a-tag` success/error 标记。

## Dependencies
### Internal
- `/@/api/msgCenter/*`, `/@/components/Table`, `/@/components/Modal`, `/@/components/Popup`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
