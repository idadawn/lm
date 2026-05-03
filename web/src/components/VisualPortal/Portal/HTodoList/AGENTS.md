<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HTodoList

## Purpose
门户运行时"待办列表"卡片。展示当前用户最近 7 条待办流程,点击进入 `/workFlowDetail`(经 Base64 加密 query 传递任务上下文)。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 拉取 `getFlowTodoList`,渲染 `fullName + creatorTime`,处理空数据占位 |

## For AI Agents

### Working in this directory
- 跳转必须经 `encryptByBase64` 加密 config,不要明文暴露任务对象。
- 列表硬截断 7 条(`slice(0, 7)`);若需可配置,通过 `activeData.option` 暴露而非局部魔数。

### Common patterns
- 与 HTodo 配对使用:HTodo 显示统计数,本组件显示明细。
- 时间使用 `dayjs` 格式化为 `YYYY-MM-DD`。

## Dependencies
### Internal
- `../CardHeader/index.vue`
- `/@/api/onlineDev/portal` (`getFlowTodoList`)
- `/@/utils/cipher` (`encryptByBase64`)
### External
- `dayjs`、`vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
