<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ModelBinders

## Purpose
脱敏词汇模块的 ASP.NET Core 模型绑定器。当字符串入参贴有 `[SensitiveDetection('*')]`（或其他替换字符）时，绑定阶段自动调用 `ISensitiveDetectionProvider.ReplaceAsync` 替换敏感词，避免在控制器内再做处理。

## Key Files
| File | Description |
|------|-------------|
| `SensitiveDetectionBinder.cs` | `IModelBinder` 实现：从 `ValueProvider` 取值，缺省时调用 `bindingContext.DefaultAsync()`；找到 `[SensitiveDetection]` 后通过 DI 解析 Provider 进行替换并 `ModelBindingResult.Success(newValue)`。 |
| `SensitiveDetectionBinderProvider.cs` | `IModelBinderProvider` 实现：仅当模型类型为 `string` 且参数贴有 `[SensitiveDetection]` 且 `Transfer != default` 时返回 `BinderTypeModelBinder(typeof(SensitiveDetectionBinder))`。 |

## For AI Agents

### Working in this directory
- 仅处理"参数级别"的 `[SensitiveDetection]`（`metadata.Attributes.ParameterAttributes`）；DTO 内属性走 `[SensitiveDetection]` 校验回写路径，不走该绑定器。
- `Transfer == default` 时 Provider 不返回该 binder，让默认绑定器接管，避免空替换浪费。
- 该 Binder 不写 `ModelState` 错误，仅做"替换 + 成功"；如果需要校验失败语义，请保留校验路径不变。

### Common patterns
- 注册次序：`AddSensitiveDetection()` 中 `ModelBinderProviders.Insert(0, ...)` 已确保优先级。

## Dependencies
### Internal
- `ISensitiveDetectionProvider`、`SensitiveDetectionAttribute`。
### External
- `Microsoft.AspNetCore.Mvc.ModelBinding`、`Microsoft.AspNetCore.Mvc.ModelBinding.Binders`、`Microsoft.AspNetCore.Mvc.ModelBinding.Metadata`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
