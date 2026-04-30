# nlq-agent 真 E2E 启动 Runbook

**Last verified:** 2026-05-01 02:30 CST (round-2 真 e2e 联调)
**Status:** 🟡 部分跑通 — 见"已知卡点"

`nlq-agent` 是 LIMS 的 Python FastAPI 微服务（端口 18100），实现两阶段 NL→SQL 问答。本 runbook 记录"从零到能在终端 curl 一次完整 SSE 流"的真实步骤。

## 依赖清单

| 服务 | 端口 | 用途 | 必须 |
|---|---|---|---|
| Qdrant | 6333 (HTTP) / 6334 (gRPC) | 向量库（rules/specs/metrics 三 collection） | ✅ |
| TEI (Text Embeddings Inference) | 8001 → 容器 80 | bge-large-zh-v1.5 嵌入（中文） | ✅ |
| MySQL | 33307 → 容器 3306 | 业务数据 + seed | ✅（生产用 47.105.59.151:8930，本地测试用 33307） |
| LLM 后端 | https://api.siliconflow.cn/v1 | DeepSeek-V4-Flash 等 | ✅（外部 API，需 key） |
| nlq-agent | 18100 | 主服务 | — |

## 0. 前置

```bash
cd /data/project/lm/nlq-agent
# .env 应该已经有 LLM_API_KEY；如果是新机器：
cp .env.example .env  # 然后填 LLM_API_KEY
```

**关键 .env 项**（必填）：
- `LLM_BASE_URL=https://api.siliconflow.cn/v1`
- `LLM_API_KEY=sk-...`（SiliconFlow / OpenRouter 任一）
- `LLM_MODEL=deepseek-ai/DeepSeek-V4-Flash`（已验证可用）
- `EMBEDDING_BASE_URL=http://localhost:8001` ⚠️ docker-compose.yml 里 TEI 是 `8001:80`，**.env 不能写 8081**（settings.py 默认值有 8081 是误导，实际看 .env）
- `MYSQL_HOST=localhost / MYSQL_PORT=33307`（本地测试）— 或填生产配置（只读账号）

## 1. 启动基础栈

```bash
cd /data/project/lm/nlq-agent
docker compose -f docker-compose.yml up -d qdrant tei
docker compose -f docker-compose.test.yml up -d --wait nlq-mysql-test
```

### 1.1 验证

```bash
curl -s http://localhost:6333/                                # Qdrant: 200
curl -s http://localhost:8001/health                          # TEI: 200（首次启动需 ~3-5 min 下载 model，cold start）
nc -z localhost 33307 && echo "MySQL: open"                   # 33307 应通
```

### 1.2 已知卡点

- **TEI 容器手动 docker run 起的版本不会绑端口** — 必须用 `docker compose up -d tei`，否则 `nc -z 8001` 通但 HTTP 000。如果有幽灵 `nlq-tei` 容器：`docker rm -f nlq-tei && docker compose up -d tei`。
- **TEI 首次启动很慢**（~3 分钟下 1.3 GB model）。看进度：`docker logs -f nlq-tei | grep -E 'Downloading|Ready'`
- **Qdrant docker 标 unhealthy 但实际可用** — 健康检查端点配置问题，忽略。
- **MySQL test 容器只 seed 三张表**：`LAB_INTERMEDIATE_DATA`、`LAB_PRODUCT_SPEC`、`LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL`。`init_semantic_layer.py` 还需要 `LAB_INTERMEDIATE_DATA_FORMULA` 等 → **见 §2 的 schema gap**。

## 2. 初始化 Qdrant 语义层

```bash
cd /data/project/lm/nlq-agent
.venv/bin/python -m scripts.init_semantic_layer
```

预期产出：
```
INFO ── 1. 加载并写入判定规则 ───
INFO 已写入 N 条规则到 lm_judgment_rules
INFO ── 2. 加载并写入产品规格 ───
INFO 已写入 N 条规格到 lm_product_specs
INFO ── 3. 加载并写入公式/指标定义 ───
INFO 已写入 N 条指标到 lm_metrics
INFO 语义层初始化完成！
```

### 2.1 验证

```bash
for c in lm_judgment_rules lm_product_specs lm_metrics; do
  curl -s http://localhost:6333/collections/$c | jq '.result.points_count'
done
# 三个数应该 > 0；目前真测一次 = 0/0/0（init 从未真跑成功）
```

### 2.2 已知卡点

<!-- TODO 跑过后回填 -->

## 3. 启动 nlq-agent 服务

```bash
cd /data/project/lm/nlq-agent
.venv/bin/uvicorn src.main:app --host 0.0.0.0 --port 18100
```

### 3.1 验证

```bash
curl -s http://localhost:18100/health   # 200
```

## 4. 真 E2E SSE 流（tracer-bullet）

```bash
curl -N -X POST http://localhost:18100/api/v1/chat/stream \
  -H 'Content-Type: application/json' \
  -d '{"query":"合格率不低于75%且抽样数量不少于100的产品规格"}'
```

预期 SSE 事件序列：
1. `event: reasoning_step` × N（kind=spec / rule / condition / grade）
2. `event: text` × N（流式回答）
3. `event: response_metadata`（含生成的 SQL）
4. `event: done`

## 5. 跑测试套件

```bash
cd /data/project/lm/nlq-agent
uv run pytest tests/ -m "not live_llm" -x --tb=short
# 期望：48 passed, 1 deselected
```

