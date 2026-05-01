<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# msgMonitor

## Purpose
"消息监控"页面：查看实际消息发送记录（消息类型/来源/标题/发送状态），支持单条删除与一键清空，点击行查看详情。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicTable + 删除/一键清空 + Detail 弹窗 |
| `Detail.vue` | 消息详情弹窗（标题/正文/接收人/发送日志）|

## For AI Agents

### Working in this directory
- 一键清空走 `emptyMsgMonitor`，需 `useMessage().createConfirm` 二次确认。
- 消息类型选项来自 `getMsgTypeList`，与 msgTemplate 共享。

### Common patterns
- `defineOptions({ name: 'msgCenter-msgMonitor' })`。

## Dependencies
### Internal
- `/@/api/msgCenter/msgMonitor`, `/@/api/msgCenter/msgTemplate`
- `/@/store/modules/base`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
