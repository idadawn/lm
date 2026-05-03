<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# chat

## Purpose
顶部栏即时通讯 (IM) 入口。提供联系人/会话抽屉与点对点聊天窗口，承载文本、图片、表情消息收发，配合后端 WebSocket 推送实现实时聊天。

## Key Files
| File | Description |
|------|-------------|
| `ChatDrawer.vue` | 联系人/会话列表抽屉：搜索、未读徽标、点击进入 `Im` 对话 |
| `Im.vue` | 一对一聊天面板：头像、昵称、消息列表（自动滚到底部）、文本/图片渲染、表情面板、发送框 |
| `emoji.json` | 表情面板使用的 emoji 图集元数据（key/标签等） |

## For AI Agents

### Working in this directory
- 消息流依赖全局 WebSocket（`/@/hooks/web/useWebSocket`），不要在组件内单独建立连接；新消息通过事件总线推入。
- 头像、附件 URL 拼装统一使用 `apiUrl`（来自 `useGlobSetting`），避免硬编码 host。
- 文本消息支持 `v-html`，发送前必须经过后端 XSS 过滤；如新增富文本类型请扩展 `messageType`。

### Common patterns
- 两个组件均为 `<script lang="ts">` Options 风格（与项目其余 vben 代码一致），状态保留在组件内 ref，不入 store。
- 使用 `chatListRef` 在新消息后 `scrollTop = scrollHeight`。

## Dependencies
### Internal
- `/@/hooks/setting/index`（apiUrl）、`/@/store/modules/user`、`/@/api/oa/im`（聊天接口）。
### External
- `ant-design-vue` (`Avatar`/`Input`)、emoji JSON 静态资源。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
