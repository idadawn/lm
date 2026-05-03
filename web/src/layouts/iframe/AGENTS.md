<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# iframe

## Purpose
处理 `route.meta.frameSrc` 类型的外链/外部页面嵌入。通过 keep-alive + `v-show` 切换实现「打开过的 iframe 页面不重新加载」，与多页签生命周期联动。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `FrameLayout`：`getFramePages` 收集所有带 `frameSrc` 的路由，命中过的 (`hasRenderFrame`) 用 `v-show` 切换；底层用 `/@/views/basic/iframe/index.vue` (FramePage) 渲染 iframe |
| `useFrameKeepAlive.ts` | 收集所有 frame 路由 (`getAllFramePages`)、与多页签 store 求交集得到 `getOpenTabList`，提供 `hasRenderFrame`/`showIframe` 判定 |

## For AI Agents

### Working in this directory
- 仅当 `getShowMultipleTab` 开启时才 keep-alive 多个 iframe；关闭多页签时仅当前路由的 frame 被渲染。
- `getAllFramePages` 通过 `uniqBy(name)` 去重，`route.name` 必须唯一且稳定，否则会出现 iframe 重复挂载。
- 不要在此目录写实际的 iframe 标签：实现位于 `/@/views/basic/iframe/index.vue`。

### Common patterns
- `computed(() => unref(getFramePages).length > 0)` 决定整个 `<div>` 是否渲染，避免 0 frame 时空容器抢占视口。

## Dependencies
### Internal
- `/@/views/basic/iframe/index.vue`、`/@/store/modules/multipleTab`、`/@/hooks/setting/useMultipleTabSetting`、`/@/router/types`。
### External
- `vue`、`vue-router`、`lodash-es` (`uniqBy`)。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
