<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Systems.Interfaces

## Purpose
系统模块的服务接口（契约）项目。所有跨模块调用通过此处的 `IXxxService` 接口注入，避免直接依赖实现项目 `Poxiao.Systems`。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Systems.Interfaces.csproj` | 仅引用 `Poxiao.Systems.Entitys` 与 `Poxiao.Apps.Entitys`；保持纯接口轻量 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Common/` | 通用接口（IFileService） (see `Common/AGENTS.md`) |
| `Permission/` | 权限/组织域接口 (see `Permission/AGENTS.md`) |
| `System/` | 系统级接口（菜单/字典/数据接口/模板等） (see `System/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 接口分组目录与 `Poxiao.Systems` 实现目录一一对应。
- 命名空间统一 `Poxiao.Systems.Interfaces.{Common|Permission|System}`。
- 跨模块（lab/workflow/ai）调用系统能力时，必须依赖接口而非实现，避免循环引用。

### Common patterns
- 仅声明方法签名，文档注释中文。
- 同步/异步方法成对出现（部分历史接口仅同步）。

## Dependencies
### Internal
- `Poxiao.Systems.Entitys`、`Poxiao.Apps.Entitys`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
