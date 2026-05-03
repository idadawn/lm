<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Providers

## Purpose
脱敏词汇（敏感词）检测提供器抽象与默认实现。给框架其他模块提供敏感词查询、校验、替换三类能力，词库通过分布式缓存（`IDistributedCache`）+ 入口程序集嵌入资源 `sensitive-words.txt` 加载。

## Key Files
| File | Description |
|------|-------------|
| `ISensitiveDetectionProvider.cs` | 敏感词提供器接口：`GetWordsAsync` / `VaildedAsync` / `ReplaceAsync(text, transfer='*')`。 |
| `SensitiveDetectionProvider.cs` | 默认实现：从 `DISTRIBUTED_KEY="SENSITIVE:WORDS"` 缓存读词；若无则用 `Reflect.GetEntryAssembly()` 读取嵌入资源 `{AssemblyName}.sensitive-words.txt`，按 `\r\n` / `|` 切分；含命中位置统计与字符替换算法。 |

## For AI Agents

### Working in this directory
- 接口与实现保持单一职责，业务侧通过 `App.GetService<ISensitiveDetectionProvider>` 调用，不要在调用方内联词库逻辑。
- 自定义词源（数据库/HTTP）应实现 `ISensitiveDetectionProvider` 后替换 DI 注册，而不是修改默认实现。
- 词库文件需以嵌入资源形式打包到入口程序集，并保持 UTF-8（兼容 BOM）。
- `text.Render()` 表明输入会先做配置模板渲染（参见 `Templates/Extensions`），编辑替换算法时勿绕过该步骤。

### Common patterns
- 类标注 `[SuppressSniffer]`，避免被框架反射扫描误判为业务类型。
- 替换算法使用 `StringBuilder` 原地改写，按命中索引偏移逐字符替换为 `transfer`。
- 缓存键采用大写命名空间式字符串（`SENSITIVE:WORDS`）。

## Dependencies
### Internal
- `Poxiao.Reflection`（入口程序集获取）、`Poxiao.Templates.Extensions`（`Render` 模板拓展）。
### External
- `Microsoft.Extensions.Caching.Distributed`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
