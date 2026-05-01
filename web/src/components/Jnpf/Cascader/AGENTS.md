<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Cascader

## Purpose
`JnpfCascader` 级联选择器包装目录，导出全局组件。封装 `ant-design-vue` 的 `Cascader`，统一字段名（`id`/`fullName`/`children`）与策略 `SHOW_CHILD`。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | barrel：导出 `JnpfCascader` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC 实现（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 默认 `fieldNames` 必须保留 `{ value: 'id', label: 'fullName', children: 'children' }`，与后端通用列表 DTO 一致。
- 不要在此层引入 `ant-design-vue`，保持 barrel 纯净。

## Dependencies
### Internal
- `/@/utils` — `withInstall`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
