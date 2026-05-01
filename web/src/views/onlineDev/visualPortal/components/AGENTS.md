<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`visualPortal` 列表页的内联子弹窗组件。当前仅一个发布弹窗，将单个门户同步到 Web 端 / APP 端的应用菜单。

## Key Files
| File | Description |
|------|-------------|
| `ReleaseModal.vue` | 同步门户弹窗：勾选 PC/APP，选择 `pcModuleParentId` / `appModuleParentId` 应用，调用 `release` API |

## For AI Agents

### Working in this directory
- 仅服务父级 `visualPortal/index.vue`，不要在其他视图中复用；如需通用门户设计组件请使用 `/@/components/VisualPortal/Design`。
- 已发布的端 (`record.pcIsRelease` / `record.appIsRelease`) 自动隐藏对应表单项。

### Common patterns
- `BasicModal` + `useModalInner` 双向通讯，`emit('reload')` 触发父表刷新。
- `JnpfSelect` 支持 multiple 选择应用节点。

## Dependencies
### Internal
- `/@/api/onlineDev/portal` (`release`)、`/@/components/Modal`
### External
- `@ant-design/icons-vue`、`ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
