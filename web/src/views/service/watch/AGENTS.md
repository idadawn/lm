<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# watch

## Purpose
服务观察 (Watch) 配置页：双栏树形界面——左侧"采集器及隶属标签"全量树，右侧"服务观察列表"已选树；通过添加/移除按钮在两棵树之间转移标签 (`tagId`/`tagName`/`tagInfos`)。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 双 `a-tree`（checkable）+ 中间添加/移除按钮，`fieldNames={ children:'tagInfos', title:'tagName', key:'tagId' }` |

## For AI Agents

### Working in this directory
- 树字段名是非默认 (`tagInfos`/`tagName`/`tagId`)，新增交互时务必显式传 `fieldNames`。
- 仅页面级表单，未抽 modal/component，逻辑全部内联在 `<script setup>`。
- 数据结构需支持"采集器 → 标签"两层嵌套。

### Common patterns
- `defaultExpandAll` + `:selectable="false"` 仅复选不选择行。
- 按钮放在每个 `gutter-box` 末尾，单一动作触发。

## Dependencies
### External
- `ant-design-vue` (`a-tree`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
