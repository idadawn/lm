<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Assets

## Purpose
SpecificationDocument 模块的前端静态资源——定制版 SwaggerUI 与 MiniProfiler HTML 模板。这些 HTML 作为嵌入式资源被 `SpecificationDocumentBuilder` 在运行时读取并注入分组列表、登录、Server 列表等占位数据。

## Key Files
| File | Description |
|------|-------------|
| `index.html` | 定制 SwaggerUI 主页面（含分组下拉、登录入口、`#(Token)`/分组占位）。 |
| `index-mini-profiler.html` | 集成 MiniProfiler 性能面板的 SwaggerUI 变体。 |

## For AI Agents

### Working in this directory
- 这些 HTML 通过项目文件 `<EmbeddedResource>` 打包；新增/重命名需同步 `csproj` 与 `SpecificationDocumentBuilder` 中的资源名常量。
- 占位语法采用项目自有模板符号（`#(...)`），不是标准的 Razor/Mustache，修改时保持原符号。
- 不要在此目录放置二进制资源（图片/字体）——SwaggerUI 自身资源由 NuGet 包提供。

### Common patterns
- 单文件多区域：分组、登录、安全、Servers 全部在同一 HTML 中以脚本方式渲染。

## Dependencies
### External
- `Swashbuckle.AspNetCore.SwaggerUI`（运行时由本 HTML 引用其内嵌 JS/CSS）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
