<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Mapper

## Purpose
扩展模块的 Mapster `IRegister` 实现。集中处理实体字段名与 DTO 字段名的映射差异，避免每个 service 单独配置。

## Key Files
| File | Description |
|------|-------------|
| `Mapper.cs` | `internal class Mapper : IRegister`：包含 EmailReceive→EmailHome/EmailList、EmailSend→EmailList、EmailConfig→ConfigInfo、OrderReceivable→OrderCollectionPlan 等映射 |

## For AI Agents

### Working in this directory
- 当前关注点主要是邮件 (`Subject↔fullName`、`Date↔fdate`、`SenderName↔sender`、`Read↔isRead`、`Ssl↔emailSsl`) 与订单收款计划 (`Abstract↔fabstract`、`ReceivableMoney→string`)。
- 新增映射保持与已有 fluent 风格一致；不要把代码移到具体服务里。
- 取消注释（如 `EmailConfigEntity → MailAccount`）前确认外部依赖是否仍引用。

## Dependencies
### Internal
- `../Dto/Email/`、`../Dto/Order/`、`../Entity/`

### External
- Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
