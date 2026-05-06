# NLQ-Agent 技术架构文档（TDD）

> **文档类型**：技术设计文档（Technical Design Document）  
> **项目名称**：基于智能体的工业质量数据自然语言问数系统（NLQ-Agent）  
> **文档版本**：V1.0  
> **撰写日期**：2026-03-05  
> **状态**：待评审

---

## 一、技术选型总览

### 1.1 完整技术栈

| 层次 | 技术 | 版本 | 选型理由 |
|------|------|------|---------|
| **Monorepo 管理** | pnpm Workspaces + Turborepo | pnpm 9.x / turbo 2.x | 依赖共享、增量构建缓存、跨包类型复用 |
| **Web 框架** | Next.js App Router | 15.x | RSC + SSR、Route Handler、ai-elements 硬性要求 |
| **Web 语言** | TypeScript | 5.x | 严格模式，与 shared-types 包共享类型定义 |
| **Web 样式** | TailwindCSS v4 | 4.x | CSS Variables 模式（shadcn/ui + ai-elements 要求） |
| **基础 UI** | shadcn/ui | latest | 可复制组件，完全可定制，无版本锁定 |
| **AI 对话 UI** | ai-elements（Vercel） | 1.8.x | AI 原生对话组件（Reasoning、Tool、Plan 等） |
| **AI SDK** | Vercel AI SDK（`@ai-sdk/react`） | 4.x | `useChat` Hook，流式响应，工具调用状态展示 |
| **AI 对话图表** | GPT-Vis（AntV） | 1.0 | LLM 直接生成 `vis-chart` 语法，零解析成本 |
| **看板/分析图表** | `@ant-design/charts`（G2） | 2.6.x | Canvas 渲染，大数据量性能优，AntV 生态统一 |
| **全局状态** | Zustand | 5.x | 轻量，无 Provider 嵌套 |
| **服务端缓存** | TanStack Query | 5.x | 指标数据缓存，自动重新验证 |
| **表单验证** | React Hook Form + Zod | — | 配置页面，类型安全验证 |
| **前端单元测试** | Vitest + React Testing Library | 3.x | Vite 原生集成，速度极快 |
| **前端 E2E 测试** | Playwright | 1.50.x | 跨浏览器 + 移动端设备模拟 |
| **移动端** | uni-app x（uts + uvue） | HBuilderX 4.x | 原生性能，覆盖 Android/iOS/鸿蒙/小程序 |
| **移动端图表** | xCharts（uni-app x 插件） | — | 原生渲染，性能最优 |
| **Agent 框架** | LangGraph（Python） | 0.3.x | 图状态机，归因分析天然适配，LangSmith 可观测 |
| **API 框架** | FastAPI + uvicorn | 0.115.x | ASGI 异步，原生 SSE，Pydantic v2 深度集成 |
| **Python 包管理** | uv + pyproject.toml | 0.6.x | 比 pip 快 100x，取代 requirements.txt |
| **LLM 网关** | LiteLLM Proxy | latest | 统一接入 100+ 模型提供商，OpenAI 兼容接口 |
| **ORM** | SQLAlchemy 2.0（异步） | 2.x | 异步 MySQL 访问，与 FastAPI 深度集成 |
| **数据验证** | Pydantic v2 | 2.x | 高性能，与 FastAPI 原生集成 |
| **会话存储** | Redis（LangGraph Checkpointer） | 7.x | 多轮对话状态持久化 |
| **知识图谱** | NetworkX（内存图） | 3.x | 轻量，启动时从 MySQL 加载，无需 Neo4j |
| **后端单元测试** | pytest + pytest-asyncio | 8.x | Python 生态标准，异步测试支持 |
| **后端集成测试** | httpx AsyncClient + pytest | — | FastAPI 官方推荐异步端点测试方案 |
| **Agent 测试** | pytest + LangGraph 状态断言 | — | 节点级单元测试 + 完整图流程测试 |
| **可观测性** | LangSmith | — | Agent Trace 完整记录，调试和优化 |
| **容器化** | Docker + Docker Compose | — | 本地开发 + 生产部署一致性 |

---

## 二、系统整体架构

### 2.1 架构分层图

```
┌──────────────────────────────────────────────────────────────────┐
│                         展示层（前端）                             │
│                                                                  │
│  ┌─────────────────────────────┐  ┌──────────────────────────┐  │
│  │     Web 端（Next.js 15）     │  │  移动端（uni-app x）      │  │
│  │  shadcn/ui + ai-elements    │  │  uvue + xCharts          │  │
│  │  GPT-Vis + @ant-design/charts│  │  SSE 流式输出适配        │  │
│  └──────────────┬──────────────┘  └────────────┬─────────────┘  │
└─────────────────┼───────────────────────────────┼────────────────┘
                  │ HTTPS / SSE                   │ HTTPS / SSE
┌─────────────────▼───────────────────────────────▼────────────────┐
│                      对话网关层（BFF）                             │
│                                                                  │
│  Next.js Route Handlers（/api/chat、/api/metrics）               │
│  JWT 认证校验 · 请求转发 · CORS 处理                              │
└──────────────────────────────┬───────────────────────────────────┘
                               │ REST / SSE（内网）
┌──────────────────────────────▼───────────────────────────────────┐
│                     Agent 编排层（核心）                           │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                  FastAPI 服务（Python）                    │   │
│  │  POST /api/v1/chat/stream  ·  GET /api/v1/history        │   │
│  │  GET /api/v1/metrics       ·  GET /api/v1/health         │   │
│  └──────────────────────────┬───────────────────────────────┘   │
│                             │                                    │
│  ┌──────────────────────────▼───────────────────────────────┐   │
│  │              LangGraph Agent 编排引擎                     │   │
│  │                                                          │   │
│  │  OrchestratorAgent                                       │   │
│  │    ├── QueryAgent（查询型）                               │   │
│  │    ├── RootCauseAgent（归因型）                           │   │
│  │    ├── InsightAgent（洞察型）                             │   │
│  │    └── HypothesisAgent（假设型）                         │   │
│  └──────────────────────────────────────────────────────────┘   │
└──────────────────────────────┬───────────────────────────────────┘
                               │
┌──────────────────────────────▼───────────────────────────────────┐
│                   语义层 + 知识图谱层                              │
│                                                                  │
│  ┌──────────────────────┐  ┌──────────────────────────────────┐  │
│  │     语义层            │  │         知识图谱（NetworkX）      │  │
│  │  指标元数据服务        │  │  指标 → 公式 → 字段 → 等级 → 条件 │  │
│  │  时间语义解析器        │  │  启动时从 MySQL 加载             │  │
│  │  同义词词典           │  │  监听变更触发增量更新             │  │
│  │  实体识别器           │  │                                  │  │
│  └──────────────────────┘  └──────────────────────────────────┘  │
└──────────────────────────────┬───────────────────────────────────┘
                               │
┌──────────────────────────────▼───────────────────────────────────┐
│                         数据层                                    │
│                                                                  │
│  ┌──────────────────────┐  ┌──────────┐  ┌────────────────────┐  │
│  │  MySQL（主数据库）    │  │  Redis   │  │  LiteLLM Proxy     │  │
│  │  LAB_INTERMEDIATE_   │  │  会话上下 │  │  OpenAI / OpenRouter│  │
│  │  DATA / FORMULA /    │  │  文存储  │  │  Ollama / vLLM     │  │
│  │  JUDGMENT_LEVEL /    │  │  图谱缓存 │  │  统一 OpenAI 接口  │  │
│  │  PRODUCT_SPEC        │  │          │  │                    │  │
│  └──────────────────────┘  └──────────┘  └────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

### 2.2 部署架构

```
生产环境（Docker Compose / Kubernetes）

  [Nginx 反向代理]
       ├── /          → Next.js 容器（Web 前端 + BFF）
       ├── /agent/    → FastAPI 容器（Agent 服务）
       └── /llm/      → LiteLLM Proxy 容器

  [FastAPI 容器]
       ├── uvicorn（ASGI 服务器，4 workers）
       └── 挂载 pyproject.toml 依赖（uv 安装）

  [LiteLLM Proxy 容器]
       └── 配置文件挂载（litellm_config.yaml）

  [Redis 容器]（会话存储 + 知识图谱缓存）

  [MySQL]（现有数据库，只读连接）
