# nlq-agent 项目结构与实现审查报告

作者：**Manus AI**  
审查时间：2026-05-14

## 一、结论摘要

基于已经迁移到 `docs/architecture/legacy-two-stage-architecture.md`、`docs/development-guide.md` 与 `docs/architecture/langgraph-agent-api-architecture.md` 的三份文档对 `nlq-agent` 目录进行检查后，可以明确判断：**现在目录确实处于“旧两阶段 FastAPI 服务、新 LangGraph Agent 服务、独立 Next.js 前端工作区、共享类型包、历史文档与计划文档并存”的过渡态**。代码并不是完全不可用，但缺少一个清晰的主线入口和废弃边界，因此看起来会非常乱。

目前实际运行主线更偏向 `services/agent-api/app` 下的新服务，它使用 FastAPI + LangGraph，提供 `/api/v1/chat/*`、`/api/v1/kg/*`、健康检查、知识图谱浏览、带材子图、QueryAgent、RootCauseAgent 和 Chat2SQL 等能力。[1] 相比之下，旧版两阶段编排器和旧 FastAPI 路由已经归档到 `legacy/two-stage-service/src`，其历史能力曾经也暴露 `/api/v1/chat/stream`、`/api/v1/sync/*`、`/api/v1/kg/*`，这是本次整理前形成**双后端并存**的主要来源。[2]

文档中的“知识图谱作为业务语义层，先做 Stage 1 语义/图谱检索，再做 Stage 2 SQL 查询”的思想已经被部分落地，但落地方式已经从早期 `src/pipelines/orchestrator.py` 的两阶段 Pipeline，演进成 `services/agent-api/app/agents/graph.py` 的 LangGraph 多 Agent 路由。[3] 这意味着文档与代码之间存在版本漂移：旧文档描述的是两阶段编排器，新代码实现的是意图分类后路由到 QueryAgent、RootCauseAgent、Chat2SQL 或回退响应的图工作流。

## 二、目录现状盘点

从整理前的文件数量和入口分布看，`nlq-agent` 不是单一 Python 服务，而是混合了两个 Python 后端栈和一个前端 monorepo 雏形。旧版 `src` 目录包含 29 个 Python 文件，旧测试目录包含 41 个 Python 测试文件；新版 `services/agent-api/app` 包含 31 个 Python 文件，新测试目录包含 13 个测试文件；同时 `apps` 与 `packages` 下还有 Next.js 前端和共享类型包。[4] 本次整理后，旧版 `src` 与旧测试已经统一移动到 `legacy/two-stage-service`。

| 区域 | 当前角色 | 状态判断 | 主要问题 |
|---|---|---|---|
| `nlq-agent/legacy/two-stage-service/src` | 旧版 FastAPI + 两阶段 Pipeline | **已归档，只作历史参考** | 与新服务重复暴露 chat、sync、kg 能力，本次整理后不再作为默认入口。 |
| `nlq-agent/services/agent-api/app` | 新版 FastAPI + LangGraph Agent API | **当前最像主服务** | 功能最完整，但仍有 TODO、占位接口和 Neo4j/MySQL 双路径并存。 |
| `nlq-agent/apps/web` | 独立 Next.js NLQ 前端 | **像实验/独立控制台** | 与仓库根目录既有 `web` Vue 前端并列，集成边界不明确。 |
| `nlq-agent/packages/shared-types` | 共享 TypeScript 类型 | **结构合理但归属待定** | 如果主前端仍是根目录 `web`，需要决定是否继续保留此 monorepo。 |
| `nlq-agent/legacy/two-stage-service/tests` 与 `services/agent-api/tests` | 旧/新两套测试 | **旧测试已归档，新测试为主线** | 测试命令、依赖、覆盖对象需要继续收敛。 |
| 根目录文档与计划文件 | 架构说明与开发计划 | **文档价值高但版本混杂** | 三份架构文档没有明确“最新版/废弃版”标识。 |

## 三、文档要求与代码实现对照

三份文档的核心要求可以归纳为：使用知识图谱作为业务语义层，先解释业务语义和规则，再生成或执行数据查询；前端继续接收标准化推理链事件；同时保留与 .NET 后端的数据同步和知识图谱浏览能力。[5] 实际代码已经实现了一部分，但实现路径分为旧实现和新实现两条线。

