<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# AppearanceFeatureLevel

## Purpose
外观特征"严重等级"（如"轻微/严重/超级"）的 CRUD DTO。等级与外观特征通过 `SeverityLevelId` 关联，参与匹配匹配过程中的等级解析（程度词匹配）。

## Key Files
| File | Description |
|------|-------------|
| `AppearanceFeatureLevelDto.cs` | `*CrInput/*UpInput`（直接继承 `AppearanceFeatureLevelEntity`）+ `*ListQuery(Keyword, Enabled)` + `*ListOutput/*InfoOutput`。继承式 DTO，无额外展示字段。 |

## For AI Agents

### Working in this directory
- 等级业务规则、唯一性校验在 `Poxiao.Lab/Service/AppearanceFeatureLevelService.cs`；本目录只暴露字段。
- 与 `AppearanceFeature` 中 `SeverityLevel` / `requiresSeverityConfirmation` 流程协同：当 AI 推荐的等级在数据库不存在时，前端会先调用本目录对应的创建接口，再回填特征。
- 新增展示字段时按 `*ListOutput : Entity` 继承模式，不要直接修改实体。

### Common patterns
- `PageInputBase + Keyword/Enabled` 是模块通用查询模板。

## Dependencies
### Internal
- `../../Entity/SeverityLevelEntity.cs` 或 `AppearanceFeatureLevelEntity`。
- `Poxiao.Infrastructure.Contracts/Filter`。

### External
- System.Text.Json。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
