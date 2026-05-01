<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
ZEditor 画布操作组合式封装。基于 `@antv/g6` 直接操作 graph 实例,提供节点新增 (`addNode`)、删除 (`delItem`)、平行边偏移等功能;同时承载 `ZBackground` 插件接入。

## Key Files
| File | Description |
|------|-------------|
| `useEditor.ts` | 模块级 `state` + 单例 `graph`;`addNode` 通过 `getPointByClient` 把屏幕坐标转渲染坐标后 `graph.addItem('node', model)` |

## For AI Agents

### Working in this directory
- `graph` 在模块作用域下声明为单例;同页面多实例 ZEditor 会冲突,需配合 graphRef 隔离。
- 节点 id 由 `'node' + guid()` 生成,默认 `type = 'rect'`、6 个锚点;新增类型时优先扩展 model.type 而非锚点。
- 删除遍历 `getNodes()` 与 `getEdges()` 检查 `hasState('active')`,active 状态由外部交互写入,务必在调用 `delItem` 前选中节点。

### Common patterns
- 与 `MindMap` 内部 hook 不同,本 hook 在 ZEditor 范围内做更细粒度的画布操作。

## Dependencies
### Internal
- `/@/utils/helper/toolHelper` (`guid`)
- `../plugins/ZBackground`
### External
- `@antv/g6`、Vue 3 reactive API

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
