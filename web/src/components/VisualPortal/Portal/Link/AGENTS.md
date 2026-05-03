<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Link

## Purpose
门户卡片通用跳转包装组件。根据 `linkType` / `type` / `urlAddress` 自动决定渲染为 `div`(无链接)、`a`(外链)或 `router-link`(内部路由),并处理大屏(dataV)模块跳转、外链加密 query (`/externalLink`)、token 注入等场景。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `<component :is="routerType">` 动态切换组件;`init()` 计算最终 `to`,处理 dataV 与 externalLink |

## For AI Agents

### Working in this directory
- `type==6` 走大屏视图 (`globSetting.dataVUrl + view/${moduleId}`),token 必须由 `getToken()` 注入。
- `type==7` 或 `linkType==2`(外链):新窗口直接替换占位符 `${dataV}` / `${jnpfToken}`;同窗口经 `/externalLink` 路由 + Base64 加密。
- 默认 slot 必须由调用方提供(图标、文字等),Link 不渲染任何展示内容。

### Common patterns
- 被同目录 `HText`、`HIcon`、`HBoard` 等卡片包装,统一跳转语义。

## Dependencies
### Internal
- `/@/utils/is` (`isUrl`)、`/@/utils/auth` (`getToken`)、`/@/utils/cipher` (`encryptByBase64`)
- `/@/hooks/setting` (`useGlobSetting`)
### External
- `vue-router` 的 `router-link`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
