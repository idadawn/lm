# NLQ-Agent Round-3 KIMI 报告 — chat_stream 防御性修复

## 改动状态

| # | 改动 | 状态 | 说明 |
|---|------|------|------|
| 1 | `chat_stream` 防御空 choices | **Done** | `src/services/llm_client.py:95-103` |
| 2 | `chat` / `chat_json` 同类风险检查 | **Done** | `chat` 已加空 choices 检查（line 65-67）；`chat_json` 调用 `chat`，无额外风险 |
| 3 | 单元测试 | **Done** | `tests/unit/test_chat_stream_defensive.py`，6 个用例 |

## 真 bug 现场

```
2026-05-01 02:45:46 | ERROR | src.services.llm_client | LLM 流式调用失败: list index out of range
File "/data/project/lm/nlq-agent/src/services/llm_client.py", line 92
  delta = chunk.choices[0].delta
          ~~~~~~~~~~~~~^^^
IndexError: list index out of range
```

## 触发场景

SiliconFlow（及许多 OpenAI-compatible backend）会发出空 `choices=[]` 的流式 chunk：
- 心跳 keep-alive chunk
- 最终 `[DONE]` 标记
- metadata-only chunk（如 usage 信息）

原代码假设每个 chunk 必有 `choices[0]`，导致 IndexError 直接崩掉整个 SSE 流。

## 修法

```python
async for chunk in stream:
    if not chunk.choices:
        continue
    delta = chunk.choices[0].delta
    if not delta:
        continue
    content = delta.content
    if not content:
        continue
    yield content
```

- 空 choices → `continue`
- `delta` 为 None → `continue`
- `content` 为 None / 空字符串 → `continue`
- 异常 chunk 不中断 stream，只记 debug 日志

同步 `chat()` 方法同样加了 `response.choices` 为空检查，防止同类问题。

## 测试结果

```
pytest tests/ -m "not live_llm" -v
54 passed, 1 deselected in 0.78s
```

- 原有 48 个测试：全部通过
- 新增 6 个测试：全部通过
  - `test_empty_choices_heartbeat_skipped`
  - `test_delta_none_skipped`
  - `test_content_none_skipped`
  - `test_content_empty_string_skipped`
  - `test_mixed_chunks_only_yield_valid_content`
  - `test_all_empty_chunks_yields_nothing`

## Commit

```
0427058 fix(nlq-agent): defensive chat_stream against empty choices chunks
```

## 是否在 chat_json 中发现同类问题

`chat_json` 内部调用 `chat()`，不直接访问 `choices[0]`。`chat()` 已加防御。结论：**无额外风险**。
