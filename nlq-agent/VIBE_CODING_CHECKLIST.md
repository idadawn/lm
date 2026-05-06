# NLQ-Agent Vibe Coding 启动清单

> **文档用途**：在正式开始 Vibe Coding 前，记录项目现状、缺口分析和需要补充的所有内容。  
> **当前日期**：2026-03-10  
> **状态**：待完成

---

## 一、项目现状（已有内容）

| 类别 | 已有内容 | 完整度 |
|------|---------|--------|
| **AI 工具配置** | `CLAUDE.md`（Claude Code 项目规范）、`.cursorrules`（Cursor 规则）、`.claude/settings.local.json` | ✅ 完整 |
| **Skills** | `nlq-agent-dev`、`nlq-agent-review`、`nlq-agent-commit`（含模板和脚本） | ✅ 完整 |
| **产品文档** | `NLQ_Agent_PRD.md`（需放入仓库） | ⚠️ 未入库 |
| **技术文档** | `NLQ_Agent_TDD.md`（需放入仓库） | ⚠️ 未入库 |
| **项目骨架** | 无任何目录结构、代码文件 | ❌ 缺失 |
| **环境配置** | 无 `.env.example`、`pyproject.toml`、`package.json` | ❌ 缺失 |
| **CI/CD** | 无 GitHub Actions 工作流 | ❌ 缺失 |
| **数据库 Schema** | 无 MySQL DDL 或 ORM 模型定义 | ❌ 缺失 |
| **Docker 配置** | 无 `docker-compose.yml` | ❌ 缺失 |

---

## 二、缺口分析

### 2.1 必须在 Vibe Coding 开始前补充（阻塞项）

以下内容是 AI 工具（Antigravity / Cursor / Claude Code）正确生成代码的**上下文基础**，缺少会导致 AI 生成偏离架构规范。

#### ① Monorepo 骨架目录结构

AI 工具需要感知到目录结构才能正确生成文件路径。需要创建：

```
nlq-agent/
├── apps/
│   ├── web/                    # Next.js 15（空目录，含 package.json 占位）
│   └── mobile/                 # uni-app x（空目录）
├── packages/
│   └── shared-types/           # 共享 TypeScript 类型（空目录）
├── services/
│   └── agent-api/              # FastAPI + LangGraph（空目录）
├── docs/                       # 产品文档和技术文档
│   ├── PRD.md
│   └── TDD.md
├── .github/
│   └── workflows/              # CI/CD（空目录）
├── pnpm-workspace.yaml         # Monorepo 工作区配置
├── turbo.json                  # Turborepo 配置
├── package.json                # 根 package.json
└── .gitignore
```

#### ② `pyproject.toml`（后端依赖声明）

Claude Code 和 Cursor 在后端开发时需要知道依赖版本。

```toml
[project]
name = "nlq-agent-api"
version = "0.1.0"
requires-python = ">=3.11"
dependencies = [
    "fastapi>=0.115.0",
    "uvicorn[standard]>=0.30.0",
    "langgraph>=0.3.0",
    "langchain-openai>=0.3.0",
    "litellm>=1.50.0",
    "sqlalchemy[asyncio]>=2.0.0",
    "aiomysql>=0.2.0",
    "redis>=5.0.0",
    "networkx>=3.4.0",
    "pydantic>=2.0.0",
    "python-jose[cryptography]>=3.3.0",
    "python-dotenv>=1.0.0",
    "scipy>=1.14.0",
    "numpy>=2.0.0",
]

[project.optional-dependencies]
dev = [
    "pytest>=8.0.0",
    "pytest-asyncio>=0.24.0",
    "pytest-cov>=5.0.0",
    "httpx>=0.27.0",
    "ruff>=0.8.0",
    "mypy>=1.13.0",
]
```

#### ③ `.env.example`（环境变量模板）

所有 AI 工具都会读取 `.env.example` 来理解配置结构：

```env
# 数据库配置
DATABASE_URL=mysql+aiomysql://user:password@localhost:3306/lab_db

# Redis 配置
REDIS_URL=redis://localhost:6379/0

# LiteLLM 配置
LITELLM_BASE_URL=http://localhost:4000
LITELLM_API_KEY=sk-xxx

# JWT 配置
JWT_SECRET_KEY=your-secret-key-here
JWT_ALGORITHM=HS256
JWT_EXPIRE_MINUTES=1440

# LangSmith 可观测性（可选）
LANGCHAIN_TRACING_V2=true
LANGCHAIN_API_KEY=ls__xxx
LANGCHAIN_PROJECT=nlq-agent

# 直连 LLM（不走 LiteLLM 时使用）
OPENAI_API_KEY=sk-xxx
OPENAI_BASE_URL=https://openrouter.ai/api/v1
```

