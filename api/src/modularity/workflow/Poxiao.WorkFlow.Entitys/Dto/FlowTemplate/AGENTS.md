<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FlowTemplate

## Purpose
已发布流程模板（`FLOW_TEMPLATE` / `FLOW_TEMPLATE_JSON`）相关 DTO。承载列表 / 树形 / 详情 / JSON 信息 / 协管设置 / 导入导出，被 `FlowTemplateService` 与 `IFlowTaskRepository.GetFlowTemplateInfo` 使用。

## Key Files
| File | Description |
|------|-------------|
| `FlowTemplateListQuery.cs` | 模板列表查询（关键字 / 分类 / 状态） |
| `FlowTemplateListOutput.cs` | 模板列表条目（含版本、协管、可见范围） |
| `FlowTemplateTreeOutput.cs` | 模板分类树（左侧导航） |
| `FlowTemplateInfoOutput.cs` | 模板基础信息 |
| `FlowTemplateJsonInfoOutput.cs` | 模板 JSON 详情（流程图 + 表单） |
| `FlowJsonInfo.cs` | 流程 JSON 数据结构（节点列表 + 连线） |
| `FlowTemplateAssistQuery.cs` | 协管查询（hasAssistBtn 关联人员） |
| `FlowTemplateImportOutput.cs` | 导入流程模板回执 |

## For AI Agents

### Working in this directory
- 模板版本通过 `version` 字段维护，发布会写入 `FlowTemplateJsonEntity`；DTO 之间共享字段保持同名。
- `hasAssistBtn` 用于「是否协管」，影响详情页按钮可见性。

### Common patterns
- 列表树形结构使用平铺数组 + parentId / childList。
- JSON 字段统一命名为 `flowTemplateJson`。

## Dependencies
### Internal
- `DependencyInjection`（`SuppressSniffer`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
