<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# RelationFormAttr

## Purpose
`JnpfRelationFormAttr` 关联表单属性回填组件入口。配合 `JnpfRelationForm` 使用——后者选中动态模型记录后，此组件按 `relationField + showField` 显示该记录字段值，可选 `isStorage` 决定是否入库。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(RelationFormAttr)` 后导出 `JnpfRelationFormAttr` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 行为与 `PopupAttr` 同构，仅来源不同（动态模型 vs 数据接口）；维护时两者建议同步演进。
- 不要发起请求——数据来自 `useGeneratorStore.getRelationData` 与 `emitter.setRelationData` 事件。

### Common patterns
- `withInstall` 全局注册；`isStorage` 切换占位文案与是否参与表单 update。

## Dependencies
### Internal
- `/@/store/modules/generator`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
