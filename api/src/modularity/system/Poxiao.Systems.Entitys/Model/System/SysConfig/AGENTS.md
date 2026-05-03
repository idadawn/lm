<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SysConfig (Model)

## Purpose
系统配置内部 Model。承载从 `BASE_SYSCONFIG` 表读出的 K-V 字段，配合枚举 `Enum/SysConfig.cs` 使用，避免散落字符串 Key。

## Key Files
| File | Description |
|------|-------------|
| `SysConfigModel.cs` | 系统配置 K-V 视图（Key、Value、Description、Group） |

## For AI Agents

### Working in this directory
- Key 必须使用 `Enum/SysConfig` 枚举值，新加配置 Key 同步加枚举项。
- 该 Model 通常在 `SysConfigService` 中通过 `IMemoryCache` 缓存，命中策略 Key+租户 ID。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
