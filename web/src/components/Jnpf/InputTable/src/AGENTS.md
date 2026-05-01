<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfInputTable` 子表（行内表格）的 SFC 实现。基于 `a-table` 渲染由 FormGenerator 配置的列（`tableData`），每个单元格根据列配置 `__config__.tag` 动态挂载对应 Jnpf 组件，并维护行级 `valid/regValid` 校验状态。

## Key Files
| File | Description |
|------|-------------|
| `InputTable.vue` | 组件实现：`a-table` + `bodyCell` 插槽 + 动态 `<component :is>`，专门分支处理 `JnpfRelationForm` / `JnpfPopupSelect`，其它组件按 tag 名透传 |

## For AI Agents

### Working in this directory
- 单元格组件通过 `__config__.tag` 名字加载（与全局注册的 `JnpfXxx` 一致），新增可插入控件时确保它已 `withInstall` 注册。
- 必须把 `rowIndex` / `tableVModel` (`config.__vModel__`) / `componentVModel` (`item.__vModel__`) 透传给单元格控件，否则跨组件联动（公式、表格内远程查询）无法工作。
- 校验展示统一通过 `tableFormData[index][cIndex].valid` 与 `regValid/regErrorText`，避免在子组件内 toast。
- 表头 `required-sign` (`*`) 与 `BasicHelp` 提示读自 `column.__config__`。

### Common patterns
- `JnpfGroupTitle` 控制顶部分组标题；`bordered` 仅在 `formStyle === 'word-form'` 时启用。
- `:scroll="{ x: 'max-content' }"` 防止列过多时挤压。

## Dependencies
### Internal
- `/@/components/JnpfGroupTitle`、`/@/components/Basic`（BasicHelp）、各 Jnpf 组件全局注册
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
