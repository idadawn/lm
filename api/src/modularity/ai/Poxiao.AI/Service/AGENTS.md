<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Service

## Purpose
存放 `Poxiao.AI` 工程中的 AI 业务服务实现。当前包含两个无状态服务，统一通过 `IChatClient` 与 vLLM/OpenAI 兼容端点通信。

## Key Files
| File | Description |
|------|-------------|
| `AiService.cs` | `IAiService` 通用聊天：支持可选 system prompt，温度 0.7，异常返回中文友好降级文本 |
| `AppearanceFeatureAnalysisService.cs` | `IAppearanceFeatureAnalysisService` 外观特性结构化分析：基于 `Dictionary<string, List<string>>` 分类与严重程度 prompt 模板 |

## For AI Agents

### Working in this directory
- 新增服务文件统一以 `xxxService.cs` 命名，类需 `: ITransient` 实现接口。
- 不要直接 `new HttpClient` 或绕过 `Microsoft.Extensions.AI`，保持单一通道便于后续切换底层模型。
- prompt 文本均为中文，调整时注意保留原有 JSON 输出契约（字段名 `name` / `level` / `category`）。

### Common patterns
- 服务构造函数仅依赖 `IConfiguration`，通过 `configuration.GetSection("AI:Chat")` 获取端点配置。
- 所有公开方法均为 `async Task<T>`，对空入参返回带错误信息的结果对象而非抛异常。

## Dependencies
### Internal
- `Poxiao.AI.Interfaces`
- `Poxiao.DependencyInjection`（`ITransient` 标记接口）
- `Poxiao.Logging`

### External
- `Microsoft.Extensions.AI` / `.AI.OpenAI`
- `OpenAI` SDK

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
