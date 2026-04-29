---
name: nlq-qdrant-init
description: 路美 NLQ Agent Qdrant 语义层初始化——从 MySQL 提取业务知识并向量化写入 Qdrant
argument-hint: "<初始化模式，例如：full（全量重建）或 incremental（增量更新）>"
level: 2
---

<Purpose>
本 Skill 指导执行路美 NLQ Agent 的 **Qdrant 语义层初始化**，将 MySQL 数据库中的业务知识（判定规则、产品规格）转化为向量表示，写入 Qdrant，为 Stage 1 的语义检索提供数据基础。

这是部署 NLQ Agent 的**必要前置步骤**，也是业务数据变更后的**维护操作**。
</Purpose>

<Use_When>
- 首次部署 nlq-agent 服务（全量初始化）
- MySQL 中的判定规则或产品规格数据发生变更（增量更新）
- Qdrant 数据损坏或需要重建（全量重建）
- 添加了新的知识类型需要向量化
- 检索命中率下降，需要重新优化知识文档
</Use_When>

<Prerequisites>
执行初始化前，确认以下服务已就绪：

```bash
# 1. 检查 Qdrant 服务
curl http://localhost:6333/health
# 期望：{"title":"qdrant - vector search engine","version":"...","commit":"..."}

# 2. 检查 MySQL 连接
mysql -h localhost -u luma_user -p luma_db -e "SELECT COUNT(*) FROM LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL;"

# 3. 检查 OpenAI API（用于 Embedding）
python -c "
import openai, os
client = openai.OpenAI(api_key=os.getenv('OPENAI_API_KEY'))
r = client.embeddings.create(input='test', model='text-embedding-3-small')
print(f'Embedding 维度: {len(r.data[0].embedding)}')
"

# 4. 检查 .env 配置
cat nlq-agent/.env | grep -E "QDRANT|MYSQL|OPENAI"
```
</Prerequisites>

<Steps>
## Phase 1 — 配置检查

1. 读取初始化脚本：
   ```bash
   cat nlq-agent/scripts/init_semantic_layer.py
   ```

2. 确认 `.env` 中的配置正确：
   ```bash
   # 必须配置的环境变量
   QDRANT_HOST=localhost
   QDRANT_PORT=6333
   QDRANT_API_KEY=          # 本地部署可留空
   MYSQL_HOST=localhost
   MYSQL_PORT=3306
   MYSQL_DATABASE=luma_db
   MYSQL_USER=luma_user
   MYSQL_PASSWORD=xxx
   OPENAI_API_KEY=sk-xxx
   EMBEDDING_MODEL=text-embedding-3-small
   EMBEDDING_DIMENSION=1536
   ```

## Phase 2 — 执行初始化

### 全量初始化（首次部署或重建）

```bash
cd nlq-agent

# 全量模式：删除现有集合并重建
python scripts/init_semantic_layer.py --mode full

# 预期输出：
# [INFO] 删除现有集合...
# [INFO] 创建集合 luma_judgment_rules（维度: 1536）
# [INFO] 创建集合 luma_product_specs（维度: 1536）
# [INFO] 创建集合 luma_metric_definitions（维度: 1536）
# [INFO] 创建集合 luma_formula_rules（维度: 1536）
# [INFO] 从 MySQL 提取判定规则...共 XX 条
# [INFO] 向量化并写入 luma_judgment_rules...完成
# [INFO] 从 MySQL 提取产品规格...共 XX 条
# [INFO] 向量化并写入 luma_product_specs...完成
# [INFO] 写入静态指标定义...共 XX 条
# [INFO] 初始化完成！总计写入 XX 条文档
```

### 增量更新（数据变更后）

```bash
# 增量模式：仅更新变化的文档
python scripts/init_semantic_layer.py --mode incremental

# 指定集合更新
python scripts/init_semantic_layer.py --mode incremental --collection luma_judgment_rules
```

## Phase 3 — 验证初始化结果

```bash
# 检查各集合的文档数量
python -c "
from qdrant_client import QdrantClient
client = QdrantClient(host='localhost', port=6333)
collections = ['luma_judgment_rules', 'luma_product_specs',
               'luma_metric_definitions', 'luma_formula_rules']
for name in collections:
    try:
        info = client.get_collection(name)
        print(f'{name}: {info.points_count} 条文档')
    except Exception as e:
        print(f'{name}: 错误 - {e}')
"

# 测试检索效果（关键业务问题）
python -c "
import asyncio, sys
sys.path.insert(0, 'src')
from services.qdrant_service import QdrantService

async def test_retrieval():
    svc = QdrantService()
    test_queries = [
        ('A类判定标准铁损阈值是多少', 'luma_judgment_rules'),
        ('合格率怎么计算', 'luma_metric_definitions'),
        ('50W470产品规格参数', 'luma_product_specs'),
    ]
    for query, collection in test_queries:
        results = await svc.search(query, collection=collection, limit=2)
        print(f'\\n查询: {query}')
        for r in results:
            print(f'  Score: {r.score:.3f} | {r.payload[\"content\"][:80]}...')

asyncio.run(test_retrieval())
"
```

## Phase 4 — 排查问题

**问题：文档数量为 0**
```bash
# 检查 MySQL 数据是否存在
mysql -e "SELECT COUNT(*) FROM LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL WHERE F_IS_ACTIVE = 1;"

# 检查脚本日志
python scripts/init_semantic_layer.py --mode full --verbose 2>&1 | grep ERROR
```

**问题：检索 score 普遍低于 0.72**
```bash
# 检查 embedding 模型是否一致（初始化时和查询时必须相同）
grep "EMBEDDING_MODEL" nlq-agent/.env

# 尝试降低阈值测试
python -c "
# 在 src/services/qdrant_service.py 中临时将 score_threshold 改为 0.5 测试
"
```

**问题：Qdrant 连接失败**
```bash
# 检查 Qdrant 容器状态
docker ps | grep qdrant
docker logs nlq-agent-qdrant-1 | tail -20

# 重启 Qdrant
docker-compose -f nlq-agent/docker-compose.yml restart qdrant
```
</Steps>

<Collection_Config>
Qdrant 集合配置（`qdrant_service.py` 中定义）：

```python
COLLECTIONS = {
    "luma_judgment_rules": {
        "vector_size": 1536,
        "distance": "Cosine",
        "description": "硅钢片判定等级规则知识库"
    },
    "luma_product_specs": {
        "vector_size": 1536,
        "distance": "Cosine",
        "description": "产品规格属性知识库"
    },
    "luma_metric_definitions": {
        "vector_size": 1536,
        "distance": "Cosine",
        "description": "统计指标定义知识库"
    },
    "luma_formula_rules": {
        "vector_size": 1536,
        "distance": "Cosine",
        "description": "中间数据计算公式知识库"
    }
}

# 检索配置
DEFAULT_SEARCH_LIMIT = 5
DEFAULT_SCORE_THRESHOLD = 0.72  # 低于此值视为未命中
```
</Collection_Config>

<Final_Checklist>
- [ ] 所有 4 个 Qdrant 集合已创建
- [ ] 各集合文档数量符合预期（判定规则 ≥ 10 条，产品规格 ≥ 20 条，指标定义 ≥ 8 条）
- [ ] 关键业务问题的检索 score ≥ 0.72
- [ ] 服务重启后 `/health` 端点返回 200
- [ ] Stage 1 能正确检索到相关知识文档
</Final_Checklist>
