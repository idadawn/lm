<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ai

## Purpose
AI 集成模块根目录。封装与 LLM (vLLM/OpenAI 兼容端点) 的对话能力，向系统其它模块提供通用聊天 (`IAiService`) 与外观特性识别 (`IAppearanceFeatureAnalysisService`) 等领域能力。本模块同时是 NL-to-SQL、Qdrant 向量检索、TEI 嵌入等 AI 链路在后端侧的接入点。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.AI/` | AI 服务实现工程，调用 `Microsoft.Extensions.AI` + OpenAI SDK (见 `Poxiao.AI/AGENTS.md`) |
| `Poxiao.AI.Interfaces/` | 公共契约：`IAiService`、`IAppearanceFeatureAnalysisService` 及结果 DTO (见 `Poxiao.AI.Interfaces/AGENTS.md`) |
| `Poxiao.AI.Web.Core/` | Web 入口，暴露 `api/ai/*` 动态控制器 (见 `Poxiao.AI.Web.Core/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 模块按 `Interfaces / 实现 / Web.Core` 三层拆分，新增能力请遵循同样分层：先在 `Poxiao.AI.Interfaces` 中定义抽象，再在 `Poxiao.AI` 实现，最后在 `Poxiao.AI.Web.Core` 暴露 HTTP 接口。
- 配置读取统一走 `IConfiguration` 的 `AI:Chat`（`Endpoint` / `ModelId` / `Key`）；新增模型/Embedding 配置请同步到根目录 `AppSetting.json`。
- 不要在此模块直接引用 SqlSugar 实体；与业务实体相关的 AI 增强逻辑应在调用方组装数据后传入。

### Common patterns
- DI 注册采用 `ITransient` 标记接口，避免显式 `services.AddXxx`。
- 错误统一通过 `Poxiao.Logging.Log.Error` 记录后返回中文友好提示，不抛业务异常给前端。

## Dependencies
### Internal
- `framework/Poxiao` 通用框架（DI、动态 API、Logging）
- `api/src/common/Poxiao.Common.Core`

### External
- `Microsoft.Extensions.AI` / `Microsoft.Extensions.AI.OpenAI` 9.0.x preview
- `OpenAI` SDK（与 vLLM 兼容）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
