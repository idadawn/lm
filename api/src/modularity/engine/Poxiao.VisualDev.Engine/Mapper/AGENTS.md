<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Mapper

## Purpose
Mapster 映射注册目录。把前端原始 JSON（`CodeGenFieldsModel`、`FieldsModel`）平滑映射到引擎内部强类型模型，并把字段提升到查询参数模型 `ListSearchParametersModel`。

## Key Files
| File | Description |
|------|-------------|
| `VisualDev.cs` | `IRegister` 实现：`FieldsModel↔ListSearchParametersModel`、`CodeGenFieldsModel→FieldsModel`，并把字符串字段（props / options / ableXXIds）懒解析为对象 |

## For AI Agents

### Working in this directory
- 所有映射都依赖 `JObject` 字符串字段的 `ToObject<T>()` 扩展方法（来自 `Poxiao.Infrastructure.Extension`），如出现 NPE 优先检查空字符串保护是否被遗漏。
- 新增字段映射保持 fluent 链式风格，并优先使用 `Map(dest=>..., src=>...)` 而不是 `MapWith`。

## Dependencies
### Internal
- `../Model/`、`Poxiao.Infrastructure.Models.VisualDev`

### External
- Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
