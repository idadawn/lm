# 你是 nlq-agent round-3 的 GLM 工人 — qdrant-client 1.10+ API 升级 + e2e regression test

## 工作区
- worktree：**`/data/project/lm-team/glm/`**（分支 `omc-team/r3/glm-qdrant-1.10-upgrade`，基于 main `3a4e602`）
- 你只能改这个目录下的文件。
- 主仓库 `/data/project/lm/` 与 KIMI worktree `/data/project/lm-team/kimi/` 禁止触碰。

## 真 bug 上下文（已修一处，需要全文档梳理）

真 e2e 跑出来撞到：`AsyncQdrantClient.search` 在 qdrant-client ≥ 1.10 已删除。我（lead）已经在 main `qdrant_service.py:152` 把单点修了：

```python
# 已修：search → query_points
response = await self._client.query_points(
    collection_name=collection_name,
    query=query_vector,
    limit=top_k or self._top_k,
    query_filter=qdrant_filter,
    score_threshold=score_threshold,
)
return [...for hit in response.points]
```

但 main 这一处 fix 不在你的 worktree（你基于 main `3a4e602`，**不含**这个 fix）。你需要：
1. 在自己 worktree 里**重新做这个 fix**（同样改法）
2. 扫整个 `nlq-agent/src/services/qdrant_service.py` 看还有没有其他 1.10 已废弃 API
3. 加 e2e regression test 防止以后回退

## 任务（3 项）

### 改动 #1 — 重做 main 的 search → query_points fix
在你 worktree 的 `nlq-agent/src/services/qdrant_service.py` 里把 `search()` 方法（约 line 145-172）的 `await self._client.search(...)` 改为 `await self._client.query_points(...)`，返回值用 `response.points`。**与我在 main 改的等价**。

### 改动 #2 — 全 qdrant-client API 梳理
扫 `nlq-agent/` 全部 `.py` 文件，找出所有 `qdrant_client` 类的方法调用：
```bash
grep -rn 'self\._client\.\|AsyncQdrantClient\|QdrantClient' nlq-agent/src nlq-agent/scripts
```

逐个核对是否在 qdrant-client ≥ 1.10 还存在。已知 1.10 删除：
- `search` → 用 `query_points`
- `search_groups` → 用 `query_points` + groupby
- `recommend` → 用 `query_points(query=RecommendQuery(...))`
- `count` 仍在但参数变了

把发现的所有问题在自己 worktree 修掉。`upsert` / `create_collection` / `delete_collection` / `scroll` / `retrieve` 仍是稳定 API，无需改。

### 改动 #3 — e2e regression test
新增 `nlq-agent/tests/e2e/test_qdrant_api_compat.py`：
- 真起一个 `AsyncQdrantClient` 连 `localhost:6333`（如果连不上，pytest.skip — 通过 `pytest.importorskip` 或 socket 检查）
- 创建临时 collection
- 写入 3 条 mock doc（用 `_fake_embedding` 类似 round-3 lead 写的 `mock_ingest_demo.py` 风格，hash → 1024 维向量）
- 跑 `qdrant_service.search()` + `search_multi_collection()` 两个公开方法
- 断言不抛任何 AttributeError（这是关键 — 旧 API 调用会导致 AttributeError）
- 删除临时 collection

测试用 `@pytest.mark.live_qdrant` 自定义 marker（在 pyproject.toml 注册），CI 默认 skip，本地或 staging 跑。

如果整 e2e regression test 太复杂（涉及真 docker），就改成**纯 unit-style smoke**：用 `inspect.getmembers(AsyncQdrantClient)` 断言 `query_points`、`upsert`、`create_collection` 等关键方法存在。

## 项目背景
- LIMS Python FastAPI 微服务，Stage1+Stage2 NL→SQL 在 main 已合并
- Qdrant 1.11.3 服务（docker，已运行）；qdrant-client 1.17.x（pyproject.toml）
- `_client = AsyncQdrantClient`
- 必读：`docs/RUNBOOK_NLQ_E2E.md`（如果存在）

## 执行约束
- **只动 `nlq-agent/src/services/qdrant_service.py` + 新增测试 + 可能动 pyproject.toml 加 marker**
- **不准** spawn 子代理
- **不准** 改 `llm_client.py`（KIMI 在并行修）
- **不准** 改 stage1/stage2/orchestrator/api/utils
- 跑 `pytest tests/ -m "not live_llm and not live_qdrant"` 全绿；额外跑一次有 Qdrant 时 `pytest -m live_qdrant` 也得绿（如果你有起 Qdrant）

## 提交规范
≥2 atomic commits：
- `fix(nlq-agent): use query_points instead of removed AsyncQdrantClient.search`
- `test(nlq-agent): qdrant-client 1.10+ API compatibility regression`

commit body 要解释 1.10 deprecation 上下文 + 为什么必须升级。`Co-Authored-By: Claude Opus 4.7` 或类似 attribution（与 round-2 风格一致）。

## 输出
完成时：
1. `git log --oneline 3a4e602..HEAD` 显示 ≥2 commits
2. `pytest tests/ -m "not live_llm"` 全绿
3. 在 `/data/project/lm/.omc/team-nlq-r3/glm/REPORT.md` 写：
   - 改动 #1 / #2 / #3 状态
   - 全文档梳理发现：qdrant_service.py 总共有几处需要升级，是否还有其他 1.10 deprecated 调用
   - e2e regression test 选了真 docker 还是 smoke 路线，为什么
   - commit 哈希

阻塞写到 `/data/project/lm/.omc/team-nlq-r3/glm/BLOCKER.md`。

## 时间约束
1h 截止（round-3 已用 ~28min，你大概还有 32min）。优先 #1 + #3 smoke 路线；#2 全文档梳理来不及就在 REPORT 标"扫了 N 个文件，发现 M 处可疑"。

开始。先 Read 当前 qdrant_service.py，再做 fix，再加测试。
