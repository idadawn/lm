<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# plugins

## Purpose
ZEditor (MindMap) 的 G6 自定义插件。在画布下方插入背景图层、扩展网格随画布变换。与 `ZChartEditor/plugins` 同源,但服务于思维导图场景。

## Key Files
| File | Description |
|------|-------------|
| `ZBackground.ts` | 继承 `Grid`,创建 `<div class="x-background"><img></div>` 作为画布背景,响应 `background:reset/update` 事件 |
| `ZGrid.ts` | 继承 `Grid`,矩阵变换时同步 transform 到 gridContainer |

## For AI Agents

### Working in this directory
- 这两个文件与 `ZChartEditor/plugins` 实质相同,改动需要双向同步,或抽离到公共目录(暂未抽离)。
- 容器 DOM 通过 `@antv/dom-util` 操作,不要混用 jQuery / 原生写法。
- `updateGrid` 在 ZBackground 中被故意置空(只做背景,不画网格),不要补全否则会破坏行为。

### Common patterns
- 通过覆盖 `getEvents`、`getContainer`、`destroy` 三件套接入 g6 插件生命周期。

## Dependencies
### External
- `@antv/g6` (`Grid`)、`@antv/dom-util`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
