<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
UnifyResult 默认结果数据契约与内部元数据载体。`RESTfulResult<T>` 是整个后端 API 对前端的标准响应壳；`UnifyMetadata` 是注册期的提供器映射记录。

## Key Files
| File | Description |
|------|-------------|
| `RESTfulResult.cs` | 公共 DTO：`code`、`msg`、`data`（泛型 T）、`extras`、`timestamp`（Unix 秒）。属性使用小写名以匹配前端约定。 |
| `UnifyMetadata.cs` | `internal sealed`：`ProviderName`、`ProviderType`、`ResultType`，存于 `UnifyContext.UnifyProviders` 字典。 |

## For AI Agents

### Working in this directory
- `RESTfulResult<T>` 字段命名为小写并被前端 `axios` 拦截器（`web/src/utils/http`）依赖；不要随意重命名或大小写化。
- 不要在 `RESTfulResult<T>` 上添加业务字段；扩展信息走 `extras`（通过 `UnifyContext.Fill/Take`）。
- `UnifyMetadata` 仅供模块内部使用，外部代码不应直接构造。

### Common patterns
- `timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()` 在 Provider 中统一填充。

## Dependencies
### Internal
- `UnifyContext`、`UnifyModelAttribute`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
