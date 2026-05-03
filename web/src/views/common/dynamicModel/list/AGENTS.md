<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# list

## Purpose
动态模型列表运行时（dynamic model list）。基于在线开发模块（onlineDev/visualDev）生成的列配置渲染查询表格、左树、子表，并组合表单弹层 / 详情弹层完成增删改查与高级查询、行内编辑、流程联动等动态业务能力。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表主页面：左树 + 搜索表单 + `BasicTable`，按 `columnData.type` 切换列表/树形/子表/行编辑模式 |
| `Form.vue` | 通用表单容器（Popup/Modal/Drawer 三态），包裹 `Parser` 渲染动态字段并提交 |
| `CustomForm.vue` | 自定义模式表单弹层（用于工作流或自定义视图绑定的表单提交场景） |
| `ChildTableColumn.vue` | 主表行展开内嵌子表列渲染（非 tabs 风格的子表展示） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `detail/` | 只读详情解析器与字段渲染（见 `detail/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 字段配置与按钮权限来源于 `columnData`（含 `searchList`、`btnsList`、`useBtnPermission`），不要硬编码列结构。
- API 走 `/@/api/onlineDev/visualDev` 与 `/@/api/systemData/dataInterface`，提交后调用 `getDataChange` 触发联动。
- 行编辑模式（`type === 4`）依赖 `jnpf-*` 前缀控件渲染单元格，新增字段类型需同时更新 `bodyCell` slot 与 `Parser`。

### Common patterns
- 通过 `usePopup` / `useModal` / `useDrawer` 三态弹层注册同一份 `Parser`，由 `formConf` 决定容器宽度与按钮文案。
- `hasBtnP('btn_xxx')` 控制按钮按权限显示；`isPreview` 旁路权限用于在线设计预览。

## Dependencies
### Internal
- `/@/api/onlineDev/visualDev`, `/@/api/systemData/dataInterface`
- `/@/components/{Popup,Modal,Drawer,Form,Table,LeftTree,PrintDesign}`
- `./detail/Parser.vue`（详情态共享解析器）
### External
- `ant-design-vue`, `lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