| 文档期望 | 代码位置 | 实现情况 | 审查判断 |
|---|---|---|---|
| Stage 1 语义解析与知识图谱检索 | `legacy/two-stage-service/src/pipelines/orchestrator.py`、`services/agent-api/app/agents/graph.py` | 旧版有显式两阶段编排，新版改为 LangGraph 路由 | **部分实现，但架构形态已变化**。 |
| Stage 2 SQL 生成与执行 | `legacy/two-stage-service/src` 旧 Pipeline、`services/agent-api/app/agents/chat2sql_agent.py` | 新版 Chat2SQL 有 schema_pick、column_pick、sql_draft、validate、execute、summary 六步链路 | **新版实现更接近实际可用 Chat2SQL**。 |
| 高频指标查询模板/工具 | `services/agent-api/app/agents/query_agent.py`、`app/tools/query_tools.py` | QueryAgent 直接根据实体调用工具，不完全依赖 LLM function calling | **已有业务查询 MVP**。 |
| 根因解释 | `services/agent-api/app/agents/root_cause_agent.py`、`app/api/kg.py` | `/kg/explain` 和 Agent 路由均能走根因路径 | **已有 MVP，但接口之间有重复入口**。 |
| 知识图谱浏览与子图 | `services/agent-api/app/api/kg.py`、`app/tools/graph_tools.py` | `ontology` 直接查 MySQL，`ribbon/subgraph` 查带材子图，部分 Neo4j 查询也存在 | **可用，但 MySQL/Neo4j 双轨需要明确主次**。 |
| 小美问答 Ask API | `services/agent-api/app/api/kg.py` | `/ask` 返回“正在学习中，暂不支持” | **未完成，占位接口**。 |
| .NET 同步回调 | `legacy/two-stage-service/src/api/routes.py` | 旧版保留 `/api/v1/sync/rules`、`/api/v1/sync/specs`、`resync-now` | **新服务中未看到等价完整迁移**。 |
| 前端推理链兼容 | `apps/web`、根目录 `web` | `apps/web` 有独立 Next.js 推理链；根目录 Vue 前端也在使用知识图谱接口 | **前端路线分裂**。 |

## 四、核心实现完成度判断

新版 `services/agent-api` 已经有比较完整的核心骨架。`main.py` 负责 FastAPI 实例、CORS、生命周期和路由挂载；`api/chat.py` 通过 LangGraph 事件流输出 SSE；`agents/graph.py` 负责意图分类、Agent 路由和响应格式化；`query_agent.py` 实现了常见质量指标、一次交检合格率、判定规则和规格查询；`chat2sql_agent.py` 实现了显式六步 Chat2SQL 链路；`api/kg.py` 提供知识图谱健康、初始化、刷新、规格、规则、指标、外观、报表、解释、实体解析、本体、带材搜索和带材子图等接口。[1]

但是，这个完成度更准确地说是**MVP 可运行骨架 + 若干业务工具已接入 + 若干接口仍处在占位状态**，还不能称为干净的正式项目结构。最明显的占位点包括 `/api/v1/kg/ask` 直接返回“暂不支持此问题的图谱问答”，`neo4j_graph.py` 中存在 `NotImplemented`，一些文件包含 TODO 或 pass。需要注意的是，`pass` 不一定都是未实现错误，有些是异常吞吐或配置类空体；但 `/ask` 和 Neo4j 图实现不完整是明确的功能缺口。[6]

| 模块 | 完成度 | 说明 |
|---|---:|---|
| FastAPI 新服务入口 | 高 | `services/agent-api/app/main.py` 是较清晰的新主入口。 |
| LangGraph 问答流 | 中高 | 路由、事件流和响应格式化已存在，但与旧两阶段文档描述不同。 |
| QueryAgent 工具查询 | 中高 | 已处理指标、时间范围、规则、规格、一次交检合格率等场景。 |
| Chat2SQL | 中 | 六步链路清晰，但依赖 schema cache、LLM JSON 稳定性和 SQL 安全校验质量。 |
| RootCause/KG Explain | 中 | 已有根因解释路径，但 kg explain 与 root cause agent 边界重叠。 |
| KG 浏览/带材子图 | 中高 | MySQL 直查本体与带材子图已经接入，近期知识图谱页面依赖它。 |
| Neo4j 知识图谱 | 中低 | 有抽象和查询，但仍存在未实现/双轨风险。 |
| Ask 小美问答 | 低 | 当前是明确占位。 |
| 同步回调迁移 | 低到中 | 旧版 `src` 有 sync，未确认新服务完整承接。 |
| 测试体系 | 中低 | 测试文件不少，但 pytest 未安装，未能直接收集运行；新旧测试分裂。 |