```

---

## 三、Monorepo 目录结构

```
nlq-agent/                              # 项目根目录
├── pnpm-workspace.yaml                 # pnpm 工作区配置
├── turbo.json                          # Turborepo 构建管道配置
├── package.json                        # 根级开发依赖（eslint、prettier 等）
├── .eslintrc.json                      # 统一 ESLint 配置
├── tsconfig.base.json                  # 基础 TypeScript 配置
│
├── apps/
│   ├── web/                            # Next.js 15 Web 前端
│   │   ├── app/
│   │   │   ├── (chat)/                 # 对话功能路由组
│   │   │   │   ├── page.tsx            # 主对话页面（Client Component）
│   │   │   │   └── layout.tsx
│   │   │   ├── (dashboard)/            # 指标看板路由组
│   │   │   │   ├── page.tsx            # 看板首页（RSC 预取数据）
│   │   │   │   └── [metricId]/         # 单指标详情页
│   │   │   ├── (config)/               # 系统配置路由组（管理员）
│   │   │   │   ├── models/             # LLM 模型配置
│   │   │   │   └── metrics/            # 指标元数据管理
│   │   │   ├── api/
│   │   │   │   ├── chat/route.ts       # SSE 流式代理 → FastAPI
│   │   │   │   └── metrics/route.ts    # 指标数据代理
│   │   │   ├── layout.tsx              # 根布局（ThemeProvider）
│   │   │   └── globals.css             # TailwindCSS + CSS Variables
│   │   ├── components/
│   │   │   ├── ai-elements/            # ai-elements 安装目录（源码级）
│   │   │   │   ├── conversation.tsx
│   │   │   │   ├── message.tsx
│   │   │   │   ├── reasoning.tsx
│   │   │   │   ├── tool.tsx
│   │   │   │   ├── plan.tsx
│   │   │   │   ├── prompt-input.tsx
│   │   │   │   ├── model-selector.tsx
│   │   │   │   ├── suggestion.tsx
│   │   │   │   └── sources.tsx
│   │   │   ├── ui/                     # shadcn/ui 基础组件
│   │   │   └── charts/                 # 图表封装组件
│   │   │       ├── MetricGauge.tsx     # 仪表盘（动态导入，ssr:false）
│   │   │       ├── TrendLine.tsx       # 趋势折线图
│   │   │       ├── GradeDistribution.tsx # 等级分布饼图
│   │   │       └── ComparisonBar.tsx   # 对比柱状图
│   │   ├── lib/
│   │   │   ├── api-client.ts           # FastAPI 客户端封装
│   │   │   └── stream-parser.ts        # SSE 流解析工具
│   │   ├── e2e/                        # Playwright E2E 测试
│   │   │   ├── chat.spec.ts
│   │   │   └── dashboard.spec.ts
│   │   ├── __tests__/                  # Vitest 单元测试
│   │   ├── next.config.ts
│   │   ├── vitest.config.ts
│   │   ├── playwright.config.ts
│   │   └── package.json
│   │
│   └── mobile/                         # uni-app x 移动端
│       ├── pages/
│       │   ├── chat/                   # 对话页面
│       │   └── dashboard/              # 看板页面
│       ├── components/
│       │   ├── ChatMessage.uvue
│       │   └── MetricChart.uvue
│       ├── utils/
│       │   └── sse-client.uts          # SSE 流式客户端（uts 实现）
│       └── manifest.json
│
├── packages/
│   ├── shared-types/                   # 共享 TypeScript 类型定义
│   │   └── src/
│   │       ├── api.ts                  # API 请求/响应类型
│   │       ├── chart-protocol.ts       # 通用图表描述协议（JSON Schema）
│   │       ├── metric.ts               # 指标、公式、等级类型
│   │       └── index.ts
│   ├── ui/                             # 跨应用共享 React 组件
│   └── chart-renderer/                 # 图表协议适配层
│       ├── web.tsx                     # Web 端渲染（@ant-design/charts）
│       └── protocol.ts                 # 图表描述协议解析
│
└── services/
    └── agent-api/                      # FastAPI + LangGraph（Python 后端）
        ├── pyproject.toml              # 依赖声明（取代 requirements.txt）
        ├── uv.lock                     # 精确锁定文件（提交到 Git）
        ├── Dockerfile
        ├── app/
        │   ├── main.py                 # FastAPI 应用入口
        │   ├── api/
        │   │   ├── chat.py             # 对话端点（SSE 流式）
        │   │   ├── metrics.py          # 指标数据端点
        │   │   └── health.py           # 健康检查
        │   ├── agents/
        │   │   ├── graph.py            # LangGraph 主图定义
        │   │   ├── orchestrator.py     # Orchestrator Agent（路由节点）
        │   │   ├── query_agent.py      # QueryAgent
        │   │   ├── root_cause_agent.py # RootCauseAgent
        │   │   ├── insight_agent.py    # InsightAgent
        │   │   └── hypothesis_agent.py # HypothesisAgent
        │   ├── tools/
        │   │   ├── metric_tools.py     # 指标查询工具
        │   │   ├── sql_tools.py        # SQL 生成与执行工具
        │   │   ├── graph_tools.py      # 知识图谱遍历工具
        │   │   ├── stats_tools.py      # 统计分析工具
        │   │   └── chart_tools.py      # 图表选择与配置工具
        │   ├── semantic/
        │   │   ├── metadata_service.py # 指标元数据服务
        │   │   ├── time_parser.py      # 时间语义解析器
        │   │   ├── entity_extractor.py # 实体识别器
        │   │   └── synonym_dict.py     # 同义词词典
        │   ├── knowledge_graph/
        │   │   ├── builder.py          # 知识图谱构建器
        │   │   ├── traversal.py        # 图遍历算法
        │   │   └── models.py           # 图节点/边数据模型
        │   ├── models/
        │   │   ├── entities.py         # SQLAlchemy ORM 实体
        │   │   └── schemas.py          # Pydantic 请求/响应模型
        │   └── core/
        │       ├── config.py           # 配置管理（pydantic-settings）
        │       ├── database.py         # 异步数据库连接
        │       └── redis_client.py     # Redis 连接
        └── tests/
            ├── unit/                   # 单元测试
            │   ├── test_time_parser.py
            │   ├── test_knowledge_graph.py
            │   └── test_tools.py
            ├── integration/            # 集成测试
            │   ├── test_chat_api.py
            │   └── test_metrics_api.py
            └── agent/                  # Agent 流程测试
                ├── test_query_agent.py
                └── test_root_cause_agent.py
