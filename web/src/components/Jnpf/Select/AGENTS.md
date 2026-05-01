<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Select

## Purpose
`JnpfSelect` 通用下拉组件入口。包装 `a-select` 并统一默认 `fieldNames`（`id`/`fullName`/`disabled`），供表单设计器与各 JnpfXxx 系列复用作为基础单/多选下拉。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(Select)` 后导出 `JnpfSelect` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC + props（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 默认 `fieldNames` 与 Radio/Organize 系列保持一致——后端列表多用 `id` + `fullName`。
- `getPopupContainer = () => document.body`：弹层挂 body，避免在 modal/drawer 内被 overflow 截断；保留此默认。

### Common patterns
- `withInstall` 全局注册，组件名 `JnpfSelect`；通过 `defineExpose` 暴露 `getSelectRef`。

## Dependencies
### Internal
- `/@/utils`、`/@/hooks/core/useAttrs`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
