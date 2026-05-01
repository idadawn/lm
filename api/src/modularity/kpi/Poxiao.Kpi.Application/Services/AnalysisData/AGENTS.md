<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# AnalysisData

## Purpose
统计图分析服务。当前用于演示/示例数据：基于 MathNet.Numerics 生成正态分布 + 直方图，及示例 25 组测点的 X̄-R 控制图。

## Key Files
| File | Description |
|------|-------------|
| `IAnalysisDataService.cs` | 接口：`GetNHChart()` 正态/直方图、`GetXRChart()` 均值-极差控制图 |
| `AnalysisDataService.cs` | 实现：`extern alias MathNetOfficial`，控制图常数 A2/D3/D4 表，硬编码示例样本 `_xRProtoData` |

## For AI Agents

### Working in this directory
- 因 `MathNet.Numerics` 与项目其他依赖存在命名冲突，使用 `extern alias MathNetOfficial`（见 csproj `Aliases="MathNetOfficial"`），新增统计代码请沿用同一别名。
- 系数表只覆盖样本量 2-10，新场景需扩展 `_factorChart`。

### Common patterns
- 用 `MathStatistics.Mean/Maximum/Minimum` 计算均值与极差；保留 4-5 位小数。
- 直方图分桶用斯特杰斯经验公式 `K = 1 + 3.3·log10(N)`。

## Dependencies
### Internal
- `../../Dtos/AnalysisData`
### External
- MathNet.Numerics (alias `MathNetOfficial`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
