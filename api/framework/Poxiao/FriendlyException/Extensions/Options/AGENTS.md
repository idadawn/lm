<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
`AddFriendlyException(Action<FriendlyExceptionOptions>)` 服务注册期选项 —— 与 `appsettings.json` 中的 `FriendlyExceptionSettingsOptions` 区分（前者代码注册期，后者运行期配置）。

## Key Files
| File | Description |
|------|-------------|
| `FriendlyExceptionOptions.cs` | POCO。当前唯一字段：`GlobalEnabled`（默认 `true`），决定是否通过 `services.AddMvcFilter<FriendlyExceptionFilter>()` 装载全局异常过滤器。 |

## For AI Agents

### Working in this directory
- 不要把运行期开关（隐藏错误码、ThrowBah 等）加在这里 —— 它们属于 `../Options/FriendlyExceptionSettingsOptions`（受 `IConfigurableOptions` 约束、随配置热更新）。
- 此目录是 `FriendlyException/Extensions/Options/`，与同级的 `FriendlyException/Options/` 分工明确：注册期选项放这里，运行期配置选项放那里。

### Common patterns
- `FriendlyExceptionServiceCollectionExtensions` 通过 `Action<FriendlyExceptionOptions>` 接受调用方覆盖。

## Dependencies
### Internal
- `FriendlyExceptionServiceCollectionExtensions`、`FriendlyExceptionFilter`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
