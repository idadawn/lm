# NLQ-Agent Production Readiness Checklist

> 本清单覆盖 nlq-agent 上线前必须验证的 5 大类事项。  
> 每项标记：✅ 已完成 / ⚠️ 部分完成或需复核 / ❌ 尚未完成  
> 每项 ❌ 附 1-2 句修复指引，含文件路径或 PR 建议。

---

## Security

- [ ] **LLM_API_KEY rotation policy**（季度/月度）  
  ❌ 当前 `.env.production` 中硬编码单 KEY，无轮换机制。  
  修复：接入 Vault / AWS Secrets Manager 或设计主备 KEY 切换逻辑，见 `src/core/settings.py`。

- [ ] **QDRANT_API_KEY 启用 + read-only role**  
  ❌ `QDRANT_API_KEY` 在 `.env.example` 中为空，docker-compose.production.yml 未传入。  
  修复：在 Qdrant 服务端配置 API Key 与只读角色，并在 `docker-compose.production.yml` 的 `qdrant.environment` 与 `nlq-agent.env_file` 中同步启用。

- [ ] **SYNC_ADMIN_TOKEN 配置 + 季度轮换**
  ✅ r10 已加。`src/core/settings.py` 新增 `sync_admin_token` 字段；`POST /api/v1/sync/resync-now` 端点通过 Bearer token 鉴权。
  测试：`tests/unit/test_resync_endpoint.py`（4 tests）。

- [ ] **MySQL 只读账号**（不许 INSERT/UPDATE/DELETE 权限）  
  ⚠️ `.env.example` 已建议 `nlq_readonly`，但需在部署手册中强制约束账号权限。  
  修复：在 `docs/RUNBOOK_NLQ_E2E.md` 增加 GRANT SELECT -only 的建账号脚本。

- [ ] **`validate_sql` 单元测试覆盖所有禁止操作**（INSERT/UPDATE/DELETE/DROP/...）  
  ⚠️ `src/services/database.py` 已有关键词黑名单，但缺少针对每条禁止操作的独立单元测试。  
  修复：在 `tests/unit/` 补充 `test_validate_sql_forbidden.py`，遍历 `_FORBIDDEN_PATTERNS` 中每个关键词断言拒绝。

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

- [ ] **Qdrant collection 索引优化**（HNSW `m`, `ef_construct` 默认值）  
  ⚠️ 当前使用 Qdrant 默认 HNSW 参数，未针对实验室数据规模调优。  
  修复：在 `scripts/init_semantic_layer.py` 创建 collection 时显式设置 `hnsw_config={"m": 16, "ef_construct": 100}`，并根据 recall  benchmark 调整。

- [ ] **LLM streaming chunk_size 合理**  
  ⚠️ `src/services/llm_client.py` 中 SSE 读取 chunk 大小未显式配置，依赖 httpx 默认。  
  修复：显式设置 `aiter_text(chunk_size=1024)` 或 `aiter_bytes(chunk_size=1024)`，避免过小导致 CPU 空转。

- [ ] **连接池: MySQL `aiomysql` pool_recycle, Qdrant grpc_port**  
  ⚠️ `src/services/database.py:39-48` 未设置 `pool_recycle`，长连接可能因防火墙/云厂商超时断开。  
  修复：在 `aiomysql.create_pool(...)` 中增加 `pool_recycle=3600`。  
  ✅ Qdrant `grpc_port=6334` 已在 `docker-compose.production.yml:65` 配置。

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

- [ ] **部署手册: `docs/RUNBOOK_NLQ_E2E.md`**  
  ⚠️ `CONTRIBUTING.md` 与 `API.md` 均引用该文件，需确认内容完整覆盖生产环境部署步骤。  
  修复：补充 MySQL 只读账号创建、Qdrant API Key 配置、容器 registry 拉取命令。

- [ ] **文档: `nlq-agent/API.md`**  
  ✅ r5 已写，覆盖端点、SSE 事件、错误码、部署命令。

- [ ] **CI/CD: `.github/workflows/nlq-agent-ci.yml`**
  ✅ r7 已加，覆盖 lint、pytest、docker-compose config 验证。

- [ ] **夜间 bulk-resync cron（curl /api/v1/sync/resync-now）**
  ✅ r10 已加。`POST /api/v1/sync/resync-now` 端点支持全量重建 Qdrant 索引，配合 host crontab `0 2 * * *` 使用。

- [ ] **dependabot config**  
  ✅ r7 已加，`.github/dependabot.yml` 已配置 pip 与 github-actions 更新。

- [ ] **container registry 推送策略**（GHCR/ACR/...）  
  ❌ 尚无自动化镜像构建与推送工作流。  
  修复：新增 `.github/workflows/nlq-agent-docker.yml`，在 tag push 时构建并推送至 GHCR（或内部 ACR），并附带 SBOM 与签名。

---

## 快速统计

| 类别 | ✅ | ⚠️ | ❌ |
|------|---|---|---|
| Security | 3 | 1 | 3 |
| Observability | 5 | 0 | 0 |
| Performance | 1 | 3 | 0 |
| Reliability | 4 | 0 | 0 |
| Operations | 3 | 1 | 1 |
| **合计** | **16** | **5** | **4** |

> 建议优先修复 ❌ 项：LLM KEY 轮换、Qdrant API Key、validate_sql 单元测试、容器 registry。