## 五、为什么你会感觉“目录很乱”

第一，**存在两个 Python 后端项目**。`nlq-agent/pyproject.toml` 声明项目名为 `nlq-agent`，依赖偏向早期 FastAPI、Qdrant、OpenAI、aiomysql；`nlq-agent/services/agent-api/pyproject.toml` 声明项目名为 `nlq-agent-api`，依赖扩展到 LangGraph、LangChain、LiteLLM、Redis、Neo4j、Pandas、JWT 等。[7] 这不是简单的子模块关系，而是两套服务依赖和入口并存。

第二，**存在两个 FastAPI 应用入口**。旧版 `src/main.py` 和新版 `services/agent-api/app/main.py` 都创建 FastAPI 实例；旧版路由暴露 `/api/v1/chat/stream`、`/api/v1/sync/*`、`/api/v1/kg/*`，新版也暴露 `/api/v1/chat` 和 `/api/v1/kg` 前缀。[2] 这会导致开发者不知道应该启动哪个服务、改哪个接口、测哪个路径。

第三，**存在两套架构叙事**。文档 v2 强调“Stage 1: Semantic & KG Agent”和“Stage 2: Data & SQL Agent”，而新代码实际采用 LangGraph 的 intent classifier + 多 Agent 分流机制。两者不是完全矛盾，但抽象层级不同。如果文档不更新，后续开发会继续按照旧 Pipeline 思路往 `src` 里加代码，或者按照新 LangGraph 思路往 `services/agent-api` 加代码，混乱会继续扩大。[3]

第四，**知识图谱底座存在 MySQL 直查、Neo4j 图查询、Qdrant 语义层三种表达**。文档强调 Qdrant 语义层和知识图谱检索，新服务中也有 Neo4j 抽象，但前端浏览和带材子图当前大量使用 MySQL 直接查询。[6] 这可以作为务实 MVP，但必须在架构上说明：MySQL 是源数据，Neo4j/Qdrant 是可选索引或增强层，否则“知识图谱到底在哪”会越来越难解释。

第五，**独立 Next.js 前端和根目录 Vue 前端并存**。`nlq-agent/apps/web` 是一个独立 Next.js 应用，带有 `@nlq-agent/shared-types`、推理链组件和 e2e 测试；但仓库根目录已有 `web` Vue 前端并且刚刚接入了知识图谱页面。这两个前端不是天然冲突，但必须明确 `apps/web` 是实验控制台、未来替代前端，还是应该删除/归档的旧尝试。[8]

## 六、验证与静态检查结果

本次没有修改业务代码，只做检查。新版 `services/agent-api/app` 和旧版 `src` 均通过了 Python 字节码编译检查，说明当前至少没有明显语法错误。尝试对新版测试执行 `pytest --collect-only` 时，当前沙箱环境缺少 `pytest`，因此未能直接收集测试；这反映出项目测试依赖没有在当前环境统一安装，不能据此判断测试一定失败，但可以判断“一键验证路径”还不够顺畅。

| 检查项 | 命令/方法 | 结果 |
|---|---|---|
| 新版服务语法检查 | `python3.11 -m py_compile $(find app -name '*.py')` | 通过。 |
| 旧版 `src` 语法检查 | `python3.11 -m py_compile $(find src -name '*.py')` | 通过。 |
| 新版测试收集 | `python3.11 -m pytest --collect-only -q tests` | 未执行成功，环境缺少 `pytest`。 |
| 目录统计 | 文件系统扫描 | 新旧后端、Next.js 前端、共享包同时存在。 |
| 占位标记扫描 | TODO/pass/NotImplemented/暂不支持 | `/kg/ask`、Neo4j 实现、部分配置/工具文件存在未完成或占位痕迹。 |

## 七、建议的整理方向

我建议不要立即大规模删除文件，而是先做一次**主线冻结**：明确 `services/agent-api` 是当前唯一后端主线，`src` 进入 `legacy` 或 `archive`，并在 README 中写清楚“旧两阶段 Pipeline 仅作为参考，不再接新功能”。这样可以避免开发者继续在两个后端之间来回加功能。

