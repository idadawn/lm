<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
多语言模块内部辅助类。集中处理 `RequestLocalizationOptions` 装配，以及多语言切换时常见的"`DateTime.Now` 在不同 Culture 下格式不一致"问题。

## Key Files
| File | Description |
|------|-------------|
| `Penetrates.cs` | `internal static`：`SetRequestLocalization` 把 `LocalizationSettingsOptions` 转写到 `RequestLocalizationOptions`（`SetDefaultCulture`/`AddSupportedCultures`/`ApplyCurrentCultureToResponseHeaders`）；`FixedCultureDateTimeFormat` 4 个重载，把指定 Culture 的 `DateTimeFormat` 强制对齐到 `LocalizationSettings.DateTimeFormatCulture`，避免英文区时间格式渗入中文界面。 |

## For AI Agents

### Working in this directory
- `Penetrates` 必须保持 `internal`——它是模块边界的私有实现。新增辅助方法也加 `internal static`。
- 修改 `FixedCultureDateTimeFormat` 时同时覆盖 `CultureInfo`/`RequestCulture` × `string`/`CultureInfo` 的 4 个重载，与 `L.SetCulture` 调用面对齐。

### Common patterns
- 帮助方法名以 `Fixed*`/`Set*` 开头表示意图——修复或装配。

## Dependencies
### Internal
- `LocalizationSettingsOptions`。

### External
- `Microsoft.AspNetCore.Builder`、`Microsoft.AspNetCore.Localization`、`System.Globalization`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
