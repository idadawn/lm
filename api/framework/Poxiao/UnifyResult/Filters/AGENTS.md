<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Filters

## Purpose
MVC Action 过滤器层规范化处理。在 Action 执行后接管返回值：对成功结果包装成 `RESTfulResult<T>`，对 `BadRequestObjectResult`（模型验证失败）走 `OnValidateFailed` 路径，对带状态码的非 2xx 结果转交状态码处理逻辑。

## Key Files
| File | Description |
|------|-------------|
| `SucceededUnifyResultFilter.cs` | `IAsyncActionFilter`，`Order=8888`；过滤 WebSocket、含状态码的 IStatusCodeActionResult、ViewResult/FileResult 等非数据返回；调用 `IUnifyResultProvider.OnSucceeded/OnValidateFailed`。 |

## For AI Agents

### Working in this directory
- Filter 顺序固定 `8888`，确保位于业务自定义 Filter 之后、异常 Filter 之前。
- 不要在此处添加业务逻辑；新增包装行为应放进 `IUnifyResultProvider` 实现。
- 401 同时存在 `access-token`、`x-access-token` 时改写为 403（与中间件保持一致），调整须双向同步。

### Common patterns
- 通过 `UnifyContext.CheckSucceededNonUnify(method, out provider)` 判断是否跳过；具体跳过条件参见 UnifyContext。
- 通过 `UnifyContext.CheckVaildResult(result, out data)` 抽取 `ContentResult`/`ObjectResult`/`JsonResult` 中的真实数据。

## Dependencies
### Internal
- `UnifyContext`、`IUnifyResultProvider`、`Poxiao.DataValidation.ValidatorContext`。
### External
- `Microsoft.AspNetCore.Mvc.Filters`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
