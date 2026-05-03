<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# event

## Purpose
DOM 事件 / 视口相关的复合钩子，集中处理断点联动、事件绑定与卸载、滚动控制、IntersectionObserver 与 window 尺寸监听。

## Key Files
| File | Description |
|------|-------------|
| `useBreakpoint.ts` | 全局响应式断点：`createBreakpointListen` 单次初始化 `globalScreenRef`/`widthRef`，`useBreakpoint()` 暴露给消费方；映射 `screenEnum`/`screenMap`/`sizeEnum` |
| `useEventListener.ts` | 类型安全的 addEventListener，含自动 `removeEvent` 与可选 throttle/debounce |
| `useScroll.ts` | 监听元素滚动，返回 `refX/refY/isScrolling` |
| `useScrollTo.ts` | 平滑滚动到指定坐标/元素 |
| `useIntersectionObserver.ts` | 元素可见性观察封装 |
| `useWindowSizeFn.ts` | window 尺寸变化的 debounced 回调 |

## For AI Agents

### Working in this directory
- `createBreakpointListen` 必须在 app 启动后调用一次（见 `setup` 流程），后续业务代码统一用 `useBreakpoint()` 读取。
- 所有事件监听都应通过 `useEventListener` 注册，自动随 unmount 清理；避免直接 `window.addEventListener` 造成内存泄漏。
- 使用 `screenMap.get(sizeEnum.MD)!` 等取值时确保 enum 与 `enums/breakpointEnum.ts` 同步。

### Common patterns
- 模块级单例：`globalScreenRef`/`globalWidthRef` 跨组件复用，避免重复监听 resize。

## Dependencies
### Internal
- `/@/enums/breakpointEnum`。
### External
- `vue`、`@vueuse/core`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
