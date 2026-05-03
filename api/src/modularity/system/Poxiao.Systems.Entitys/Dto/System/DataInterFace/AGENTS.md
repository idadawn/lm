<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DataInterFace (Dto)

## Purpose
数据接口（HTTP/SQL）配置 DTO。提供 CRUD、列表查询、预览调用、选择器、运行期取列表（用于下拉数据源绑定）。

## Key Files
| File | Description |
|------|-------------|
| `DataInterfaceCrInput.cs` / `DataInterfaceUpInput.cs` | 接口创建/更新（含 ProperJson 嵌套） |
| `DataInterfaceInfoOutput.cs` | 详情 |
| `DataInterfaceListOutput.cs` / `DataInterfaceListQuery.cs` | 列表/查询 |
| `DataInterfacePreviewInput.cs` | 预览调用（带参） |
| `DataInterfaceSelectorOutput.cs` | 选择器（树形） |
| `DateInterfaceGetListOutput.cs` | 运行期取列表（被前端数据源绑定调用，注意类名拼写 "Date"） |

## For AI Agents

### Working in this directory
- `DateInterfaceGetListOutput` 类名拼写错误，已被前端引用，**不要重命名**。
- ProperJson 字段对应 `Model/System/DataInterFace/DataInterfaceProperJson.cs` 的反序列化目标。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
