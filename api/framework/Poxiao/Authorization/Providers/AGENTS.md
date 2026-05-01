<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Providers

## Purpose
动态授权策略提供器。`AppAuthorizationPolicyProvider` 拦截 `[AppAuthorize]` 编码出的 `Policy` 字符串（带 `Penetrates.AppAuthorizePrefix` 前缀），按需创建带 `AppAuthorizeRequirement` 的策略；其余策略名回退到默认 `DefaultAuthorizationPolicyProvider`。

## Key Files
| File | Description |
|------|-------------|
| `AppAuthorizationPolicyProvider.cs` | `internal sealed` 实现 `IAuthorizationPolicyProvider`：`GetPolicyAsync` 按前缀分流；其余委托给 `FallbackPolicyProvider` |

## For AI Agents

### Working in this directory
- 业务侧通过 `AddAppAuthorization` 自动注入本类，**不要直接 `new`**。
- 修改前缀分流逻辑前请同时核对 `AppAuthorizeAttribute.Policies` 的 setter/getter，两端必须对称。
- 保持 `internal sealed`：避免被外部派生破坏策略解析。

### Common patterns
- “前缀编码 + Provider 解析”模式，避免预先注册海量静态策略。

## Dependencies
### Internal
- `Authorization/Internal/Penetrates`、`Authorization/Requirements`
### External
- `Microsoft.AspNetCore.Authorization`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
