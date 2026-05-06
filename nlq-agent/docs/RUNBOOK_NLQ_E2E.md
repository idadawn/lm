# RUNBOOK: nlq-agent 端到端生产部署

> 适用范围:nlq-agent v0.1.x 生产环境部署、健康检查、回滚与故障处理。
> 配套清单:[`PRODUCTION_CHECKLIST.md`](../PRODUCTION_CHECKLIST.md) 列出所有上线前必须验证的项目;本文档是其操作手册。

## 1. 前置依赖

| 组件 | 用途 | 部署位置 |
|---|---|---|
| MySQL 5.7+ / 8.0 | 业务数据(只读访问) | 外部数据库实例,不在本 compose 内 |
| Qdrant 1.7+ | 向量检索 | 同一 Docker network 内的容器 |
| TEI(text-embeddings-inference) | bge-m3 向量化服务 | 同一 Docker network,或宿主机端口 8001 |
| LLM API(vLLM / OpenAI-compatible) | SQL 生成与推理 | 外部服务 |

部署主机要求:Docker 24+、docker compose v2、4 核 / 8GB 内存起步。

## 2. MySQL 只读账号创建

nlq-agent 通过 `_FORBIDDEN_PATTERNS` 在应用层拒绝写操作,但**生产环境必须用只读账号作为最后一道防线**。

在生产 MySQL 上以管理员身份执行:

```sql
-- 创建只读账号(配合 .env.production 中 MYSQL_USER=nlq_readonly)
CREATE USER 'nlq_readonly'@'%' IDENTIFIED BY '<填入强密码>';

-- 仅授予业务库的 SELECT 权限,绝对不要 GRANT ALL
GRANT SELECT ON poxiao_lab.* TO 'nlq_readonly'@'%';

-- 显式撤销可能继承到的写权限(防御深度)
-- 注意:在全新账号上执行此语句会因"未授予的权限无法 REVOKE"返回
-- ERROR 1141 (There is no such grant defined),这属于预期行为,可忽略;
-- 保留它的目的是当账号被意外加过其他权限时,这一步会真正生效。
REVOKE INSERT, UPDATE, DELETE, DROP, ALTER, CREATE, TRUNCATE,
       GRANT OPTION ON poxiao_lab.* FROM 'nlq_readonly'@'%';

FLUSH PRIVILEGES;
```

验证账号确实只读:

```sql
-- 用 nlq_readonly 登录后执行,以下应全部失败
INSERT INTO some_table VALUES (1);          -- 期望: ERROR 1142 INSERT command denied
UPDATE some_table SET x=1 WHERE id=1;       -- 期望: ERROR 1142
DELETE FROM some_table WHERE id=1;          -- 期望: ERROR 1142
```

settings 默认值(`src/core/settings.py`)中 `mysql_user` 即为 `nlq_readonly`,与 `.env.production.example` 一致。

## 3. Qdrant API Key 启用

生产环境必须启用 API Key,否则任何持有 Qdrant 端口访问权的进程都能读写向量库。

### 3.1 生成密钥

```bash
# 64 字节 URL-safe 随机字符串
python -c "import secrets; print(secrets.token_urlsafe(64))"
```

### 3.2 写入 .env.production

```bash
QDRANT_HOST=nlq-qdrant-prod        # docker-compose.production.yml 中的容器名
QDRANT_PORT=6333
QDRANT_GRPC_PORT=6334
QDRANT_API_KEY=<上一步生成的随机字符串>
```

### 3.3 在 docker-compose.production.yml 中传给 Qdrant 服务端

Qdrant 通过 `QDRANT__SERVICE__API_KEY` 环境变量启用鉴权(双下划线表示嵌套配置):

```yaml
services:
  qdrant:
    image: qdrant/qdrant:v1.7.4
    environment:
      QDRANT__SERVICE__API_KEY: ${QDRANT_API_KEY}
    # ... 其他配置
```

启用后:任何对 6333/6334 的请求都必须带 `api-key: <密钥>` header,否则 401。

> **可选:只读 collection 角色**
> Qdrant 同时支持 `QDRANT__SERVICE__READ_ONLY_API_KEY`,可签发只读 key 给只查询不写入的客户端。nlq-agent 启动时会写入(创建 collection)和读取,因此使用主 API Key 即可;如有外部分析工具仅需查询,再单独配置只读 key。

## 4. 生产环境部署快速开始

### 4.1 准备配置

```bash
cd nlq-agent
cp .env.production.example .env.production
# 用编辑器填入: MYSQL_HOST/USER/PASSWORD/DATABASE, QDRANT_API_KEY,
# LLM_BASE_URL/API_KEY/MODEL, SYNC_ADMIN_TOKEN
```

`.env.production` 已被 `.gitignore` 排除,确保不要提交。

### 4.2 验证配置

```bash
python scripts/verify_env.py --env-file .env.production
```

退出码:
- `0` — 全部通过
- `1` — 必填项缺失或值是 placeholder(`YOUR_*` / `sk-test*`),**必须修正后才能继续**
- `2` — 仅网络连通性警告(可继续,但需确认依赖服务可达)

### 4.3 启动服务

```bash
docker compose -f docker-compose.production.yml up -d
```

确认容器全部 healthy:

```bash
docker compose -f docker-compose.production.yml ps
# 期望: nlq-agent-prod 和 nlq-qdrant-prod 都是 Up (healthy)
```

### 4.4 烟雾测试

```bash
curl -s http://localhost:18100/health | python -m json.tool
```

期望响应:

```json
{
  "status": "ok",
  "qdrant_connected": true,
  "mysql_connected": true,
  "llm_available": true
}
```

任意一个为 `false` → 进入第 6 节 故障处理。

## 5. 健康检查与验收

### 5.1 端点清单

