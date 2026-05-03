<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Localization

## Purpose
检测室系统多语言（i18n）支持模块，封装 `Microsoft.Extensions.Localization`：提供 `L.Text`/`L.Html`/`L.TextOf<T>()` 全局静态门面、运行时切换语言（写入 `Cookie` + 修改当前线程 UI Culture），并修复 `DateTime.Now` 在不同 Culture 下格式不一致的问题。

## Key Files
| File | Description |
|------|-------------|
| `L.cs` | 静态门面，暴露 `IStringLocalizer/IHtmlLocalizer` 取值、`SetCulture(culture, immediately)`、`GetSelectCulture()`、`GetCurrentUICulture()`、`GetCultures()`（系统支持的语言列表）、`GetString<TResource>(propertyExpression)`（按属性表达式取多语言字符串）。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | DI 注册 + Razor View/数据注解多语言扩展 (see `Extensions/AGENTS.md`) |
| `Internal/` | `Penetrates` 内部公共方法（DateTime 修复等） (see `Internal/AGENTS.md`) |
| `Options/` | `LocalizationSettingsOptions` 配置选项 (see `Options/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 调用 `L.SetCulture` 时记住默认仅影响**下一次请求**；要立即对当前线程生效需 `immediately:true`。
- `L.SetCulture` 在 WebSocket 请求或响应已开始时会被静默跳过——不是 bug。
- 新建资源文件时遵循 `LanguageFilePrefix`（默认 `Lang`） + `AssemblyName` 命名约定，与 `LocalizationSettingsOptions` 一致。

### Common patterns
- `App.GetService<IXxxFactory>(App.RootServices)?.Create()` 取本地化器；通过 `Cookie` 持久化用户语言偏好。

## Dependencies
### Internal
- `Poxiao.App` 服务定位、`HttpContextExtensions.IsWebSocketRequest`。

### External
- `Microsoft.Extensions.Localization`、`Microsoft.AspNetCore.Localization`、`Microsoft.AspNetCore.Mvc.Localization`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
