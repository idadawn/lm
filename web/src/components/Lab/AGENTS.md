<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Lab

## Purpose
检测室（Lab）业务专用组件目录。当前仅包含 `UnitSelect.vue`，提供基于「单位维度（categoryId）」筛选的单位选择器，对接 `/api/lab/unit` 中的单位库。

## Key Files
| File | Description |
|------|-------------|
| `UnitSelect.vue` | `a-select` 封装；按 `categoryId` 调 `getUnitsByCategory`，未指定时调 `getAllUnitsGroupedByCategory` 并 flatten。emit `update:modelValue` 与 `change(value, unit)` 返回完整 `UnitDefinition`。 |

## For AI Agents

### Working in this directory
- 选项 `displayName` 来自后端拼接（一般为 `name(symbol)`），前端不再二次拼接。
- 响应同时支持 `{ data: ... }` 包裹与裸数据，沿用 `(response as any)?.data || response` 兜底写法。
- 新增 lab 业务组件请放在本目录，与通用组件（`Jnpf`、`Modal` 等）保持隔离。

### Common patterns
- `filterOption` 使用 `option.children` 做大小写不敏感匹配。
- 通过 `watch(props.categoryId, loadUnits)` 维度切换时自动重载。

## Dependencies
### Internal
- `/@/api/lab/unit`（`getUnitsByCategory`、`getAllUnitsGroupedByCategory`）
### External
- `ant-design-vue`、`vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
