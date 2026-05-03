<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
产品规格扩展属性的旧式访问层。**整个目录已 `[Obsolete]`**：扩展属性已从 `ProductSpecEntity.PropertyJson` 迁移到独立的 `LAB_PRODUCT_SPEC_ATTRIBUTE` 表，文件保留仅为向后兼容。

## Key Files
| File | Description |
|------|-------------|
| `ProductSpecExtensions.cs` | `GetExtendedProperties/SetExtendedProperties/Get/SetExtendedValue` 等扩展方法，全部标 `[Obsolete("扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表，请使用属性表查询")]`，方法体已退化为空实现/空字典。 |
| `ProductSpecCalculationAttributes.cs` | `ProductSpecCalculationAttributes(productSpec)` 包装类（`Length` 默认 4m、`Layers` 默认 20 等），同样标 `[Obsolete]`，构造函数不再读 `PropertyJson`。 |

## For AI Agents

### Working in this directory
- **新代码不要使用本目录任何方法/类**：使用 `IProductSpecAttributeService` 与 `LAB_PRODUCT_SPEC_ATTRIBUTE` 表读写扩展属性。
- 计算上下文里的 `Length/Layers/Density` 由 `IntermediateDataService.GenerateIntermediateDataAsync` 显式传参（参考 `IIntermediateDataService` 接口签名）。
- 删除前需 grep 调用方：仍有少量历史代码引用，删除应配合迁移 PR。

### Common patterns
- 所有公开 API 都挂 `[Obsolete]` 并保留运行期"无操作"语义，便于编译期警告引导迁移。

## Dependencies
### Internal
- `../Entity/ProductSpecEntity.cs`（仅类型签名）。

### External
- `Poxiao.Infrastructure.Security`（旧引用）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
