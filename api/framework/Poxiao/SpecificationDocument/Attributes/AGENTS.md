<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
为 OpenAPI 文档生成提供细粒度控制的特性集合：覆盖 `SchemaId` 冲突、强制枚举输出为数字、自定义 OperationId。供业务 DTO 与控制器方法标注，由本模块的 Builder/Filters 反射读取。

## Key Files
| File | Description |
|------|-------------|
| `SchemaIdAttribute.cs` | `[SchemaId(id, replace=false)]`：解决多个 DTO 同名导致的 SchemaId 冲突；`replace=true` 完全替换默认 SchemaId，否则在头部叠加。 |
| `OperationIdAttribute.cs` | 自定义接口的 OpenAPI `operationId`，便于前端代码生成器稳定命名。 |
| `EnumToNumberAttribute.cs` | 控制单个枚举类型在文档中是输出名称还是数值（覆盖全局 `SpecificationDocumentSettings:EnumToNumber`）。 |

## For AI Agents

### Working in this directory
- 这些特性是数据契约的一部分；改动语义可能破坏前端生成代码。
- `SchemaIdAttribute` 仅作用于 `AttributeTargets.Class`；DTO 命名冲突优先用此特性，不要在 Builder 内 hardcode 映射。
- 含中文枚举名时 `EnumSchemaFilter` 会强制 `convertToNumber=true`，与 `EnumToNumberAttribute` 协作时注意优先级。

### Common patterns
- 简单 `Attribute` 子类，`sealed`，构造函数携带必填值并暴露只读属性。

## Dependencies
### Internal
- 由 `../Filters/EnumSchemaFilter.cs` 与 `../Builders/SpecificationDocumentBuilder.cs` 反射消费。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
