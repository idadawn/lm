<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# postilTable

## Purpose
演示「行内批注/评论」：列表中显示批注计数 `批注(N)`，点击弹出 `Form.vue` —— 一个带 `BasicForm` 输入框 + `ant-design-vue` Timeline 历史记录的批注弹窗，可新增/删除批注。对接 `getPostilList / sendPostil / delPostil`。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表展示批注数；`postilCount` 列点击唤起 Form。 |
| `Form.vue` | BasicModal + Timeline，发送/删除批注，序列化 `postilJson` 字符串。 |

## For AI Agents

### Working in this directory
- 批注内容通过 JSON 字符串字段 `postilJson` 持久化，新增/删除后整体回写。
- Form 组件本目录独有（不复用 commonForm）；保持 `okText="发送"`。

### Common patterns
- `Timeline + TimelineItem` 渲染历史
- `useMessage().createConfirm` 二次确认删除

## Dependencies
### Internal
- `/@/components/Modal`, `/@/components/Form`, `/@/api/extend/table`
### External
- `ant-design-vue` (Timeline)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
