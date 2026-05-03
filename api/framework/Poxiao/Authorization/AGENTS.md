<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Authorization

## Purpose
策略授权框架：把多策略以 `[AppAuthorize("policy1","policy2")]` 形式落到方法/类/接口上，通过 `AppAuthorizationPolicyProvider` 即时构造策略，由业务侧继承 `AppAuthorizeHandler` 实现 `PipelineAsync`/策略管道。同时提供 `[SecurityDefine]` 用于规范化文档/资源 ID 标识。是 LIMS 检测室权限模块（`api/src/modularity/system`）的运行时基础。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[AppAuthorize]`、`[SecurityDefine]` 特性 (see `Attributes/AGENTS.md`) |
| `Extensions/` | DI 注册扩展 (`AddAppAuthorization`) (see `Extensions/AGENTS.md`) |
| `Handlers/` | 抽象 `AppAuthorizeHandler` (see `Handlers/AGENTS.md`) |
| `Internal/` | 内部前缀常量 `Penetrates` (see `Internal/AGENTS.md`) |
| `Providers/` | `AppAuthorizationPolicyProvider` 动态策略生成 (see `Providers/AGENTS.md`) |
| `Requirements/` | `AppAuthorizeRequirement` 持有策略数组 (see `Requirements/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 业务侧不要自定义 `IAuthorizationPolicyProvider`，统一通过 `services.AddAppAuthorization<TAuthorizationHandler>()` 注入，由 `AppAuthorizationPolicyProvider` 动态生成策略。
- `AppAuthorizeHandler.HandleAsync` 在未登录时主动 `SignoutToSwagger`——保留该副作用以兼容 Swagger UI。
- 策略名通过逗号拼接编码进 `Policy` 字符串，前缀为 `<Poxiao.Authorization.AppAuthorizeRequirement>`，**不要修改前缀**，会破坏既有授权策略解析。

### Common patterns
- 特性→Requirement→Handler 标准 .NET 授权三件套，但用前缀字符串实现“按需策略”。
- `[SuppressSniffer]` 普遍标注，避免被 DI 扫描误注册。

## Dependencies
### Internal
- `App`、`AspNetCore/Extensions`（`SignoutToSwagger`）
### External
- `Microsoft.AspNetCore.Authorization`、`Microsoft.AspNetCore.Mvc.Authorization`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
