<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ZEditor

## Purpose
指标价值链(MindMap)编辑器组件包。基于 `/@/components/MindMap` 提供节点拖入、节点详情抽屉、规则/分级/通知配置等编辑能力,服务于检测室"指标价值链"建模 (`createModel/model`) 业务。通过 `withInstall(ZEditor)` 暴露顶层组件。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 入口,`withInstall` 包装 `src/index.vue` 并导出 `ZEditorProps` 类型 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 主组件:左侧 MindMap 画布 + 右侧抽屉表单 (see `src/AGENTS.md`) |
| `components/` | 子组件:节点表单、节点树 (see `components/AGENTS.md`) |
| `hooks/` | `useEditor` 基于 `@antv/g6` 操作画布节点 (see `hooks/AGENTS.md`) |
| `plugins/` | g6 背景与网格插件 (see `plugins/AGENTS.md`) |
| `types/` | 规则/分级状态选项 TS 类型 (see `types/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 与 `ZChartEditor` 区分:本组件用于"指标价值链"思维导图;复用 `MindMap` 组件而非内置画布。
- 节点删除走 `deleteIndicatorValueChain(id)`,删除前必须 `Modal.confirm`,不要默默删除。
- 选中节点状态由 `useMindMapResult().currentItem` 提供,本组件仅控制抽屉显示。

### Common patterns
- 使用 `i18n` (`useI18n`) 提示文案,中文默认 key 在 `common.tipTitle / common.delTip`。

## Dependencies
### Internal
- `/@/components/MindMap`(及其 `hooks/useMindMap`)
- `/@/api/createModel/model`、`/@/enums/httpEnum`
### External
- Vue 3 + TS,`ant-design-vue`,`@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
