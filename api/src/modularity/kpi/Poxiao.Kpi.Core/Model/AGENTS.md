<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Model

## Purpose
KPI 模块跨层共享模型。这里的对象既被实体（JSON 字段反序列化）也被 DTO 引用，故下沉到 Core 程序集。

## Key Files
| File | Description |
|------|-------------|
| `DbSchemaOutput.cs` | 数据源/Schema 通用模型，继承 `TreeModel`：`type`(私有 `DataModelType`)/`schemaStorageType?`/`dbType`/`name`/`host`/`sortCode?` |

## For AI Agents

### Working in this directory
- 此目录文件只放跨层数据载体，**不要**写业务逻辑或访问数据库。
- `DbSchemaOutput` 已被 `MetricInfoEntity.DataModelId` 通过 JSON 序列化使用，添加属性时务必兼容现有数据。

### Common patterns
- 继承 `TreeModel` 以便支持 `ToTree("-1")` 树渲染。

## Dependencies
### Internal
- `Poxiao.Common`(`TreeModel`)、`Enums/DataModelType`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
