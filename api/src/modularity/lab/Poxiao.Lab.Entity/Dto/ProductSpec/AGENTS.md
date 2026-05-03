<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ProductSpec

## Purpose
产品规格 (`LAB_PRODUCT_SPEC`) CRUD DTO。产品规格在导入流水线中是关键的"分组键"——`CompleteImport` 时按 `ProductSpecId` 分组生成中间数据并匹配公式与判定。

## Key Files
| File | Description |
|------|-------------|
| `ProductSpecDto.cs` | `*CrInput/*UpInput/*ListQuery/*ListOutput/*InfoOutput`，全部继承 `ProductSpecEntity`，并附带 `Attributes: List<ProductSpecAttributeEntity>` 用于扩展属性表（已从 `PropertyJson` 迁移到 `LAB_PRODUCT_SPEC_ATTRIBUTE`）。`*UpInput` 还包含 `CreateNewVersion?`/`VersionDescription`，用于显式控制版本化。 |

## For AI Agents

### Working in this directory
- 扩展属性 (`Length/Layers/Density` 等用于公式上下文的字段)**已迁移到 `LAB_PRODUCT_SPEC_ATTRIBUTE` 子表**：始终通过 `Attributes` 列表读写，不要再用旧的 `PropertyJson`（`Extensions/ProductSpecExtensions.cs` 已 `[Obsolete]`）。
- `CreateNewVersion=true` 时服务端会生成新的 `ProductSpecVersionEntity`，并把当时的属性快照入版本表；旧的中间数据通过 `specVersion` 关联历史版本，方便审计。
- 列表 query 仅暴露 `Keyword`，复杂过滤通常由前端二次筛选或单独接口完成。

### Common patterns
- 输入/输出 DTO 全部直接继承实体类，不另起字段，避免序列化漂移。

## Dependencies
### Internal
- `../../Entity/ProductSpecEntity.cs`、`ProductSpecAttributeEntity.cs`、`ProductSpecVersionEntity.cs`。
- `Poxiao.Infrastructure.Filter.PageInputBase`。

### External
- 无（纯 POCO）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