| 优先级 | 建议动作 | 目标效果 | 风险控制 |
|---|---|---|---|
| P0 | 确认 `services/agent-api` 为唯一主服务入口 | 统一启动、部署和接口开发位置 | 保留旧目录但改名为 `legacy-src` 或增加弃用说明。 |
| P0 | 梳理 `/api/v1/sync/*` 是否需要迁移到新服务 | 避免旧服务因为同步接口而无法下线 | 先迁移接口壳，再接 Qdrant/缓存刷新逻辑。 |
| P0 | 更新 README 和三份文档状态 | 让开发者知道哪份是最新版 | 在旧文档顶部加“历史版本，仅供参考”。 |
| P1 | 合并测试命令和依赖安装方式 | 实现一条命令验证新主线 | 在 `services/agent-api` 固化 `pytest` 依赖和 test 命令。 |
| P1 | 明确 KG 数据底座：MySQL 源数据 + Neo4j/Qdrant 索引 | 消除“知识图谱到底查哪里”的疑惑 | 给每个接口标注数据源和刷新机制。 |
| P1 | 决定 `apps/web` 的定位 | 避免前端路线分裂 | 若根目录 Vue 是主前端，则 `apps/web` 标为实验控制台。 |
| P2 | 清理占位接口或改成明确 501 | 避免调用方误以为已支持 | `/kg/ask` 在实现前返回 HTTP 501 更清晰。 |
| P2 | 抽取共享 DTO/事件协议 | 降低 Vue、Next.js、后端协议漂移 | 将 `shared-types` 与根目录 Vue 类型同步或删除重复。 |

## 八、推荐的目标目录形态

如果目标是尽快让项目看起来清晰，我建议采用“主服务 + 归档旧实现 + 可选控制台”的形态，而不是继续保持现在的混合布局。一个可落地的目标结构如下：

```text
nlq-agent/
  README.md                         # 唯一入口说明：如何启动、如何测试、主接口在哪里
  docs/
    architecture-current.md          # 当前 LangGraph + KG + Chat2SQL 架构
    architecture-legacy-stage.md     # 旧两阶段设计归档
    dev-guide.md                     # 当前开发指南
  services/
    agent-api/                       # 唯一 Python 主服务
      app/
      tests/
      pyproject.toml
  legacy/
    stage-pipeline-src/              # 原 src 归档，只读参考
    old-tests/                       # 原旧测试归档
  packages/
    shared-types/                    # 若继续维护 Next.js 或跨前端类型，则保留
  apps/
    web/                             # 若作为实验控制台，则 README 标明非主前端
```

这种整理方式的关键不是移动文件本身，而是先统一决策：**当前业务主线以 `services/agent-api` 为准，旧 `src` 不再接新功能**。在此基础上，再迁移缺失的 sync 接口、统一测试、更新文档，目录自然会清晰。

## 九、建议的下一步执行计划

下一步可以分两步走。第一步是低风险整理，不改变业务逻辑，只增加说明、标记废弃边界、统一启动命令和测试命令。第二步再做代码迁移，把旧 `src/api/routes.py` 中仍有价值的 sync 能力迁到 `services/agent-api/app/api`，然后把旧目录移动到 `legacy`。

| 阶段 | 工作内容 | 是否改业务逻辑 | 建议提交粒度 |
|---|---|---:|---|
| 第 1 步 | 新增/更新 README，标注当前主线和历史文档状态 | 否 | 1 个提交。 |
| 第 2 步 | 给旧 `src` 加弃用说明，禁止继续新增功能 | 否 | 1 个提交。 |
| 第 3 步 | 迁移 `/api/v1/sync/*` 到新服务 | 是 | 1 个提交，附测试。 |
| 第 4 步 | 将旧 `src` 与旧测试移动到 `legacy` | 否或极低 | 1 个提交，避免与功能修改混在一起。 |
| 第 5 步 | 清理 `/kg/ask` 占位或实现小美问答主链路 | 是 | 单独提交。 |
| 第 6 步 | 统一前端类型和推理链协议 | 是 | 单独提交。 |

## References

[1]: nlq-agent/services/agent-api/app/main.py "新版 FastAPI 主入口"
[2]: ../../legacy/two-stage-service/src/main.py "旧版 FastAPI 主入口" 
[3]: nlq-agent/services/agent-api/app/agents/graph.py "新版 LangGraph 工作流"
[4]: nlq-agent/services/agent-api/tests "新版测试目录" 
[5]: ../architecture/langgraph-agent-api-architecture.md "知识图谱到数据查询两阶段问答架构设计"
[6]: nlq-agent/services/agent-api/app/api/kg.py "新版知识图谱 API"
[7]: nlq-agent/pyproject.toml "旧版服务依赖配置" 
[8]: nlq-agent/apps/web/package.json "独立 Next.js 前端配置"
