<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# RelationForm

## Purpose
`JnpfRelationForm` 关联表单选择器入口。区别于 `PopupSelect`：数据源是动态模型（`modelId`）的提交记录，而非数据接口；选中后联动 `JnpfRelationFormAttr` 回填字段，并支持点击只读项打开 `Detail` 详情视图。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(RelationForm)` 后导出 `JnpfRelationForm` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC + props（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 该组件依赖 `views/common/dynamicModel/list/detail` 渲染只读详情，路径不可循环——保持单向引用。
- 与 `RelationFormAttr` 通过 `emitter` + `useGeneratorStore.relationData` 联动，禁止直接 prop 传值。

### Common patterns
- 弹窗模式：`dialog`/`drawer`，无 popover；只支持单选（`type: 'radio'`）。

## Dependencies
### Internal
- `/@/api/onlineDev/visualDev`、`/@/store/modules/generator`、`/@/views/common/dynamicModel/list/detail`、`/@/components/Modal`
### External
- `ant-design-vue`、`@ant-design/icons-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
