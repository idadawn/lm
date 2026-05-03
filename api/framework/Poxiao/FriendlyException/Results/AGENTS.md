<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Results

## Purpose
自定义错误页 `IActionResult`。把异常元数据渲染为内联 HTML 错误页，主要用于 Razor Pages 与未启用统一返回的 WebAPI/Controllers 在 500/401/403/404 等场景下的兜底响应。

## Key Files
| File | Description |
|------|-------------|
| `BadPageResult.cs` | 继承 `StatusCodeResult`。属性：`Title`、`Description`、`Base64Icon`（默认 SVG）、`Code`、`CodeLang`（默认 `json`）。提供静态便捷成员 `Status401Unauthorized` / `Status403Forbidden` / `Status404NotFound` / `Status500InternalServerError`。`ExecuteResult` 跳过 WebSocket 与 `Response.HasStarted`；`ToString()` 通过 `Assembly.GetManifestResourceStream` 加载 `Assets/error.html` 嵌入资源，并把 `@{Title}` 等占位符替换为实例属性。 |

## For AI Agents

### Working in this directory
- 通常**不要直接 `return new BadPageResult(...)`** —— 让 `FriendlyExceptionFilter` 根据 Razor Pages / WebAPI 分流自动选择；仅在自定义中间件/控制器需要返回错误页时才显式使用。
- 资源路径推导依赖 `Poxiao` 命名空间存在 —— 重命名根命名空间会破坏 `errorhtml` 路径计算。
- `Code` 字段会原样写入 `<pre><code>` 块，可能含敏感堆栈 —— 生产环境需评估是否暴露完整异常字符串（结合 `App.HostEnvironment.IsDevelopment()` 控制）。

### Common patterns
- 带 `[CodeLang = "txt"]` 的预设静态结果用于纯文本展示，HTML 上下文中通过模板渲染语法高亮。

## Dependencies
### Internal
- 嵌入资源 `Assets/error.html`、`Poxiao.Reflection.Reflect.GetAssemblyName`。
### External
- `Microsoft.AspNetCore.Mvc.StatusCodeResult`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
