# 你是 round-10 GLM 工人 — nlq-agent 管理端 bulk-resync endpoint

## 工作区（严格）
- worktree：**`/data/project/lm-team/glm/`**（分支 `omc-team/r10/glm-resync-endpoint`，基于 main `0bfcc64`）
- **第一件事**：`pwd` + `git rev-parse --abbrev-ref HEAD`。错则 BLOCKER（写 `/data/project/lm/.omc/team-nlq-r10/glm/BLOCKER.md`）。
- **绝对禁止** 在 `/data/project/lm` 主仓库执行任何 git 操作。
- **绝对禁止** `cd` 出 `/data/project/lm-team/glm/` 范围。
- 主仓库与 KIMI worktree 禁止触碰。

## 背景（必读）
- 设计稿：`docs/decisions/2026-05-01-nlq-tracer-bullet-r4-r8-final/F3_NET_EVENT_DESIGN.md`（hybrid 决定）
- KIMI 在并行做 .NET 端事件总线发布"实时增量"；你做"夜间 cron / 手动触发"那一半 —— 一个能让外部 cron / 运维一键全量重建的 admin endpoint。
- 现成 `nlq-agent/scripts/init_semantic_layer.py` 已经写过批量 ingest 逻辑；你的任务是把它**抽出可在进程内调用的函数**，再 wrap 成 HTTP endpoint。

## 范围 — 增量 #1～#3

### 改动 #1 — 重构 `nlq-agent/scripts/init_semantic_layer.py`（small refactor）
- 把 main 脚本里的 bulk-load 逻辑抽出为 `async def bulk_resync_all() -> dict[str, int]`（返回 `{"rules": 12, "specs": 5, "duration_ms": 432}`）
- 放到 `nlq-agent/src/services/resync_service.py` 新模块
- 脚本入口改成 `if __name__ == "__main__": asyncio.run(bulk_resync_all())` + log 输出
- **不**改变 CLI 行为（仍可 `python -m scripts.init_semantic_layer` 跑）

### 改动 #2 — `nlq-agent/src/api/routers/sync.py` 加新 endpoint
（如果 sync.py 不存在，看现有 `/api/v1/sync/rules` 在哪个 router 文件，加到那里）

```python
@router.post("/api/v1/sync/resync-now", status_code=202)
async def resync_now(authorization: str = Header(...)) -> dict:
    expected = settings.sync_admin_token
    if not expected or authorization != f"Bearer {expected}":
        raise HTTPException(status_code=401, detail="invalid_admin_token")
    result = await bulk_resync_all()
    return {"status": "ok", **result}
```

- `nlq-agent/src/core/settings.py` 加 `sync_admin_token: str = ""` 字段
- `.env.example` 加 `SYNC_ADMIN_TOKEN=` 占位
- `.env.production.example` 同步加（**不**填实际值）
- 401 if token 缺失/不匹配；空 token 配置时也拒绝（不允许公开访问）

### 改动 #3 — 测试
`tests/unit/test_resync_endpoint.py`（≥4 用例）：
- token 缺失 → 401
- token 不匹配 → 401
- token 匹配 + bulk_resync_all mock → 202 + 正确 body
- settings.sync_admin_token 为空 → 401（即使请求 header 带 token）

`tests/unit/test_bulk_resync.py`（≥2 用例）：mock Qdrant/MySQL/embedding，断言 bulk_resync_all 返回字典 schema 正确。

### 改动 #4 — 文档
`nlq-agent/API.md` 加一节：

```markdown
### POST /api/v1/sync/resync-now (admin)

全量重建 Qdrant 索引。供夜间 cron / 运维手动触发使用。

**Headers:** `Authorization: Bearer <SYNC_ADMIN_TOKEN>`
**Response 202:** `{"status": "ok", "rules": 12, "specs": 5, "duration_ms": 432}`
**Response 401:** `{"detail": "invalid_admin_token"}`

设置 `.env`:
SYNC_ADMIN_TOKEN=<rotate quarterly>

夜间 cron 示例（host crontab）:
0 2 * * * curl -fsS -X POST -H "Authorization: Bearer ${SYNC_ADMIN_TOKEN}" http://localhost:18100/api/v1/sync/resync-now
```

`nlq-agent/PRODUCTION_CHECKLIST.md`：
- Operations 加一行 ✅ "夜间 bulk-resync cron（curl /api/v1/sync/resync-now）"
- Security 加一行 ✅ "SYNC_ADMIN_TOKEN 配置 + 季度轮换"

## 执行约束
- **不准** spawn 子代理
- **不准**改 `api/` 下任何 .NET 代码（KIMI 的领域）
- **不准**改 `web/` 下任何代码（lead 的领域）
- **不准**改 `nlq-agent/scripts/init_semantic_layer.py` 的 CLI 接口（只能抽函数+保留入口）
- 跑外部服务 — 不准（测试用 mock）
- 现有 e2e + load 必须仍全绿（≥225 default + 4 load）

## 提交规范
≥2 atomic commits：
- `refactor(nlq-agent): extract bulk_resync_all from init_semantic_layer.py`
- `feat(nlq-agent): POST /api/v1/sync/resync-now admin endpoint with bearer token`

`Co-Authored-By: Claude Opus 4.7` attribution。`git status` 干净。

## 输出
1. `git log --oneline 0bfcc64..HEAD` ≥2 commits
2. `pytest tests/ -m "not live_llm and not live_qdrant and not load"` 全绿（≥225 + 6 new = ≥231）
3. `pytest tests/ -m load` 全绿（4）
4. `/data/project/lm/.omc/team-nlq-r10/glm/REPORT.md`：
   - bulk_resync_all 函数签名 + 返回 schema
   - endpoint 路径 + 401 条件
   - 改动文件 + LOC
   - 测试用例数
   - commits

阻塞写到 `/data/project/lm/.omc/team-nlq-r10/glm/BLOCKER.md`。

## 时间约束
~25 min。优先 #1+#2+#3；#4 文档 5 分钟收口。

开始 — 先 pwd！
