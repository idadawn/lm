<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Message

## Purpose
消息模块的实现工程（csproj），承载所有动态 API 控制器与业务管理类。引用 `Poxiao.Message.Interfaces`、`Poxiao.WorkFlow.Entitys`、`Poxiao.Systems.Interfaces`，并依赖 `SkiaSharp` / `System.Drawing.Common` 用于生成二维码/图片附件。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Message.csproj` | 项目文件；引用 Interfaces/Systems/Workflow + SkiaSharp 3.119.1 |
| `Service/` | 各业务服务（站内信、模板、账号、监控、短链、微信、IM、发送配置）入口目录 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Service/` | 所有服务实现类（IDynamicApiController + ITransient）(see `Service/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 新增服务实现：必须实现 `IDynamicApiController` + `ITransient`（自动注册为瞬时服务）。如要被外部模块调用，应实现 Interfaces 中的接口。
- 引用图形/PDF 库（SkiaSharp）只用于消息附件/二维码生成，不要扩散到 Entity 层。

### Common patterns
- 服务路由统一前缀 `api/message/...`（`MessageService` 例外，路由为 `api/[controller]`）。
- 控制台 Tag 一律 `Message`，Order 240，便于 Swagger 分组。

## Dependencies
### Internal
- `Poxiao.Message.Interfaces`、`Poxiao.WorkFlow.Entitys`、`Poxiao.Systems.Interfaces`、`Poxiao.Common.Core`
### External
- SkiaSharp 3.119.1、System.Drawing.Common 10.0.2、StyleCop.Analyzers

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
