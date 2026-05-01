<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Requirements

## Purpose
授权策略的需求载体。`AppAuthorizeRequirement` 持有由 `[AppAuthorize]` 解析出的策略名数组，被 `AppAuthorizeHandler` 在 `AuthorizationHandlerContext` 里读取并按业务规则评估。

## Key Files
| File | Description |
|------|-------------|
| `AppAuthorizeRequirement.cs` | `sealed` 实现 `IAuthorizationRequirement`，构造函数 `params string[] policies`，`Policies` 公开只读 |

## For AI Agents

### Working in this directory
- 单一职责：仅持数据，不做校验。校验逻辑在 `Handlers/AppAuthorizeHandler` 派生类。
- 保持 `sealed`：派生会破坏 `IsAssignableFrom(typeof(AppAuthorizeRequirement))` 的快速判断。

### Common patterns
- POCO Requirement，`params` 构造便于策略数组传入。

## Dependencies
### External
- `Microsoft.AspNetCore.Authorization`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
