<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FormGenerator

## Purpose
JNPF 风格的可视化表单设计器（拖拽式 form builder） — 检测室低代码表单设计核心组件。左侧组件库 → 中间画布（拖拽/排序）→ 右侧属性面板，外加运行时解析器 (`Parser.vue`) 把设计 JSON 渲染成可填表单。供工作流表单设计、报表设计、动态业务表单等模块复用。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 引入 `style/index.less` + `style/rightPanel.less`，导出 `FormGenerator` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 设计器主组件、Parser、components/helper/hooks/rightComponents/types（见 `src/AGENTS.md`） |
| `style/` | 设计器与右侧面板的 Less 样式（见 `style/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 该组件依赖外部包 `vuedraggable` 实现拖拽；不要替换为其它库，下游 `__config__.dragDisabled` 等元数据是其约定。
- 设计器输出/解析的核心 schema 形如 `{ __config__: ItemCfg, __vModel__: 'fieldName', ...componentProps }`，详见 `src/types/genItem.ts`。
- 修改组件库元数据需同时更新 `src/helper/config.ts`（默认列表）与 `src/helper/componentMap.ts`（组件映射）。

### Common patterns
- 通过 `pinia` 的 `useGeneratorStore` 在设计器与解析器间共享 drawing list / undo-redo 状态。
- 业务方常用组合：设计器保存 JSON → 后端持久化 → 运行时 `Parser.vue` 反序列化渲染。

## Dependencies
### Internal
- `/@/store/modules/generator`、`/@/components/Jnpf`、`/@/components/Drawer`、`/@/api/systemData/*`
### External
- `vuedraggable`、`ant-design-vue`、`lodash-es`、`dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