```

---

## 四、后端详细设计

### 4.1 Python 包管理（uv + pyproject.toml）

完全告别 `requirements.txt`，采用现代化的 `pyproject.toml` 声明依赖，`uv.lock` 锁定精确版本。

```toml
# services/agent-api/pyproject.toml
[project]
name = "nlq-agent-api"
version = "1.0.0"
description = "NLQ-Agent FastAPI + LangGraph 后端服务"
requires-python = ">=3.11"

dependencies = [
    # Web 框架
    "fastapi>=0.115.0",
    "uvicorn[standard]>=0.32.0",
    # Agent 框架
    "langgraph>=0.3.0",
    "langchain-core>=0.3.0",
    "langchain-openai>=0.3.0",
    # LLM 网关
    "litellm>=1.55.0",
    # 数据库
    "sqlalchemy[asyncio]>=2.0.0",
    "aiomysql>=0.2.0",
    # 缓存
    "redis[hiredis]>=5.0.0",
    # 知识图谱
    "networkx>=3.4.0",
    # 数据验证
    "pydantic>=2.10.0",
    "pydantic-settings>=2.7.0",
    # 统计分析（InsightAgent）
    "numpy>=2.2.0",
    "scipy>=1.15.0",
    "pandas>=2.2.0",
    # 可观测性
    "langsmith>=0.3.0",
]

[project.optional-dependencies]
dev = [
    "pytest>=8.3.0",
    "pytest-asyncio>=0.25.0",
    "httpx>=0.28.0",
    "pytest-cov>=6.0.0",
    "ruff>=0.9.0",
    "mypy>=1.14.0",
]
```

**常用命令**：

```bash
uv sync                          # 安装所有依赖
uv sync --extra dev              # 安装含开发依赖
uv add langgraph                 # 添加依赖（自动更新 pyproject.toml + uv.lock）
uv run uvicorn app.main:app --reload   # 启动开发服务器
uv run pytest                    # 运行测试
uv run ruff check .              # 代码检查
```

### 4.2 FastAPI 应用结构

```python
# app/main.py
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
from app.knowledge_graph.builder import KnowledgeGraphBuilder
from app.semantic.metadata_service import MetadataService
from app.core.database import engine
from app.api import chat, metrics, health

@asynccontextmanager
async def lifespan(app: FastAPI):
    """应用启动时初始化知识图谱和元数据"""
    # 从 MySQL 加载指标元数据
    await MetadataService.load_from_db()
    # 构建知识图谱
    await KnowledgeGraphBuilder.build()
    yield
    # 清理资源
    await engine.dispose()

