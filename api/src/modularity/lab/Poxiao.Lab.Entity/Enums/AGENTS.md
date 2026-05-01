<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enums

## Purpose
公式计算引擎相关的状态/类型枚举，区别于 `Enum/` 业务枚举：这里的枚举驱动 `IntermediateDataFormulaBatchCalculator`、`CalcTaskMessage.TaskType`、中间数据 `CalcStatus/JudgeStatus` 等流水线状态机。

## Key Files
| File | Description |
|------|-------------|
| `IntermediateDataCalcStatus.cs` | `PENDING=0/PROCESSING=1/SUCCESS=2/FAILED=3`，落到 `IntermediateDataEntity.CalcStatus`/`JudgeStatus`，使用 `EnumUseNameConverter` 序列化（前端按字符串名称解析）。 |
| `IntermediateDataFormulaType.cs` | `CALC=1(计算公式)/JUDGE=2(判定公式)/NO=3(只展示)`，决定 `IntermediateDataFormulaService.GetListAsync` 分组、计算器分发、前端编辑器 UI（CALC 是表达式、JUDGE 是 JSON 规则数组）。 |

## For AI Agents

### Working in this directory
- **不要修改枚举字面值或顺序**：`CalcStatus` 在数据库中以整数存储（`0..3`），改值会破坏历史数据。
- 序列化输出按 `Name`（`PENDING/SUCCESS/...`），与 RabbitMQ 消息和前端约定一致；增加状态时务必同步前端 `CalcStatus` 文案与表格筛选。
- `FormulaType.NO` 表示该列只在前端展示、不参与计算与判定；`IntermediateDataFormulaService` 会跳过它。

### Common patterns
- 同时使用 `[JsonConverter(typeof(EnumUseNameConverter<T>))]` + `[Description]`，保证 Newtonsoft 输出 name 而非整数。

## Dependencies
### Internal
- `../Entity/IntermediateDataEntity.cs`、`IntermediateDataFormulaEntity.cs`、`../Dto/IntermediateData/CalcTaskMessage.cs`。
- `Poxiao.Infrastructure` 提供 `EnumUseNameConverter`。

### External
- Newtonsoft.Json、System.ComponentModel.Description。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
