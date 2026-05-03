<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# VisualDevModelData

## Purpose
低代码运行时模型数据（`VisualDevModelDataEntity` 或用户业务表）的输出 DTO：列表查询、批量删除、导出、配置抓取。

## Key Files
| File | Description |
|------|-------------|
| `VisualDevModelListQueryInput.cs` | 列表查询入参（VisualDev 主键 + 动态表单字段查询条件） |
| `VisualDevModelDataBatchDelInput.cs` | 批量删除主键集合 |
| `VisualDevModelDataExportOutput.cs` | 导出输出（数据 + 表头） |
| `VisualDevModelDataConfigOutput.cs` | 表单/列表/工作流模板配置一并返回（formData/columnData/appColumnData/flowTemplateJson/enableFlow） |

## For AI Agents

### Working in this directory
- `VisualDevModelDataConfigOutput.flowId` 仅在启用流程的功能上有值；`enableFlow=1` 时配合 `flowTemplateJson` 提交工作流引擎。
- 列表查询字段是动态的（来自 `formData`），后端需用 `JObject`/`Dictionary<string, object>` 解析后下发给 `RunService`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
