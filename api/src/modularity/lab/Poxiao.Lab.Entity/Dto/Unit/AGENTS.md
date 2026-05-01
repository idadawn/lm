<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Unit

## Purpose
单位维度 (`UnitCategoryEntity`) 与单位定义 (`UnitDefinitionEntity`) 的 CRUD/换算 DTO。单位换算贯穿 Excel 导入精度处理、公式计算（公式单位 vs 导入单位）以及前端显示。

## Key Files
| File | Description |
|------|-------------|
| `UnitCategoryDto.cs` | 单位大类（长度/质量/磁感应强度等）展示 DTO。 |
| `UnitCategoryInput.cs` | 单位大类创建/更新入参。 |
| `UnitDefinitionDto.cs` | 单位定义（含基准换算系数 `ScaleToBase`、偏移 `Offset`、显示精度 `Precision`）。 |
| `UnitDefinitionInput.cs` | 单位定义入参：`CategoryId/Name/Symbol/IsBase/ScaleToBase/Offset/Precision/SortCode/ConversionFactor?`；`ConversionFactor` 仅在切换基准单位时使用，强制按该比例重算同维度其他单位。 |
| `UnitConversionRequestDto.cs` | 单位换算请求 DTO（源单位/目标单位/数值/精度）。 |

## For AI Agents

### Working in this directory
- 切换基准单位是高风险操作：传入 `ConversionFactor` 时服务端会**忽略 `ScaleToBase`** 并整批重算同维度其他单位的 `ScaleToBase` —— DTO 注释明确说明，新接口要保持该语义不变。
- `Precision` 默认精度参与公式精度优先级链条：公式精度 → `unitPrecisions` → 单位 `Precision` → `LabOptions.Formula.DefaultPrecision`。
- 公式计算遇到不可换算的单位组合时记录 `UNIT` 错误并跳过列（参见根模块 AGENTS.md 公式异常处理）。

### Common patterns
- 所有计算字段用 `decimal`，精度信息单独以 `int Precision` 给出，避免与值耦合。

## Dependencies
### Internal
- `../../Entity/UnitCategoryEntity.cs`、`UnitDefinitionEntity.cs`。
- 服务：`Poxiao.Lab/Service/UnitDefinitionService.cs`、`UnitConversionService.cs`、`UnitCategoryService.cs`。

### External
- 无。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
