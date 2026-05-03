<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# core

## Purpose
最底层、零业务依赖的 Vue Composition 原语。提供 provide/inject 上下文工厂、超时、attrs 透传、refs 数组收集、防重复执行锁等通用能力，被 hooks 其他子目录及组件库复用。

## Key Files
| File | Description |
|------|-------------|
| `useContext.ts` | `createContext`/`useContext` 工厂：包装 `provide(key, readonly(reactive(state)))` 与默认 `inject` |
| `useTimeout.ts` | `useTimeoutFn`/`useTimeoutRef` 封装可清理的 setTimeout |
| `useAttrs.ts` | 过滤透传 attrs（剔除 class/style 或保留）方便包装组件 |
| `useLockFn.ts` | 异步函数防并发：执行中再次调用直接 return |
| `useRefs.ts` | `v-for` 中收集子组件 ref 数组 |
| `onMountedOrActivated.ts` | 兼容 keep-alive：mounted 与 activated 都触发，但首次只一次 |

## For AI Agents

### Working in this directory
- 这里只能依赖 `vue`，避免引入 `/@/store`、`/@/api` 等业务模块以维持原语层纯净。
- `createContext` 默认 `readonly: true`：传 `readonly:false` 才允许外部修改 state，决定后再下发 key。
- `useLockFn` 不会缓存结果，仅返回锁定后的同名函数；如需 loading 状态请在外层自管 ref。

### Common patterns
- 导出函数式 hook，不导出类；所有副作用都暴露 cleanup（`stop`、`removeEvent` 等）。

## Dependencies
### External
- `vue` 与 `@vueuse/core`（部分文件）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
