<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Assets

## Purpose
友好异常模块的嵌入式静态资源（错误页 HTML 模板）。文件以嵌入资源 (`EmbeddedResource`) 方式编译进 `Poxiao` 程序集，被 `BadPageResult.ToString()` 通过 `Assembly.GetManifestResourceStream` 加载。

## Key Files
| File | Description |
|------|-------------|
| `error.html` | 通用错误页模板。包含占位符 `@{Title}` / `@{Description}` / `@{StatusCode}` / `@{Code}` / `@{CodeLang}` / `@{Base64Icon}`，由 `BadPageResult` 替换后写入响应体。 |

## For AI Agents

### Working in this directory
- 这是**资产/嵌入资源**目录。`.csproj` 必须以 `<EmbeddedResource Include="FriendlyException\Assets\*.html" />`（或全局 glob）登记。
- 修改占位符命名时同步更新 `BadPageResult.ToString()` 中的 `Replace` 调用 —— 二者通过 `nameof()` 弱耦合，但模板里仍是字符串硬编码。
- 资源路径为 `{AssemblyName}.FriendlyException.Assets.error.html` —— `BadPageResult` 用 `thisType.Namespace.Replace(nameof(Poxiao), "")` 推导，重命名命名空间会破坏。

### Common patterns
- 资源文件不参与代码 lint；保持轻量内联 CSS，便于邮件/日志直接展示。

## Dependencies
### Internal
- `BadPageResult`（`../Results/`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
