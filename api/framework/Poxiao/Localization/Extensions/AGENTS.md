<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
`Localization` 的 DI/Builder 接线层。`AddAppLocalization` 是项目启动接入多语言的唯一入口：注册 `IStringLocalizerFactory`、配置 `RequestLocalizationOptions`（语言列表、默认语言、时间格式修复），并解决 Razor 视图中文乱码（`HtmlEncoder.Create(UnicodeRanges.All)`）。

## Key Files
| File | Description |
|------|-------------|
| `LocalizationServiceCollectionExtensions.cs` | `AddAppLocalization(IMvcBuilder)`/`(IServiceCollection)` 重载；从 `LocalizationSettings` 配置节读取选项，调用 `Penetrates.SetRequestLocalization` 配置请求本地化；MVC 版本同时启用 `AddViewLocalization` 与 `AddDataAnnotationsLocalization`。 |
| `LocalizationApplicationBuilderExtensions.cs` | `IApplicationBuilder.UseAppLocalization()`：注入 `RequestLocalizationMiddleware` 到管道。 |
| `IStringLocalizerFactoryExtensions.cs` | 工厂帮助：默认按 `LocalizationSettings.LanguageFilePrefix + AssemblyName` 创建 Localizer。 |
| `IHtmlLocalizerFactoryExtensions.cs` | 同上，针对 `IHtmlLocalizerFactory`。 |
| `ILocalizerExtensions.cs` | `IStringLocalizer.GetString<TResource>(Expression)` 之类的便利 API。 |

## For AI Agents

### Working in this directory
- 命名空间故意定为 `Microsoft.Extensions.DependencyInjection`，让 `services.AddAppLocalization()` 在用户代码中无需额外 `using`。
- 新增配置项时同步在 `Options/LocalizationSettingsOptions.PostConfigure` 设默认值，否则首次启动会得到 `null`。

### Common patterns
- 重载链：`IMvcBuilder` 入口 → 内部转调 `IServiceCollection` 入口。

## Dependencies
### Internal
- `Localization.Penetrates`、`LocalizationSettingsOptions`；`Poxiao.ConfigurableOptions.AddConfigurableOptions`。

### External
- `Microsoft.AspNetCore.Mvc.Razor`（`AddViewLocalization`）、`System.Text.Encodings.Web`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
