# nlq-agent 文档索引

本目录是 `nlq-agent` 的统一文档入口。历史上散落在仓库根目录和 `nlq-agent` 根目录的架构、开发、审查、路线图文档已经集中到这里，便于后续维护和检索。

## 文档分区

| 分区 | 内容 | 维护建议 |
|---|---|---|
| `architecture/` | 当前架构与历史设计记录。 | 新架构以 `langgraph-agent-api-architecture.md` 为准，旧两阶段文档仅用于理解演进背景。 |
| `audits/` | 项目结构审查、知识图谱建议、前端体验建议和调研材料。 | 审查类文档统一放这里，不再放仓库根目录。 |
| `changes/` | 增量变更记录和 MVP 交付说明。 | 每个较大功能建议增加一份变更记录。 |
| `roadmap/` | 开发计划、Vibe coding 检查清单和后续路线图。 | 保持计划与实际主线一致。 |
| 根目录文档 | PRD、TDD、Runbook、知识图谱设计与验证指南。 | 面向产品、测试、运维和功能验证。 |

## 快速入口

| 需求 | 推荐阅读 |
|---|---|
| 想了解当前主线架构 | `architecture/langgraph-agent-api-architecture.md` |
| 想确认为什么整理目录 | `audits/nlq-agent-structure-audit.md` |
| 想启动和开发项目 | `development-guide.md` 与 `../README.md` |
| 想验证知识图谱接口 | `kg-validation-guide.md` |
| 想查看 Neo4j 图谱设计 | `kg-neo4j-design.md` |
| 想追踪后续任务 | `roadmap/development-plan.md` |

## 命名规范

新增文档优先使用小写 kebab-case 文件名，例如 `query-agent-routing-design.md`。除非已有工具强依赖，不再新增全大写和小写并存的重复主题文件，避免跨平台大小写差异造成歧义。
