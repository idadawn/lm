<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dataService

## Purpose
数据服务配置页：以表格形式管理服务条目，按"类型 + 名称"配置；支持添加、保存配置、导出配置等操作。整页只有一个 `index.vue`，弹窗内嵌于其中。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表 + 内嵌新增/编辑 `a-modal`（600px）：字段 `type` 选择驱动后续表单 |

## For AI Agents

### Working in this directory
- 表单使用 `a-form` + `a-form-item` 直接写在模板里（未走 `BasicForm`/`useForm`），保持简单可读但缺少 schema 复用。
- 顶部三个按钮：添加 / 保存配置 / 导出配置——后两个是文件级动作，不是行级。
- `formState.type` 决定配置形态，`handleChange` 切换时需重置依赖字段。

### Common patterns
- 行操作走 `TableAction`，表头按钮直接 `<a-button>`。

## Dependencies
### Internal
- `/@/components/Table`、`/@/hooks/web/useI18n`、`/@/hooks/web/useMessage`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
