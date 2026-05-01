<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# UnifyResult

## Purpose
全局响应规范化模块。把所有 Web API 的成功值、模型验证失败、未捕获异常以及 4xx/5xx 短路状态统一包装为 `RESTfulResult<T>`（`{code, msg, data, extras, timestamp}`），并提供按控制器/方法关闭规范化、自定义提供器、自定义序列化等扩展点。是整个后端 Controller 返回结构的事实标准。

## Key Files
| File | Description |
|------|-------------|
| `UnifyContext.cs` | 静态门面：保存 `EnabledUnifyHandler` 开关、`UnifyProviders` 元数据字典；提供异常 `ExceptionMetadata` 解析、`Fill/Take` 附加数据、状态码改写、有效结果检查 (`CheckVaildResult`)。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | NonUnify / UnifyResult / UnifyProvider / UnifyModel / UnifySerializerSetting 标记 (see `Attributes/AGENTS.md`) |
| `Extensions/` | `AddUnifyResult`、`UseUnifyResultStatusCodes` 注册扩展 (see `Extensions/AGENTS.md`) |
| `Filters/` | `SucceededUnifyResultFilter` 成功路径过滤器 (see `Filters/AGENTS.md`) |
| `Internal/` | `RESTfulResult<T>` 结构体与 `UnifyMetadata` 内部元数据 (see `Internal/AGENTS.md`) |
| `Middlewares/` | 状态码（401/403/500 等）短路中间件 (see `Middlewares/AGENTS.md`) |
| `Options/` | `UnifyResultSettingsOptions` 配置（200 改写、状态码适配） (see `Options/AGENTS.md`) |
| `Providers/` | `IUnifyResultProvider` 与默认 RESTful 实现 (see `Providers/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Controller 默认应返回业务对象，由本模块包装；不要手工 `return new JsonResult(new { code, msg, data })`。
- 想跳过包装：方法/类加 `[NonUnify]`，或方法已声明 `ProducesResponseTypeAttribute`、或 OData 程序集均会自动跳过（见 `UnifyContext.CheckSucceededNonUnify`）。
- 自定义返回结构：实现 `IUnifyResultProvider` + `[UnifyModel(typeof(YourResult<>))]`，调用 `services.AddUnifyProvider<T>(name)` 注册，并在 Controller/Action 上加 `[UnifyProvider("name")]`。
- `UnifyResultSettingsOptions.Return200StatusCodes` 默认 `[401, 403]` 被强制重写为 200（前端拦截需求）；调整前确认前端 `axios` 拦截器逻辑。

### Common patterns
- `[SuppressSniffer]` 普遍标记，避免被自动扫描器收录为业务类型。
- 401 + 同时存在 `access-token`、`x-access-token` Header 时被改写为 403（避免 token 刷新窗口期误踢登录）。

## Dependencies
### Internal
- `Poxiao.FriendlyException`（`AppFriendlyException`、`IfExceptionAttribute`），`Poxiao.DataValidation`（`ValidationMetadata`），`Poxiao.Localization`（`L.Text`）。
### External
- `Microsoft.AspNetCore.Mvc.*`、`Microsoft.AspNetCore.Http`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
