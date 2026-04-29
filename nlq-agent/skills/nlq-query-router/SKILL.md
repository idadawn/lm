---
name: nlq-query-router
description: 路美 NLQ Agent 意图识别与查询路由——配置三类意图的分类规则和处理路径
argument-hint: "<需要配置的意图类型或路由问题，例如：添加磁性数据根因分析路由>"
level: 2
---

<Purpose>
本 Skill 专注于路美 NLQ Agent 的**意图识别与查询路由**模块，即 `src/pipelines/orchestrator.py` 中的 `_route_intent()` 方法和 `src/utils/prompts.py` 中的 `INTENT_CLASSIFICATION_PROMPT`。

路由层是整个 Pipeline 的"大脑"，决定用户的问题走哪条处理路径，直接影响回答质量和响应速度。
</Purpose>

<Intent_Types>
系统支持四种意图类型（`src/models/schemas.py` 中的 `IntentType`）：

| 意图类型 | 触发场景 | 处理路径 | 示例问题 |
|----------|----------|----------|----------|
| `statistical` | 统计聚合查询 | Stage1 + Stage2（SQL 查询） | "本月A类合格率是多少？" |
| `root_cause` | 根因分析查询 | Stage1 + Stage2（明细查询） | "为什么这批次不合格？" |
| `concept` | 概念解释查询 | 仅 Stage1（直接返回知识） | "A类是什么标准？" |
| `out_of_scope` | 超出业务范围 | 直接 fallback | "今天天气怎么样？" |
</Intent_Types>

<Use_When>
- 用户反馈某类问题被错误路由（如统计问题被识别为概念问题）
- 需要添加新的意图类型（如新增"趋势分析"意图）
- 需要调整意图分类的 Prompt 以提高准确率
- 需要为特定问题模式添加快捷路由规则（绕过 LLM 分类）
- 需要配置 `out_of_scope` 的边界（哪些问题不在业务范围内）
</Use_When>

<Routing_Logic>
`orchestrator.py` 中的路由逻辑（三层决策）：

```python
async def _route_intent(self, question: str) -> IntentType:
    # 第一层：关键词快捷路由（无需调用 LLM，响应最快）
    STATISTICAL_KEYWORDS = [
        "合格率", "通过率", "产量", "产量", "统计", "多少", "几个",
        "平均", "最大", "最小", "分布", "占比", "比例", "趋势"
    ]
    ROOT_CAUSE_KEYWORDS = [
        "为什么", "原因", "分析", "异常", "不合格", "问题", "排查"
    ]
    CONCEPT_KEYWORDS = [
        "是什么", "定义", "标准", "规格", "含义", "解释", "怎么算"
    ]

    # 第二层：LLM 分类（处理复杂问题）
    # 使用 INTENT_CLASSIFICATION_PROMPT

    # 第三层：默认回退
    return IntentType.STATISTICAL  # 默认走统计路径
```
</Routing_Logic>

<Steps>
## Phase 1 — 分析路由问题

1. 读取路由相关文件：
   ```bash
   cat nlq-agent/src/pipelines/orchestrator.py
   cat nlq-agent/src/utils/prompts.py  # 查看 INTENT_CLASSIFICATION_PROMPT
   ```

2. 测试当前路由效果：
   ```bash
   # 使用同步接口测试意图分类
   curl -X POST http://localhost:18100/api/v1/query/sync \
     -H "Content-Type: application/json" \
     -d '{"question": "本月铁损异常的批次有哪些", "context": {}}' \
     | python -m json.tool | grep '"intent"'
   ```

## Phase 2 — 修改路由规则

### 添加关键词快捷路由

在 `orchestrator.py` 的 `_route_intent()` 中添加关键词：

```python
# 示例：添加趋势分析意图
TREND_KEYWORDS = ["趋势", "变化", "走势", "同比", "环比", "月度对比"]
if any(kw in question for kw in TREND_KEYWORDS):
    return IntentType.STATISTICAL  # 趋势分析归入统计类
```

### 优化 LLM 分类 Prompt

在 `prompts.py` 的 `INTENT_CLASSIFICATION_PROMPT` 中添加示例：

```python
INTENT_CLASSIFICATION_PROMPT = """
你是路美硅钢片检测系统的意图分类器。将用户问题分类为以下四种意图之一：

- statistical：统计聚合查询（合格率、产量、均值等数字统计）
- root_cause：根因分析（异常原因、不合格分析、问题排查）
- concept：概念解释（定义、标准、规格说明）
- out_of_scope：超出业务范围（非硅钢片检测相关）

示例：
问题："本月A类合格率是多少？" → statistical
问题："为什么2024-01-15这批次不合格？" → root_cause
问题："A类硅钢片的铁损标准是什么？" → concept
问题："今天股市怎么样？" → out_of_scope
问题："本月铁损异常的批次有哪些？" → root_cause  ← 新增示例

用户问题：{question}
意图类型（只输出一个单词）：
"""
```

## Phase 3 — 测试路由准确率

```bash
# 批量测试路由准确率
python -c "
import asyncio, sys
sys.path.insert(0, 'nlq-agent/src')
from pipelines.orchestrator import Orchestrator

TEST_CASES = [
    ('本月A类合格率是多少', 'statistical'),
    ('为什么这批次不合格', 'root_cause'),
    ('A类是什么标准', 'concept'),
    ('今天天气怎么样', 'out_of_scope'),
    ('本月铁损平均值', 'statistical'),
    ('叠片系数怎么计算', 'concept'),
    ('最近铁损异常原因分析', 'root_cause'),
]

async def test():
    orch = Orchestrator()
    correct = 0
    for question, expected in TEST_CASES:
        intent = await orch._route_intent(question)
        status = '✓' if intent == expected else '✗'
        print(f'{status} [{intent}] {question}')
        if intent == expected:
            correct += 1
    print(f'\\n准确率: {correct}/{len(TEST_CASES)} = {correct/len(TEST_CASES)*100:.0f}%')

asyncio.run(test())
"
```

## Phase 4 — 验证端到端效果

```bash
# 验证 statistical 路由（应触发 Stage2 SQL 查询）
curl -N -X POST http://localhost:18100/api/v1/query \
  -H "Content-Type: application/json" \
  -d '{"question": "本月A类合格率是多少", "context": {"month": "2024-01"}}' \
  | grep "event:"

# 验证 concept 路由（应只有 Stage1，无 SQL）
curl -N -X POST http://localhost:18100/api/v1/query \
  -H "Content-Type: application/json" \
  -d '{"question": "A类是什么标准", "context": {}}' \
  | grep "event:"
```
</Steps>

<Final_Checklist>
- [ ] 关键词快捷路由覆盖了最常见的问题模式
- [ ] LLM 分类 Prompt 包含足够的示例（每种意图至少 3 个示例）
- [ ] 路由准确率 ≥ 85%（基于测试用例集）
- [ ] `out_of_scope` 意图能正确拦截非业务问题
- [ ] `concept` 意图不触发 Stage2 SQL 查询（节省资源）
- [ ] 新增意图类型已同步更新 `schemas.py` 中的 `IntentType` 枚举
</Final_Checklist>
