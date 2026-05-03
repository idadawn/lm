<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
授权子系统的内部常量容器。当前只有 `Penetrates.AppAuthorizePrefix = "<Poxiao.Authorization.AppAuthorizeRequirement>"`，被 `AppAuthorizeAttribute` 与 `AppAuthorizationPolicyProvider` 共同使用以编码/解析多策略名。

## Key Files
| File | Description |
|------|-------------|
| `Penetrates.cs` | `internal static` 常量：`AppAuthorizePrefix`；命名遵循 Furion 习惯（“贯穿/内部”） |

## For AI Agents

### Working in this directory
- 此处全部 `internal`，**业务侧不要引用**。
- 修改 `AppAuthorizePrefix` 会破坏既有 `Policy` 字符串解析逻辑——不要轻易改动。

### Common patterns
- “Penetrates” 文件作为内部常量与共享方法集中地，参考 Furion 框架命名习惯。

## Dependencies
- 被 `Authorization/Attributes`、`Authorization/Providers` 使用。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
