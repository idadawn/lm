<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# workFlow

## Purpose
工作流 (BPM) feature area — provides the full workflow engine UI: design forms, design flows, launch/monitor/handle approvals, and host built-in business forms (leave, sales, CRM order).

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | Shared `FlowParser`, modals, comment/record widgets (see `components/AGENTS.md`). |
| `entrust/` | 委托/被委托 流程 (delegation) management. |
| `flowCirculate/` | 抄送 (cc'd flows) list. |
| `flowDone/` | 已办 (completed) tasks list. |
| `flowEngine/` | 流程引擎模板 management with import (`.ffe`). |
| `flowLaunch/` | 我发起的 flows. |
| `flowMonitor/` | 流程监控 (admin monitor). |
| `flowQuickLaunch/` | 流程快捷发起入口. |
| `flowTodo/` | 待办 tasks with batch approval. |
| `formDesign/` | 表单设计器 templates with import (`.fff`). |
| `workFlowDetail/` | Detail page rendering `FlowParser` from a query-string config. |
| `workFlowForm/` | Built-in business forms + dynamic-form host. |

## For AI Agents

### Working in this directory
- Status codes are repeated across many index.vue files: 1=等待审核, 2=审核通过, 3=审核退回, 4=流程撤回, 5=审核终止, 6=已被挂起 — keep the exact tag/colors when copying.
- All flow handling pages funnel through `components/FlowParser.vue` (a `BasicPopup`-hosted parser). Don't fork it.

## Dependencies
### Internal
- `/@/api/workFlow/*`, `/@/components/Popup`, `/@/components/Table`, `/@/store/modules/generator`
