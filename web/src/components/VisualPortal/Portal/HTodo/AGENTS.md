<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HTodo

## Purpose
门户运行时"待办统计"卡片。以网格图标 + 数字方式展示工作流待办/已办/抄送数量,点击跳转到对应业务路由,数据来源于 `getFlowTodoCount` 流程接口。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 渲染图标方块、数值与名称;按 `option.rowNumber` 控制每行数量,支持显示/隐藏边框 |

## For AI Agents

### Working in this directory
- 跳转使用 `router-link` + `category` query,保持与列表页 `category` 过滤约定一致。
- 数值字段(`flowDone` / `toBeReviewed` / `flowCirculate`)由 id 匹配回写到 `list[i].num`,新增类别时务必同步 query 字段。

### Common patterns
- 样式通过 `option.valueFontSize/Color/Weight` 与 `option.labelFont*` 双轴控制,主题与其他 H* 组件保持一致。

## Dependencies
### Internal
- `../CardHeader/index.vue`
- `/@/api/onlineDev/portal` (`getFlowTodoCount`)
### External
- Ant Design Vue (`a-card`)、`vue-router` 的 `router-link`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
