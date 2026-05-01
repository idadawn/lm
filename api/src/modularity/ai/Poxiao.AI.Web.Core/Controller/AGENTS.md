<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Controller

## Purpose
AI 模块的 HTTP 控制器集合。当前仅 `AiController` 暴露通用聊天端点，外观特性识别等服务通过被其它业务模块直接注入使用，不必单独暴露。

## Key Files
| File | Description |
|------|-------------|
| `AiController.cs` | `[Route("api/ai")]` 控制器，仅 `POST chat` 端点；空消息直接返回中文提示，不调用 LLM |

## For AI Agents

### Working in this directory
- 新增端点需补充中文 XML 注释及 `[HttpXxx("path")]` 显式路由，避免依赖 `[ApiDescriptionSettings].Name` 推断带来的兼容问题。
- 输入校验（如空消息）放在控制器内做短路返回，业务异常交由全局过滤器处理。
- 不要在控制器中组装 prompt 字符串——这是服务层职责。

### Common patterns
- DTO（`ChatRequest` / `ChatResponse`）与控制器同文件定义，便于阅读单端点契约。

## Dependencies
### Internal
- `Poxiao.AI.Interfaces`
- `Poxiao.DynamicApiController`

### External
- `Microsoft.AspNetCore.Mvc`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
