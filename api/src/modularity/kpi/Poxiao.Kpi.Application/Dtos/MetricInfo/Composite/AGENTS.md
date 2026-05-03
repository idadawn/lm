<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Composite

## Purpose
复合指标 DTO。复合指标通过表达式（NCalc）引用多个已有指标 (`${指标A}/${指标B}`) 计算得到，对应 `MetricInfo4CompositeService`。

## Key Files
| File | Description |
|------|-------------|
| `FormulaInput.cs` | 公式校验入参 (`formulaData` 字符串) |
| `MetricInfo4CompositeCrInput.cs` | 新建复合指标（含父级指标 IDs、公式、维度、筛选、时间维度） |
| `MetricInfo4DCompositeUpInput.cs` | 更新入参 |
| `MetricInfo4CompositeOutput.cs` | 详情：`parentIds` 列表 + 反序列化的格式/维度/筛选 |

## For AI Agents

### Working in this directory
- 公式中 `${...}` 在校验前会被服务层去除，再由 NCalc 解析；非法表达式返回 `ErrorCode.K10018/K10020`。
- `ParentIds`(列表) 落库为 `parent_id` 字符串（逗号拼接），由 `MetricInfoMapper` 完成转换。

### Common patterns
- 复合指标共享 `MetricInfoEntity` 表，区分靠 `Type=Composite`。
- 公式数据落库列：`formula_data`。

## Dependencies
### Internal
- `../`（共用 `MetricFilterDto` 等）
- `Services/MetricInfo/MetricInfo4CompositeService.cs`
### External
- Newtonsoft.Json, NCalcSync

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
