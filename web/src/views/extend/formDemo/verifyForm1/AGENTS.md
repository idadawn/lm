<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# verifyForm1

## Purpose
表单校验示例（扩展版）。在 `verifyForm` 基础上增补信用卡号、URL、支付金额等格式校验，演示更细的正则与跨字段联合校验场景。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 扩展校验主页：账号 / 密码 / 邮箱 / 数量 / 信用卡 / URL / 金额校验 |

## For AI Agents

### Working in this directory
- 规则尽量参考 `verifyForm` 保持风格一致，避免在两个示例间分歧。
- 信用卡 `maxlength=18` 仅 UI 限位，真正校验靠 `validator`（Luhn 等）；提示文案与 `tip` span 同步。
- 业务化时把规则提取至 `/@/utils/validator`，并在两个示例中导入使用。

### Common patterns
- `name` 字段必须与 `dataForm` 键和 `dataRule` 键三者保持一致才能触发 ant-design-vue 校验。
- URL 规则通过 `pattern: /^https?:/i` 简易校验，复杂场景换 `validator`。

## Dependencies
### Internal
- `/@/components/Container`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
