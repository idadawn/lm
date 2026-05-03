<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.AI.Web.Core

## Purpose
AI 模块的 Web 接入工程。基于 `Poxiao.DynamicApiController` 自动暴露控制器，向前端 (web/) 与移动端 (mobile/) 提供 `api/ai/*` HTTP 端点。

## Key Files
| File | Description |
|------|-------------|
| `Controller/AiController.cs` | `POST api/ai/chat`：注入 `IAiService`，封装 `ChatRequest` / `ChatResponse` |
| `GlobalUsings.cs` | 全局引入 `Microsoft.AspNetCore.Mvc`、`Poxiao.DynamicApiController`、`Poxiao.FriendlyException` |
| `Poxiao.AI.Web.Core.csproj` | 引用 `Poxiao.AI.Interfaces` 与框架基础工程 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Controller/` | HTTP 控制器实现 (见 `Controller/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 控制器须继承/实现 `IDynamicApiController` 而非 `ControllerBase`，路由由 `[ApiDescriptionSettings]` + `[Route]` 决定。
- 控制器仅依赖 `Poxiao.AI.Interfaces` 中的接口，禁止直接 `new` 服务或引用 `Poxiao.AI` 实现工程。
- 请求/响应模型可定义在控制器同文件，使用中文 XML 注释对接 Swagger。

### Common patterns
- `[ApiDescriptionSettings(Groups = new[] { "AI" }, Tag = "ai", Name = "ai", Order = 200)]` 用于 Swagger 分组。
- 路由前缀统一 `api/ai`。

## Dependencies
### Internal
- `Poxiao.AI.Interfaces`
- `Poxiao.DynamicApiController`、`Poxiao.FriendlyException`

### External
- `Microsoft.AspNetCore.App`（FrameworkReference）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
