---
name: nlq-semantic-layer
description: 路美 NLQ Agent 语义层（MDL）设计与维护——定义业务指标、判定规则、产品规格的向量化知识表示
argument-hint: "<需要添加或更新的业务知识，例如：添加磁性数据的铁损判定规则>"
level: 3
---

<Purpose>
本 Skill 专注于路美 NLQ Agent 的**语义层（Semantic Layer / MDL）**设计与维护。

语义层是两阶段问答架构的核心基础，它将路美业务中的"隐性知识"（判定标准、指标定义、计算公式）转化为 LLM 可检索的向量表示，存储在 Qdrant 中。

没有准确的语义层，LLM 就无法理解"A 类合格率"、"铁损 P17/50"、"叠片系数"等专业术语，SQL 生成的准确率将大幅下降。

**类比 WrenAI**：本 Skill 对应 WrenAI 的 MDL（Model Definition Language）层，是整个 NLQ 系统准确性的根基。
</Purpose>

<Knowledge_Architecture>
语义层由四类知识文档构成，分别存储在不同的 Qdrant 集合中：

## 1. 判定规则知识（luma_judgment_rules）

描述硅钢片各等级的判定标准，来源于 `LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL` 表。

**文档格式**：
```
判定等级：{level_name}（{level_code}）
适用产品：{product_model}
判定条件：
  - 铁损 P17/50：≤ {iron_loss_threshold} W/kg
  - 磁感 B50：≥ {magnetic_induction_threshold} T
  - 叠片系数：≥ {stacking_factor_threshold}
判定逻辑：同时满足以上所有条件，则判定为 {level_name}
```

## 2. 产品规格知识（luma_product_specs）

描述各型号产品的技术规格，来源于 `LAB_PRODUCT_SPEC` 和 `LAB_PRODUCT_SPEC_ATTRIBUTE` 表。

**文档格式**：
```
产品型号：{model_name}
规格类别：{spec_category}
技术参数：
  - {attribute_name}：{standard_value} {unit}（允许偏差：±{tolerance}）
适用标准：{standard_code}
```

## 3. 指标定义知识（luma_metric_definitions）

描述统计报表中各指标的计算方式，为静态配置（`scripts/init_semantic_layer.py` 中硬编码）。

**核心指标**：
| 指标名称 | 计算公式 | 说明 |
|----------|----------|------|
| 合格率 | COUNT(判定等级 IN ('A','B')) / COUNT(*) × 100% | A+B 类视为合格 |
| A 类率 | COUNT(判定等级 = 'A') / COUNT(*) × 100% | 最优等级占比 |
| 平均铁损 | AVG(F_IRON_LOSS_P17_50) | 铁损均值 |
| 平均磁感 | AVG(F_MAGNETIC_INDUCTION_B50) | 磁感均值 |
| 平均叠片系数 | AVG(F_STACKING_FACTOR) | 叠片系数均值 |
| 月产量 | SUM(F_DETECTION_WEIGHT) | 检测重量总和（kg） |

## 4. 公式规则知识（luma_formula_rules）

描述中间数据的计算公式，来源于 `LAB_INTERMEDIATE_DATA_FORMULA` 表。
</Knowledge_Architecture>

<Use_When>
- 需要向 Qdrant 添加新的判定规则（如新产品型号的等级标准）
- 需要更新现有业务指标的定义（如合格率计算逻辑变更）
- 需要添加新的统计指标（如新增"优等品率"指标）
- 需要排查 Stage 1 检索不准确的问题（知识文档质量问题）
- 需要设计新的知识文档格式
- 需要理解语义层与 MySQL 数据库的同步机制
</Use_When>

<Execution_Policy>
1. 修改语义层知识文档后，必须重新运行 `init_semantic_layer.py` 更新 Qdrant
2. 新增知识类型时，同步更新 `qdrant_service.py` 中的集合配置
3. 知识文档应使用业务人员熟悉的语言，避免技术术语
4. 每条知识文档的长度控制在 200-500 字符，过长会降低检索精度
5. 同一概念应在不同集合中保持一致的描述方式
</Execution_Policy>