app = FastAPI(
    title="NLQ-Agent API",
    version="1.0.0",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # 生产环境替换为具体域名
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(chat.router, prefix="/api/v1")
app.include_router(metrics.router, prefix="/api/v1")
app.include_router(health.router, prefix="/api/v1")
```

**流式对话端点**：

```python
# app/api/chat.py
from fastapi import APIRouter, Depends
from fastapi.responses import StreamingResponse
from app.agents.graph import create_agent_graph
from app.models.schemas import ChatRequest
import json

router = APIRouter()

@router.post("/chat/stream")
async def chat_stream(request: ChatRequest):
    """流式对话端点，返回 SSE 格式的流式响应"""
    graph = create_agent_graph()

    async def event_generator():
        async for event in graph.astream_events(
            {
                "messages": request.messages,
                "session_id": request.session_id,
            },
            version="v2",
        ):
            # 将 LangGraph 事件转换为 SSE 格式
            if event["event"] == "on_chat_model_stream":
                chunk = event["data"]["chunk"].content
                if chunk:
                    yield f"data: {json.dumps({'type': 'text', 'content': chunk})}\n\n"
            elif event["event"] == "on_tool_start":
                yield f"data: {json.dumps({'type': 'tool_start', 'name': event['name']})}\n\n"
            elif event["event"] == "on_tool_end":
                yield f"data: {json.dumps({'type': 'tool_end', 'name': event['name'], 'result': event['data']['output']})}\n\n"
        yield "data: [DONE]\n\n"

    return StreamingResponse(
        event_generator(),
        media_type="text/event-stream",
        headers={"Cache-Control": "no-cache", "X-Accel-Buffering": "no"},
    )
```

### 4.3 LangGraph Agent 编排设计

LangGraph 使用**图状态机**组织 Agent 工作流，每个节点是一个处理函数，边定义了路由逻辑。

**主图状态定义**：

```python
# app/agents/graph.py
from typing import Annotated, Literal
from langgraph.graph import StateGraph, START, END
from langgraph.checkpoint.redis import AsyncRedisSaver
from langchain_core.messages import BaseMessage
from langgraph.graph.message import add_messages
from pydantic import BaseModel

class AgentState(BaseModel):
    """Agent 全局状态，贯穿整个图的执行过程"""
    messages: Annotated[list[BaseMessage], add_messages]
    session_id: str
    intent: Literal["query", "root_cause", "insight", "hypothesis", "unknown"] = "unknown"
    entities: dict = {}          # 提取的实体（指标、时间、规格、炉号）
    context: dict = {}           # 多轮对话上下文（跨轮复用的实体）
    tool_results: dict = {}      # 工具调用结果
    chart_config: dict | None = None  # 图表配置

def create_agent_graph():
    """创建并编译 LangGraph 主图"""
    workflow = StateGraph(AgentState)

    # 添加节点
    workflow.add_node("orchestrator", orchestrator_node)
    workflow.add_node("query_agent", query_agent_node)
    workflow.add_node("root_cause_agent", root_cause_agent_node)
    workflow.add_node("insight_agent", insight_agent_node)
    workflow.add_node("hypothesis_agent", hypothesis_agent_node)
    workflow.add_node("response_formatter", response_formatter_node)

    # 定义边（路由逻辑）
    workflow.add_edge(START, "orchestrator")
    workflow.add_conditional_edges(
        "orchestrator",
        route_by_intent,
        {
            "query": "query_agent",
            "root_cause": "root_cause_agent",
            "insight": "insight_agent",
            "hypothesis": "hypothesis_agent",
        }
    )
    # 所有专项 Agent 完成后汇聚到响应格式化节点
    for agent in ["query_agent", "root_cause_agent", "insight_agent", "hypothesis_agent"]:
        workflow.add_edge(agent, "response_formatter")
    workflow.add_edge("response_formatter", END)

    # 使用 Redis 持久化多轮对话状态
    checkpointer = AsyncRedisSaver.from_conn_string("redis://localhost:6379")
    return workflow.compile(checkpointer=checkpointer)

def route_by_intent(state: AgentState) -> str:
    """根据 Orchestrator 识别的意图路由到专项 Agent"""
    return state.intent
```

**Orchestrator 节点（意图识别 + 实体提取）**：

```python
# app/agents/orchestrator.py
from langchain_openai import ChatOpenAI
from langchain_core.prompts import ChatPromptTemplate
from app.semantic.metadata_service import MetadataService
from app.semantic.time_parser import TimeParser
from app.semantic.entity_extractor import EntityExtractor

ORCHESTRATOR_SYSTEM_PROMPT = """
你是一个工业质量数据分析助手的路由器。你的任务是：
1. 识别用户问题的意图类型（query/root_cause/insight/hypothesis）
2. 提取关键实体（指标名称、时间范围、产品规格、炉号）
3. 结合对话历史中的上下文实体，补全缺失信息

可用指标列表：
{metric_list}

意图类型定义：
- query：查询某指标的聚合值（均值/最大值/最小值/等级分布）
- root_cause：解释某炉号被判定为某等级的原因
- insight：发现数据中的异常趋势或规律
- hypothesis：模拟改变判定标准后的影响

请以 JSON 格式返回：{{"intent": "...", "entities": {{"metric": "...", "time_range": "...", "spec": "...", "furnace_no": "..."}}}}
"""

async def orchestrator_node(state: AgentState) -> AgentState:
    """意图识别和实体提取节点"""
    llm = ChatOpenAI(model="gpt-4o", temperature=0)
    
    # 获取可用指标列表（用于提示词）
    metric_list = await MetadataService.get_metric_names()
    
    prompt = ChatPromptTemplate.from_messages([
        ("system", ORCHESTRATOR_SYSTEM_PROMPT.format(metric_list=", ".join(metric_list))),
        ("placeholder", "{messages}"),
    ])
    
    chain = prompt | llm
    result = await chain.ainvoke({"messages": state.messages})
    
    # 解析 LLM 返回的 JSON
    parsed = parse_orchestrator_result(result.content)
    
    # 与历史上下文合并（多轮对话实体复用）
    merged_entities = {**state.context, **{k: v for k, v in parsed["entities"].items() if v}}
    
    return {
        "intent": parsed["intent"],
        "entities": merged_entities,
        "context": merged_entities,  # 更新上下文供下轮使用
    }
```

### 4.4 四类专项 Agent 工具集

#### QueryAgent 工具集

```python
# app/tools/metric_tools.py
from langchain_core.tools import tool
from app.semantic.metadata_service import MetadataService
from app.tools.sql_tools import execute_safe_sql
from app.tools.chart_tools import select_chart_type, build_chart_config

@tool
async def get_metric_meta(metric_name: str) -> dict:
    """根据指标名称（支持同义词）获取指标元数据，包含公式、单位、自然语言描述"""
    return await MetadataService.get_by_name(metric_name)

@tool
async def resolve_time_range(time_expression: str) -> dict:
    """将自然语言时间表达转换为 SQL 时间条件
    
    示例：
    - "最近三天" → {"sql": "DetectionDate >= DATE_SUB(NOW(), INTERVAL 3 DAY)", "label": "最近三天"}
    - "1月甲班" → {"sql": "MONTH(DetectionDate) = 1 AND Shift = '甲'", "label": "2026年1月甲班"}
    """
    from app.semantic.time_parser import TimeParser
    return TimeParser.parse(time_expression)

@tool
async def execute_metric_query(
    metric_column: str,
    agg_func: str,
    where_clause: str,
    group_by: str | None = None
) -> list[dict]:
    """执行指标聚合查询（仅允许 SELECT，经过白名单校验）
    
    Args:
        metric_column: 指标对应的数据库字段名
        agg_func: 聚合函数（AVG/MAX/MIN/COUNT/SUM）
        where_clause: WHERE 条件（由 resolve_time_range 生成）
        group_by: 分组字段（可选，如按班次分组）
    """
    sql = f"""
        SELECT {group_by + ',' if group_by else ''} {agg_func}({metric_column}) as value
        FROM LAB_INTERMEDIATE_DATA
        WHERE {where_clause}
        {('GROUP BY ' + group_by) if group_by else ''}
    """
    return await execute_safe_sql(sql)

@tool
async def get_judgment_levels(formula_id: int, spec_id: int | None = None) -> list[dict]:
    """获取指标的等级判定规则，支持按产品规格筛选"""
    ...

@tool
async def build_response_chart(data: list[dict], question_type: str, metric_name: str) -> dict:
    """根据数据特征和问题类型自动选择并生成图表配置（GPT-Vis 协议）"""
    chart_type = select_chart_type(data, question_type)
    return build_chart_config(chart_type, data, metric_name)
```

#### RootCauseAgent 工具集

```python
@tool
async def get_record_by_furnace_no(furnace_no: str) -> dict:
    """按炉号查询该检测记录的所有字段值"""
    sql = "SELECT * FROM LAB_INTERMEDIATE_DATA WHERE FurnaceNo = :furnace_no LIMIT 1"
    return await execute_safe_sql(sql, {"furnace_no": furnace_no})

@tool
async def compare_with_target_level(
    record: dict,
    formula_id: int,
    target_level_name: str,
    spec_id: int
) -> dict:
    """对比记录与目标等级的所有判定条件，返回满足/不满足的详细列表
    
    Returns:
        {
            "satisfied": [{"condition": "带宽 ∈ [118, 122]", "actual": 120.3, "threshold": "118~122"}],
            "failed": [{"condition": "Ps铁损 ≤ 1.30", "actual": 1.45, "threshold": 1.30, "gap": 0.15}]
        }
    """
    from app.knowledge_graph.traversal import get_level_conditions
    conditions = await get_level_conditions(formula_id, target_level_name, spec_id)
    return evaluate_conditions(record, conditions)

@tool
async def traverse_dependency_chain(metric_name: str) -> str:
    """遍历知识图谱，追踪指标的完整计算依赖链，返回自然语言描述"""
    from app.knowledge_graph.traversal import trace_dependency
    chain = await trace_dependency(metric_name)
    return format_dependency_chain(chain)
```

#### InsightAgent 工具集

```python
@tool
async def fetch_time_series(metric_column: str, days: int, group_by: str = "day") -> list[dict]:
    """获取指标的时序数据"""
    ...

@tool
async def run_statistical_analysis(data: list[float]) -> dict:
    """计算统计量：均值、标准差、趋势斜率、变异系数
    
    Returns:
        {"mean": 0.963, "std": 0.012, "trend_slope": 0.002, "cv": 0.012,
         "is_trending_up": True, "volatility_level": "increasing"}
    """
    import numpy as np
    from scipy import stats
    arr = np.array(data)
    slope, _, _, _, _ = stats.linregress(range(len(arr)), arr)
    return {
        "mean": float(np.mean(arr)),
        "std": float(np.std(arr)),
        "trend_slope": float(slope),
        "cv": float(np.std(arr) / np.mean(arr)),
        "is_trending_up": slope > 0,
        "volatility_level": "increasing" if np.std(arr[-7:]) > np.std(arr[:-7]) else "stable",
    }

@tool
async def detect_anomalies(data: list[dict], method: str = "3sigma") -> list[dict]:
    """检测时序数据中的异常点（3σ 或 IQR 方法）"""
    ...
```

#### HypothesisAgent 工具集

```python
@tool
async def simulate_condition_change(
    formula_id: int,
    level_name: str,
    condition_field: str,
    new_threshold: float,
    days: int = 30
) -> dict:
    """在内存中修改判定条件并重新计算等级分布
    
    Returns:
        {
            "before": {"A级": 850, "B级": 120, "C级": 30, "A级率": 0.85},
            "after": {"A级": 920, "B级": 60, "C级": 20, "A级率": 0.92},
            "improvement": {"A级": "+70", "A级率": "+7.0%"},
            "affected_records": ["1丙20260110-1", "1甲20260112-3", ...]
        }
    """
    # 1. 获取近期批量数据到内存
    records = await fetch_raw_data_batch(days)
    # 2. 获取当前判定规则
    levels = await get_judgment_levels(formula_id)
    # 3. 修改目标条件
    modified_levels = modify_condition(levels, level_name, condition_field, new_threshold)
    # 4. 重新计算每条记录的等级
    before_dist = calculate_distribution(records, levels)
    after_dist = calculate_distribution(records, modified_levels)
    return compare_distributions(before_dist, after_dist)
```

### 4.5 语义层设计

**时间语义解析器**：

```python
# app/semantic/time_parser.py
import re
from datetime import datetime, timedelta
from dateutil.relativedelta import relativedelta

class TimeParser:
    """将自然语言时间表达转换为 SQL 时间条件"""

    PATTERNS = [
        (r"最近(\d+)天", lambda m: f"DetectionDate >= DATE_SUB(NOW(), INTERVAL {m.group(1)} DAY)"),
        (r"最近(\d+)周", lambda m: f"DetectionDate >= DATE_SUB(NOW(), INTERVAL {int(m.group(1))*7} DAY)"),
        (r"上个月", lambda _: "MONTH(DetectionDate) = MONTH(DATE_SUB(NOW(), INTERVAL 1 MONTH)) AND YEAR(DetectionDate) = YEAR(DATE_SUB(NOW(), INTERVAL 1 MONTH))"),
        (r"本月", lambda _: "MONTH(DetectionDate) = MONTH(NOW()) AND YEAR(DetectionDate) = YEAR(NOW())"),
        (r"(\d{1,2})月([甲乙丙]班)", lambda m: f"MONTH(DetectionDate) = {m.group(1)} AND Shift = '{m.group(2)[0]}'"),
        (r"(\d{4})年(\d{1,2})月", lambda m: f"YEAR(DetectionDate) = {m.group(1)} AND MONTH(DetectionDate) = {m.group(2)}"),
        (r"今天|今日", lambda _: "DATE(DetectionDate) = CURDATE()"),
        (r"昨天|昨日", lambda _: "DATE(DetectionDate) = DATE_SUB(CURDATE(), INTERVAL 1 DAY)"),
    ]

    @classmethod
    def parse(cls, expression: str) -> dict:
        for pattern, handler in cls.PATTERNS:
            match = re.search(pattern, expression)
            if match:
                return {"sql": handler(match), "label": expression}
        # 无法解析时返回默认（最近30天）
        return {"sql": "DetectionDate >= DATE_SUB(NOW(), INTERVAL 30 DAY)", "label": "最近30天（默认）"}
```

**同义词词典**：

```python
# app/semantic/synonym_dict.py
METRIC_SYNONYMS = {
    "Ps铁损": ["铁损", "PS损耗", "P15/50", "PsLoss", "ps铁损", "铁损值"],
    "叠片系数": ["叠片", "LF", "laminationfactor", "叠片率"],
    "厚度极差": ["厚差", "厚度差", "thickness range", "极差"],
    "矫顽力": ["Hc", "hc值", "矫顽磁力"],
    "带宽": ["宽度", "width", "板宽"],
}

SHIFT_SYNONYMS = {
    "甲": ["甲班", "早班", "A班", "第一班"],
    "乙": ["乙班", "中班", "B班", "第二班"],
    "丙": ["丙班", "晚班", "夜班", "C班", "第三班"],
}
```

### 4.6 知识图谱设计

**图谱构建器**：

```python
# app/knowledge_graph/builder.py
import networkx as nx
from app.core.database import get_db_session

class KnowledgeGraphBuilder:
    """从 MySQL 构建指标知识图谱"""
    
    _graph: nx.DiGraph = None

    @classmethod
    async def build(cls) -> nx.DiGraph:
        """启动时从 MySQL 加载数据，构建有向图"""
        G = nx.DiGraph()
        
        async with get_db_session() as session:
            # 加载公式（指标节点）
            formulas = await session.execute("SELECT * FROM LAB_INTERMEDIATE_DATA_FORMULA")
            for f in formulas:
                G.add_node(f"metric:{f.Id}", type="metric", name=f.FormulaName,
                           formula=f.Formula, formula_type=f.FormulaType,
                           unit=f.UnitName, column=f.ColumnName)
            
            # 加载等级判定规则（等级节点 + 条件节点）
            levels = await session.execute("SELECT * FROM LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL")
            for lv in levels:
                level_id = f"level:{lv.Id}"
                G.add_node(level_id, type="level", name=lv.Name,
                           priority=lv.Priority, is_default=lv.IsDefault,
                           quality_status=lv.QualityStatus, color=lv.Color)
                G.add_edge(f"metric:{lv.FormulaId}", level_id, relation="HAS_LEVEL")
                
                # 解析条件 JSON
                if lv.Condition:
                    conditions = parse_condition_json(lv.Condition)
                    for i, cond in enumerate(conditions):
                        cond_id = f"condition:{lv.Id}:{i}"
                        G.add_node(cond_id, type="condition", **cond)
                        G.add_edge(level_id, cond_id, relation="LEVEL_COND")
            
            # 加载产品规格关联
            specs = await session.execute("SELECT * FROM LAB_PRODUCT_SPEC")
            for spec in specs:
                G.add_node(f"spec:{spec.Id}", type="spec", name=spec.Name, code=spec.Code)
        
        cls._graph = G
        return G

    @classmethod
    def get_graph(cls) -> nx.DiGraph:
        return cls._graph
```

**图遍历算法**：

```python
# app/knowledge_graph/traversal.py
import networkx as nx
from app.knowledge_graph.builder import KnowledgeGraphBuilder

async def get_level_conditions(formula_id: int, level_name: str, spec_id: int | None) -> list[dict]:
    """获取指定指标在指定等级下的所有判定条件"""
    G = KnowledgeGraphBuilder.get_graph()
    metric_node = f"metric:{formula_id}"
    
    # 找到对应等级节点
    level_node = None
    for _, neighbor, data in G.out_edges(metric_node, data=True):
        if data["relation"] == "HAS_LEVEL":
            node_data = G.nodes[neighbor]
            if node_data["name"] == level_name:
                level_node = neighbor
                break
    
    if not level_node:
        return []
    
    # 获取该等级的所有条件
    conditions = []
    for _, cond_node, data in G.out_edges(level_node, data=True):
        if data["relation"] == "LEVEL_COND":
            conditions.append(G.nodes[cond_node])
    
    return sorted(conditions, key=lambda x: x.get("priority", 0))

async def trace_dependency(metric_name: str) -> list[str]:
    """追踪指标的完整计算依赖链"""
    G = KnowledgeGraphBuilder.get_graph()
    # 找到指标节点
    metric_node = find_metric_by_name(G, metric_name)
    if not metric_node:
        return []
    # BFS 遍历 DEPENDS_ON 和 CALC_FROM 边
    chain = []
    for node in nx.bfs_tree(G, metric_node, reverse=True).nodes():
        chain.append(G.nodes[node])
    return chain
```

### 4.7 LiteLLM 多模型接入配置

```yaml
# litellm_config.yaml
model_list:
  # OpenAI
  - model_name: gpt-4o
    litellm_params:
      model: openai/gpt-4o
      api_key: os.environ/OPENAI_API_KEY
  
  # OpenRouter（支持 Claude、Gemini 等）
  - model_name: claude-3-5-sonnet
    litellm_params:
      model: openrouter/anthropic/claude-3.5-sonnet
      api_key: os.environ/OPENROUTER_API_KEY
  
  - model_name: gemini-2-flash
    litellm_params:
      model: openrouter/google/gemini-2.0-flash-exp
      api_key: os.environ/OPENROUTER_API_KEY
  
  # Ollama（本地私有化）
  - model_name: qwen2.5-72b
    litellm_params:
      model: ollama/qwen2.5:72b
      api_base: http://ollama:11434
    # 标记：支持 Function Calling
    model_info:
      supports_function_calling: true
  
  - model_name: deepseek-r1
    litellm_params:
      model: ollama/deepseek-r1:32b
      api_base: http://ollama:11434
    model_info:
      supports_function_calling: false  # 不支持，归因/假设型问题不可用
  
  # vLLM（自托管高性能）
  - model_name: qwen2.5-72b-vllm
    litellm_params:
      model: openai/Qwen/Qwen2.5-72B-Instruct
      api_base: http://vllm:8000/v1
      api_key: EMPTY

# 全局配置
litellm_settings:
  drop_params: true           # 不支持的参数自动忽略
  request_timeout: 120
  num_retries: 3

# 路由策略（负载均衡）
router_settings:
  routing_strategy: least-busy
```

**在 LangGraph 中使用 LiteLLM**：

```python
# app/core/llm_factory.py
from langchain_openai import ChatOpenAI

def get_llm(model_name: str = "gpt-4o", require_function_calling: bool = False) -> ChatOpenAI:
    """通过 LiteLLM Proxy 获取统一的 LLM 实例"""
    return ChatOpenAI(
        model=model_name,
        base_url="http://litellm-proxy:4000/v1",  # LiteLLM Proxy 地址
        api_key="sk-internal",                     # 内部 API Key
        streaming=True,
        temperature=0 if require_function_calling else 0.1,
    )
```

---

## 五、前端详细设计

### 5.1 Next.js Route Handler（BFF 层）

```typescript
// apps/web/app/api/chat/route.ts
import { NextRequest } from "next/server";

export async function POST(req: NextRequest) {
  const { messages, sessionId, modelName } = await req.json();

  // 验证 JWT Token
  const token = req.headers.get("Authorization")?.replace("Bearer ", "");
  if (!token || !verifyToken(token)) {
    return new Response("Unauthorized", { status: 401 });
  }

  // 转发到 FastAPI Agent 服务
  const agentResponse = await fetch(
    `${process.env.AGENT_API_URL}/api/v1/chat/stream`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Internal-Key": process.env.INTERNAL_API_KEY!,
      },
      body: JSON.stringify({
        messages,
        session_id: sessionId,
        model_name: modelName ?? "gpt-4o",
      }),
    }
  );

  // 直接透传 SSE 流
  return new Response(agentResponse.body, {
    headers: {
      "Content-Type": "text/event-stream",
      "Cache-Control": "no-cache",
      "X-Accel-Buffering": "no",
    },
  });
}
```

### 5.2 对话页面核心实现

```typescript
// apps/web/app/(chat)/page.tsx
"use client";

