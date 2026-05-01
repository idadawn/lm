<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Handlers

## Purpose
框架抽象的策略授权处理基类。`AppAuthorizeHandler` 实现 `IAuthorizationHandler.HandleAsync`，已认证用户进入 `AuthorizeHandleAsync` / `PipelineAsync` 执行业务策略校验，未认证则触发 Swagger 自动登出。LIMS 业务侧（如 `Lab.Authorization.JwtHandler`）继承本类完成 JWT、菜单按钮权限的细粒度判断。

## Key Files
| File | Description |
|------|-------------|
| `AppAuthorizeHandler.cs` | `abstract` 基类：`HandleAsync` 路由已认证/未认证；`PipelineAsync(context, httpContext)` 默认通过；派生类重写 `AuthorizeHandleAsync(context)` 实现策略 |

## For AI Agents

### Working in this directory
- 业务侧应**继承**而不是修改本基类；新策略请扩展派生类（一般在 `api/src/modularity/system` 或对应模块）。
- 重写 `AuthorizeHandleAsync` 时务必显式调用 `context.Succeed(requirement)` 或 `context.Fail()`，否则授权处于未决状态。
- 未登录默认调 `SignoutToSwagger`，保留该 UX 行为。

### Common patterns
- 模板方法：基类编排，派生类实现具体策略；`virtual` 钩子方便业务扩展。

## Dependencies
### Internal
- `AspNetCore/Extensions` 中 `SignoutToSwagger`/`HttpContext` 工具
### External
- `Microsoft.AspNetCore.Authorization`、`Microsoft.AspNetCore.Http`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
