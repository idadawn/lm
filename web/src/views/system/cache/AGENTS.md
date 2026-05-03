<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# cache

## Purpose
系统缓存 (Cache) 查看与清理：列出 Redis/内存中所有缓存键，显示过期时间、大小，支持单条删除、查看内容、一键清空全部。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`getCacheList` + `delCache` + `delAllCache`，`toFileSize` 格式化大小 |
| `Form.vue` | 缓存内容查看弹窗（`BasicPopup`） |

## For AI Agents

### Working in this directory
- "清空所有"是高危操作，需经 `createConfirm` 二次确认。
- 列 `name` 用 `<a @click="handleView">` 触发查看弹窗。
- `defineOptions({ name: 'system-cache' })`。

### Common patterns
- `format: 'date|YYYY-MM-DD HH:mm'` 用于过期时间列。
- `format: toFileSize` 格式化字节数。

## Dependencies
### Internal
- `/@/api/system/cache`、`/@/utils/jnpf` (`toFileSize`)、`/@/components/Popup`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
