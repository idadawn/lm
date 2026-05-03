<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# formDemo

## Purpose
表单组件示例集合。集中演示项目内常用的输入控件、字段组合、动态字段、Schema 驱动表单与多种校验模式，便于业务开发者快速复制片段或对照行为。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `audio/` | 录音上传示例（开始/暂停/继续/停止录音 + 播放） |
| `examples/` | 综合示例：动态字段、公式输入框、单选/多选/树选择 |
| `fieldForm1/` | Schema 驱动 `BasicForm`（多控件类型 + tsx schema 数据） |
| `fieldForm2/` | 普通文本 / 数值 / 日期时间字段示例 |
| `fieldForm3/` | 下拉框 / 多选框 / 单选框 / 树形选择 / 级联示例 |
| `fieldForm4/` | 单附件、图片上传组件示例 |
| `fieldForm5/` | 文本域 + HTML 编辑器示例 |
| `fieldForm6/` | 查询选择、客户选择、商品选择器示例（联动选择） |
| `verifyForm/` | 表单校验示例（账号/密码/邮箱/数字/卡号/URL/金额规则） |
| `verifyForm1/` | 表单校验扩展示例（继承 verifyForm 规则） |

## For AI Agents

### Working in this directory
- 这些示例不可作为正式业务页直接复用，因数据均为前端 mock；改造为业务时需对接 `/@/api/*`。
- 业务表单优先使用 `/@/components/Form` 的 Schema 驱动形式（参考 `fieldForm1`），避免每个字段手写 `<a-form-item>`。
- 控件统一用 `jnpf-*` 包装版（`jnpf-select`、`jnpf-date-picker`、`jnpf-upload-file` 等），保证主题与权限拦截一致。

### Common patterns
- 入口命名 `extend-formDemo-<feature>`；外层包裹 `page-content-wrapper-form` + `ScrollContainer`。
- 校验示例使用 ant-design-vue `rules` 数组 + `validator` 函数。

## Dependencies
### Internal
- `/@/components/{Form,Container,Table,Modal}`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
