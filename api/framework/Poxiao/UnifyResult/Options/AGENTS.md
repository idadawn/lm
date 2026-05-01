<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
UnifyResult 模块的配置承载（`IConfigurableOptions`），从 `appsettings*.json` 的 `UnifyResultSettings` 节点绑定。控制返回 200 的状态码白名单、状态码适配映射表，以及是否参与 MVC Controller（非 ApiController）规范化。

## Key Files
| File | Description |
|------|-------------|
| `UnifyResultSettingsOptions.cs` | `Return200StatusCodes` 默认 `[401, 403]`（写入响应时强制改写为 200）；`AdaptStatusCodes` 二维数组做 `[from, to]` 改写；`SupportMvcController` 默认 false（仅 ApiController 参与）。 |

## For AI Agents

### Working in this directory
- `Return200StatusCodes = null` 表示 **所有** 错误状态码都改写为 200（兼容前端只看 body.code 的旧策略）；调整需评估前端拦截器。
- `PostConfigure` 仅设默认值；新增字段记得在此补默认，避免 null 反向传播。
- 修改默认行为前确认 `web/src/utils/http/index.ts` 与移动端 `mobile/` 的处理保持一致。

### Common patterns
- 通过 `App.GetOptions<UnifyResultSettingsOptions>()` / `IOptions<...>` 注入获取。

## Dependencies
### Internal
- `Poxiao.ConfigurableOptions.IConfigurableOptions<T>`。
### External
- `Microsoft.Extensions.Configuration`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
