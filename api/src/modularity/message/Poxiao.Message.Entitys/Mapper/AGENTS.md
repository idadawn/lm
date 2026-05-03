<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Mapper

## Purpose
Mapster `IRegister` 配置：实体 ↔ DTO/Model 字段差异映射。例如 `IMContentEntity.State → IMUnreadNumModel.unreadNum`、`UserOnlineModel → OnlineUserListOutput`。

## Key Files
| File | Description |
|------|-------------|
| `Mapper.cs` | 当前注册两组：IMContent→IMUnreadNum；UserOnline→OnlineUserList（多字段重命名） |

## For AI Agents

### Working in this directory
- 仅在字段重命名/类型转换时显式 `Map(...)`；同名字段交给 Mapster 自动匹配。
- 新增 DTO 与实体字段不一致时，先在此注册，避免在控制器内手写赋值。
- 类必须是 `public class Mapper : IRegister` 才会被 Mapster 启动扫描。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
