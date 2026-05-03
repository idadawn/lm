<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# template

## Purpose
"Excel 导入模板管理"页面：管理 rawData/magneticData 等导入流程使用的 Excel 模板配置（列映射、必填校验）。模板可关联到产品规格。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 模板卡片网格展示页 |
| `TemplateForm.vue` | 模板新增/编辑表单（含列映射）|

## For AI Agents

### Working in this directory
- 模板字段映射需与后端 Excel 解析器约定保持一致；前端不要硬编码字段名。
- 关联产品规格变化时应提醒"现有导入历史使用旧规格"。

### Common patterns
- 蓝色主题 + Tailwind 卡片 + 空数据 illustration。
- `useModal` 注册 TemplateForm。

## Dependencies
### Internal
- `/@/api/lab/template`, `/@/api/lab/product`
- `/@/components/Modal`, `/@/components/Form`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
