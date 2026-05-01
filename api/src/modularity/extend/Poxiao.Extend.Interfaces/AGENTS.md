<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Extend.Interfaces

## Purpose
扩展模块对外暴露的接口/契约项目。当前**仅包含 csproj**，没有具体接口实现——预留给其他模块需要反向依赖 Extend 时填充（避免循环依赖）。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Extend.Interfaces.csproj` | 仅引用 `Poxiao.Extend.Entitys` |

## For AI Agents

### Working in this directory
- 添加接口时遵循模块约定：接口命名 `I<Feature>Service`、放在 `namespace Poxiao.Extend.Interfaces.<Feature>` 下。
- 不要在此处放任何实现；实现统一回到 `../Poxiao.Extend/`。
- 当前空 csproj 仍参与构建，是为了让外部模块可以仅依赖契约层。

## Dependencies
### Internal
- `../Poxiao.Extend.Entitys`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
