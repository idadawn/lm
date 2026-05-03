<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
单位管理子组件。

## Key Files
| File | Description |
|------|-------------|
| `UnitCategoryModal.vue` | 单位维度新增/编辑弹窗 |
| `UnitDefinitionModal.vue` | 单位定义新增/编辑弹窗（含换算系数、基准标记）|

## For AI Agents

### Working in this directory
- 一个维度只能有一个基准单位（baseUnit），UI 上需做单选限制。
- 换算系数 = `targetValue = sourceValue * factor + offset`（offset 可选，温度类需要）。

### Common patterns
- `useModal` + `useForm`。

## Dependencies
### Internal
- `/@/api/lab/unit`, `/@/components/Modal`, `/@/components/Form`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
