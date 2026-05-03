<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
脱敏词汇模块的标注入口。`[SensitiveDetection]` 是 `ValidationAttribute` 派生属性，可贴在属性或参数上：未指定 `Transfer` 时仅作校验，指定字符时配合模型绑定器进行自动替换。

## Key Files
| File | Description |
|------|-------------|
| `SensitiveDetectionAttribute.cs` | `ValidationAttribute`：通过 `validationContext.GetService(typeof(ISensitiveDetectionProvider))` 获取 Provider，执行 `VaildedAsync` 校验或 `ReplaceAsync` 替换；替换时直接通过反射回写对象属性。 |

## For AI Agents

### Working in this directory
- 单一 `Transfer` 字段（`char`），默认值 `default`(`'\0'`) 表示"仅校验"；配置非默认字符自动启用替换逻辑。
- Attribute 校验流程使用 `.GetAwaiter().GetResult()` 同步等待异步 Provider —— Provider 实现需保持非阻塞、不要在内部再 `Task.Run`。
- 错误消息可被多语言模块覆盖（`L.Text != null` 时走 i18n 字典），自定义 `ErrorMessage` 也兼容。
- 仅作用于 `string` 类型；非字符串/null/空白都直接 `ValidationResult.Success` 跳过。

### Common patterns
- 单字符串参数（query/route）通过 `ModelBinders` 替换，`IsValid` 中跳过避免重复处理。
- 对象属性（DTO）通过 `validationContext.ObjectInstance + ObjectType.GetProperty(...).SetValue(...)` 直接回写。

## Dependencies
### Internal
- `Poxiao.Localization.L.Text`、`ISensitiveDetectionProvider`。
### External
- `System.ComponentModel.DataAnnotations`、`System.Reflection`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
