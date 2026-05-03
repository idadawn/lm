<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# treeTable

## Purpose
演示「左树 + 右表」经典布局：左侧 `BasicLeftTree` 列出项目分类（字典 `IndustryType`），右侧 `BasicTable` 通过 `getTableListByType` 按类别加载数据。是 LIMS 中按类别筛选数据的主流页面骨架。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 左树 + 右表组合；`@select` 触发右表重载；`reloadTree` 字典刷新。 |

## For AI Agents

### Working in this directory
- 左树标题写死为「项目分类」；切换业务时记得改 title 与字典 key。
- `getTableListByType(type)` 是按类别查询接口，不要混用 `getTableList`。

### Common patterns
- `BasicLeftTree` ref + `TreeActionType`
- `useTable({ searchInfo, useSearchForm: true })` 搭配 `searchInfo.type` 自动带参

## Dependencies
### Internal
- `/@/components/Tree`, `/@/components/Table`, `/@/api/extend/table`, `/@/store/modules/base`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
