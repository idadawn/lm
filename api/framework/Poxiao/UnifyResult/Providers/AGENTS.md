<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Providers

## Purpose
规范化结果提供器抽象与默认实现。`IUnifyResultProvider` 定义四个回调：`OnException` / `OnSucceeded` / `OnValidateFailed` / `OnResponseStatusCodes`，由 Filter 与 Middleware 调用，把不同输入塑形为统一响应。`RESTfulResultProvider` 是默认 RESTful 风格实现。

## Key Files
| File | Description |
|------|-------------|
| `IUnifyResultProvider.cs` | 提供器契约接口，业务可实现替换默认包装。 |
| `RESTfulResultProvider.cs` | 默认实现，输出 `RESTfulResult<object>`；带 `Poxiao_API` Header 时直接透传原始数据；401 → `code=600 + msg="登录过期,请重新登录"`；403 → `403 Forbidden`。 |

## For AI Agents

### Working in this directory
- 自定义提供器示例：`[UnifyModel(typeof(MyResult<>))] public class MyProvider : IUnifyResultProvider`，调 `services.AddUnifyProvider<MyProvider>("name")`，Action 上 `[UnifyProvider("name")]`。
- 默认实现里 `Poxiao_API` Header 是给前端/移动端"原始数据"调试通道用的；删除会破坏部分对接 SDK。
- 401 状态码映射 `code=600` 是和前端登录拦截器约定的魔术值，改动需同步前端 `web/src/utils/http`。

### Common patterns
- 静态工厂 `RESTfulResult(statusCode, succeeded, data, errors)` 集中构造响应；每次都 fill `extras = UnifyContext.Take()`。
- 通过 `UnifyContext.GetSerializerSettings(context)` 取按 `[UnifySerializerSetting("name")]` 命名的序列化设置。

## Dependencies
### Internal
- `RESTfulResult<T>`、`UnifyContext`、`UnifyResultSettingsOptions`、`Poxiao.DataValidation.ValidationMetadata`、`Poxiao.FriendlyException.ExceptionMetadata`。
### External
- `Microsoft.AspNetCore.Mvc.JsonResult`、`Microsoft.AspNetCore.Http`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