#### ④ `litellm_config.yaml`（LLM 网关配置）

LiteLLM 的核心配置，定义所有支持的模型：

```yaml
model_list:
  - model_name: gpt-4o
    litellm_params:
      model: openai/gpt-4o
      api_key: os.environ/OPENAI_API_KEY
      api_base: os.environ/OPENAI_BASE_URL

  - model_name: gemini-2.5-flash
    litellm_params:
      model: gemini/gemini-2.5-flash
      api_key: os.environ/GEMINI_API_KEY

  - model_name: qwen3-32b
    litellm_params:
      model: openrouter/qwen/qwen3-32b
      api_key: os.environ/OPENROUTER_API_KEY

  - model_name: llama3-local
    litellm_params:
      model: ollama/llama3.1:8b
      api_base: http://localhost:11434

general_settings:
  master_key: os.environ/LITELLM_MASTER_KEY
```

#### ⑤ `docker-compose.yml`（本地开发环境）

一键启动 MySQL + Redis + LiteLLM Proxy：

```yaml
version: "3.9"
services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: lab_db
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
      - ./docs/schema.sql:/docker-entrypoint-initdb.d/schema.sql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  litellm:
    image: ghcr.io/berriai/litellm:main-latest
    ports:
      - "4000:4000"
    volumes:
      - ./litellm_config.yaml:/app/config.yaml
    command: ["--config", "/app/config.yaml", "--port", "4000"]
    env_file:
      - .env

volumes:
  mysql_data:
```

#### ⑥ MySQL Schema 定义（`docs/schema.sql`）

AI 工具在生成 SQL 查询和 ORM 模型时需要参考表结构。需要从 `lm` 仓库提取以下表的 DDL：

- [ ] `LAB_INTERMEDIATE_DATA`（中间数据主表）
- [ ] `LAB_INTERMEDIATE_DATA_FORMULA`（公式定义表）
- [ ] `LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL`（等级判定规则表）
- [ ] `LAB_PRODUCT_SPEC`（产品规格表）
- [ ] `LAB_PRODUCT_SPEC_ATTRIBUTE`（产品规格属性表）

#### ⑦ GitHub Actions CI 配置（`.github/workflows/ci.yml`）

```yaml
name: CI
on:
  pull_request:
    branches: [main, develop]
jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: astral-sh/setup-uv@v4
      - run: cd services/agent-api && uv sync --extra dev
      - run: cd services/agent-api && uv run ruff check .
      - run: cd services/agent-api && uv run mypy app/
      - run: cd services/agent-api && uv run pytest --cov=app --cov-report=xml
  frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: pnpm/action-setup@v4
      - run: pnpm install
      - run: pnpm type-check
      - run: pnpm lint
      - run: pnpm --filter web test --run
```

---

### 2.2 建议补充（提升 Vibe Coding 质量）

以下内容不是硬性阻塞，但能显著提升 AI 生成代码的质量和准确性。

#### ⑧ `docs/changes/` 目录（功能提案系统）

借鉴 OpenSpec 理念，每次开发新功能前在此目录创建提案：

```
docs/changes/
├── 001-query-agent-mvp.md        # 第一个功能：QueryAgent MVP
├── 002-knowledge-graph-loader.md # 知识图谱加载器
└── template.md                   # 提案模板
```

**提案模板（`template.md`）**：

```markdown
# [功能编号] 功能名称

## 背景与目标
（为什么要做这个功能，解决什么问题）

## 范围
（本次做什么，明确不做什么）

## 技术方案
（关键实现思路，涉及哪些文件）

## 验收标准
（怎么验证功能完成了）

## 影响的 Spec
（会修改 TDD/PRD 的哪些部分）
```

#### ⑨ `docs/specs/` 目录（活文档拆分）

将 TDD 拆分为更细粒度的 spec 文件，方便 AI 工具按需加载：

```
docs/specs/
├── agents.md          # 四类 Agent 的规格说明
├── api-contracts.md   # API 接口契约（请求/响应格式）
├── data-models.md     # 数据模型（Pydantic Schema）
├── knowledge-graph.md # 知识图谱结构说明
└── chart-protocol.md  # 通用图表描述协议（JSON Schema）
```

