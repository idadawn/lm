<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
SFC implementation of the schema-driven dropdown.

## Key Files
| File | Description |
|------|-------------|
| `Dropdown.vue` | `<script setup>` SFC. Builds `<a-dropdown>` overlay menu from `dropMenuList`; per-item `popConfirm` wraps the label in `<a-popconfirm>` (mapping `confirm`→`onConfirm`, `cancel`→`onCancel`); per-item `modelConfirm` triggers `useMessage.createConfirm` with default warning copy `此操作将永久删除该数据, 是否继续？`. |
| `typing.ts` | `DropMenu` shape (event, text, icon, divider, disabled, popConfirm, modelConfirm, onClick). |

## For AI Agents

### Working in this directory
- Default modal-confirm copy is in Chinese — preserve unless caller passes `modelConfirm.title/content`.
- `getPopConfirmAttrs` strips raw `confirm/cancel/icon` props before passing to `<a-popconfirm>`; keep this filter when adding fields.

### Common patterns
- Items render `<i :class="item.icon">` for icon font support; pass an `iconfont` class string.

## Dependencies
### Internal
- `/@/utils/is`, `/@/hooks/web/useMessage`.
### External
- `ant-design-vue` (Dropdown, Menu, Popconfirm), `lodash-es`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
