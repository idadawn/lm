<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Entity

## Purpose
消息模块的 SqlSugar 实体集合。覆盖系统消息、IM、模板、模板参数、账号、监控、收发记录、短链、微信用户、用户设备等。表名前缀 `BASE_MESSAGE_*` / `BASE_IM*` / `BASE_USER_DEVICE`。

## Key Files
| File | Description |
|------|-------------|
| `MessageEntity.cs` | `BASE_MESSAGE` 系统消息（type/flowType/category/remindCategory/sendConfigId/expirationTime） |
| `MessageReceiveEntity.cs` | 用户-消息收件关联（已读、收件人扩展） |
| `MessageSendEntity.cs` | 发送配置主表（关联模板与账号） |
| `MessageSendRecordEntity.cs` | 实际下发流水 |
| `MessageSendTemplateEntity.cs` | 发送配置-模板从表 |
| `MessageTemplateEntity.cs` | `BASE_MESSAGE_TEMPLATE_CONFIG` 模板（含微信跳转、小程序 AppId） |
| `MessageTemplateParamEntity.cs` | 模板参数（占位符）从表 |
| `MessageAccountEntity.cs` | `BASE_MESSAGE_ACCOUNT_CONFIG` 多渠道账号凭据（SMTP/阿里/腾讯/钉/企微/Webhook） |
| `MessageMonitorEntity.cs` | `BASE_MESSAGE_MONITOR` 发送监控记录 |
| `MessageDataTypeEntity.cs` | 消息数据类型字典 |
| `MessageSmsFieldEntity.cs` | 短信变量定义 |
| `MessageShortLinkEntity.cs` | 短链（PC/App 分别落库） |
| `MessageWechatUserEntity.cs` | 公众号 OpenId 与系统用户绑定 |
| `IMContentEntity.cs` | `BASE_IMCONTENT` 即时通讯内容（state 0/1 已读） |
| `ImReplyEntity.cs` | `BASE_IMREPLY` 会话索引 |
| `UserDeviceEntity.cs` | `BASE_USER_DEVICE` 个推 ClientId-用户绑定 |

## For AI Agents

### Working in this directory
- 实体大多继承 `CLDEntityBase`；`IMContentEntity` / `ImReplyEntity` 例外，使用 `OEntityBase<string>`（仅 Id + 租户）。
- 字段命名混合：旧表全大写带下划线（`F_FULLNAME`、`F_TENANTID`），新表混合大小写（`F_TenantId`、`F_LinkType`）。**必须**逐字段用 `[SugarColumn(ColumnName = "...")]` 写明，不要依赖默认推断。详见 `.cursorrules`。
- `[Tenant(ClaimConst.TENANTID)]` 仅放置在需要租户隔离的实体上；账号/模板/监控等普通业务实体仅靠 `F_TENANTID` 字段过滤。

### Common patterns
- 字符串主键 + 雪花 ID（`OEntityBase<string>`）；金额/排序/层级等可空字段用 `int?` / `long?` 表示「未设定」。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
