<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enums

## Purpose
VirtualFileServer 文件提供器类型枚举。在 DI 解析的工厂委托 `Func<FileProviderTypes, object, IFileProvider>` 中作为分支键，区分物理盘提供器 vs 程序集嵌入资源提供器。

## Key Files
| File | Description |
|------|-------------|
| `FileProviderTypes.cs` | 两值枚举：`Physical`（args 期望 `string root` 路径）、`Embedded`（args 期望 `Assembly`），均带中文 `[Description]`。 |

## For AI Agents

### Working in this directory
- 添加新 Provider 类型（OSS / MinIO / FTP）须**同时**：在此枚举追加值、在 `../Extensions/VirtualFileServerServiceCollectionExtensions` 的 switch 中加 case、在 `FS.cs` 加便捷工厂方法。
- 不要复用现有枚举值的整数顺序——业务代码可能按数值序列化存储。

### Common patterns
- `[Description]` 用于 `EnumExtensions.GetDescription()` 输出中文展示，在 Swagger / 日志中可读。

## Dependencies
### Internal
- 由 `FS`、`Extensions/VirtualFileServerServiceCollectionExtensions` 引用。
### External
- `System.ComponentModel.DescriptionAttribute`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