| 端点 | 用途 | 期望状态 |
|---|---|---|
| `GET /health` | 三依赖连通性 | `qdrant_connected`、`mysql_connected`、`llm_available` 全部 `true` |
| `GET /metrics` | Prometheus 指标(JSON 计数 + 直方图) | 200 OK,包含 `chat_stream_requests_total`、`active_chat_streams` |
| `POST /api/v1/chat/stream` | SSE 流式问答 | 200 OK,`Content-Type: text/event-stream`,逐 chunk 返回 SSE 事件 |
| `POST /api/v1/sync/resync-now` | 全量重建 Qdrant 索引(管理员) | 需 `Authorization: Bearer <SYNC_ADMIN_TOKEN>`,返回 202 |

### 5.2 SSE 烟雾测试

```bash
curl -N -X POST http://localhost:18100/api/v1/chat/stream \
  -H "Content-Type: application/json" \
  -d '{"messages":[{"role":"user","content":"50W470 牌号硅钢片的铁损合格率"}]}'
```

期望:看到 `event: stage1`、`event: sql`、`event: result`、`event: done` 等连续 SSE 事件。详细协议见 [`API.md`](../API.md)。

### 5.3 夜间 bulk-resync(可选)

如启用语义层夜间重建,在宿主机 crontab 添加:

```cron
0 2 * * * curl -X POST -H "Authorization: Bearer ${SYNC_ADMIN_TOKEN}" \
  http://localhost:18100/api/v1/sync/resync-now \
  >> /var/log/nlq-resync.log 2>&1
```

`scripts/resync_nlq_nightly.sh` 是同等功能的 shell 包装。

## 6. 回滚与故障处理

### 6.1 回滚到前一镜像

`docker-compose.production.yml` 用了具名 image tag。回滚步骤:

```bash
# 1. 编辑 docker-compose.production.yml,把 nlq-agent.image 改回上一个版本 tag
# 2. 重新拉起
docker compose -f docker-compose.production.yml up -d --force-recreate nlq-agent
# 3. 等待 healthcheck
docker compose -f docker-compose.production.yml ps
```

### 6.2 常见故障与排查

| 症状 | 排查命令 | 可能原因 |
|---|---|---|
| `/health` 返回 `mysql_connected: false` | `docker exec nlq-agent-prod python -c "import asyncio; from src.services.database import DatabaseService; asyncio.run(DatabaseService().init_pool())"` | MySQL 账号无权限 / 网络不通 / `MYSQL_HOST` 拼写错 |
| `/health` 返回 `qdrant_connected: false` | `docker exec nlq-agent-prod curl -H "api-key: $QDRANT_API_KEY" http://nlq-qdrant-prod:6333/healthz` | API Key 不匹配 / 容器名解析失败 |
| `/health` 返回 `llm_available: false` | `docker exec nlq-agent-prod curl $LLM_BASE_URL/models -H "Authorization: Bearer $LLM_API_KEY"` | LLM 服务下线 / API Key 失效 |
| SSE 流没数据 | `docker compose -f docker-compose.production.yml logs --tail=200 nlq-agent` | LLM 超时 / Qdrant 索引未建好 |
| `verify_env.py` 退出码 1 | 看输出红色 ❌ 行 | `.env.production` 必填项缺失或仍是 placeholder |
| `verify_env.py` 退出码 2 | 看输出黄色 ⚠️ 行 | 网络可达性问题,通常不影响启动但要尽快修 |

### 6.3 日志位置

- 应用 JSON 日志:`docker compose logs nlq-agent`,字段含 `correlation_id`(可串起一次请求的全链路)
- Sentry:已配 `SENTRY_DSN` 时,异常自动上报
- Prometheus 指标:`/metrics` 端点抓取

### 6.4 紧急下线

如发现严重问题需立即停服:

```bash
docker compose -f docker-compose.production.yml stop nlq-agent
# Qdrant 数据持久化在 named volume,可继续保留
```

恢复:

```bash
docker compose -f docker-compose.production.yml start nlq-agent
```

## 7. 上线前 Checklist 速查

完整清单见 [`PRODUCTION_CHECKLIST.md`](../PRODUCTION_CHECKLIST.md)。本 RUNBOOK 配套的最小核对项:

- [ ] `.env.production` 所有必填字段填写完毕,`verify_env.py --env-file .env.production` 退出码 0 或 2
- [ ] MySQL `nlq_readonly` 账号已建,且 `INSERT/UPDATE/DELETE` 全部被服务端拒绝
- [ ] `QDRANT_API_KEY` 已生成且同时在 nlq-agent `.env.production` 与 qdrant 容器 `QDRANT__SERVICE__API_KEY` 中配置一致
- [ ] `SYNC_ADMIN_TOKEN` 已生成强随机值,准备季度轮换
- [ ] `docker compose -f docker-compose.production.yml ps` 全部 `healthy`
- [ ] `curl /health` 三依赖全 `true`
- [ ] `curl /metrics` 返回 Prometheus 指标
- [ ] SSE 烟雾测试能看到 `event: done`

## 8. 配套文档

- [`API.md`](../API.md) — 端点协议、SSE 事件、错误码
- [`PRODUCTION_CHECKLIST.md`](../PRODUCTION_CHECKLIST.md) — 上线就绪 5 大类清单
- [`CONTRIBUTING.md`](../CONTRIBUTING.md) — 开发与提交规范
- `.github/workflows/nlq-agent-ci.yml`(仓库根)— CI 流水线,合并前必须通过 ruff + pytest + docker-compose config
- `scripts/verify_env.py` — `.env` 配置校验器(本 RUNBOOK 第 4.2 节)
- `scripts/resync_nlq_nightly.sh` — 夜间 Qdrant 重建脚本(本 RUNBOOK 第 5.3 节)
