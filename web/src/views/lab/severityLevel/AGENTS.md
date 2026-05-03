<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# severityLevel

## Purpose
"特性等级管理"页面：维护外观特性的等级定义（用于 AI 匹配与特征分类）。等级名称必须唯一，支持拖拽排序。橙色主题。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 标题/新增 + 可拖拽卡片网格（vuedraggable）|

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 等级编辑弹窗 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 拖拽 `@end="handleDragEnd"` 后调用排序保存接口。
- 名称唯一约束由前后端共同把关。
- 与 `appearance` 模块联动：等级值用于 AI 匹配置信度。

### Common patterns
- `vuedraggable` + Tailwind grid + `ghost-class`/`chosen-card`。

## Dependencies
### Internal
- `/@/api/lab/severityLevel`
### External
- `vuedraggable`, `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
