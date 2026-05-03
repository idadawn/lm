# 你是 nlq-agent round-3 的 KIMI 工人 — 修 llm_client.chat_stream 防御 + 测试

## 工作区
- worktree：**`/data/project/lm-team/kimi/`**（分支 `omc-team/r3/kimi-llm-chat-stream-fix`，基于 main `3a4e602`）
- 你只能改这个目录下的文件。
- 主仓库 `/data/project/lm/` 与 GLM worktree `/data/project/lm-team/glm/` 禁止触碰。

## 真 bug 上下文（已实测）
真 e2e SSE 跑通了 Stage1 + Stage2 SQL 执行，但卡在最终 LLM 流式答案生成：

```
2026-05-01 02:45:46 | ERROR | src.services.llm_client | LLM 流式调用失败: list index out of range
File "/data/project/lm/nlq-agent/src/services/llm_client.py", line 92
  delta = chunk.choices[0].delta
          ~~~~~~~~~~~~~^^^
IndexError: list index out of range
```

`chunk.choices[0]` 假设流式 chunk 必有 choices，但 SiliconFlow（与许多 OpenAI 兼容 backend）会发出空 `choices=[]` 的 chunk（心跳、最后的 [DONE]、metadata-only chunk）。当前代码没防御。

## 任务（单文件改动 + 测试）

### 改动 #1 — `nlq-agent/src/services/llm_client.py:88-95`（chat_stream）
现在的代码：
```python
async for chunk in stream:
    delta = chunk.choices[0].delta
    if delta.content:
        yield delta.content
```

改成防御版：
- `chunk.choices` 为空 → continue
- `delta` 为 None → continue
- `delta.content` 为 None / 空 → continue
- 任何 chunk 解析异常都不应让整个 stream 崩；`logger.debug` 打印异常细节后 continue

具体形态自定，但**必须满足**：
1. 空 choices chunk 不抛异常
2. 真有 content 的 chunk 仍正常 yield
3. 整个 stream 即使遇到异常 chunk 也能跑完（除非真的网络/认证错误）

### 改动 #2 — `nlq-agent/src/services/llm_client.py:chat_json`
扫一遍同文件的 `chat_json`、`chat`（如果有）方法，看是否有同样的 `chunk.choices[0]` 或 `response.choices[0]` 风险。如果有，加防御。如果没有，REPORT 里说明"已检查 chat_json 无此风险"。

### 改动 #3 — 单元测试
新增 `nlq-agent/tests/unit/test_chat_stream_defensive.py`：
- 喂一个 mock stream，混入 `MagicMock(choices=[])` 的"心跳" chunk
- 喂一个 mock stream，含正常 chunk + delta=None chunk + content=None chunk + 正常 chunk
- 断言只有正常 chunk 的 content 被 yield 出来
- 断言整个 stream 被消费完（不抛 IndexError）
- ≥4 个测试用例

测试可以用 `unittest.mock.AsyncMock` 模拟 `OpenAI()` async client。参考 `test_pipeline.py` 里现有的 mock 风格。

## 项目背景
- LIMS Python FastAPI 微服务，Stage1+Stage2 NL→SQL 已合并到 main
- LLM 后端：SiliconFlow（OpenAI 兼容）。当前 `LLM_MODEL=deepseek-ai/DeepSeek-V4-Flash` 已实测可用
- 主路径：`/api/v1/chat/stream` → orchestrator → stage1 → stage2 → 最后 stage2 调 `chat_stream` 流式回答
- 必读：`docs/RUNBOOK_NLQ_E2E.md`（如果存在）— 我刚写的真 e2e 启动手册

## 执行约束
- **只动 `nlq-agent/src/services/llm_client.py` + 新增 `tests/unit/test_chat_stream_defensive.py`**
- **不准** spawn 子代理
- **不准** 改其他文件（`stage1/`、`stage2/`、`models/`、`api/`、`utils/`、`scripts/` 全不动）
- **不准** 改 e2e tests（GLM 在并行做 e2e regression test）
- 跑 `pytest tests/ -m "not live_llm"` 全绿（特别是新加的测试 + 原 53 个测试）

## 提交规范
1 个 atomic commit，Conventional Commits：
- `fix(nlq-agent): defensive chat_stream against empty choices chunks`

commit message body 要写：
- 真 bug 的现场（IndexError at line 92）
- 触发场景（SiliconFlow 心跳 chunk）
- 修法（empty choices skip）
- 测试覆盖

`Co-Authored-By: Claude Sonnet 4.6` 或类似 attribution（与 round-2 风格一致）。

## 输出
完成时：
1. `git log --oneline 3a4e602..HEAD` 显示 1 commit
2. `pytest tests/ -m "not live_llm"` 全绿
3. 在 `/data/project/lm/.omc/team-nlq-r3/kimi/REPORT.md` 写：
   - 改动 #1 / #2 / #3 状态
   - 测试结果（passed 数）
   - 是否在 `chat_json` 里也发现同类问题
   - commit 哈希

阻塞写到 `/data/project/lm/.omc/team-nlq-r3/kimi/BLOCKER.md`。

## 时间约束
1h 截止（round-3 时间已经过去 ~28min，你大概还有 32min）。优先 #1 + #3，#2 来不及就在 REPORT 标 TODO。

开始。先 Read 当前 llm_client.py，再写 fix，再写测试。
