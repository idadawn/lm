<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Layout

## Purpose
门户运行时栅格画布容器。基于 `vue-grid-layout` 渲染 `layout[]` 中所有卡片节点,通过 `Parser` 解析每个节点 (jnpfKey) 为对应 H* 组件;在受锁定/详情模式下接管拖拽与尺寸事件,并通过 `emitter` 通知 ECharts 卡片重排尺寸。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | grid-layout 容器,处理 `resized`/`moved` 事件,递归触发子节点 `eChart{i}` 重绘 |

## For AI Agents

### Working in this directory
- `static` 由 `enabledLock` 控制;预览态务必传 `enabledLock=1`,设计态传 0 才能拖拽。
- `mask` 用于设计态遮罩,但 `noNeedMaskList`(来自 Design/helper/dataMap)中的 `jnpfKey` 例外,不要硬编码。
- 缩放/拖动后必须通过 `emitter.emit('eChart' + key)` 通知 ECharts 卡片 `resize`,递归覆盖嵌套子节点。

### Common patterns
- 容器外层使用 `ScrollContainer` 与 `useDesign('basic-portal')` 提供的 BEM 前缀。

## Dependencies
### Internal
- `/@/components/Container` (`ScrollContainer`)
- `../../Design/components/Parser.vue`、`../../Design/helper/dataMap`
- `/@/hooks/web/useDesign`
### External
- `vue-grid-layout` (`grid-layout`/`grid-item`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
