<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Subsystem-scoped permission and menu modules opened from the parent menu page. Each subdirectory is a self-contained drawer/popup feature.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `buttonAuthorize/` | 按钮权限 drawer (see `buttonAuthorize/AGENTS.md`). |
| `columnAuthorize/` | 列权限 drawer with batch + connect (see `columnAuthorize/AGENTS.md`). |
| `connectForm/` | 数据连接 modal + table/field pickers (see `connectForm/AGENTS.md`). |
| `dataAuthorize/` | 数据权限：方案/字段管理 (see `dataAuthorize/AGENTS.md`). |
| `formAuthorize/` | 表单权限 drawer with batch + connect (see `formAuthorize/AGENTS.md`). |
| `menu/` | 菜单 (Web/App) management popup (see `menu/AGENTS.md`). |
| `portal/` | 门户 management with portal/transfer modals (see `portal/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Each child receives `moduleId` (subsystem id) via `useDrawerInner`/`useModalInner` data; do not introduce a parallel pinia store.
- Authorize drawers share the `connectForm/` data-link concept (button "数据连接") — reuse it instead of duplicating.
