# 路美项目知识图谱增强 NLQ：最快见效的选型与落地指南

针对“结合哪个项目能最快看到效果”这一核心问题，基于对路美项目（`idadawn/lm`）现有架构的深入分析，我们给出的明确建议是：

**强烈建议采用 Vanna AI 作为基础框架，并通过“轻量级挂载”的方式注入知识图谱能力。这是最快、代码侵入性最小的落地路径。**

## 1. 为什么 Vanna AI 能最快见效？

Wren AI 虽然原生带有“语义层（Semantic Layer）”，但它的架构非常重（包含前端、Java 服务、Rust 引擎），且强依赖其自带的管理后台来配置语义模型，很难与你们现有的 `KgReasoningChain` 组件和 SSE 协议深度融合。

相比之下，Vanna AI 是一个纯粹的 Python 库，具有极高的灵活性：

1.  **基础设施无缝复用**：你们现有的 `.env` 显示，你们已经部署了 `Qdrant`（端口 6333）、`TEI`（端口 8081）和 `vLLM`（端口 8082）。Vanna 原生支持自定义大模型和向量库，只需几行代码就能接入这些现有服务。
2.  **SSE 协议完美对齐**：Vanna 可以很容易地被 FastAPI 包装，直接输出你们前端 `nlqAgent.ts` 期望的 `text`、`reasoning_step` 和 `response_metadata` 事件流。
3.  **图谱注入极其简单**：Vanna 的核心是 RAG。我们不需要立刻引入像 Neo4j 这样重型的图数据库，而是可以**把知识图谱（业务规则、公式、术语解释）序列化为 Markdown 或 JSON 文本，作为 DDL 或 Documentation 注入到 Vanna 的向量库（Qdrant）中**。

## 2. “轻量级知识图谱”落地路线图（最快见效版）

为了在最短时间内（例如一周内）看到效果，我们不建议一开始就上 Neo4j 图数据库，而是采用**“知识图谱文档化 + 向量检索”**的轻量级方案。

### 阶段一：梳理业务本体（Ontology）并文档化

不要写复杂的代码，先用 Markdown 把你们在 `AppearanceFeatureRuleMatcher.cs` 和业务里的核心概念写清楚。

例如，创建一个 `knowledge_graph.md`，内容如下：
```markdown
# 业务术语定义
- **叠片系数**：指物理字段 `F_LAMINATION_COEFFICIENT`。如果不达标，意味着该值 `< 0.95`。
- **Ps铁损**：指物理字段 `F_PERF_PS_LOSS`。
- **炉号**：指物理字段 `F_FURNACE_NO`。

# 判定规则定义 (Product Spec)
对于产品规格 120 (Code: 120)：
- **C级判定条件**：带宽 (F_WIDTH) >= 119.5 且 Ps铁损 (F_PERF_PS_LOSS) <= 1.30。
```

### 阶段二：将知识注入 Vanna (Python 伪代码)

用 FastAPI 搭建一个轻量级微服务，接入 Vanna，并将上面的知识文档注入。

```python
from vanna.base import VannaDefault
from fastapi import FastAPI
from sse_starlette.sse import EventSourceResponse

# 1. 初始化 Vanna，接入你们的 vLLM 和 Qdrant
class MyVanna(VannaDefault):
    def __init__(self, config=None):
        # 配置连接到 localhost:8082 的 vLLM 和 localhost:6333 的 Qdrant
        pass

vn = MyVanna()
vn.connect_to_mysql(host='...', db='lumei', ...)

# 2. 注入轻量级知识图谱
# Vanna 会调用 TEI 把这些知识变成向量存入 Qdrant
vn.train(documentation="叠片系数对应字段 F_LAMINATION_COEFFICIENT，不达标是指 < 0.95")
vn.train(documentation="Ps铁损对应字段 F_PERF_PS_LOSS")
vn.train(ddl="CREATE TABLE LAB_INTERMEDIATE_DATA (F_FURNACE_NO varchar(100), F_LAMINATION_COEFFICIENT float, F_PERF_PS_LOSS float);")

# 3. 启动 FastAPI 服务，对齐路美项目的 SSE 协议
app = FastAPI()

@app.post("/api/v1/chat/stream")
async def chat_stream(request: dict):
    user_query = request['messages'][-1]['content']
    
    async def event_generator():
        # A. Vanna 检索知识图谱（向量召回）
        training_data = vn.get_training_data(user_query)
        
        # B. 触发 Reasoning Step 事件（前端 KgReasoningChain 会立刻显示）
        for doc in training_data['documentation']:
            yield {
                "event": "message",
                "data": json.dumps({
                    "type": "reasoning_step",
                    "reasoning_step": {
                        "kind": "rule", 
                        "label": f"匹配到业务规则：{doc}"
                    }
                })
            }
            
        # C. Vanna 生成 SQL
        sql = vn.generate_sql(question=user_query)
        yield {
            "event": "message",
            "data": json.dumps({
                "type": "reasoning_step",
                "reasoning_step": {
                    "kind": "condition", 
                    "label": f"生成查询逻辑：{sql}"
                }
            })
        }
        
        # D. 执行 SQL 并返回结果
        df = vn.run_sql(sql)
        yield {
            "event": "message",
            "data": json.dumps({
                "type": "response_metadata",
                "response_payload": {
                    "sql": sql,
                    "data": df.to_dict(orient='records')
                }
            })
        }
        yield {"event": "message", "data": '{"type": "done"}'}

    return EventSourceResponse(event_generator())
```

### 阶段三：前端零修改接入

只需要修改 `web/.env.development` 或生产环境变量：
```env
VITE_NLQ_AGENT_API_BASE=http://你的Python微服务地址:8000
```
前端的 `KgReasoningChain` 就会自动渲染出 Vanna 检索到的业务规则和生成的 SQL 步骤！

## 3. 为什么不选其他方案？

*   **Wren AI**：虽然语义层强大，但它是一套完整的系统。如果用它，你们前端的 `KgReasoningChain` 就成了摆设，因为 Wren AI 的内部推理过程很难抽取出来适配你们的协议。
*   **DB-GPT**：偏向于 Agent 工作流，配置复杂，对于单纯的“知识增强 Text-to-SQL”来说有点杀鸡用牛刀。
*   **原生 Neo4j + LangChain**：开发工作量极大，需要自己写 Cypher 语句生成器、意图识别路由等，至少需要 1-2 个月的开发周期。

## 4. 总结

为了最快看到效果，**采用 Vanna AI 结合“文档化知识图谱”的轻量级方案是最佳选择**。

你们只需要写一个几百行的 Python FastAPI 服务，把业务术语和判定规则作为文本“喂”给 Vanna。当用户提问时，Vanna 会通过你们现有的 Qdrant 召回这些规则，然后用你们的 vLLM 生成准确的 SQL，最后通过标准的 SSE 协议推给前端，完美点亮你们已经开发好的 `KgReasoningChain` 组件！
