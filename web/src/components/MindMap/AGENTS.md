<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MindMap

## Purpose
基于 AntV G6 的指标价值树（指标体系 / KPI 链路）思维导图组件。为「创建模型 → 指标价值链」业务提供节点增删、状态着色、缩略趋势图、缩放/全屏工具栏等可视化能力。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 通过 `withInstall` 暴露 `MindMap`，并导出 `MindMapProps` 类型。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `hooks/` | G6 画布初始化、工具栏、数据类型 (see `hooks/AGENTS.md`) |
| `src/` | 单/双视图入口（`index.vue` / `indexMore.vue`）与 props (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 当前默认导出 `index.vue`（`indexMore.vue` 已在 `index.ts` 中注释保留），切换前确认两个视图的差异。
- 新增/删除节点接口请走 `addItem/onDeleteItem` 回调，业务方可注入自定义实现避免直连 `addIndicatorValueChain` API。

### Common patterns
- `useMindMap` 内部维护单例 `graph`，组件销毁时不重新声明（注意热更新场景）。

## Dependencies
### Internal
- `/@/api/createModel/model`、`/@/enums/publicEnum`、`/@/enums/httpEnum`
### External
- `@antv/g6`、`@antv/chart-node-g6`、`ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
