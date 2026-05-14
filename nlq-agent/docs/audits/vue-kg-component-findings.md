# Vue 知识图谱组件调研要点

## AntV G6

官方文档显示，G6 是图可视化框架，并提供 Vue 集成示例。G6 Vue 集成页面明确警告：不要把 Vue reactive data 直接传入 G6 instance，否则可能导致渲染异常甚至页面崩溃。使用方式是通过 `onMounted` 创建 `new Graph({ container, data, ... })` 并调用 `graph.render()`。这说明 G6 适合在 Vue 中作为独立图引擎使用，但需要通过 `ref`/普通对象隔离响应式数据。

来源：https://g6.antv.antgroup.com/en/manual/getting-started/integration/vue

## v-network-graph

官方首页定位为 Vue 3 的交互式 network graph visualization component。它基于 SVG，以 Vue 数据响应式方式绘制图，支持 pan、zoom、drag nodes、select 与 multi-touch，并且可配置外观和交互。适合中小规模、Vue 原生、快速集成的知识图谱展示。

来源：https://dash14.github.io/v-network-graph/

## Relation Graph

Relation Graph 官方首页显示其支持 React、Vue 3、Svelte、Vue 2、HTML/Web Component，并提供大量 graph visualization / graph editing 场景模板。它更偏“快速落地业务关系图/图编辑器”的组件，适合希望少写底层布局与交互代码、快速替换当前难看图谱页面的场景。

来源：https://relation-graph.com/

## Sigma.js

官方首页说明 Sigma.js 是用于在浏览器中渲染和交互网络图的现代 JavaScript 库，与 graphology 配合使用；graphology 负责图数据模型和算法，sigma.js 负责图渲染和交互。官方明确说明 Sigma.js 面向 thousands of nodes and edges，并使用 WebGL 渲染，因此比 Canvas 或 SVG 方案更适合大规模图谱。但官方也提示，如果只是几百个节点/边且需要高度定制渲染，D3.js 更合适。Sigma.js 没有官方 Vue wrapper，需要在 Vue 组件中手动绑定生命周期。

来源：https://www.sigmajs.org/

## Cytoscape.js

官方首页定位为 graph theory/network library for visualisation and analysis，具有 MIT 许可、npm 包、扩展生态，并列出多种布局示例，包括 Circle、Concentric、Grid、CoSE、CoSE Bilkent、fCoSE、Cola、ELK、Dagre、Klay、Breadthfirst 等。它适合需要图分析、路径、选择、布局扩展和复杂交互的知识图谱场景，但 Vue 集成同样需要封装生命周期，或者使用社区 wrapper。

来源：https://js.cytoscape.org/
