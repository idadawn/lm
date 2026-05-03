<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# InputTable

## Purpose
`JnpfInputTable` 子表（行内表格）控件包装目录。LIMS 表单中常用于"批量样品"等多行结构数据录入；可在每个单元格内动态渲染 Jnpf 组件（`JnpfRelationForm`、`JnpfPopupSelect` 以及任意 `__config__.tag`）。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | barrel：导出 `JnpfInputTable` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC 实现（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 仅维护导出；不要在此引入 FormGenerator 内部类型，保持 barrel 简洁。

## Dependencies
### Internal
- `/@/utils` — `withInstall`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
