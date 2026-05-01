<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# feature

## Purpose
默认布局的横切特性入口。挂载锁屏页、BackTop 回到顶部、SettingDrawer、会话超时登录窗，以及 LIMS 专属的「实验室智能问数」助手 (XiaoMei) 嵌入入口。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `LayoutFeatures`：根据 `getUseOpenBackTop`/`getShowSettingButton` 等开关条件渲染 BackTop、LayoutLockPage、SettingDrawer、SessionTimeoutLogin |
| `XiaoMeiAssistant.vue` | 实验室智能问数浮动按钮 + 抽屉/全屏双模式 iframe，嵌入 `http://127.0.0.1:13000`，并通过 `postMessage` 注入 `NLQ_AUTH_CONTEXT`（token/user/permissions）实现 SSO |

## For AI Agents

### Working in this directory
- `XiaoMeiAssistant.vue` 中 `assistantBaseUrl` 目前硬编码本地端口 `13000`，部署到正式环境时需改为环境变量驱动；`postMessage` 目标 origin 必须严格匹配，不要写 `*`。
- 凭证负载来自 `getToken()` 与 `PERMISSIONS_KEY`/`USER_INFO_KEY` 缓存，修改 enum 名后需同步此处。
- BackTop / SettingDrawer / SessionTimeoutLogin 通过 `createAsyncComponent` 懒加载；勿改为 sync import 以免拖慢首屏。

### Common patterns
- 抽屉模式 (`dock`) 与全屏模式 (`fullscreen`) 互斥，切换时需 `nextTick` + `setTimeout` 确保 iframe 已挂载再投递消息。
- 通过 props/computed 拼接 `?embed=1&mode=...` 控制嵌入页面行为。

## Dependencies
### Internal
- `/@/hooks/setting/{useRootSetting,useHeaderSetting}`、`/@/store/modules/user`、`/@/utils/auth`、`/@/enums/cacheEnum`、`/@/utils/factory/createAsyncComponent`。
### External
- `ant-design-vue` (`BackTop`/`Drawer`/`Button`)、`vue`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
