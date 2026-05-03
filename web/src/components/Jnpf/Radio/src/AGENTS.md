<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfRadio` SFC 实现：`a-radio-group` + `optionType` 分支渲染 `a-radio-button` 或 `a-radio`；`vertical` 时按钮项额外包一层 `<span class="vertical-button">` 实现纵向布局。

## Key Files
| File | Description |
|------|-------------|
| `Radio.vue` | 模板按 `optionType` × `direction` 4 路分支；`onChange` 同时 emit value 与匹配的选项对象 |
| `props.ts` | `radioProps`：`value`(string\|number\|boolean) / `options` / `fieldNames` / `optionType`(default\|button) / `direction`(horizontal\|vertical)，`FieldNames` 接口 |

## For AI Agents

### Working in this directory
- emit `change(val, data)`：`data` 是已过滤出的选项数组（不是单一 option 对象）——业务侧请用 `data[0]` 拿当前项。
- `getFieldNames` 合并默认值（`id`/`fullName`/`disabled`）与传入 `fieldNames`，保证默认能解析后端 `fullName` 标签。
- 样式 less 中 `.jnpf-vertical-radio .ant-radio-wrapper` 强制 `display: flex` 实现长文本 word-break。

### Common patterns
- `useAttrs` 透传 ant 原生 props（如 `size`、`disabled`），同时通过 class 计算追加 `jnpf-{direction}-radio`。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
