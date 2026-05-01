<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DictionaryType (Dto)

## Purpose
字典分类 DTO。提供 CRUD、详情、列表、选择器。每个字典类型下挂一组 `DictionaryData`。

## Key Files
| File | Description |
|------|-------------|
| `DictionaryTypeCrInput.cs` / `DictionaryTypeUpInput.cs` | 创建/更新分类 |
| `DictionaryTypeInfoOutput.cs` | 详情 |
| `DictionaryTypeListOutput.cs` | 列表 |
| `DictionaryTypeSelectorOutput.cs` | 选择器 |

## For AI Agents

### Working in this directory
- 分类删除时必须级联检查 `DictionaryData`；DTO 不提供级联删除参数。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
