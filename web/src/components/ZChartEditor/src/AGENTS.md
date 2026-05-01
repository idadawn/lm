<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
ZChartEditor 顶层组件实现。负责装配 tree/dashboard/elements 三栏,根据 `params.type`(`edit` / 预览)切换面板可见性,并把画布缩放比例同步到 `chartStore.scale`。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 三栏布局 + 缩放计算(基于 1920×1080 基准);通过 `dashboardRef.addNode` 桥接 tree 拖入事件 |
| `props.ts` | 顶层组件 props:`params{id,type}` / `source{nodes}` / `dragItem` / `onClick` |

## For AI Agents

### Working in this directory
- `chartStore.setLayoutEditType(type)` 在 isEdit computed 内调用,带副作用,避免重复触发。
- 缩放计算 (`width/1920`、`height/1080`) 当前被注释掉(`chartStore.setScale` 暂未启用),改动前确认是否影响 dashboard 的 `parentScaleX/Y`。
- 不要在此处直接发起 API,逻辑下沉到 `useEditor` / `chartStore`。

### Common patterns
- `defineOptions({ name: 'ZEditorDashboard' })` 与子组件同名,devtools 中通过路径区分。

## Dependencies
### Internal
- `../components/editorDashboard|editorTree|editorElements`
- `/@/store/modules/chart`
### External
- Vue 3 Composition API

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
