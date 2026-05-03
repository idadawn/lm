<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
`FormGenerator` 设计器的两个核心 composables：`useDynamic` 处理字段的动态数据源（静态/字典/远端接口）配置展示与拉取；`useRedo` 维护设计器画布的撤销/重做栈。

## Key Files
| File | Description |
|------|-------------|
| `useDynamic.ts` | 处理动态选项面板：`dataTypeOptions`、`valueOptions`，加载字典 (`getDictionaryDataSelector`) 与接口 (`getDataInterfaceRes`) 的真实数据，刷新右侧面板下拉 |
| `useRedo.ts` | 撤销/重做栈：`pushHistory(snapshot)`、`undo()`、`redo()`、`getCanUndo`/`getCanRedo` 计算属性 |

## For AI Agents

### Working in this directory
- `useRedo` 通过 `cloneDeep` 整个 drawing list 入栈，操作大表单时注意性能；新增高频 mutation 应去抖后再 push。
- `useDynamic(activeData)` 接收当前选中字段的响应式引用 — 修改 `activeData` 内字段会立即反映到画布。
- 不要在 hook 内直接调用画布 DOM；通过 store / drawing list 改动驱动重渲染。

### Common patterns
- 字典与接口选项缓存使用本地 ref，避免每次面板切换重复请求。
- ID 选项 `static / dictionary / dynamic` 是字段元数据约定，与 `Parser.vue` 中数据源处理逻辑保持一致。

## Dependencies
### Internal
- `/@/utils/uuid`、`/@/api/systemData/dictionary`、`/@/api/systemData/dataInterface`
- `../helper/db`、`../helper/config`
### External
- `vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
