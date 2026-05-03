<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Handlers

## Purpose
Real-time WebSocket handlers. The single occupant — `IMHandler` — backs the IM (instant-messaging) channel: connection lifecycle, online-user list, message routing, unread badges for messages/notices/system/schedule, and GeTui (个推) APP push notifications.

## Key Files
| File | Description |
|------|-------------|
| `IMHandler.cs` | Inherits `WebSocketHandler` (`Poxiao.Extras.WebSockets`). Reads JWT claims from the `token` field of the inbound `MessageInput`, registers the client into the connection manager, then routes by `MothodType`: `OnConnection`, `SendMessage` (text/image/voice), `UpdateReadMessage`, `MessageList`, `HeartCheck`. Handles `LoginMethod.Single` vs `SameTime` for multi-device login. Loads tenant DB via `_sqlSugarClient.AddConnection(...)` + `ChangeDatabase(...)`. |

## For AI Agents

### Working in this directory
- The handler is bound by `Poxiao.Extras.WebSockets` registration — don't `services.AddTransient<IMHandler>` manually.
- New `MothodType` cases must update both this switch and the front-end `MothodType` enum (in `web/`).
- Single-login enforcement: when `tenant.SingleLogin == LoginMethod.Single`, the previous device receives `{ method: "logout" }` before the new connection takes over. Don't refactor this branch without coordinating with the mobile/uni-app client.
- IM image/voice payloads are routed under `/api/file/Image/IM/{name}` — keep that path stable.

### Common patterns
- Online-user cache key: `$"{CommonConst.CACHEKEYONLINEUSER}:{tenantId}"`.
- Unread badge counts query `MessageEntity` joined with `MessageReceiveEntity` for type 1 (notice), 2 (message), 3 (system), 4 (schedule).

## Dependencies
### Internal
- `Poxiao.WebSockets.WebSocketHandler` + `WebSocketConnectionManager`, `Poxiao.Message.Entitys` (IMContentEntity, ImReplyEntity, MessageEntity, MessageReceiveEntity), `Poxiao.Systems.Entitys.Permission.UserEntity`, `Poxiao.Infrastructure.Manager.ICacheManager`, `Poxiao.Infrastructure.Configuration.FileVariable`, `Poxiao.RemoteRequest.Extensions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
