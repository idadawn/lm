<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# TreeSelect

## Purpose
Jnpf 表单的树形下拉选择器封装。在 `ant-design-vue` 的 `TreeSelect` 之上加入字段名映射（默认 `id` / `fullName` / `children`）、图标渲染、最末级节点限制等业务约束。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 通过 `withInstall` 导出 `JnpfTreeSelect`，并暴露 `TreeSelectProps` 类型。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件主体与 props 定义 (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 类型导出必须使用 `Partial<ExtractPropTypes<typeof treeSelectProps>>` 以保持可选字段兼容低代码引擎。
- 新增 prop 同时更新 `src/props.ts` 默认值，避免在 setup 中再次硬编码默认值。

### Common patterns
- `withInstall` + 类型再导出，是 `Jnpf/*` 子模块的统一对外形式。

## Dependencies
### Internal
- `/@/utils`、`/@/components/Tree`
### External
- `ant-design-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
