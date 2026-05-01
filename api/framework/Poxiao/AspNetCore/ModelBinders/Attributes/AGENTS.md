<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
模型绑定相关的特性，目前只有 `[FromConvert]`：业务参数加上后由 `FromConvertBinder` 接管，根据参数类型查表选择 `IModelConvertBinder` 进行转换。

## Key Files
| File | Description |
|------|-------------|
| `FromConvertAttribute.cs` | `[FromConvert]`：`AllowStringEmpty` 控制空串处理、`ModelConvertBinder` 显式指定转换器、`Extras` 透传额外数据、`Customize=true` 让框架完全跳过 |

## For AI Agents

### Working in this directory
- 命名空间 `Microsoft.AspNetCore.Mvc`——保持与 `[FromBody]`/`[FromQuery]` 同级，便于业务侧统一 `using`。
- 仅 `AttributeTargets.Parameter`，不允许多次叠加。

### Common patterns
- POCO 特性 + 框架内部反射读取属性。

## Dependencies
- 被 `AspNetCore/ModelBinders/Binders/FromConvertBinder.cs` 解析。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
