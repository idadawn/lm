<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Extend.Entitys

## Purpose
扩展模块的"实体 / Dto / Mapper / 共享 Model"项目。被 `Poxiao.Extend` 服务层引用，对外不直接暴露。所有数据库表均以 `EXT_*` 命名。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Extend.Entitys.csproj` | 仅引用 `Poxiao.Common`（Infrastructure），无业务依赖 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Dto/` | 按业务划分的 Input/Output/Query DTO（BigData/Document/Email/Employee/Order/Product*/ProjectGantt/Schedule/TableExample/WorkLog） |
| `Entity/` | SqlSugar 实体（继承 `CLDEntityBase`，表名 `EXT_*`） |
| `Mapper/` | Mapster `IRegister` 实现，处理实体↔DTO 字段重命名 |
| `Model/` | 多服务共用的内嵌 Model（CollectionPlan/Goods/Postil/ProductEntry） |

## For AI Agents

### Working in this directory
- 所有实体继承 `CLDEntityBase`，业务字段强制使用 `[SugarColumn(ColumnName = "F_XXX")]` 大写下划线命名（详见 `.cursorrules`）。
- DTO 属性 camelCase（如 `fullName`/`createTime`），与前端 JSON 一致。
- 新增重命名映射时编辑 `Mapper/Mapper.cs`；不要在每个服务里写一次性 Mapster 配置。

### Common patterns
- DTO 文件名约定：`<Feature><Action>Input/Output/Query`。
- Model 文件夹存放被多个 Dto 共用的"行项"对象（如订单中的 `GoodsModel`、`CollectionPlanModel`）。

## Dependencies
### Internal
- `Poxiao.Common`（Infrastructure）

### External
- Mapster、SqlSugar、`Poxiao.DependencyInjection`（仅 `[SuppressSniffer]`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
