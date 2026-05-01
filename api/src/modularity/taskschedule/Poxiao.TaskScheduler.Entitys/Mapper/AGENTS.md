<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Mapper

## Purpose
Mapster `IRegister` 配置：解析 `TimeTaskEntity.ExecuteContent` JSON，把 `startTime`/`endTime` 抽取到 `TimeTaskListOutput` 上方便前端排序/过滤。

## Key Files
| File | Description |
|------|-------------|
| `Mapper.cs` | 注册 `TimeTaskEntity → TimeTaskListOutput`，使用 `ExecuteContent.ToObject<ContentModel>()` 解析时间字段 |

## For AI Agents

### Working in this directory
- 该 Mapster 表达式在 `IQueryable.Select` 内使用时**不能翻译为 SQL**，需先 `ToList`/`ToListAsync` 再投影；当前实现按 in-memory 映射方式工作。
- 若新增 ExecuteContent 字段并要在列表展示，扩展 `ContentModel` 后在此追加 `.Map(...)`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
