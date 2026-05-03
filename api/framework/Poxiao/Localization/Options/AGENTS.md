<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
多语言配置选项。绑定 `appsettings.json` 中的 `LocalizationSettings` 节点，被 `AddAppLocalization` 与 `Penetrates` 消费。

## Key Files
| File | Description |
|------|-------------|
| `LocalizationSettingsOptions.cs` | `IConfigurableOptions<>` 实现。字段：`ResourcesPath`（默认 `Resources`）、`SupportedCultures`、`DefaultCulture`、`LanguageFilePrefix`（默认 `Lang`）、`AssemblyName`（默认入口程序集名）、`DateTimeFormatCulture`（统一时间格式的标准语言）。`PostConfigure` 给所有字段设默认值，避免 null 引用。 |

## For AI Agents

### Working in this directory
- 任何新增字段必须在 `PostConfigure` 中设默认值——`Penetrates.SetRequestLocalization` 直接读取，不做空检查。
- `AssemblyName` 默认走 `Reflect.GetAssemblyName(Reflect.GetEntryAssembly())`；如果业务把资源文件放到独立程序集，必须在 `appsettings.json` 显式指定。

### Common patterns
- `IConfigurableOptions<TSelf>` + `??=` 默认值模式。

## Dependencies
### Internal
- `Poxiao.ConfigurableOptions.IConfigurableOptions<>`、`Poxiao.Reflection.Reflect`。

### External
- `Microsoft.Extensions.Configuration`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
