<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# map

## Purpose
百度地图（BMap）集成演示页面，通过动态加载远端 JS SDK 在容器中创建地图实例，是 `extend` 模块下第三方地图能力的最小示例。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 使用 `defineComponent` (Options API) + `useScript` 异步加载 `https://api.map.baidu.com/getscript?...` SDK，`onMounted` 初始化 `BMap.Map`，居中北京（116.404, 39.915），开启滚轮缩放。 |

## For AI Agents

### Working in this directory
- 百度地图 ak（API Key）以常量 `BAI_DU_MAP_URL` 内联，生产环境应迁出到 `.env` 配置；改 ak 时一并替换。
- `useScript` 已经做去重，无需手动管理 `<script>` 注入；`toPromise()` 等待加载完成。
- 改用 vue3 `<script setup>` 时注意保留 `name: 'extend-map'`（路由 keep-alive 需要）。

### Common patterns
- `wrapRef` + `nextTick` + `unref` 拿到 DOM 后实例化第三方组件；`window.BMap` 通过 `(window as any)` 访问。

## Dependencies
### Internal
- `/@/hooks/web/useScript`
### External
- 百度地图 JavaScript API v3.0（远端加载，非 npm 包）
