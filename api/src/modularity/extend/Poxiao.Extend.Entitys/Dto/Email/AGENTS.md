<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Email

## Purpose
邮件模块 DTO。覆盖邮箱配置、收件箱/已发送/草稿/星标列表、详情、发送、保存草稿、检查邮箱可用性等。

## Key Files
| File | Description |
|------|-------------|
| `EmailConfigUpInput.cs` / `EmailConfigInfoOutput.cs` | 邮箱 IMAP/SMTP 配置（含 SSL） |
| `EmailConfigActionsCheckMailInput.cs` | 测试邮箱连通性入参 |
| `EmailListQuery.cs` | 列表查询（startTime/endTime/type，继承 PageInputBase） |
| `EmailListOutput.cs` / `EmailInfoOutput.cs` / `EmailHomeOutput.cs` | 列表 / 详情 / 首页快览 |
| `EmailSendInput.cs` | 发件入参（收件人/主题/正文/附件） |
| `EmailActionsSaveDraftInput.cs` | 草稿保存 |

## For AI Agents

### Working in this directory
- `emailSsl` 在 DTO 中是 `int?/bool?` 之一，Mapper 已处理 `Ssl ↔ emailSsl` 重命名映射，新加 SSL 相关字段时同步 `Mapper/Mapper.cs`。
- `type` 字段约定（收件箱/已发送/草稿箱/星标）与服务侧字符串常量保持一致。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