import { useChat } from "@ai-sdk/react";
import { useState } from "react";
import dynamic from "next/dynamic";

// AI 对话组件（ai-elements）
import { Conversation, ConversationContent } from "@/components/ai-elements/conversation";
import { Message, MessageContent, MessageResponse } from "@/components/ai-elements/message";
import { Reasoning } from "@/components/ai-elements/reasoning";
import { Tool } from "@/components/ai-elements/tool";
import { Plan } from "@/components/ai-elements/plan";
import { PromptInput, PromptInputTextarea, PromptInputActions } from "@/components/ai-elements/prompt-input";
import { Suggestion } from "@/components/ai-elements/suggestion";
import { ModelSelector } from "@/components/ai-elements/model-selector";
import { Sources } from "@/components/ai-elements/sources";
import { Shimmer } from "@/components/ai-elements/shimmer";

// GPT-Vis 图表（动态导入，禁用 SSR）
const GptVisRenderer = dynamic(() => import("@/components/charts/GptVisRenderer"), { ssr: false, loading: () => <Shimmer /> });

const SUGGESTED_QUESTIONS = [
  "最近三天的叠片系数有什么异常吗？",
  "1月甲班的 Ps 铁损均值达到什么等级？",
  "本月 A 级品率是多少？",
  "为什么炉号 1丙20260110-1 被判定为 B 级？",
];

