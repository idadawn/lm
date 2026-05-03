<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`FormGenerator` 表单设计器的主体实现。三个顶层 `.vue`：`FormGenerator.vue`（左/中/右三栏布局）、`DraggableItem.vue`（画布单字段拖拽渲染）、`RightPanel.vue`（右侧属性面板路由）。配合 `components/`、`helper/`、`hooks/`、`rightComponents/`、`types/` 子目录构成完整设计器。

## Key Files
| File | Description |
|------|-------------|
| `FormGenerator.vue` | 设计器主组件：左侧组件库（draggable）、中间画布、动作栏（撤销/重做/预览/导入/清空）、右侧 `RightPanel` |
| `DraggableItem.vue` | 画布中的可拖拽字段渲染器，使用 `render.tsx` 工厂；支持复制、删除、嵌套行/列布局 |
| `RightPanel.vue` | 右侧属性面板：根据当前选中字段加载对应 `R*.vue` 配置组件，处理 form attribute / style / script tabs |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 设计器内部对话框/面板：Parser、FieldModal、FormAttrPane、FormScript、PreviewModal、StylePane、StyleScript（见 `components/AGENTS.md`） |
| `helper/` | componentMap、config（默认配置）、render、transform、utils、db（drawing list 管理） — 设计器的"逻辑核"（见 `helper/AGENTS.md`） |
| `hooks/` | `useDynamic`（远端/字典数据源）与 `useRedo`（撤销重做）composables（见 `hooks/AGENTS.md`） |
| `rightComponents/` | 60+ 个 `R*.vue` 字段属性配置面板 + RTable 与 components 子目录（见 `rightComponents/AGENTS.md`） |
| `types/` | `GenItem` / `ItemCfg` 等设计器 schema 类型（见 `types/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 三栏拖拽通过 `vuedraggable` 的 `group: { name: 'componentsGroup', pull: 'clone', put: false }` 实现"克隆而非移动"。
- 撤销/重做基于 `useRedo` 的不可变快照栈，新增对画布的写操作必须 push 一次快照。
- 字段属性面板按 `__config__.tag` 路由，新增控件类型须在 `helper/config.ts` 注册并在 `rightComponents/` 添加配套 `R*.vue`。

### Common patterns
- TSX + SFC 混用；render 工厂位于 `helper/render.ts`，供 `DraggableItem` 与 `Parser` 共享。
- 通过 `provide/inject` 暴露 generator 上下文给嵌套行/列子组件。

## Dependencies
### Internal
- `/@/store/modules/generator`、`/@/components/Jnpf`、`/@/utils/uuid`、`/@/utils/jnpf`
### External
- `vuedraggable`、`ant-design-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
