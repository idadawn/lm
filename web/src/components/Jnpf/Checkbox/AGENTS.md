<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Checkbox

## Purpose
`JnpfCheckbox` / `JnpfCheckboxSingle` 多选与单选包装目录。前者基于 `CheckboxGroup` 渲染选项数组，后者用于布尔型单选场景。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | barrel：导出 `JnpfCheckbox` 与 `JnpfCheckboxSingle` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC 实现 + props（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 同时维护两个组件名导出（`JnpfCheckbox`/`JnpfCheckboxSingle`），FormGenerator 通过名字加载，请勿合并。
- 字段名默认 `{ value: 'id', label: 'fullName', disabled: 'disabled' }`，保持与后端列表 API 一致。

## Dependencies
### Internal
- `/@/utils` — `withInstall`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
