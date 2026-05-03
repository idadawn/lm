<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# PopupAttr

## Purpose
`JnpfPopupAttr` 弹窗选择器属性回填组件入口。配合 `JnpfPopupSelect` 使用：在主表单选择数据后，用此组件展示该数据其它字段（按 `relationField + showField` 取值）；可选是否随主字段一起入库。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(PopupAttr)` 后导出 `JnpfPopupAttr` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件 SFC（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 不要在此组件中发起接口请求——数据完全来自 `useGeneratorStore.getRelationData` 与 `emitter.setRelationData`。
- 与 `RelationFormAttr` 行为高度类似，但来源是 `PopupSelect`（接口数据）而非 `RelationForm`（表单数据）。

### Common patterns
- `withInstall` 全局注册；只读 input 或 `<p>` 详情态二选一。

## Dependencies
### Internal
- `/@/store/modules/generator`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
