<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfInputNumber` 的 SFC 实现。当 `detailed` 为真时改为只读 `<p>` 文本（带 `addonBefore/After` 与金额大写）；普通态下渲染 `a-input-number`，并按 `thousands` 注入 `formatter/parser`。

## Key Files
| File | Description |
|------|-------------|
| `InputNumber.vue` | 组件实现：`useDesign('input-number')`、`getAmountChinese`、`thousandsFormat`，处理 `precision` 透传 |

## For AI Agents

### Working in this directory
- `precision` 必须显式判断 `0` 是否传入（代码里使用 `precision || precision === 0` 判断），不要简化为真值判断。
- `thousands` 模式下 `formatter` 用 `thousandsFormat`，`parser` 必须 `replace(/\$\s?|(,*)/g, '')` 还原。
- `isAmountChinese` 仅在数值非空时计算大写，避免空字符串显示 "零元整"。

### Common patterns
- 与 `Calculate.vue` 共享 `getAmountChinese` / `thousandsFormat`，行为应保持一致。

## Dependencies
### Internal
- `/@/utils/jnpf`、`/@/hooks/core/useAttrs`、`/@/hooks/web/useDesign`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
