<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# IntermediateDataJudgmentLevel

## Purpose
判定等级 (`LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL`) DTO。每条等级关联到具体的 JUDGE 公式 (`FormulaId`) 与可选的产品规格 (`ProductSpecId`)，是 JUDGE 流程"配置等级 → 生成规则 → 完善条件"的入口数据。

## Key Files
| File | Description |
|------|-------------|
| `IntermediateDataJudgmentLevelDto.cs` | `Id/FormulaId/FormulaName/Code/Name/QualityStatus(QualityStatusEnum)/Priority/Color/IsStatistic/IsDefault/Description/Condition(JSON)/ProductSpecId/ProductSpecName` 等。本文件还应包含对应的 `*CrInput/*UpInput/*ListQuery`。 |

## For AI Agents

### Working in this directory
- `IsDefault=true` 的等级必须唯一（每个 FormulaId 一条）；前端"编辑判定"会把它的 `Name` 推给公式 `DefaultValue`，是"无规则匹配兜底"语义。
- `Priority` 决定生成 JSON 规则框架的顺序，运行期 `IntermediateDataFormulaBatchCalculator` 按优先级首条命中即写入 `resultValue`。
- `Condition` 字段是 JSON 字符串（一组 `groups`/`rootGroup`，含 `logic/conditions/subGroups`）；不要在 DTO 层做任何解析。
- `QualityStatus` 是 `QualityStatusEnum`（合格/不合格/其他）：用于月度质量报表合格率统计。

### Common patterns
- 同时支持全局公式（`ProductSpecId` 为空）与按规格的等级覆盖。

## Dependencies
### Internal
- `../../Entity/IntermediateDataJudgmentLevelEntity.cs`、`../../Enum/QualityStatusEnum.cs`。

### External
- System.ComponentModel.DataAnnotations。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