export default function ChatPage() {
  const [selectedModel, setSelectedModel] = useState("gpt-4o");

  const { messages, input, handleInputChange, handleSubmit, isLoading, append } = useChat({
    api: "/api/chat",
    body: { modelName: selectedModel },
    // 会话 ID 从 localStorage 获取，支持多轮对话
    id: typeof window !== "undefined" ? localStorage.getItem("sessionId") ?? undefined : undefined,
  });

  return (
    <div className="flex flex-col h-screen bg-background">
      {/* 顶部工具栏 */}
      <header className="flex items-center justify-between px-4 py-2 border-b">
        <h1 className="font-semibold">NLQ-Agent</h1>
        <ModelSelector
          value={selectedModel}
          onValueChange={setSelectedModel}
          models={[
            { id: "gpt-4o", name: "GPT-4o", provider: "OpenAI" },
            { id: "claude-3-5-sonnet", name: "Claude 3.5 Sonnet", provider: "OpenRouter" },
            { id: "qwen2.5-72b", name: "Qwen2.5 72B", provider: "Ollama（本地）" },
          ]}
        />
      </header>

      {/* 对话内容区 */}
      <Conversation className="flex-1 overflow-y-auto">
        <ConversationContent>
          {/* 首页引导建议 */}
          {messages.length === 0 && (
            <div className="flex flex-wrap gap-2 p-4">
              {SUGGESTED_QUESTIONS.map((q) => (
                <Suggestion key={q} onClick={() => append({ role: "user", content: q })}>
                  {q}
                </Suggestion>
              ))}
            </div>
          )}

          {messages.map((message) => (
            <Message key={message.id} from={message.role}>
              <MessageContent>
                {/* AI 推理过程（可折叠） */}
                {message.role === "assistant" && message.reasoning && (
                  <Reasoning defaultOpen={false}>{message.reasoning}</Reasoning>
                )}

                {/* 工具调用状态 */}
                {message.toolInvocations?.map((tool) => (
                  <Tool
                    key={tool.toolCallId}
                    name={getToolDisplayName(tool.toolName)}
                    state={tool.state}
                    result={tool.state === "result" ? JSON.stringify(tool.result) : undefined}
                  />
                ))}

                {/* 消息正文（含 GPT-Vis 图表语法） */}
                <MessageResponse>
                  <GptVisRenderer content={message.content} />
                </MessageResponse>

                {/* 数据来源 */}
                {message.role === "assistant" && message.sources && (
                  <Sources sources={message.sources} />
                )}
              </MessageContent>
            </Message>
          ))}

          {/* 加载中状态 */}
          {isLoading && <Shimmer className="mx-4 mb-4" />}
        </ConversationContent>
      </Conversation>

      {/* 输入区 */}
      <form onSubmit={handleSubmit} className="border-t p-4">
        <PromptInput value={input} onValueChange={handleInputChange}>
          <PromptInputTextarea placeholder="请用自然语言提问，例如：最近三天叠片系数有什么异常？" />
          <PromptInputActions>
            <button type="submit" disabled={isLoading || !input.trim()}>
              发送
            </button>
          </PromptInputActions>
        </PromptInput>
      </form>
    </div>
  );
}
```

### 5.3 图表渲染架构

**通用图表描述协议**（Web 端和移动端共享）：

```typescript
// packages/shared-types/src/chart-protocol.ts
export interface ChartDescriptor {
  type: "line" | "bar" | "pie" | "gauge" | "radar" | "scatter";
  title: string;
  data: ChartDataPoint[];
  xField?: string;
  yField?: string;
  colorField?: string;
  annotations?: ChartAnnotation[];  // 等级阈值参考线
  meta?: {
    metricName: string;
    unit: string;
    gradeThresholds: GradeThreshold[];
  };
}

