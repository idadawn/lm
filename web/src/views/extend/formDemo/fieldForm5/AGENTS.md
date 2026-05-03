<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# fieldForm5

## Purpose
富文本类字段示例：演示 `a-textarea` 自适应高度多行输入与 `jnpf-editor`（项目内置富文本编辑器）的用法，作为业务表单中长文本输入的样板。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 文本域 + HTML 编辑器示例 |

## For AI Agents

### Working in this directory
- 富文本统一用 `jnpf-editor`（包装了项目主题、上传图片接口、敏感字过滤）；不要直接引入第三方 `wangeditor` 实例。
- `autoSize: { minRows, maxRows }` 控制 `a-textarea` 高度，避免手写 CSS。
- 编辑器内容是 HTML 字符串，业务侧持久化前需脱敏 / 过滤标签。

### Common patterns
- 表单仅显示输入控件，不绑定提交按钮，便于在外层页或弹层组合。

## Dependencies
### Internal
- `/@/components/Container`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
