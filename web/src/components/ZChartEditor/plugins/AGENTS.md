<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# plugins

## Purpose
G6 画布插件目录(预留扩展)。当前提供画布背景图与网格自定义实现,继承 `@antv/g6` 的 `Grid` 类并接管容器渲染逻辑。当 ZChartEditor 切换为 G6 渲染或 ZEditor 思维导图时被复用。

## Key Files
| File | Description |
|------|-------------|
| `ZBackground.ts` | 继承 Grid,在画布下层插入 `<img class="x-background-img">`,监听 `background:reset` / `background:update` 事件 |
| `ZGrid.ts` | 继承 Grid,矩阵变换时同步 transform 到 gridContainer,使网格随画布缩放 |

## For AI Agents

### Working in this directory
- 两个类都通过覆盖 `updateGrid` / `getEvents` / `getContainer` 来扩展 G6 内置 Grid;新增插件遵循同一模式。
- 注意当前 ZChartEditor 主路径使用 ECharts + DragResize,本目录主要被 ZEditor 思维导图(同名 plugins 文件)与未来 g6 扩展使用,改动需同步两处。

### Common patterns
- DOM 操作统一使用 `@antv/dom-util` 的 `createDom` / `modifyCSS`,不要直接用 jQuery 或原生写法。

## Dependencies
### External
- `@antv/g6` (`Grid`)、`@antv/dom-util`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
