<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MessageAccount

## Purpose
消息渠道账号配置（SMTP/阿里云/腾讯云/钉钉/企微/Webhook）的查询、列表、详情与邮件发送测试 DTO。

## Key Files
| File | Description |
|------|-------------|
| `MessageAccountQuery.cs` | 列表查询（继承 `PageInputBase`：enabledMark / webhookType / type / channel） |
| `MessageAccountListOutput.cs` | 列表行 |
| `MessageAccountInfoOutput.cs` | 详情，包含全部渠道凭据字段（密码字段需后端脱敏） |
| `EmailSendTestQuery.cs` | 邮件配置发送测试入参 |

## For AI Agents

### Working in this directory
- 凭据类字段（SmtpPassword / AppSecret / Bearer / Password）严禁日志输出；列表场景下应屏蔽。
- 新增渠道字段先加 `MessageAccountEntity`，再加 InfoOutput，再加表单 DTO。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