<Steps>
## Phase 1 — 分析现有语义层

1. 读取初始化脚本了解当前知识结构：
   ```bash
   cat nlq-agent/scripts/init_semantic_layer.py
   ```

2. 检查 Qdrant 中现有的集合和文档数量：
   ```python
   # 在 Python 中检查
   from qdrant_client import QdrantClient
   client = QdrantClient(host="localhost", port=6333)
   for collection in ["luma_judgment_rules", "luma_product_specs",
                       "luma_metric_definitions", "luma_formula_rules"]:
       info = client.get_collection(collection)
       print(f"{collection}: {info.points_count} 条文档")
   ```

## Phase 2 — 设计知识文档

根据需求类型选择对应的知识文档模板（见 `<Knowledge_Architecture>` 节）。

**设计原则**：
- 使用业务人员的语言，而非数据库字段名
- 包含同义词和别名（如"铁损"="铁芯损耗"="P17/50"）
- 包含数值单位和量纲（W/kg、T、%）
- 包含上下文关系（"A 类优于 B 类"）

## Phase 3 — 更新初始化脚本

在 `scripts/init_semantic_layer.py` 中添加新的知识文档：

```python
# 示例：添加新的判定规则
JUDGMENT_RULES_DOCS = [
    {
        "id": "rule_new_product_A",
        "content": "判定等级：A 类（最优级）\n适用产品：50W470\n判定条件：\n  - 铁损 P17/50：≤ 4.70 W/kg\n  - 磁感 B50：≥ 1.74 T\n判定逻辑：同时满足以上条件判定为 A 类",
        "metadata": {
            "source": "JUDGMENT_LEVEL",
            "product_model": "50W470",
            "level_code": "A",
            "level_name": "A 类"
        }
    },
    # ...
]
```

## Phase 4 — 重新初始化并验证

```bash
cd nlq-agent

# 重新初始化语义层
python scripts/init_semantic_layer.py --mode incremental

# 验证检索效果
python -c "
from src.services.qdrant_service import QdrantService
import asyncio

async def test():
    svc = QdrantService()
    results = await svc.search('A类判定标准铁损阈值', collection='luma_judgment_rules', limit=3)
    for r in results:
        print(f'Score: {r.score:.3f} | {r.payload[\"content\"][:100]}')

asyncio.run(test())
"
```

## Phase 5 — 触发知识库同步

如果 .NET 后端数据已更新，通过 API 触发增量同步：

```bash
curl -X POST http://localhost:18100/api/v1/knowledge/sync \
  -H "Content-Type: application/json" \
  -d '{"collection": "luma_judgment_rules", "mode": "incremental"}'
```
</Steps>

<Sync_Strategy>
语义层与 MySQL 数据库的同步策略：

```
MySQL 数据变更
    ↓
.NET 后端（EF Core SaveChanges 事件）
    ↓
HTTP POST /api/v1/knowledge/sync
    ↓
nlq-agent 增量向量化
    ↓
Qdrant 集合更新
```

**触发时机**：
- `LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL` 表变更 → 同步 `luma_judgment_rules`
- `LAB_PRODUCT_SPEC` / `LAB_PRODUCT_SPEC_ATTRIBUTE` 表变更 → 同步 `luma_product_specs`
- 公式配置变更 → 同步 `luma_formula_rules`

**增量同步逻辑**（`qdrant_service.py` 中实现）：
1. 计算文档内容的 MD5 哈希
2. 与 Qdrant 中存储的哈希对比
3. 仅更新发生变化的文档
4. 删除 MySQL 中已不存在的文档
</Sync_Strategy>

<Quality_Checklist>
- [ ] 每条知识文档包含足够的业务上下文（不依赖其他文档即可理解）
- [ ] 数值阈值与 MySQL 数据库中的实际值一致
- [ ] 同义词和别名已在文档中体现
- [ ] 检索测试：关键业务问题能命中正确的知识文档（score ≥ 0.72）
- [ ] 初始化脚本执行无报错
- [ ] Qdrant 集合文档数量符合预期
</Quality_Checklist>
