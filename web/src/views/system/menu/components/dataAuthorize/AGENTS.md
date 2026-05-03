<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dataAuthorize

## Purpose
数据权限 drawer — two-tab UI: 方案管理 (data scope schemes) and 字段管理 (field-level rules), shared by all subsystems.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tabs for 方案 / 字段 with separate `useTable` registrations. |
| `SchemeForm.vue` | Edit form for a 方案 (e.g. 全部数据 / 本部门 / 自定义条件). |
| `FieldForm.vue` | Edit form for a field-level data rule. |

## For AI Agents

### Working in this directory
- Tab switch (`onTabsChange`) must reload the active table only; preserve the `destroyInactiveTabPane` flag.
- Reuses 数据连接 button when `type == '2'`; flows through `connectForm/`.
