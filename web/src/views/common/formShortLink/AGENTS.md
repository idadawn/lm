<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# formShortLink

## Purpose
表单/列表短链分享运行时入口。免登录的对外访问页：根据加密的 `query.encryption` 解出 `modelId` 与类型 (form / list)，动态加载对应运行时视图，对外发布在线表单或数据列表。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 短链入口路由：`decryptForShortLink` 解密 -> `getConfigData` 取配置 -> 根据 `type` 用 `markRaw` 切换 `Form` 或 `List` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `form/` | 短链表单填报页（含密码锁、二维码分享）（见 `form/AGENTS.md`） |
| `list/` | 短链列表页（含密码锁、子表展开）（见 `list/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 此入口为公开访问，不挂任何登录守卫；新增逻辑需考虑未登录上下文（不要依赖 `useUserStore`）。
- 校验失败统一 `router.replace('/404')` + `useTabs.close`，避免在异常分支保留半渲染界面。
- 加密载荷由后端配合，结构 `{ modelId, type }`，扩展字段需同步 `decryptForShortLink` 与后端短链生成器。

### Common patterns
- 使用 `markRaw` 避免动态组件被深度响应化导致性能问题。
- `state.encryption` 传给子页用于二次接口鉴权（带签名调用 `getConfigData`）。

## Dependencies
### Internal
- `/@/api/onlineDev/shortLink`
- `/@/utils/jnpf` (`decryptForShortLink`)
- `/@/hooks/web/{useMessage,useTabs}`
### External
- `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