export interface GradeThreshold {
  level: string;       // "A级"
  value: number;       // 1.30
  color: string;       // "#52c41a"
}
```

**Web 端图表渲染（`@ant-design/charts`，动态导入）**：

```typescript
// apps/web/components/charts/MetricGauge.tsx
"use client";
import { Gauge } from "@ant-design/charts";

interface MetricGaugeProps {
  value: number;
  max: number;
  level: string;
  levelColor: string;
  unit: string;
  metricName: string;
}

export default function MetricGauge({ value, max, level, levelColor, unit, metricName }: MetricGaugeProps) {
  const config = {
    data: { target: value, total: max, name: metricName },
    legend: false,
    style: { textContent: `${value} ${unit}\n${level}` },
    scale: { color: { range: [levelColor] } },
  };
  return <Gauge {...config} />;
}
```

---

## 六、测试策略

### 6.1 前端测试

**单元测试（Vitest + React Testing Library）**：

```typescript
// apps/web/__tests__/stream-parser.test.ts
import { describe, it, expect } from "vitest";
import { parseSSEEvent } from "@/lib/stream-parser";

describe("SSE 流解析器", () => {
  it("应正确解析文本类型事件", () => {
    const raw = 'data: {"type":"text","content":"叠片系数均值为"}\n\n';
    const result = parseSSEEvent(raw);
    expect(result).toEqual({ type: "text", content: "叠片系数均值为" });
  });

  it("应正确解析工具调用开始事件", () => {
    const raw = 'data: {"type":"tool_start","name":"execute_metric_query"}\n\n';
    const result = parseSSEEvent(raw);
    expect(result.type).toBe("tool_start");
    expect(result.name).toBe("execute_metric_query");
  });
});
```

**E2E 测试（Playwright）**：

```typescript
// apps/web/e2e/chat.spec.ts
import { test, expect, devices } from "@playwright/test";

test.describe("对话功能 - 查询型问题", () => {
  test("应正确回答指标查询并展示图表", async ({ page }) => {
    await page.goto("/chat");
    await page.getByPlaceholder("请用自然语言提问").fill("最近三天的叠片系数均值是多少？");
    await page.keyboard.press("Enter");

    // 等待 AI 回答完成
    await expect(page.getByText("叠片系数")).toBeVisible({ timeout: 30000 });
    // 验证图表已渲染
    await expect(page.locator("canvas")).toBeVisible();
    // 验证计算说明存在
    await expect(page.getByText("计算方式")).toBeVisible();
  });
});

// 移动端测试（iPhone 模拟）
test.describe("移动端适配", () => {
  test.use({ ...devices["iPhone 14"] });

  test("移动端对话界面应正常显示", async ({ page }) => {
    await page.goto("/chat");
    await expect(page.getByPlaceholder("请用自然语言提问")).toBeVisible();
  });
});
```

### 6.2 后端测试

**单元测试（pytest）**：

```python
# services/agent-api/tests/unit/test_time_parser.py
import pytest
from app.semantic.time_parser import TimeParser

class TestTimeParser:
    def test_parse_recent_days(self):
        result = TimeParser.parse("最近三天")
        assert "INTERVAL 3 DAY" in result["sql"]

    def test_parse_month_shift(self):
        result = TimeParser.parse("1月甲班")
        assert "MONTH(DetectionDate) = 1" in result["sql"]
        assert "Shift = '甲'" in result["sql"]

    def test_parse_last_month(self):
        result = TimeParser.parse("上个月")
        assert "DATE_SUB" in result["sql"]

    def test_unknown_expression_returns_default(self):
        result = TimeParser.parse("某个时间")
        assert "INTERVAL 30 DAY" in result["sql"]  # 默认30天
```

**Agent 流程测试（Mock LLM）**：

```python
# services/agent-api/tests/agent/test_query_agent.py
import pytest
from unittest.mock import AsyncMock, patch
from app.agents.graph import create_agent_graph, AgentState
from langchain_core.messages import HumanMessage

@pytest.mark.asyncio
async def test_query_agent_full_flow():
    """测试查询型问题的完整 Agent 流程"""
    
    # Mock LLM 响应（避免真实 API 调用）
    with patch("app.agents.orchestrator.ChatOpenAI") as mock_llm:
        mock_llm.return_value.ainvoke = AsyncMock(return_value=MockMessage(
            '{"intent": "query", "entities": {"metric": "叠片系数", "time_range": "最近三天"}}'
        ))
        
        graph = create_agent_graph()
        result = await graph.ainvoke({
            "messages": [HumanMessage(content="最近三天的叠片系数均值是多少？")],
            "session_id": "test-session-001",
        })
    
    # 验证意图识别正确
    assert result["intent"] == "query"
    # 验证实体提取正确
    assert result["entities"]["metric"] == "叠片系数"
    # 验证有图表配置输出
    assert result["chart_config"] is not None
```

**集成测试（httpx AsyncClient）**：

```python
# services/agent-api/tests/integration/test_chat_api.py
import pytest
from httpx import AsyncClient, ASGITransport
from app.main import app

@pytest.mark.asyncio
async def test_chat_stream_endpoint():
    """测试流式对话端点"""
    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        async with client.stream(
            "POST",
            "/api/v1/chat/stream",
            json={
                "messages": [{"role": "user", "content": "最近三天叠片系数均值是多少？"}],
                "session_id": "test-001",
            },
        ) as response:
            assert response.status_code == 200
            assert response.headers["content-type"] == "text/event-stream"
            
            # 收集流式事件
            events = []
            async for line in response.aiter_lines():
                if line.startswith("data: ") and line != "data: [DONE]":
                    events.append(json.loads(line[6:]))
            
            # 验证至少有文本事件
            text_events = [e for e in events if e["type"] == "text"]
            assert len(text_events) > 0
