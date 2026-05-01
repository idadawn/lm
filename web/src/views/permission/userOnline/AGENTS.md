<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# userOnline

## Purpose
在线用户监控：列出当前已登录用户（账号/IP/登录时间/设备），支持单条踢出与批量"强制下线"。仅一个表格页，无表单弹窗。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`getOnlineUser` + `batchDelOnlineUser` + `deleteOnlineUser`，`pagination=false` 一次拉满 |

## For AI Agents

### Working in this directory
- `rowKey: 'userId'`，批量操作通过 `getSelectRowKeys()` 提交。
- 强制下线属破坏性操作，依赖后端将 token 失效；改造时务必保留 `createConfirm` 二次确认。
- `defineOptions({ name: 'permission-userOnline' })`。

### Common patterns
- `BasicTable` + `useTable` + `useSearchForm`，唯一搜索项一般为账号/姓名。

## Dependencies
### Internal
- `/@/api/permission/onlineUser`、`/@/components/Table`、`/@/hooks/web/useMessage`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
