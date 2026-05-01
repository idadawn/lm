<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ProjectGantt

## Purpose
项目甘特模块 DTO。覆盖"项目（Gantt）"与其下"任务（Task）"两层结构，含树形视图。

## Key Files
| File | Description |
|------|-------------|
| `ProjectGanttCrInput.cs` / `ProjectGanttUpInput.cs` | 项目创建/更新（编码/状态/开始时间/进度...） |
| `ProjectGanttInfoOutput.cs` / `ProjectGanttListOutput.cs` | 项目详情 / 列表 |
| `ProjectGanttTaskCrInput.cs` / `ProjectGanttTaskUpInput.cs` | 任务创建/更新（含 parentId 自关联） |
| `ProjectGanttTaskInfoOutput.cs` / `ProjectGanttTaskListOutput.cs` | 任务详情 / 列表 |
| `ProjectGanttTaskTreeViewOutput.cs` | 任务树视图（甘特图渲染用） |

## For AI Agents

### Working in this directory
- 项目状态约定：1-进行中、2-已暂停。
- 任务自关联（parentId）支持多级；前端 gantt 通过 `TaskTreeViewOutput.children` 渲染。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