```

---

## 七、Docker Compose 部署配置

```yaml
# docker-compose.yml
version: "3.9"

services:
  # Next.js Web 前端
  web:
    build:
      context: ./apps/web
      dockerfile: Dockerfile
    ports:
      - "3000:3000"
    environment:
      - AGENT_API_URL=http://agent-api:8000
      - INTERNAL_API_KEY=${INTERNAL_API_KEY}
    depends_on:
      - agent-api

  # FastAPI Agent 服务
  agent-api:
    build:
      context: ./services/agent-api
      dockerfile: Dockerfile
    ports:
      - "8000:8000"
    environment:
      - DATABASE_URL=mysql+aiomysql://${DB_USER}:${DB_PASS}@${DB_HOST}:3306/${DB_NAME}
      - REDIS_URL=redis://redis:6379
      - LITELLM_BASE_URL=http://litellm-proxy:4000/v1
      - LITELLM_API_KEY=${LITELLM_API_KEY}
      - LANGSMITH_API_KEY=${LANGSMITH_API_KEY}
      - LANGSMITH_PROJECT=nlq-agent
    depends_on:
      - redis
      - litellm-proxy

  # LiteLLM 多模型网关
  litellm-proxy:
    image: ghcr.io/berriai/litellm:main-latest
    ports:
      - "4000:4000"
    volumes:
      - ./litellm_config.yaml:/app/config.yaml
    command: ["--config", "/app/config.yaml", "--port", "4000"]
    environment:
      - OPENAI_API_KEY=${OPENAI_API_KEY}
      - OPENROUTER_API_KEY=${OPENROUTER_API_KEY}

  # Redis（会话存储 + 知识图谱缓存）
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

volumes:
  redis_data:
```

**FastAPI Dockerfile（使用 uv）**：

```dockerfile
# services/agent-api/Dockerfile
FROM python:3.11-slim

# 安装 uv
COPY --from=ghcr.io/astral-sh/uv:latest /uv /usr/local/bin/uv

WORKDIR /app

# 复制依赖文件
COPY pyproject.toml uv.lock ./

# 安装生产依赖（使用 --frozen 确保与 uv.lock 完全一致）
RUN uv sync --frozen --no-dev

# 复制应用代码
COPY app/ ./app/

# 启动服务
CMD ["uv", "run", "uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000", "--workers", "4"]
```

---

## 八、环境变量配置

```bash
# .env.example（提交到 Git，不含真实值）

# 数据库（只读账号）
DB_HOST=your-mysql-host
DB_PORT=3306
DB_USER=nlq_readonly
DB_PASS=your-db-password
DB_NAME=poxiao_lab

# Redis
REDIS_URL=redis://localhost:6379

# LLM 提供商
OPENAI_API_KEY=sk-...
OPENROUTER_API_KEY=sk-or-...

# 内部服务通信
INTERNAL_API_KEY=sk-internal-...
LITELLM_API_KEY=sk-litellm-...

# 可观测性
LANGSMITH_API_KEY=ls__...
LANGSMITH_PROJECT=nlq-agent

# Next.js
NEXT_PUBLIC_APP_URL=https://your-domain.com
AGENT_API_URL=http://agent-api:8000
```

---

## 九、安全设计

### 9.1 SQL 注入防护

所有 SQL 查询通过 SQLAlchemy 参数化执行，禁止字符串拼接。Agent 生成的 SQL 在执行前经过**白名单校验**：

```python
# app/tools/sql_tools.py
import re
from app.core.database import get_db_session

SQL_WHITELIST_PATTERN = re.compile(
    r"^\s*SELECT\s+",  # 必须以 SELECT 开头
    re.IGNORECASE
)

FORBIDDEN_KEYWORDS = ["INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER", "EXEC", "EXECUTE", "--", "/*"]

async def execute_safe_sql(sql: str, params: dict | None = None) -> list[dict]:
    """安全执行 SQL，仅允许 SELECT 语句"""
    # 白名单检查
    if not SQL_WHITELIST_PATTERN.match(sql):
        raise ValueError(f"不允许执行非 SELECT 语句：{sql[:50]}...")
    
    # 黑名单检查
    sql_upper = sql.upper()
    for keyword in FORBIDDEN_KEYWORDS:
        if keyword in sql_upper:
            raise ValueError(f"SQL 包含禁止关键字：{keyword}")
    
    async with get_db_session() as session:
        result = await session.execute(sql, params or {})
        return [dict(row) for row in result.mappings()]
```

### 9.2 认证与授权

JWT Token 由现有 .NET 系统签发，NLQ-Agent 通过共享密钥验证：

```python
# app/core/auth.py
from fastapi import HTTPException, Security
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
import jwt

security = HTTPBearer()

async def verify_token(credentials: HTTPAuthorizationCredentials = Security(security)):
    try:
        payload = jwt.decode(
            credentials.credentials,
            key=settings.JWT_SECRET,
            algorithms=["HS256"]
        )
        return payload
    except jwt.ExpiredSignatureError:
        raise HTTPException(status_code=401, detail="Token 已过期")
    except jwt.InvalidTokenError:
        raise HTTPException(status_code=401, detail="无效的 Token")
```

---

## 十、关键风险与应对措施

| 风险 | 影响 | 应对措施 |
|------|------|---------|
| LLM 幻觉导致 SQL 语法错误 | 查询失败或结果不准确 | SQL 白名单校验 + 执行前语法验证 + 结果数据类型校验 |
| 知识图谱与数据库不同步 | 归因分析结果错误 | 监听 Formula/JudgmentLevel 表变更，触发图谱增量更新；每小时全量刷新 |
| 多轮对话上下文混乱 | 追问结果不正确 | 限制上下文窗口（10轮），超过时提示用户开启新对话 |
| 私有化 LLM 不支持 Function Calling | 归因/假设型 Agent 无法工作 | LiteLLM 配置中标记 `supports_function_calling`，前端模型选择器自动过滤 |
| 大数据量查询超时 | 用户等待时间过长 | 查询结果缓存（Redis，TTL 5分钟）；假设型分析限制数据量（最近30天） |
| 移动端 Canvas 图表性能 | 渲染卡顿 | 移动端限制数据点数量（≤200点），超过阈值自动降采样 |
| GPT-Vis 与 Next.js SSR 不兼容 | 服务端渲染报错 | 使用 `next/dynamic` + `{ ssr: false }` 动态导入所有 Canvas 图表组件 |

---

## 十一、参考资料

[1] LangGraph 官方文档：https://langchain-ai.github.io/langgraph/  
[2] Vercel AI SDK 文档：https://ai-sdk.dev/docs  
[3] ai-elements 组件库：https://elements.ai-sdk.dev  
[4] shadcn/ui 文档：https://ui.shadcn.com  
[5] LiteLLM 文档：https://docs.litellm.ai  
[6] GPT-Vis 仓库：https://github.com/antvis/GPT-Vis  
[7] Ant Design Charts 文档：https://ant-design-charts.antgroup.com  
[8] uni-app x 文档：https://doc.dcloud.net.cn/uni-app-x/  
[9] uv 包管理工具：https://docs.astral.sh/uv/  
[10] Playwright 测试框架：https://playwright.dev  
