<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
UnifyResult 模块对外暴露的标记特性集合，用于在 Controller / Action 维度配置规范化包装、选择规范化提供器与序列化策略。所有特性均带 `[SuppressSniffer]`，命名空间多为 `Microsoft.AspNetCore.Mvc` 以便 Controller 直接使用。

## Key Files
| File | Description |
|------|-------------|
| `NonUnifyAttribute.cs` | 标记方法或类跳过规范化包装；用于文件下载、SSE、原生 Json 等场景。 |
| `UnifyResultAttribute.cs` | 派生自 `ProducesResponseTypeAttribute`；声明返回类型时自动包装为 `RESTfulResult<T>`，供 Swagger 展示。 |
| `UnifyProviderAttribute.cs` | 选择具名提供器（如对外接口使用 RESTful，内部回调使用 Plain 等）。 |
| `UnifyModelAttribute.cs` | 标记在 IUnifyResultProvider 实现类上，声明其包装的泛型结果类型，注册期通过反射读取。 |
| `UnifySerializerSettingAttribute.cs` | 按 name 选用预先 `AddUnifyJsonOptions` 注册的序列化配置（如不同接口需要不同 JSON 命名风格）。 |

## For AI Agents

### Working in this directory
- 添加自定义提供器时，类上必须同时挂 `[UnifyModel(typeof(...))]`；缺失会在 `AddUnifyProvider` 中 NRE。
- `[NonUnify]` 与 `[ProducesResponseType]` / `[UnifyResult]` 三者只需任一即可跳过成功路径包装；不要重复堆叠。
- `[UnifyResult(typeof(Foo))]` 在 `UnifyContext.EnabledUnifyHandler == true` 时会自动 `MakeGenericType` 包装到 `RESTfulResult<Foo>`，不要再手工写 `RESTfulResult<Foo>`。

### Common patterns
- `AttributeUsage` 为 Method 或 Class 级别；`UnifyResultAttribute` 允许 `AllowMultiple = true`（支持声明多状态码）。

## Dependencies
### Internal
- `UnifyContext`、`Poxiao.Extensions.HasImplementedRawGeneric`。
### External
- `Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute`、`Microsoft.AspNetCore.Http.StatusCodes`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
