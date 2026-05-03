<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
公共维度页面专用子组件：表单弹窗与版本管理弹窗。

## Key Files
| File | Description |
|------|-------------|
| `PublicDimensionForm.vue` | 公共维度新增/编辑表单（名称、单位、类型、备注）|
| `VersionManageModal.vue` | 版本列表 + 切换/对比/新建版本 |

## For AI Agents

### Working in this directory
- 切换版本前需提示"将影响所有引用此维度的公式"。
- 表单字段需与后端 `publicDimension` schema 严格对齐。

### Common patterns
- `useModal` + `useForm`，emit `success` reload 父页。

## Dependencies
### Internal
- `/@/api/lab/publicDimension`, `/@/components/Modal`, `/@/components/Form`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
