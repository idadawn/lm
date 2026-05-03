<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# verifyForm

## Purpose
表单校验示例：演示账号、密码、确认密码、邮箱、商品数量等字段的常见前端校验规则，包括必填、长度、字符范围、确认一致性、整数 / 金额格式等校验。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 校验示例主页：包含 `重置` 与 `提交验证` 操作按钮以及多字段 `rules` 规则集合 |

## For AI Agents

### Working in this directory
- 规则统一通过 `dataRule` 对象定义 `validator` / `pattern` / `min` / `max`；密码确认走自定义 validator 比较 `dataForm.password`。
- 提交按钮 `loading` 由 `btnLoading` 控制，避免重复提交。
- 项目通用规则（如手机号、身份证）建议复用 `/@/utils/validator`，不要在示例里硬编码同款实现。

### Common patterns
- 表单头部 `jnpf-common-page-header` 模式：右侧操作按钮、左侧标题占位。
- `tip` 类（`<span class="tip">`) 在字段下方说明校验要求，UX 一致。

## Dependencies
### Internal
- `/@/components/Container`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
