<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# fieldForm3

## Purpose
选择类字段示例：开关、单选组、多选组、`jnpf-select`、`jnpf-tree-select`、`jnpf-cascader` 等下拉/多选/树形/级联控件的常见用法演示。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单选 / 多选 / 下拉 / 树选 / 级联组合示例 |

## For AI Agents

### Working in this directory
- 选项使用 `options` 数组直接传入；树/级联使用 `treeData` / `options1` 嵌套结构（含 `children`）。
- 图标按钮型单选用 `<a-radio>` 包裹 `<i class="icon-ym icon-ym-extend-*" />`；勿写内联样式覆盖项目主题。
- 业务化时把硬编码 options 替换为字典接口（`/@/api/system/dictionary`）。

### Common patterns
- `a-checkbox-group v-model:value` 绑定数组型字段。
- `jnpf-tree-select` 通过 `@change` 取节点数据回填关联字段。

## Dependencies
### Internal
- `/@/components/Container`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
