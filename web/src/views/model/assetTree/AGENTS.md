<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# assetTree

## Purpose
"资产树"页面：树形管理资产/模型目录节点，支持新建目录、节点表单、成员授权。当前 `index.vue` 与 `kpi/statusManagement` 风格相似（BasicTable 树形 + DepForm + Member）。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 树形 BasicTable + 新建目录按钮 + 操作列 |
| `Form.vue` | 资产/模型基本信息表单 |
| `DepForm.vue` | 目录新增表单 |
| `Member.vue` | 成员授权弹窗 |

## For AI Agents

### Working in this directory
- API 走 `/@/api/targetDirectory` 系列（与 KPI metric 模块共享）；如需独立 API 应在落地时切换。
- `defineOptions({ name: 'permission-organize' })` 与其它模块复用，路由 keep-alive 时注意冲突。

### Common patterns
- BasicTable + `useModal` 注册 DepForm/Form/Member。

## Dependencies
### Internal
- `/@/api/targetDirectory`, `/@/components/Table`, `/@/components/Modal`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
