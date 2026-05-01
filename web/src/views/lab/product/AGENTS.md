<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# product

## Purpose
"产品规格定义"页面：维护产品规格信息及扩展属性、公共属性。卡片网格视图，支持版本管理、扩展字段配置。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页：标题 + 搜索 + 产品规格卡片网格 |
| `Form.vue` | 产品规格新增/编辑表单 |
| `ExtendedAttributesForm.vue` | 扩展属性配置表单（动态字段定义）|
| `PublicAttributeForm.vue` | 公共属性表单 |
| `PublicAttributeList.vue` | 公共属性列表 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 产品规格扩展属性、版本管理 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 公共属性与产品级扩展属性是两种概念：公共 = 跨规格共用；扩展 = 单规格特有。
- 版本管理用于在规格变更时保留历史。
- API `/@/api/lab/product` + `/@/api/lab/publicAttribute`。

### Common patterns
- Tailwind 卡片 + grid 响应式（1/2/3 列）。
- `useI18n` 提供 `t('common.addText')` 国际化。

## Dependencies
### Internal
- `/@/api/lab/product`, `/@/components/Form`
### External
- `ant-design-vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
