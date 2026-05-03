<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Organize

## Purpose
组织架构相关选择器集合入口：组织、部门、岗位、角色、分组、用户（单/多）。`index.ts` 统一 `withInstall` 导出 7 个 `JnpfXxxSelect` 组件，供表单设计器与权限/审批场景使用。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 导出 `JnpfOrganizeSelect` / `JnpfDepSelect` / `JnpfPosSelect` / `JnpfGroupSelect` / `JnpfRoleSelect` / `JnpfUserSelect` / `JnpfUsersSelect` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 7 个选择器 SFC + 共享 props（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 新增选择器：先在 `src/props.ts` 加 `xxxSelectProps`（基于 `baseProps` 扩展），再添加 SFC 并在 `index.ts` 用 `withInstall` 注册。
- 所有选择器组件命名前缀 `Jnpf`，与全局注册保持一致。

### Common patterns
- 弹窗 + 树/列表 + 搜索 + 已选区的"穿梭"交互模式；返回 id 数组。

## Dependencies
### Internal
- `/@/api/permission/*`、`/@/store/modules/organize`、`/@/components/Tree`、`/@/components/Container`、`/@/components/Modal`
### External
- `ant-design-vue`、`@ant-design/icons-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
