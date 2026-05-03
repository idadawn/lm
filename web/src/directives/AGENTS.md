<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# directives

## Purpose
Vue 3 全局/局部自定义指令集合。承载权限控制、点击外部、加载遮罩、连续点击、波纹效果等通用 DOM 行为。`index.ts` 仅注册需要全局可用的指令（auth、loading），其余按需 import。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `setupGlobDirectives(app)` 入口，注册 `v-auth` 和 `v-loading` 全局指令 |
| `permission.ts` | `v-auth="'btn-edit'"` 按钮级权限：根据 `userStore.getPermissionList` 中 `modelId.button[].enCode` 校验，无权限直接 `removeChild` |
| `loading.ts` | `v-loading` 指令，挂载/卸载 loading 遮罩 DOM |
| `clickOutside.ts` | `v-click-outside` 指令：单 document 监听 mouseup/mousedown，diff popperRef 与 excludes 后触发回调 |
| `repeatClick.ts` | `v-repeat-click` 长按循环触发指令 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `ripple/` | Material 风波纹点击指令（含 less 样式） (see `ripple/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 新增需要全局可用的指令时，必须在 `index.ts` 增加 `setupXxxDirective(app)` 调用；按需指令保持单独 import 即可。
- 权限指令 (`v-auth`) 依赖路由 `meta.modelId`，使用前确认目标视图的路由元信息已配置。
- `permission.ts` 在模块加载时即读取一次 `permissionList`，注意热更新场景需重启 dev server 才能生效——这是历史设计，请勿误改为响应式以免触发循环依赖。

### Common patterns
- 指令文件导出 `setupXxxDirective(app: App)` 工厂；默认导出原始 Directive 对象用于按需注册。
- 副作用清理走 Vue 3 钩子（`unmounted`、`beforeMount/updated`），并通过 `Map` 注册节点列表（见 `clickOutside.ts`）。

## Dependencies
### Internal
- `/@/store/modules/user`（permission.ts）、`/@/utils/domUtils` 与 `/@/utils/is`。
### External
- `vue` (`Directive`、`DirectiveBinding`、`App`)。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
