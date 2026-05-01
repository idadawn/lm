<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
ZEditor 子组件聚合层。导出节点抽屉表单 (`editorForm`) 与节点树 (`editorTree`),供 `src/index.vue` 与外部消费。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall` 包装 `EditorForm` / `EditorTree` 并以 `ZEditorForm` / `ZEditorTree` 命名导出 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `editorForm/` | 节点抽屉表单 (Tab:数据/分级/通知 + 规则列表) (see `editorForm/AGENTS.md`) |
| `editorTree/` | 节点拖拽树 (see `editorTree/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 注意 `ZEditorForm` / `ZEditorTree` 与 `ZChartEditor/components/index.ts` 的导出同名,但二者属于不同组件命名空间,使用 import 路径而非裸组件名引用。

### Common patterns
- 子目录均使用 `props.ts` 抽离 prop 定义;`defineOptions({ name: 'ZEditor*' })` 风格统一。

## Dependencies
### Internal
- `/@/utils` (`withInstall`)
### External
- Vue 3 SFC

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
