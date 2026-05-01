<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.AI.Interfaces

## Purpose
AI 模块对外公共契约程序集。定义其它模块（如 lab、workflow、Web.Core）调用 AI 能力时所需的接口与传输 DTO，避免直接依赖具体实现。

## Key Files
| File | Description |
|------|-------------|
| `IAiService.cs` | 通用聊天接口：`Task<string> ChatAsync(string message, string? systemPrompt = null)` |
| `IAppearanceFeatureAnalysisService.cs` | 外观特性识别接口 + `AppearanceFeatureAnalysisResult` / `AIFeatureItem` 结果模型 |
| `Poxiao.AI.Interfaces.csproj` | 仅引用 `Poxiao.Common`（基础设施），保持轻量 |

## For AI Agents

### Working in this directory
- 此程序集仅放接口与 DTO，**禁止**引入实现类或 SqlSugar 依赖。
- 新增 DTO 使用 `System.Text.Json` 的 `[JsonPropertyName]`（不要混用 Newtonsoft），以匹配 LLM 输出反序列化习惯。
- 接口方法返回值使用结果包装对象（`Success` / `ErrorMessage` / 数据载荷），而不是抛异常或裸 `null`。

### Common patterns
- DTO 字段使用 `Name`/`Level`/`Category` 等业务名词，配合 `[JsonPropertyName("name")]` 等小写映射 LLM JSON。
- 命名空间统一 `Poxiao.AI.Interfaces`。

## Dependencies
### Internal
- `Poxiao.Common`（基础设施：日志、DI 标记）

### External
- `System.Text.Json` / `System.Text.Json.Serialization`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
