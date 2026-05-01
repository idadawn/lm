<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# fieldForm1

## Purpose
Schema 驱动表单示例。演示通过 `FormSchema[]` 配置 `BasicForm` 渲染单行输入、密码、文本域、数字、开关、分割线等多种控件，是业务页接入项目 `/@/components/Form` 的范本。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 示例 1：直接消费 `getFormSchema()` 渲染 `BasicForm` |
| `index1.vue` | 示例 2：另一种 schema 组装方式（异步默认值 / 联动） |
| `schemaData.tsx` | 字段 schema 定义工厂 `getFormSchema(): FormSchema[]`，集中管理控件配置 |

## For AI Agents

### Working in this directory
- 新增字段优先扩展 `schemaData.tsx`，避免在模板内堆叠 `<a-form-item>`。
- `componentProps` 接受 ant-design-vue 控件原生属性（如 `showPassword`、`min/max`）；`Divider` 通过 `content` 属性渲染分割文案。
- `helpMessage` 字段会渲染 `?` 提示，UX 文案保留中文。

### Common patterns
- `tsx` 后缀允许 schema 内嵌 JSX（自定义渲染节点）。
- 表单提交校验通过 `useForm` 暴露的 `validate` / `getFieldsValue`。

## Dependencies
### Internal
- `/@/components/Form`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
