<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# PopupSelect

## Purpose
`JnpfPopupSelect` 数据接口弹窗选择器入口。基于 `interfaceId`（数据接口）+ `templateJson`（参数模板）+ `columnOptions`（列）构建可搜索 + 分页表格弹窗，支持 dialog/drawer/popover 三种弹出形态，单/多选回填关联字段。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(PopupSelect)` 后导出 `JnpfPopupSelect` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件 SFC + props（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- `interfaceId` 通过 `getDataInterfaceDataSelect` 取列表，`getDataInterfaceDataInfoByIds` 回填详情；改字段名需同步 `/@/api/systemData/dataInterface`。
- 选中后通过 `emitter.emit('setRelationData', {...})` 通知 `JnpfPopupAttr` 进行属性回填——保持事件 payload 含 `jnpfRelationField`。

### Common patterns
- 与 `RelationForm` 结构同源（弹窗 + 搜索 + 表格），但数据源是数据接口而非动态模型表单。

## Dependencies
### Internal
- `/@/api/systemData/dataInterface`、`/@/store/modules/generator`、`/@/components/Modal`
### External
- `ant-design-vue`、`@ant-design/icons-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
