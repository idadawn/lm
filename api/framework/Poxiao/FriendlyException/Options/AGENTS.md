<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
友好异常的 `appsettings.json` 配置选项 —— 通过 `IConfigurableOptions` 自动绑定 `FriendlyExceptionSettings` 与 `ErrorCodeMessageSettings` 配置节，控制运行期错误展示策略与外部错误码字典。

## Key Files
| File | Description |
|------|-------------|
| `FriendlyExceptionSettingsOptions.cs` | 配置节 `FriendlyExceptionSettings`。字段：`HideErrorCode`（隐藏 `[code]` 前缀，默认 false）、`DefaultErrorCode`、`DefaultErrorMessage`、`ThrowBah`（让 `Oh` 也按业务异常返回 400，默认 false）、`LogError`（默认 true）。`PostConfigure` 给定默认值。 |
| `ErrorCodeMessageSettingsOptions.cs` | 配置节 `ErrorCodeMessageSettings`。字段：`Definitions: object[][]`，每条形如 `[ "ErrorCode", "Message {0}", arg0, arg1, ... ]`。`Oops` 启动时收录并优先于枚举元数据。 |

## For AI Agents

### Working in this directory
- 通过 `services.AddConfigurableOptions<TOptions>()` 注册（已在 `FriendlyExceptionServiceCollectionExtensions.AddFriendlyException` 中完成）。
- `Definitions` 是数组的数组 —— 在 JSON 中务必保持每行至少 2 元素，否则被 `Oops.GetErrorCodeMessages` 中 `Where(u => u.Length > 1)` 过滤掉。
- 改这两个配置 **无需重启**，下次 `App.GetConfig` 会读取最新值；但 `Oops.ErrorCodeMessages` 是静态字段、`static` 构造时初始化，**热更新不会感知**。

### Common patterns
- 与多语言 `Poxiao.Localization.L` 协作：错误消息文本会先经 `L.Text[realErrorMessage]`，再 `string.Format`。

## Dependencies
### Internal
- `Poxiao.ConfigurableOptions.IConfigurableOptions<T>`、`Oops`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
