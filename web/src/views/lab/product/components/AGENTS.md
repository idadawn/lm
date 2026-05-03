<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
产品规格页面专用子组件：扩展属性独立面板、版本管理。

## Key Files
| File | Description |
|------|-------------|
| `ProductSpecExtendedProps.vue` | 产品规格扩展属性详情/编辑 |
| `VersionManage.vue` | 版本列表 + 切换/对比 |

## For AI Agents

### Working in this directory
- 版本管理在切换版本时应弹窗确认（避免误操作丢失编辑中数据）。
- 扩展属性 schema 与后端 `productSpec.extendedProps` 字段同步。

### Common patterns
- props 接收 `productSpecId` + `record`，emit `success` 让父页 reload。

## Dependencies
### Internal
- `/@/api/lab/product`, `/@/components/Modal`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
