<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DictionaryData (Dto)

## Purpose
字典数据（明细）DTO。LIMS 中海量字典使用，覆盖 CRUD、树形、选择器、按字典类型查、导出、缓存全量等场景。

## Key Files
| File | Description |
|------|-------------|
| `DictionaryDataCrInput.cs` / `DictionaryDataUpInput.cs` | 创建/更新 |
| `DictionaryDataInfoOutput.cs` | 详情 |
| `DictionaryDataListOutput.cs` / `DictionaryDataListQuery.cs` | 列表/查询条件 |
| `DictionaryDataAllListOutput.cs` | 全量字典（用于缓存） |
| `DictionaryDataTreeOutput.cs` | 树形输出（部分字典分层） |
| `DictionaryDataSelectorOutput.cs` / `DictionaryDataSelectorDataOutput.cs` | 选择器 / 选择器数据 |
| `DictionaryDataExportInput.cs` | 导出 |

## For AI Agents

### Working in this directory
- 字典查询结果会被 `IMemoryCache` 缓存（Key = TenantId + Type）；DTO 字段尽量精简。
- 父子结构通过 `ParentId` 串联，前端按 enCode 取值，调整字段时保留 `enCode`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
