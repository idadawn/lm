<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# echartsFunnel

## Purpose
ECharts 漏斗图（Funnel Chart）演示页面，展示销售签约流程从“初次接触”到“签约”的预期与实际转化对比。属于 `extend/graphDemo` 图表示例集，用作前端图表组件的参考实现。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，使用 `/@/components/Chart` 渲染漏斗图，`reactive` 提供静态 series（预期 + 实际），含中文 legend 与 emphasis 提示。 |

## For AI Agents

### Working in this directory
- 仅修改 `options` reactive 对象即可调整图表外观；保持 `defineOptions({ name: 'extend-graphDemo-echartsFunnel' })` 路由命名约定。
- 不要把通用 Chart 工具函数放在这里 —— 共享逻辑放到 `/@/components/Chart`。
- 数据均为静态演示数据，无后端调用；如需接入真实数据，应通过 `/@/api/extend/*` 单独建文件。

### Common patterns
- `<script setup lang="ts">` + `reactive` 配置 + 基于内部封装 `Chart` 组件的渲染。
- 容器使用 `page-content-wrapper` / `page-content-wrapper-center` 全局样式类。

## Dependencies
### Internal
- `/@/components/Chart`（项目封装的 ECharts 包装组件）
### External
- `echarts`（通过内部组件间接依赖）、`vue` reactive API
