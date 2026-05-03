<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# extend

## Purpose
Catch-all for "extend" business module APIs — auxiliary feature endpoints under `/api/extend/*` plus a few sibling integrations. Each file is independent and maps to a feature page (knowledge document tree, email, employee directory, sales/orders, project gantt, schedule calendar, big-data dashboard, generic table data).

## Key Files
| File | Description |
|------|-------------|
| `bigData.ts` | 大屏 / big-data dashboard data fetch. |
| `document.ts` | 知识管理: tree + folder + file CRUD (`/api/extend/Document`). |
| `documentPreview.ts` | Office/PDF preview URL generation. |
| `email.ts` | 邮件 send / inbox / detail. |
| `employee.ts` | 员工目录查询。 |
| `order.ts` | 订单列表 / 详情. |
| `projectGantt.ts` | 项目甘特图数据. |
| `saleOrder.ts` | 销售订单 CRUD. |
| `schedule.ts` | 日程 / 排班. |
| `table.ts` | 通用表格数据接口 (used by online-dev / report widgets). |

## For AI Agents

### Working in this directory
- Each file has its own `enum Api { Prefix = '/api/extend/...' }` — match backend `Poxiao.Apps` / extend modules.
- Document API uses RESTful verbs against the same prefix (`POST /Document`, `PUT /Document/{id}`, `DELETE /Document/{id}`); preserve shape.
- Use `/@/api/system/dictionary` (not extend) for dict lookups.

### Common patterns
- Most modules export `getXxxList`, `create`, `update`, `getInfo`, `delXxx` plus feature-specific actions.

## Dependencies
### Internal
- `/@/utils/http/axios`, `/@/enums/httpEnum`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
