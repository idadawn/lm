<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# var

## Purpose
LESS 全局变量定义。集中维护项目命名空间 (`@namespace: jnpf`)、布局尺寸（header/tabs/logo）、z-index 分层（侧栏拖拽、loading、锁屏、tab、setting drawer 等）以及屏幕断点和缓动函数变量，供 `web/src/design/` 主题与各组件 less 引用。

## Key Files
| File | Description |
|------|-------------|
| `index.less` | 主入口：聚合 `easing`/`breakpoint`，导出 `@namespace`、各类高度与 z-index 常量、`.bem()` mixin |
| `breakpoint.less` | 屏幕断点（xs/sm/md/lg/xl/2xl）的 min/max 像素映射，对齐 Ant Design Vue 的 Grid 体系 |
| `easing.less` | 标准缓动曲线变量（in / out / inOut 三族），用于全局过渡动画 |

## For AI Agents

### Working in this directory
- 修改命名空间或 z-index 顺序会影响整站浮层叠放（sider drag/page-loading/lock/tabs/sider/mix-sider 已分级），调整前先 grep 引用方。
- 这里只放纯变量与 mixin（`.bem`），不写具体样式。具体主题色/语义色在 `../color.less`、`../theme/`、`../ant/` 中。
- 新增断点必须同步 `web/src/enums/breakpointEnum.ts`，以保持 less 与 TS 双源一致。

### Common patterns
- 通过 `@import (reference)` 引入，仅暴露变量，不输出实际 CSS。
- `.bem(@n; @content)` 用 `@{namespace}-@{n}` 生成 BEM 块名，与 `useDesign('xxx')` 钩子（产出 `jnpf-xxx`）配对。

## Dependencies
### Internal
- `web/src/design/color.less`、`web/src/design/index.less` 通过 `@import` 使用本目录变量。
- 与 TS 端 `web/src/enums/breakpointEnum.ts` 在数值上保持一致。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
