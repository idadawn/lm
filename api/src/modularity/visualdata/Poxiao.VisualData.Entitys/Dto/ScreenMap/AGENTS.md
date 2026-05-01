<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ScreenMap

## Purpose
自定义地图（`VisualMapEntity` → `BLADE_VISUAL_MAP`）的 DTO。地图 GeoJSON 数据存储在 `data` 字段。

## Key Files
| File | Description |
|------|-------------|
| `ScreenMapCrInput.cs` | 创建（name/data） |
| `ScreenMapUpInput.cs` | 更新 |
| `ScreenMapInfoOutput.cs` | 详情（含完整 GeoJSON） |
| `ScreenMapListOutput.cs` | 列表行（仅 id/name，避免拉取大量 GeoJSON） |
| `ScreenMapListQueryInput.cs` | 列表查询参数 |

## For AI Agents

### Working in this directory
- 列表禁止返回 `data` 字段（GeoJSON 体积大）；详情接口才返回。
- `data` 必须为合法 JSON；后端入库前用 `JsonSerializer` 轻校验避免坏数据。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
