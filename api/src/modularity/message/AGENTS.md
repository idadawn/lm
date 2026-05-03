<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# message

## Purpose
消息中心模块根目录，包含 IM 即时通讯、系统通知/公告、消息模板、账号配置、监控、短链以及发送渠道（站内信、邮件、短信、微信公众号、钉钉、企微、Webhook）。本目录是 Poxiao.Message 三件套（实现 / 实体 / 接口）的容器，由 `Poxiao.API.Entry` 通过 DI 注入加载。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.Message/` | 服务实现项目，含 IM/消息/模板/账号/监控/短链/微信发送服务 (see `Poxiao.Message/AGENTS.md`) |
| `Poxiao.Message.Entitys/` | 实体、DTO、枚举、Mapper、Model 共享项目 (see `Poxiao.Message.Entitys/AGENTS.md`) |
| `Poxiao.Message.Interfaces/` | 服务契约（IMessageService/IMessageManager/IImReplyService/IShortLinkService）(see `Poxiao.Message.Interfaces/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 三件套结构必须保持：实现依赖 Interfaces 与 Entitys，Interfaces 仅依赖 Entitys。新增对外可注入的服务务必同时在 Interfaces 中加抽象。
- 第三方发送渠道走 `Poxiao.Extras.Thirdparty.*`（DingDing/Email/Sms/WeChat），不要在本模块内重新实现 SMTP/HTTP 调用。
- IM 实时推送通过 `IMHandler`（WebSocket）；短链入库走 `MessageShortLinkEntity`，不要直接拼前端 URL。

### Common patterns
- 服务类标注 `[ApiDescriptionSettings(Tag = "Message", ...)]` + `[Route("api/message/...")]`，由 DynamicApi 自动暴露。
- 仓储统一为 `ISqlSugarRepository<T>`，配置读取走 `App.GetConfig<MessageOptions>("Message", true)`。

## Dependencies
### Internal
- `api/src/modularity/system/Poxiao.Systems.Interfaces`（用户/用户关系/字典）
- `api/src/modularity/workflow/Poxiao.WorkFlow.Entitys`（流程跳转消息）
- `api/framework/Poxiao/*`（DI、SqlSugar、DynamicApi、IMHandler）
### External
- Senparc.Weixin.MP、SkiaSharp（二维码/图片）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
