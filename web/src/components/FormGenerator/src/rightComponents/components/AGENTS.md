<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`rightComponents` 中多个 `R*.vue` 共享的批量编辑模态对话框。主要服务于 Select/Radio/Checkbox 的"批量编辑选项"和 TreeSelect/Cascader 的"批量编辑树"场景，避免每个 `R*.vue` 重复实现同类弹窗。

## Key Files
| File | Description |
|------|-------------|
| `BatchOperate.vue` | 选项批量编辑模态：使用 `<a-textarea>` 接收"选项名\|选项值"格式文本，解析为 `{ id, fullName }[]` 后回写 |
| `TreeBatchOperate.vue` | 树形选项批量编辑：支持缩进表示层级，解析为带 `children` 的树结构 |
| `TreeNodeModal.vue` | 树节点单点编辑（添加/重命名/移动一个 node） |

## For AI Agents

### Working in this directory
- 文本解析约定：分隔符固定为英文 `|`，缩进固定为两空格代表一级。改动需同步前端文档与提示文案 (`<a-alert>`)。
- 解析失败应给出明确 `useMessage` 错误，不要静默吞掉。
- 这些模态都通过 vben-admin 的 `useModal` 注册，调用方拿 `[register, { openModal }]`。

### Common patterns
- `<BasicModal v-bind="$attrs" @register="registerModal" showOkBtn @ok="handleSubmit">`
- 提交结果通过 `emit('confirm', list)` 回传给父级 `R*.vue`。

## Dependencies
### Internal
- `/@/components/Modal` (`BasicModal`、`useModalInner`)
- `/@/hooks/web/useMessage`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
