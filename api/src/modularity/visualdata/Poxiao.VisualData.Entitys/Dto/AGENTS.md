<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dto

## Purpose
大屏五子域 API DTO 根目录。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Screen/` | 大屏主体（`VisualEntity`）DTO (see `Screen/AGENTS.md`) |
| `ScreenCategory/` | 分类（`VisualCategoryEntity`）DTO (see `ScreenCategory/AGENTS.md`) |
| `ScreenConfig/` | 大屏配置 JSON（`VisualConfigEntity`）DTO (see `ScreenConfig/AGENTS.md`) |
| `ScreenDataSource/` | 数据源（`VisualDBEntity`）DTO + 动态查询入参 (see `ScreenDataSource/AGENTS.md`) |
| `ScreenMap/` | 自定义地图 DTO (see `ScreenMap/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 字段 camelCase；与 BladeX 大屏前端的 axios 请求体保持一致。
- 输入后缀 `*CrInput`/`*UpInput`/`*ListQueryInput`；输出后缀 `*ListOutput`/`*InfoOutput`/`*SelectorOuput`（注意拼写沿用 `ScreenSelectorOuput`/`ScreenDataSourceSeleectOutput`，不要修正）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