#### ⑩ `AGENTS.md`（Agent 工具配置文件）

spec-kit 风格的 Agent 配置文件，告诉所有 AI 工具项目的关键约定：

```markdown
# AGENTS.md - NLQ-Agent 项目 AI 工具配置

## 项目简介
工业质量数据自然语言问数系统。用户用中文提问，AI 查询 MySQL 中的检测数据并返回图表和分析结论。

## 关键约定
- 后端语言：Python 3.11+，包管理：uv（禁止 pip）
- 前端：Next.js 15 + TypeScript，包管理：pnpm
- 所有 SQL 必须参数化，禁止字符串拼接
- LangGraph 工具函数必须有 @tool 装饰器和完整 docstring

## 核心文档
- 技术架构：docs/TDD.md
- 产品需求：docs/PRD.md
- 当前功能提案：docs/changes/（开发前必读）

## 禁止事项
- 不得修改 uv.lock（使用 uv add/remove）
- 不得在 React Server Component 中直接导入 Canvas 图表
- 不得硬编码任何 API Key 或密码
```

---

## 三、Vibe Coding 启动顺序

按以下顺序完成准备工作后，即可开始 Vibe Coding：

### Phase 0：仓库骨架搭建（约30分钟）

```bash
# 1. 创建 Monorepo 目录结构
mkdir -p apps/{web,mobile} packages/shared-types services/agent-api docs/{specs,changes} .github/workflows

# 2. 创建根配置文件
touch pnpm-workspace.yaml turbo.json package.json .gitignore .env.example

# 3. 创建 pyproject.toml（后端）
touch services/agent-api/pyproject.toml

# 4. 创建 Docker 配置
touch docker-compose.yml litellm_config.yaml

# 5. 将 PRD 和 TDD 放入 docs/
cp NLQ_Agent_PRD.md docs/PRD.md
cp NLQ_Agent_TDD.md docs/TDD.md

# 6. 提取 MySQL Schema
# 从 lm 仓库导出 DDL → docs/schema.sql

# 7. 创建第一个功能提案
cp docs/changes/template.md docs/changes/001-query-agent-mvp.md
# 填写提案内容

# 8. 提交骨架
git add . && git commit -m "chore(deps): 初始化 Monorepo 骨架和开发环境配置"
```

### Phase 1：第一个 Vibe Coding 任务（QueryAgent MVP）

骨架完成后，第一个任务建议是：

> **目标**：实现 QueryAgent 的完整闭环——用户用中文问一个指标查询问题，Agent 生成 SQL、执行查询、返回数据，前端以折线图展示，并附带"计算方式说明"。

**在 Cursor / Claude Code 中的启动 Prompt**：

```
请阅读 docs/TDD.md、docs/PRD.md 和 docs/changes/001-query-agent-mvp.md，
然后实现 QueryAgent 的 MVP 版本。

具体要求：
1. 在 services/agent-api/app/ 下创建完整的 FastAPI + LangGraph 项目结构
2. 实现 QueryAgent 的 query_metric_tool 工具函数
3. 实现 /api/v1/chat/stream SSE 端点
4. 在 apps/web/ 下创建 Next.js 对话页面，使用 ai-elements 组件
5. 使用 @ant-design/charts 的 Line 组件展示趋势图

请先输出实现计划，确认后再生成代码。
```

---

## 四、缺口完成状态追踪

| 编号 | 内容 | 优先级 | 状态 |
|------|------|--------|------|
| ① | Monorepo 骨架目录 | 🔴 阻塞 | ⬜ 待完成 |
| ② | `pyproject.toml` | 🔴 阻塞 | ⬜ 待完成 |
| ③ | `.env.example` | 🔴 阻塞 | ⬜ 待完成 |
| ④ | `litellm_config.yaml` | 🔴 阻塞 | ⬜ 待完成 |
| ⑤ | `docker-compose.yml` | 🔴 阻塞 | ⬜ 待完成 |
| ⑥ | MySQL Schema DDL | 🔴 阻塞 | ⬜ 待完成 |
| ⑦ | GitHub Actions CI | 🟡 重要 | ⬜ 待完成 |
| ⑧ | `docs/changes/` 功能提案 | 🟡 重要 | ⬜ 待完成 |
| ⑨ | `docs/specs/` 活文档 | 🟢 建议 | ⬜ 待完成 |
| ⑩ | `AGENTS.md` | 🟢 建议 | ⬜ 待完成 |
