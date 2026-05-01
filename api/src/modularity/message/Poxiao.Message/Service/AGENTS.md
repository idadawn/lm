<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Service

## Purpose
消息模块全部 HTTP 服务与领域管理类。每个文件对应一个独立 API 控制器（DynamicApi）或后端协调器，负责站内信/IM/模板/账号/监控/短链/公众号等业务编排。

## Key Files
| File | Description |
|------|-------------|
| `MessageService.cs` | 系统消息 (`api/message`)：列表、未读、已读、公告 CRUD，使用 `MessageEntity` + `IMHandler` 推送 |
| `MessageManager.cs` | 消息中心协调类（`IMessageManager`）：组装 `MessageEntity` + `MessageReceiveEntity`，调用站内/邮件/短信/微信/钉钉/Webhook 渠道 |
| `SendMessageService.cs` | 发送配置 (`api/message/SendMessageConfig`)：管理 `MessageSendEntity` 与渠道-模板-账号关系 |
| `MessageTemplateService.cs` | 消息模板 CRUD（`MessageTemplateEntity` + 短信变量 + 模板参数） |
| `MessageAccountService.cs` | 账号配置（SMTP / 阿里云 / 腾讯云 / 钉钉 / 企微 / Webhook 凭据） |
| `MessageMonitorService.cs` | 消息发送监控记录（`MessageMonitorEntity`）查询/清理 |
| `ImReplyService.cs` | 聊天会话列表 (`api/message/imreply`)，未读统计 |
| `MessageDataTypeService.cs` | 消息分类/类型字典服务 |
| `ShortLinkService.cs` | 短链生成与跳转 (`api/message/ShortLink`) |
| `WechatOpenService.cs` | 微信公众号 OAuth 与模板消息 |

## For AI Agents

### Working in this directory
- 服务通过 `[ApiDescriptionSettings(Tag = "Message", ...)]` 标注；新增服务路由请保持 `api/message/[controller]` 前缀（`MessageService` 已是历史例外）。
- 业务级别副作用（实际下发消息）集中在 `MessageManager`；控制器只做参数校验、权限、读写仓储。
- 推送实时消息走 `IMHandler.SendMessage(...)`；离线/多端通过 `MessageReceiveEntity` 持久化。

### Common patterns
- 仓储 `ISqlSugarRepository<TEntity>` + Mapster 映射 + `WhereIF` 拼接查询；分页输出 `SqlSugarPagedList`。
- 配置读取：`App.GetConfig<MessageOptions>("Message", true)`。

## Dependencies
### Internal
- `Poxiao.Message.Entitys/Entity`、`Poxiao.Message.Interfaces`
- `Poxiao.Systems.Interfaces.Permission`（`IUsersService`、`IUserManager`、`IUserRelationService`）
- `Poxiao.Extras.Thirdparty.{Email,Sms,WeChat,DingDing}`、`Poxiao.RemoteRequest`
### External
- Senparc.Weixin.MP、Mapster、SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
