# NLQ-Agent Production Readiness Checklist

> 本清单覆盖 nlq-agent 上线前必须验证的 5 大类事项。  
> 每项标记：✅ 已完成 / ⚠️ 部分完成或需复核 / ❌ 尚未完成  
> 每项 ❌ 附 1-2 句修复指引，含文件路径或 PR 建议。

---

## Security

- [x] **LLM_API_KEY rotation policy**（季度/月度）
  ✅ 由运维层处理(用户已确认):key 轮换走部署侧 secret 管理流程,代码侧只需 `LLM_API_KEY` env 注入。`src/core/settings.py` 字段保持简单 string,不在仓库代码内实现轮换器。

- [x] **QDRANT_API_KEY 启用 + read-only role**
  ✅ 用户已在 `.env.production` 中配置 `QDRANT_API_KEY`,Qdrant 服务端通过 `QDRANT__SERVICE__API_KEY` env 启用鉴权(详见 `docs/RUNBOOK_NLQ_E2E.md` 第 3 节)。`src/core/settings.py` 已有 `qdrant_api_key: str | None` 字段。

- [ ] **SYNC_ADMIN_TOKEN 配置 + 季度轮换**
  ✅ r10 已加。`src/core/settings.py` 新增 `sync_admin_token` 字段；`POST /api/v1/sync/resync-now` 端点通过 Bearer token 鉴权。
  测试：`tests/unit/test_resync_endpoint.py`（4 tests）。

- [x] **MySQL 只读账号**（不许 INSERT/UPDATE/DELETE 权限）
  ✅ `docs/RUNBOOK_NLQ_E2E.md` 第 2 节提供了 `CREATE USER 'nlq_readonly'` + `GRANT SELECT ON poxiao_lab.*` + 显式 `REVOKE` 写权限的完整 SQL 脚本与验证步骤。`.env.example` / `.env.production.example` 默认 `MYSQL_USER=nlq_readonly`。

- [x] **`validate_sql` 单元测试覆盖所有禁止操作**（INSERT/UPDATE/DELETE/DROP/...）
  ✅ `tests/unit/test_validate_sql_forbidden.py` 通过 66 个 parametrized 测试覆盖 `_FORBIDDEN_PATTERNS` 全部关键词（INSERT/UPDATE/DELETE/DROP/ALTER/CREATE/TRUNCATE/GRANT/REVOKE/EXEC/EXECUTE/CALL/SET/LOAD/INTO OUTFILE/INTO DUMPFILE）、大小写不敏感、子查询嵌入、空白变体、`\b` 词边界(updated_at 等列名不误伤)、空输入与 SELECT 前缀拒绝。

- [ ] **`.env.production` 不入版本控制**  
  ✅ 已 `.gitignore`，且仓库中仅保留 `.env.example`。

- [x] **CORS allow_origins 白名单**（FastAPI middleware 检查）
  ✅ r9 已完成。`src/main.py` 通过 `_resolve_cors_origins()` 从 `settings.cors_allow_origins`（`CORS_ALLOW_ORIGINS` env，逗号分隔）读取白名单；空值回退 `["*"]` 并输出警告日志。
  测试：`tests/unit/test_cors_middleware.py`（7 tests）。commit: `a82d439`。

- [ ] **RequestSizeLimit / QueryLengthGuard / RateLimit 中间件启用**  
  ✅ r5 已加，`src/main.py:73-76` 已注册三层中间件；`tests/unit/test_middleware.py` 已覆盖。

---

## Observability

- [ ] **`/health` 端点返回所有依赖状态**（Qdrant/MySQL/LLM）  
  ✅ `src/api/routes.py` 已实现，返回 `qdrant_connected` / `mysql_connected` / `llm_available`。

- [ ] **`/metrics` 端点 exposed**  
  ✅ r6 已加，`src/api/routes.py` 暴露 Prometheus 标准格式。

- [ ] **structured JSON logging 启用**（`LOG_FORMAT=json`）  
  ✅ r6 已加，`src/core/logging_config.py` 自动根据 `LOG_FORMAT=json` 切换 JSONFormatter。

- [ ] **correlation_id 注入**  
  ✅ r6 已加，`CorrelationIDMiddleware` 生成/复用 UUID，注入 response header 与日志上下文。

- [x] **Sentry / 错误追踪集成**
  ✅ r9 已完成。`src/core/sentry_integration.py` 读取 `settings.sentry_dsn`（`SENTRY_DSN` env），有值则 `sentry_sdk.init()`，无值或 SDK 未安装则 no-op + 日志。`src/main.py` lifespan startup 调用。
  测试：`tests/unit/test_sentry_integration.py`（3 tests）。commit: `a9c6350`。

---

## Performance

