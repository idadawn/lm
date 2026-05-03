<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Templates

## Purpose
轻量字符串模板与日志/控制台美化工具集。`TP` 提供形如"┏━━━ Title ━━━"装饰框、矩形居中包裹与 GBK 等宽对齐；`Extensions/` 下提供 `string.Render()` 用于 `{prop}` 与 `#(Config:Path)` 模板替换。被 `Poxiao.SensitiveDetection`、`Poxiao.TaskScheduler` 等多个模块复用。

## Key Files
| File | Description |
|------|-------------|
| `TP.cs` | 静态 `[SuppressSniffer]`：`Wrapper(title, description, items)` 解析 `##prop##: value` 项做对齐；`WrapperRectangle(lines, align, pad)` 居中/左/右对齐绘制矩形；GBK 编码计算字节宽度以适配中文等宽。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | `StringRenderExtensions`：`{prop}` / `#(Config:Path)` 双语法模板渲染 (see `Extensions/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `TP` 大量使用 `Encoding.GetEncoding("gbk")` 计算可见宽度——已通过 `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)` 注册；不要替换为 UTF8 字节计数，会破坏中文对齐。
- 模板项前缀正则为 `^##(?<prop>.*)?##[:：]?\s*(?<content>[\s\S]*)`，同时兼容中文冒号；改正则需同步调用方。
- 渲染语法见子目录：`{name}` 接收对象/字典；`#(Section:Key)` 直接读 `App.Configuration`。

### Common patterns
- `Lazy<Regex>` 缓存模板正则；`StringBuilder.AppendLine()` 流式拼接。

## Dependencies
### Internal
- `Poxiao.ClayObject.Extensions.ToDictionary`、`Poxiao.Extensions.IsRichPrimitive`、`App.Configuration`。
### External
- `System.Text.RegularExpressions`、`System.Text.CodePagesEncodingProvider`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
