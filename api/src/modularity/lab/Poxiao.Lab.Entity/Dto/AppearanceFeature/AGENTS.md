<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# AppearanceFeature

## Purpose
外观/缺陷特征 (`AppearanceFeatureEntity`) 与其分类、批量匹配、AI 识别、人工修正的 DTO 集合。在导入流水线第 5 步"特性匹配"以及独立的特性维护界面被消费。

## Key Files
| File | Description |
|------|-------------|
| `AppearanceFeatureDto.cs` | 特性 CRUD（`*CrInput/*UpInput/*ListQuery/*ListOutput/*InfoOutput`）+ 单条匹配输出（`category/categoryPath/severityLevel/matchMethod/degreeWord/requiresSeverityConfirmation/featureExists/suggestedSeverity` 等丰富字段）+ `AppearanceFeatureCorrectionInput`。 |
| `AppearanceFeatureCategoryDto.cs` | 特性大类（树形分类）DTO，承载分类路径与父子关系。 |
| `BatchMatchDto.cs` | 批量匹配输入/输出：`BatchMatchInput → List<MatchItemInput>(id, query)`，输出 `MatchItemOutput` 含 `isPerfectMatch/manualCorrections (List<ManualCorrectionOption>)`，支持 `add_keyword/select_existing` 两种 actionType。 |
| `CorrectionListDto.cs` | 修正历史列表 DTO，便于运营查看 AI 误判/纠正记录。 |
| `FeatureClassification.cs` | AI 返回的多特性结构：`AIFeatureItem(name/level/category)` + 兼容旧版的 `main_category/sub_category/severity` 字段。 |

## For AI Agents

### Working in this directory
- 匹配逻辑放在 `Poxiao.Lab/Service/AppearanceFeatureService.cs` 与 `AppearanceFeatureRuleMatcher.cs`：DTO 仅是数据载体，不要在此添加规则。
- `MatchItemOutput.isPerfectMatch=false` 时前端需走人工确认流程，`manualCorrections` 必须填充——保持该契约。
- `FeatureClassification` 同时存在新旧字段是为了兼容历史 AI 返回；新代码只用 `Features`。

### Common patterns
- 字段使用 `[JsonPropertyName]`（camelCase）暴露给前端，C# 端保留 PascalCase 属性。
- 输出对象通过继承 `AppearanceFeatureEntity` 直接拿到所有字段，再追加展示用 `category/severityLevel` 等字符串。

## Dependencies
### Internal
- `../../Entity/AppearanceFeatureEntity.cs`、`AppearanceFeatureCategoryEntity.cs`、`AppearanceFeatureCorrectionEntity.cs`。
- `Poxiao.Infrastructure.Contracts/Filter`。

### External
- System.Text.Json。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