- [ ] **uvicorn workers ≥ 2**  
  ✅ Dockerfile `CMD` 已设 `--workers 2`。

- [x] **Qdrant collection 索引优化**（HNSW `m`, `ef_construct` 默认值）
  ✅ 用户已在部署侧针对实际数据规模调优 HNSW 参数(`scripts/init_semantic_layer.py` 创建 collection 流程或 Qdrant 集群配置外部完成)。仓库代码使用 SDK 默认值不阻塞。

- [x] **LLM streaming 超时显式化**
  ✅ 原 checklist 文案要求"显式 `aiter_text(chunk_size=1024)`"，但实际代码用 OpenAI Python SDK (`AsyncOpenAI`)，SDK 内部已抽象 SSE chunk 解析，`chunk_size` 不直接可设置。等价且生效的修复：通过 `httpx.Timeout(connect/read/write)` 显式控制超时，防止流挂死/连接堆积。
  - `src/core/settings.py` 新增 `llm_http_connect_timeout_s` (10s)、`llm_http_read_timeout_s` (300s 长流容忍)、`llm_http_timeout_s` (60s 兜底)。
  - `src/services/llm_client.py` 用 `AsyncOpenAI(timeout=httpx.Timeout(...))` 注入。
  - 测试：`tests/unit/test_llm_client_timeout.py` (3 tests，含环境变量覆盖)。

- [x] **连接池: MySQL `aiomysql` pool_recycle, Qdrant grpc_port**
  ✅ `src/services/database.py` `aiomysql.create_pool(...)` 已增加 `pool_recycle=3600`，每小时回收闲置连接以躲避云 LB / 防火墙的 5-30 分钟 idle drop。Qdrant `grpc_port=6334` 已在 `docker-compose.production.yml` 配置。
  - 测试：`tests/unit/test_database_pool_recycle.py` (2 tests，断言 pool_recycle=3600 + 其他 kwargs 不漂移)。

---

## Reliability

- [ ] **重启策略 `restart=unless-stopped`**  
  ✅ r7 已设，`docker-compose.production.yml:35` 与 `qdrant:71` 均已配置。

- [ ] **healthcheck 配置**  
  ✅ r7 已设，Dockerfile 与 docker-compose.production.yml 均配置了 `curl -f http://localhost:18100/health`。

- [ ] **graceful shutdown lifespan 处理**  
  ✅ `src/main.py:28-49` 已使用 `@asynccontextmanager` lifespan，shutdown 时调用 `shutdown_services()` 关闭连接池与客户端。

- [ ] **backpressure: `active_chat_streams` Gauge 监控**  
  ✅ r6 已加，`src/core/metrics.py:49-52` 定义 Gauge，`track_chat_stream_duration` 在流开始/结束时 inc/dec。

---

## Operations

- [x] **部署手册: `docs/RUNBOOK_NLQ_E2E.md`**
  ✅ 文件已创建，覆盖 8 节：前置依赖、MySQL 只读账号 SQL、Qdrant API Key 启用、生产部署快速开始、健康检查与验收、回滚与故障处理、上线 Checklist 速查、配套文档索引。

- [ ] **文档: `nlq-agent/API.md`**  
  ✅ r5 已写，覆盖端点、SSE 事件、错误码、部署命令。

- [ ] **CI/CD: `.github/workflows/nlq-agent-ci.yml`**
  ✅ r7 已加，覆盖 lint、pytest、docker-compose config 验证。

- [ ] **夜间 bulk-resync cron（curl /api/v1/sync/resync-now）**
  ✅ r10 已加。`POST /api/v1/sync/resync-now` 端点支持全量重建 Qdrant 索引，配合 host crontab `0 2 * * *` 使用。

- [ ] **dependabot config**  
  ✅ r7 已加，`.github/dependabot.yml` 已配置 pip 与 github-actions 更新。

- [x] **container registry 推送策略**（GHCR/ACR/...）
  ✅ 用户当前部署模式为本地 docker 直接 build + run(`docker build` → `docker compose -f docker-compose.production.yml up -d`),无需 registry 中转,故不引入推送 workflow。如未来转向多机分发再补 `.github/workflows/nlq-agent-docker.yml`。

---

## 快速统计

| 类别 | ✅ | ⚠️ | ❌ |
|------|---|---|---|
| Security | 8 | 0 | 0 |
| Observability | 5 | 0 | 0 |
| Performance | 4 | 0 | 0 |
| Reliability | 4 | 0 | 0 |
| Operations | 6 | 0 | 0 |
| **合计** | **27** | **0** | **0** |

> 全部 27 项就绪。LLM_API_KEY 轮换、QDRANT_API_KEY 启用、HNSW 调优、registry 策略由用户在部署侧完成,代码侧已具备所有可配置接入点(env vars / 现有 init script / Dockerfile)。
