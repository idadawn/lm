<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
发送配置页面专用子组件：模板表单/选择弹窗、测试发送、发送结果展示。

## Key Files
| File | Description |
|------|-------------|
| `TemplateForm.vue` | 当前发送配置内嵌的模板编辑/绑定表单 |
| `TemplateModal.vue` | 模板选择/预览弹窗 |
| `TestSend.vue` | 测试发送弹窗（输入参数 → 触发后端实际发送）|
| `SendResults.vue` | 发送结果列表展示（成功/失败明细）|

## For AI Agents

### Working in this directory
- 测试发送会真实调用渠道账号，文案中提示"将真实发送"，按 confirm 后再执行。
- 模板与发送配置是 N:1 关系；TemplateForm 在 sendConfig 内编辑时不应影响全局模板（如需保存到全局模板需明确按钮）。

### Common patterns
- `useModal` + `useForm`，emit `success`/`reload`。

## Dependencies
### Internal
- `/@/api/msgCenter/sendConfig`, `/@/api/msgCenter/msgTemplate`
- `/@/components/Modal`, `/@/components/Form`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
