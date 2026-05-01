<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# diagrams

## Purpose
项目级架构图与流程图源文件目录。集中存放 D2 与 Mermaid 源（`.d2` / `.mmd`）以及对应渲染产物（`.png`），用于 README、设计文档和 NLQ Agent 两阶段架构说明的可视化插图。

## Key Files
| File | Description |
|------|-------------|
| `overall_architecture.d2` | nlq-agent 总体架构图（D2）：用户 / 前端 / FastAPI / Qdrant / MySQL / LLM / Embedding 七层关系 |
| `overall_architecture.png` | 上图渲染产物 |
| `two_stage_flow.mmd` | 两阶段问答时序图（Mermaid sequenceDiagram）：用户 → FE → API → Qdrant → MDL → DB |
| `two_stage_flow.png` | 上图渲染产物 |
| `mdl_semantic_layer.d2` / `.png` | 语义层（MDL）四集合知识布局示意 |
| `query_routing.mmd` / `.png` | 意图路由四分支决策图 |
| `wrenai_vs_luma.d2` / `.png` | 与 WrenAI 架构的对比图，论证设计选择 |

## For AI Agents

### Working in this directory
- 修改源文件后必须重新渲染 `.png` 并一起提交，文档站不会即时编译 D2/Mermaid。
- D2 渲染：`d2 input.d2 output.png`；Mermaid 渲染：`mmdc -i input.mmd -o output.png`。
- 标题与节点中文文案保持与 SKILL 文档一致（"两阶段问答"、"Semantic & KG Agent"、"Data & SQL Agent"）。
- 不要把图片当作真值——任何字段/枚举的真值在 `nlq-agent/src/models/schemas.py` 与对应 SKILL 文档中。
- 颜色搭配遵循文件中现有 D2 主题（蓝色=用户/前端，绿色=API，黄色=Stage1，浅绿=Stage2），新增节点保持同色系。

### Common patterns
- D2 用 `style.fill` / `style.stroke` 描色；Mermaid 用 `rect rgb(...)` 阶段分组。
- 文件名 snake_case 表意，源文件与产物同名。

## Dependencies
### Internal
- 文档引用方：项目 README、`nlq-agent/skills/*/SKILL.md`

### External
- D2、Mermaid CLI（`@mermaid-js/mermaid-cli`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
