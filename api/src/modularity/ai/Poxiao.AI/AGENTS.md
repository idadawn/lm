<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.AI

## Purpose
AI 服务的具体实现工程。基于 `Microsoft.Extensions.AI` 抽象 + OpenAI 客户端连接 vLLM 推理端点（默认模型 `/data/qwen2.5-7b`），实现 `IAiService` 通用聊天与 `IAppearanceFeatureAnalysisService` 外观特性结构化识别。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.AI.csproj` | net10.0 工程，引用 `Poxiao.AI.Interfaces` 与 `Microsoft.Extensions.AI*` (preview 9.0.1) |
| `Service/AiService.cs` | `IAiService` 实现：构造 `OpenAIClient` -> `IChatClient`，`ChatAsync` 接收消息+可选 system prompt |
| `Service/AppearanceFeatureAnalysisService.cs` | 接收特性描述、特性分类字典与严重程度，调用 LLM 输出结构化 `AIFeatureItem` 列表 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Service/` | AI 服务实现 (见 `Service/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 所有服务必须实现对应 `Poxiao.AI.Interfaces` 中的接口并以 `ITransient` 标记进行自动 DI 注册。
- 配置缺失时（特别是 `AI:Chat:Endpoint`）抛 `InvalidOperationException`，不要静默降级。
- 不要在 `catch` 中直接 `throw`；按现有模式使用 `Poxiao.Logging.Log.Error` 记录并返回中文降级文本，避免影响前端业务流。
- LLM 输出 JSON 解析使用 `System.Text.Json` + `[JsonPropertyName]`，与 `AIFeatureItem` 字段保持一致。

### Common patterns
- 构造函数接收 `IConfiguration`，从 `AI:Chat` 节读取 `Endpoint` / `ModelId` / `Key`。
- 通过 `openAiClient.AsChatClient(modelId)` 获取 `IChatClient`，统一调用 `CompleteAsync`，温度默认 `0.7f`。

## Dependencies
### Internal
- `Poxiao.AI.Interfaces`
- `Poxiao.Common.Core`（DI 注解、日志）

### External
- `Microsoft.Extensions.AI` / `.AI.OpenAI`（preview）
- `OpenAI` SDK
- `System.Text.Json`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
