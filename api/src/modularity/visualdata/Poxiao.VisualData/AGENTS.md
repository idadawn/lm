<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.VisualData

## Purpose
数据大屏服务实现工程。所有控制器路由统一前缀 `api/blade-visual/[controller]`，Tag = `BladeVisual`。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.VisualData.csproj` | 工程文件 |
| `ScreenService.cs` | 大屏 CRUD（`VisualEntity`）+ 配置 JSON（`VisualConfigEntity`）+ 截图/背景上传（`IFileManager`） |
| `ScreenCategoryService.cs` | 大屏分类（`VisualCategoryEntity`）分页/CRUD |
| `ScreenDataSourceService.cs` | 数据源（`VisualDBEntity`）CRUD + 动态查询（运行时 `ISqlSugarClient` 切库） |
| `ScreenMapConfigService.cs` | 自定义地图（`VisualMapEntity`）CRUD |

## For AI Agents

### Working in this directory
- 控制器命名上下文 `Tag = "BladeVisual"`；保持现有路由不变（前端 BladeX 大屏组件依赖该 URL 形态）。
- `ScreenService` 上传背景/截图走 `IFileManager`，文件落到 `/api/file/VisusalImg/...`；不要直接在控制器拼路径。
- 数据源测试连接/动态查询通过 `_sqlSugarClient.CopyNew()` 建临时连接，避免污染主库。

### Common patterns
- 分页输出：`data = await _repo.AsQueryable().ToPagedListAsync(input.current, input.size)`，并按 BladeX 协议返回 `{current, pages, records, size, total}`。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Core.Manager.Files.IFileManager`、`ITenant`/`ISqlSugarClient`
### External
- Mapster、SqlSugar、Yitter.IdGenerator

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
