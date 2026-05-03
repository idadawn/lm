<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# scripts

## Purpose
nlq-agent 的运维 / 数据初始化脚本目录。当前仅含语义层一次性/增量初始化脚本——从 MySQL 业务库（判定规则、产品规格、公式定义）抽取数据，调用 TEI 向量化后写入 Qdrant 三个 Collection（`luma_rules` / `luma_specs` / `luma_metrics`）。

## Key Files
| File | Description |
|------|-------------|
| `init_semantic_layer.py` | 语义层初始化主脚本：拉取 `LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL`、`LAB_PRODUCT_SPEC(+ATTRIBUTE)`、`LAB_INTERMEDIATE_DATA_FORMULA`，按文档模板格式化并 upsert 到 Qdrant；附加 5 条硬编码 `PREDEFINED_METRICS`（合格率/产量/铁损/叠片系数/平均厚度） |
| `__init__.py` | 空包标记，使 scripts 可作为模块被 `python -m scripts.init_semantic_layer` 调用 |

## For AI Agents

### Working in this directory
- 脚本必须在已配置 `.env` 的环境下运行，依赖 Qdrant + TEI + MySQL 三方就绪。
- 入口模式：`python -m scripts.init_semantic_layer` 或 `python scripts/init_semantic_layer.py`；脚本会自动把仓库根加入 `sys.path`。
- 修改硬编码的 `PREDEFINED_METRICS` 时同步更新 `nlq-semantic-layer` skill 文档与 `src/models/ddl.py` 的 `METRIC_SQL_TEMPLATES`。
- 该脚本对 `F_DELETE_MARK IS NULL OR F_DELETE_MARK = 0` 做软删除过滤，与 `.cursorrules` 中 CLDEntityBase 字段名约定一致——新增字段过滤时需先确认字段名规范（混合大小写）。
- 不要在此目录提交 .env / 凭证 / 敏感数据。

### Common patterns
- 三段式：`load_*` 拉取 → `format_*_document` 格式化为 `{id, text, metadata}` → `qdrant.upsert_documents()` 批量写入。
- 文档 `text` 字段是给 embedding 模型看的——使用业务人员可读的中文短句，包含同义词与单位。

## Dependencies
### Internal
- `src/services/database.py` — 异步 MySQL 客户端
- `src/services/qdrant_service.py` — 向量库操作封装
- `src/services/embedding_client.py` — TEI 客户端
- `src/core/settings.py` — 配置读取

### External
- aiomysql / qdrant-client / httpx

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