## 6. 关停

```bash
cd /data/project/lm/nlq-agent
docker compose -f docker-compose.yml down
docker compose -f docker-compose.test.yml down -v   # -v 删 MySQL 数据
```

## 7. 完整真测 transcript

下面是 2026-05-01 03:04 真发一次的 SSE 流（mock-ingested Qdrant + 真 LLM + 真 MySQL，但生产 MySQL 没有 50W470 1月样本数据 → row_count=0；LLM 流式答案完整）。

输入：
```
POST /api/v1/chat/stream
{"messages":[{"role":"user","content":"50W470 牌号硅钢片样品的铁损 P17/50 合格率"}]}
```

事件统计：
- 6 个 `reasoning_step`：spec×2（metric_pass_rate, spec_50W470）+ rule×1（rule_001）+ condition×2（F_PRODUCT_SPEC=50W470, F_PERF_PS_LOSS≤1.08）+ grade×1（未查询到数据）
- 164 个 `text` chunk（业务 LLM 流式回答）
- 1 个 `response_metadata`（含完整 SQL、reasoning_steps 全档、sql_explanation、row_count=0）
- 1 个 `done`

总计 ~344 SSE lines。生成的 SQL（LLM 一次过自选 `F_PRODUCT_SPEC_CODE` 列）：
```sql
SELECT
    DATE_FORMAT(F_PROD_DATE, '%Y-%m') AS month_bucket,
    COUNT(*) AS total_count,
    SUM(CASE WHEN F_PERF_PS_LOSS <= 1.08 THEN 1 ELSE 0 END) AS qualified_count,
    ROUND(SUM(CASE WHEN F_PERF_PS_LOSS <= 1.08 THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 2) AS qualified_rate
FROM LAB_INTERMEDIATE_DATA
WHERE F_PRODUCT_SPEC_CODE = '50W470'
  AND F_PROD_DATE >= '2024-01-01'
  AND F_PROD_DATE < '2024-02-01'
GROUP BY DATE_FORMAT(F_PROD_DATE, '%Y-%m')
ORDER BY month_bucket
LIMIT 500
```

完整 transcript 副本：`/tmp/sse-final-merged.txt`（不入库，下次跑会变）。

## 8. 故障排查（已实测）

### 8.1 SSE 一调用就 500 + `_orchestrator is None`
**症状**：`/health` 显示 `qdrant_connected: false`，`POST /api/v1/chat/stream` 返回 `Internal Server Error`，uvicorn 日志含 `服务初始化失败: [SSL: WRONG_VERSION_NUMBER]`。
**根因**：`.env` 的 `QDRANT_API_KEY=`（空字符串而不是 unset）。pydantic settings 解析为 `""`，传给 `AsyncQdrantClient(api_key="")` 后 client 把"有 key"误判为 cloud（启用 HTTPS），与本地 HTTP Qdrant 握手失败。
**修复**：`qdrant_service.py` 在传参时 `api_key=settings.qdrant_api_key or None`（已合入 main 2026-05-01）。
**验证**：`/health` 应该 `qdrant_connected: true`。

### 8.2 SSE 返回 `error: 'AsyncQdrantClient' object has no attribute 'search'`
**症状**：reasoning_step 都没流出来直接 error。
**根因**：qdrant-client ≥1.10 删除了 `AsyncQdrantClient.search`。
**修复**：用 `query_points()` 替代（已合入 main 2026-05-01，分支 `omc-team/r3/glm-qdrant-1.10-upgrade`）。

### 8.3 SSE 在最后 LLM 流式答案处 `IndexError: list index out of range`
**症状**：reasoning_step / SQL 都跑通，最后 `text` chunk 没流出来直接 error。
**根因**：SiliconFlow 等 OpenAI-兼容 backend 偶尔发出 `chunk.choices=[]` 的心跳/metadata-only chunk，`llm_client.py:chat_stream` 没防御。
**修复**：四层防御 — 空 choices skip / delta None skip / content None skip（已合入 main 2026-05-01，分支 `omc-team/r3/kimi-llm-chat-stream-fix`）。

### 8.4 TEI 8001 端口通但 HTTP 000
**症状**：`nc -z localhost 8001` 通，但 `curl localhost:8001/health` 返回 000。
**根因**：TEI 容器只监听 `0.0.0.0`（IPv4），但 `localhost` 解析到 `::1`（IPv6）。
**修复**：`.env` 把 `EMBEDDING_BASE_URL=http://localhost:8001` 改成 `http://127.0.0.1:8001`。或改 `settings.py` 默认值。

### 8.5 TEI 容器 `Up N hours` 但端口 column 空
**症状**：`docker ps` 显示 `nlq-tei` 在跑但 `Ports` 列空，`curl 8001` 不通。
**根因**：手动 `docker run` 起的容器没绑端口。
**修复**：用 `docker compose -f docker-compose.vectors.yml up -d tei`。先 `docker rm -f nlq-tei`。

### 8.6 真 init_semantic_layer.py 跑不通
**症状**：MySQL test 容器起来但 `INSERT INTO LAB_PRODUCT_SPEC` 立刻 fail。
**根因**：`tests/fixtures/seed_lab.sql` 假设业务表已存在，但 fresh `mysql:8.0` 容器没有这些表。需要补 `init_schema.sql`（≥3 张表的 CREATE TABLE）。
**当前状态**：未实现，跑真 init 须连真生产 MySQL（有完整 schema）。
