<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Kpi.Web.Core

## Purpose
KPI 模块的 Web 入口程序集 (`net10.0`)。承载基于 `IDynamicApiController`（Furion 动态 API）的控制器，路由前缀 `api/kpi/v1/*`，依赖 `Poxiao.Kpi.Application`。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Kpi.Web.Core.csproj` | 引用 `Poxiao.Kpi.Application`；`Compile Remove="Controller\WorkflowController.cs"`（暂时禁用旧版 Workflow 控制器） |
| `GlobalUsings.cs` | 全局 `using`：`Poxiao.Kpi.Application`、`Poxiao.Infrastructure.*`、`Microsoft.AspNetCore.Mvc` 等 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Controller/` | 19 个 KPI 业务控制器 + Mes 数据示例 (see `Controller/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 所有控制器必须实现 `IDynamicApiController`，使用 `[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "...", Name = "...", Order = ...)]` 与 `[Route("api/kpi/v1/[controller]")]`。
- 不直接处理业务，构造注入 `I*Service` 并转发；错误统一 `Oops.Oh(ErrorCode.COM10xx)`。
- 已被排除编译的 `WorkflowController.cs` 不要重新启用——其依赖项目已不再维护。

### Common patterns
- 创建：`HttpPost("")`；详情：`HttpGet("{id}")`；列表：`HttpGet("list")`；更新：`HttpPut("{id}")`；删除：`HttpDelete("{id}")`；选择器：`HttpGet("selector")`。
- 部分接口（Chat/Mes/部分 IE）使用 `[AllowAnonymous]`。

## Dependencies
### Internal
- `Poxiao.Kpi.Application`（接口与 DTO）
- `Poxiao.Infrastructure`（`IDynamicApiController`、`Oops`、`ErrorCode`、`ApiDescriptionSettings`）
### External
- Microsoft.AspNetCore.Mvc, Microsoft.AspNetCore.Authorization, Furion (动态 API)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
