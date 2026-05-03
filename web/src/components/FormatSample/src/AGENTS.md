<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`FormatSample` 组件实现层。`index.vue` 是一个 `<a-modal>` 容器，使用 `<a-radio-group>` 切换格式类型，根据所选类型条件性显示"小数位数 / 单位 / 货币标志 / 分隔符"等子项；`props.ts` 仅暴露 `visible` 控制可见性。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 编辑指标格式模态：radio-group(None/Number/Currency/Percentage) + 条件字段 + `formatValue` 实时样例预览，`@ok=handleFormat` 提交 |
| `props.ts` | 组件 props 定义，仅 `visible: Boolean` |

## For AI Agents

### Working in this directory
- 文案与 enum 值已存在隐含合约：`'None' / 'Number' / 'Currency' / 'Percentage'`、`'CNY1' / 'USD1'`、`'Wan' / 'Yi'` — 修改 enum 字符串需同步后端业务字典。
- `radio_group`、`format` 状态使用 `reactive` + `ref` 双轨；新增字段建议放入 `format` 对象，便于一次性 emit 出去。
- 内部使用 `defineOptions({ name: 'FormatSample' })`，与 `index.ts` 的 `withInstall` 命名保持一致。

### Common patterns
- `<a-modal>` + `v-model:visible` 的标准 antdv 模态控制流。
- `@change` 处理函数链：`handleRadioGroup → handleChange_decimal/unit/symbol/separator` 分项更新格式预览。

## Dependencies
### Internal
- `./props`
### External
- `vue` (`reactive/watch/ref`)、`ant-design-vue` (`Modal`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
