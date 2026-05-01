<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Mapper

## Purpose
Mapster `IRegister`：把模板/发布实体的 `Category` 映射为 `VisualDevSelectorOutput.ParentId`，方便前端按分类构建树。

## Key Files
| File | Description |
|------|-------------|
| `Mapper.cs` | 注册 `VisualDevEntity → VisualDevSelectorOutput` 与 `VisualDevReleaseEntity → VisualDevSelectorOutput` 两组映射 |

## For AI Agents

### Working in this directory
- 同源字段保持自动映射；只在 `Category → ParentId` 这种字段语义变化处显式 `Map`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
