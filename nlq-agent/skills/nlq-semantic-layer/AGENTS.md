<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# nlq-semantic-layer

## Purpose
OMC skill (level 3)：指导 Claude 设计与维护 NLQ Agent 的 **语义层（Semantic Layer / MDL）**。语义层把路美业务"隐性知识"（铁损 P17/50、A 类合格标准、叠片系数等）转化为 Qdrant 中的向量化文档，是两阶段问答的根基；类比 WrenAI 的 MDL 层。

## Key Files
| File | Description |
|------|-------------|
| `SKILL.md` | 语义层架构文档：四类知识集合（判定规则 / 产品规格 / 指标定义 / 业务术语）+ 文档模板 + 同步触发器 |

## For AI Agents

### Working in this directory
- 知识文档遵守"自然语言模板 + 关键术语保留"策略，便于 Embedding 匹配；禁止压缩为 JSON-only 表示。
- 集合命名固定为 `luma_judgment_rules` / `luma_product_specs` / `luma_metrics` / `luma_terms`；改名需同步 `core/settings.py`。
- 新增字段时必须更新文档模板的"判定条件"小节，保持与 `LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL` 等表的列对齐。
- 文档变更后必须触发增量同步（参见 `nlq-qdrant-init` skill）；不要假设 Qdrant 自动失效。
- 业务术语不可编造，须从 `LAB_PRODUCT_SPEC` / `LAB_PRODUCT_SPEC_ATTRIBUTE` / 现有 SKILL 文档中抽取。

### Common patterns
- 中文领域术语保持原样（铁损、磁感、叠片系数）—— 不要英文化，影响检索召回。
- 每个文档块以"判定等级 / 产品型号 / 指标名"作为天然标题。

## Dependencies
### Internal
- `nlq-agent/src/services/qdrant_service.py`（写入端）
- `nlq-agent/src/pipelines/stage1/semantic_kg_agent.py`（消费端）

### External
- Qdrant、Embedding 服务（TEI bge-m3 / OpenAI text-embedding-3）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
