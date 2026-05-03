<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FlowEngine

## Purpose
流程设计（FLOW_ENGINE）相关的 DTO。承载流程引擎设计阶段的 CRUD、字段元数据、列表（树形 / 全量 / 下拉）、导入导出。被 `FlowEngineService` 控制器使用。

## Key Files
| File | Description |
|------|-------------|
| `FlowEngineCrInput.cs` | 新建流程入参（fullName/enCode/category/type/formType/formData/flowTemplateJson/tables/dbLinkId/...） |
| `FlowEngineUpInput.cs` | 更新流程入参 |
| `FlowEngineInfoInput.cs` | 详情查询入参 |
| `FlowEngineListInput.cs` | 列表查询入参（关键字、分类、状态等） |
| `FlowEngineListOutput.cs` | 列表输出（树形 TreeModel） |
| `FlowEngineListAllOutput.cs` | 全量列表输出（含 formData = formTemplateJson 映射） |
| `FlowEngineListSelectOutput.cs` | 下拉选择输出 |
| `FlowEngineFieldOutput.cs` | 表单字段元数据输出（用于条件 / 变量审批人） |
| `FlowEngineImportInput.cs` / `FlowEngineImportOutput.cs` | 流程 JSON 导入入参与回执 |

## For AI Agents

### Working in this directory
- `FlowEngineCrInput.formData` 在 Mapster 中被映射为实体的 `FormTemplateJson`，新增字段时检查 `Mapper.cs` 的 `ForType<FlowEngineCrInput, FlowEngineEntity>()` 配置。
- 树形列表通过继承 `TreeModel` 实现，新增字段不要破坏树结构。

### Common patterns
- 流程 JSON 的存储字段统一命名 `flowTemplateJson` / `formData` / `propertyJson` / `tableJson` / `draftJson`。
- `formType` ：1 系统表单 / 2 自定义；`type`：0 发起流程 / 1 功能流程。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Security`（`TreeModel`）、`DependencyInjection`（`SuppressSniffer`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
