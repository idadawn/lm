# 001 QueryAgent MVP — 指标查询完整闭环

> **状态**：待开发  
> **创建日期**：2026-03-10  
> **优先级**：P0（第一阶段核心功能）

---

## 背景与目标

这是整个 NLQ-Agent 系统的第一个可运行版本。目标是验证"自然语言 → SQL → 图表 + 计算说明"的完整链路，让用户可以用中文提问指标查询类问题并得到有意义的回答。

## 范围

**本次做：**
- FastAPI 服务基础骨架（`app/main.py`、`app/core/`、`app/api/`）
- LangGraph `QueryAgent` 完整实现（状态机 + 工具函数）
- 3 个核心 Tool：`query_metric_tool`、`get_formula_definition_tool`、`get_grade_rules_tool`
- MySQL 连接层（只读，参数化查询，SQL 白名单）
- `/api/v1/chat/stream` SSE 端点
- Next.js 对话页面（ai-elements `Conversation` + `Message` + `Tool`）
- `@ant-design/charts` Line 组件（趋势图）
- 计算方式说明展示（从 `LAB_INTERMEDIATE_DATA_FORMULA.Formula` 读取）

**明确不做（留到后续）：**
- 归因分析（RootCauseAgent）
- 洞察型分析（InsightAgent）
- 假设型分析（HypothesisAgent）
- 知识图谱（NetworkX）
- 多轮对话持久化（Redis Checkpointer）
- JWT 认证
- 移动端（uni-app x）
- 模型选择器 UI

## 技术方案

### 数据流

```
用户提问（中文）
    ↓ POST /api/chat（Next.js Route Handler）
    ↓ SSE 代理转发
    ↓ POST /api/v1/chat/stream（FastAPI）
    ↓ LangGraph QueryAgent
        ├── get_formula_definition_tool（读取指标计算公式）
        ├── query_metric_tool（生成并执行 SELECT SQL）
        └── get_grade_rules_tool（读取等级判定规则）
    ↓ 流式返回 AgentEvent（文字 + 工具调用状态 + 图表描述）
    ↓ 前端渲染（ai-elements + @ant-design/charts）
```

### 核心文件

| 文件 | 内容 |
|------|------|
| `services/agent-api/app/main.py` | FastAPI 应用入口 |
| `services/agent-api/app/core/config.py` | 配置（pydantic-settings） |
| `services/agent-api/app/core/database.py` | SQLAlchemy 异步连接池 |
| `services/agent-api/app/core/llm_factory.py` | LiteLLM 模型工厂 |
| `services/agent-api/app/agents/graph.py` | LangGraph 图定义（AgentState + 节点 + 路由） |
| `services/agent-api/app/tools/query_tools.py` | 3 个核心 Tool 函数 |
| `services/agent-api/app/tools/sql_tools.py` | SQL 安全执行层（白名单 + 参数化） |
| `services/agent-api/app/api/chat.py` | SSE 端点实现 |
| `services/agent-api/app/models/schemas.py` | Pydantic 请求/响应模型 |
| `apps/web/app/page.tsx` | 对话主页面 |
| `apps/web/app/api/chat/route.ts` | SSE 代理 Route Handler |
| `apps/web/components/charts/TrendLine.tsx` | 趋势折线图组件 |
| `packages/shared-types/src/index.ts` | 共享类型定义 |

### 典型问答场景

**输入**：`2026年1月甲班的Ps铁损平均值是多少？`

**Agent 执行步骤**：
1. `get_formula_definition_tool("Ps铁损")` → 返回公式定义和字段名
2. `query_metric_tool(metric="PerfPsLoss", filters={班次:"甲", 月份:"2026-01"}, agg="AVG")` → 执行 SQL，返回数值
3. `get_grade_rules_tool(formula_id=xxx, value=1.23)` → 返回等级判定结果

**输出**：
- 文字说明：Ps铁损的计算方式是...，2026年1月甲班平均值为 **1.23 W/kg**，依据产品规格 XX 判定为 **优等品**
- 折线图：展示该班次每日 Ps铁损趋势

## 验收标准

- [ ] 提问"上个月的Ps铁损平均值"，返回正确数值和等级
- [ ] 提问"最近7天叠片系数趋势"，返回折线图
- [ ] 回答中包含"计算方式说明"（公式来源于数据库）
- [ ] SSE 流式输出正常（打字机效果）
- [ ] 工具调用状态在前端可见（ai-elements `Tool` 组件）
- [ ] 后端测试覆盖率 ≥75%
- [ ] SQL 注入防护测试通过

## 影响的文档

完成后更新 `docs/TDD.md` 中 QueryAgent 的实现状态标记。
