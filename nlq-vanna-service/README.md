# nlq-vanna-service

Vanna 独立微服务 — 检测室数据分析系统 NLQ（自然语言查询）后端。

基于 [Vanna](https://vanna.ai/) + Qdrant + TEI + vLLM，实现从中文自然语言问题到 SQL 的自动生成与执行，并通过 SSE 推流 6 种推理步骤（spec / rule / condition / grade / record / fallback）。

---

## 目录结构

```
nlq-vanna-service/
├── app/                    # FastAPI 应用
│   ├── main.py             # 入口，lifespan bootstrap
│   ├── config.py           # Pydantic Settings
│   ├── deps.py             # FastAPI 依赖注入
│   ├── cli.py              # CLI 工具（reindex / dump-ddl / ...）
│   ├── api/                # 路由层
│   │   ├── health.py       # GET /healthz
│   │   ├── schemas.py      # Pydantic 请求/响应模型
│   │   └── chat_stream.py  # POST /api/v1/chat/stream（Wave 2）
│   ├── adapters/           # Vanna mixin（Wave 1B）
│   ├── reasoning/          # 推理步骤 emitter/mapper（Wave 1C）
│   └── knowledge/          # 知识注入 bootstrap（Wave 2）
├── knowledge/              # 知识源（业务方维护）
│   ├── ddl.md
│   ├── terminology.md
│   ├── judgment_rules.md
│   └── qa_examples.yaml
├── tests/
│   ├── unit/
│   ├── integration/
│   └── eval/               # questions.yaml + runner（Day-1 EOD 锁）
├── snapshots/              # Day-12 Qdrant snapshot 存档
├── pyproject.toml
├── uv.lock                 # 锁定依赖版本，已提交到版本控制
├── Dockerfile
├── docker-compose.yml      # 独立 stack（qdrant + tei + vllm + 本服务）
└── .env.example            # 环境变量模板
```

---

## 快速启动

### 前置条件

- Docker + Docker Compose v2
- （本地模式）Python 3.11–3.12 + [uv](https://github.com/astral-sh/uv)

---

### 模式 A：完整容器化（docker compose）

```bash
# 1. 复制并填写环境变量
cp .env.example .env
# 编辑 .env，填写 MYSQL_PASSWORD、VLLM_MODEL 等

# 2. 启动全部服务（qdrant + tei + vllm + nlq-vanna-service）
docker compose up -d

# 3. 验证健康
curl http://localhost:8088/healthz
# 期望：{"status":"ok","version":"0.1.0","knowledge_version":"v1"}

# 4. 查看日志
docker compose logs -f nlq-vanna-service

# 5. 停止
docker compose down
```

> **注意**：首次启动 TEI 会拉取 BAAI/bge-m3 模型（约 2.5 GB），请确保网络畅通或预先挂载模型缓存目录。

---

### 模式 C：本地开发（uv + 外部三件套）

适用于已有独立运行的 qdrant/tei/vllm，仅本地运行 Python 服务进行开发调试。

```bash
# 1. 安装依赖
uv sync --extra dev

# 2. 配置环境
cp .env.example .env
# 编辑 .env，填写各项（QDRANT_URL / TEI_URL / VLLM_URL 指向外部服务）

# 3. 启动开发服务器（热重载）
uv run uvicorn app.main:app --reload --port 8088

# 4. 运行测试
uv run pytest tests/unit/ -v
uv run pytest -m eval -q        # Tier 1+2 eval（80% 门槛）
uv run pytest -m eval_stretch   # Tier 3 加分题

# 5. 代码格式检查
uv run ruff check . --fix
uv run ruff format .
uv run mypy app/
```

---

## 知识源更新流程

知识源位于 `knowledge/` 目录，由业务方/工程师维护：

| 文件 | 内容 |
|------|------|
| `knowledge/ddl.md` | 核心表 DDL + 字段注释 |
| `knowledge/terminology.md` | 业务术语（≥ 8 条） |
| `knowledge/judgment_rules.md` | 判级规则 + F_CONDITION JSON schema |
| `knowledge/qa_examples.yaml` | Q&A 示例（含 ≥ 5 条 JSON_EXTRACT few-shot） |

**更新步骤：**

```bash
# 1. 编辑 knowledge/*.md 或 qa_examples.yaml

# 2. Bump 版本号（二选一）：
#    方式 A：在 .env 中修改 KNOWLEDGE_VERSION=v2
#    方式 B：直接执行强制重建（会提示二次确认）

# 3. 强制重建 Qdrant collection
python -m app.cli reindex
# 系统会提示：请输入 collection 名称以确认（防误操作）
# 输入：nlq_vanna_knowledge

# 4. 验证
curl http://localhost:8088/healthz
```

> 服务启动时若检测到 `KNOWLEDGE_VERSION` 与 Qdrant collection 记录的版本不一致，会自动触发 reindex（无需手动操作）。

---

## Demo Runbook（Day-14 演示）

### 演示前准备（Day-12/13）

```bash
# 1. 打 Qdrant snapshot（防演示现场误操作）
python -m app.cli snapshot
# 输出：snapshots/demo-YYYYMMDD.snapshot

# 2. 验证服务健康
curl http://localhost:8088/healthz

# 3. 预跑演示题集（Tier 1 必达题）
uv run pytest -m "eval and tier1" -v --tb=short

# 4. 录屏备份（建议在 Day-13 完成）
```

### 演示流程

1. 打开前端页面，确认 `VITE_NLQ_AGENT_API_BASE` 已指向 `http://localhost:8088`
2. 按 Tier 1 题单依次提问（3-4 题纯查询 + 1-2 题简单判级）
3. 展示推理链面板（spec → rule → condition → grade/record）
4. 如需展示 Tier 2 题，准备好降级话术

### Day-14 兜底话术（Off-list 问题）

当演示现场出现不在预定题单内的问题，主持人可使用以下话术：

**主持人话术参考：**

> "这道题涉及到一些更复杂的业务逻辑，我们这个 Demo 版本的知识库目前还在持续丰富中。不过大家可以看到，系统已经识别出了相关的表结构和判级规则，基础推理链路是通的。完整的生产版本在知识库补全后可以直接支持这类问题。"

> "这个问题我们的 Tier 1 题集没有覆盖到，但您看这里系统已经找到了相关的 DDL 和规则片段——说明检索路径是正确的。SQL 生成部分需要更多的示例 few-shot 来调优，这是 Day 11 之后的工作。"

**UI 层 fallback 文案样例**（前端 `kind=fallback` 事件时显示）：

> "暂时无法生成准确的查询，请尝试换一种表述方式，或联系系统管理员补充相关知识。"

> "当前问题超出了已知知识范围，建议拆分为更具体的查询（例如：先查原始数据，再查判级结果）。"

---

## 故障排查

### G1：MySQL 不可达

服务启动时若 MySQL 连接失败，检查以下项目：

| 替代方案 | 说明 |
|---------|------|
| **SSH 隧道** | `ssh -L 8930:localhost:8930 user@内网机` 后将 `MYSQL_HOST` 改为 `127.0.0.1` |
| **内网机迁移** | 将本服务部署到与 MySQL 同网段的内网机器，避免跨公网访问 |
| **只读账号** | 联系 DBA 创建只读账号，仅开放 `SELECT` 权限，减少网络策略审批周期 |

验证连通性：

```bash
mysql -h "${MYSQL_HOST}" -P "${MYSQL_PORT}" -u "${MYSQL_USER}" -p"${MYSQL_PASSWORD}" \
      -e "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA='${MYSQL_DATABASE}' LIMIT 3"
```

### G2：TEI 维度与 Qdrant collection 不匹配

服务启动时（`bootstrap_knowledge`）会向 TEI `/info` 接口查询实际向量维度，并与 `TEI_EMBEDDING_DIM` 环境变量及 Qdrant collection 的向量配置进行三方核对。

**fast-fail 机制**：任意一方不一致时，服务拒绝启动并输出明确错误：

```
FATAL: TEI reports dim=768 but TEI_EMBEDDING_DIM=1024 and/or Qdrant collection
       'nlq_vanna_knowledge' was created with dim=1024. Fix before proceeding:
       1. Set TEI_EMBEDDING_DIM to match the actual model dimension
       2. Drop and recreate the Qdrant collection if you changed models:
          python -m app.cli reindex
```

**解决方法**：
1. 确认 `TEI_EMBEDDING_DIM` 与实际模型维度一致（BAAI/bge-m3 = 1024）
2. 如更换了嵌入模型，需 `python -m app.cli reindex` 重建 collection

### vLLM 连接失败

```bash
# 检查 vLLM 服务是否正常
curl http://localhost:8082/v1/models

# 确认 VLLM_MODEL 与实际部署的模型 ID 完全一致
# 注意：模型 ID 大小写敏感
```
