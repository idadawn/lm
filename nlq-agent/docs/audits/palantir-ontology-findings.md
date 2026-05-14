# Palantir Ontology 关键摘录

来源：

1. https://palantir.com/docs/foundry/ontology/overview/
2. https://palantir.com/docs/foundry/ontology/core-concepts/

## 官方概念

Palantir Ontology 被描述为组织的 operational layer，位于 datasets、virtual tables、models 等数字资产之上，把数据资产连接到现实世界对象，例如工厂、设备、产品、订单和交易。它在很多场景中充当组织的 digital twin，同时包含 semantic elements（objects、properties、links）和 kinetic elements（actions、functions、dynamic security）。

Core concepts 页面强调：Ontology 是组织世界的分类与数字孪生，它把 datasets and models 映射到 object types、properties、link types 和 action types。

| 概念 | 官方含义 | 对 lm 项目的启发 |
|---|---|---|
| Object type | 组织中的实体或事件类型 | 需要把产品规格、检测记录、指标、判定规则、缺陷/外观特性、报表配置等建成稳定业务对象，而不是只画数据库节点 |
| Property | 对象类型的特征定义 | 应将字段、单位、阈值、格式、状态、版本、来源等纳入属性元数据 |
| Link type | 两类对象之间关系的定义 | 应明确 HAS_RULE、EVALUATES、HAS_ATTRIBUTE、BELONGS_TO、DERIVED_FROM、OBSERVED_IN 等关系语义和方向 |
| Action type | 用户一次性对对象、属性或链接做出的变更/编辑定义，并可包含提交后的副作用 | 可演进为“规则调整”“规格版本发布”“异常复核”“阈值模拟”等可治理动作 |
| Functions | 与 Ontology 原生集成，可接收对象或对象集合，读取属性，并在动作和应用中复用 | 可将判定逻辑、趋势计算、根因解释、SQL 生成校验封装成对象感知函数 |

## 设计含义

Palantir Ontology 不是单纯知识图谱浏览器，也不是数据库 ER 图。它的关键价值在于：业务对象化、语义关系化、逻辑函数化、操作动作化、权限治理化、应用场景化。对 lm 的知识图谱问答而言，应该避免只把 Neo4j 当作可视化数据源，而应让问答 Agent 面向对象和关系路径进行规划、取证、计算和解释。

## 最佳实践与反模式

来源：https://palantir.com/docs/foundry/ontology/ontology-best-practices-and-anti-patterns/

Palantir 官方最佳实践强调：**model reality, not systems**，对象类型应代表现实世界实体，而不是源系统或部门视图；**curate intentionally**，每个属性都要有明确业务或技术价值；**keep object types focused**，每个对象类型代表一个清晰实体；同时要为人或 Agent 决策使用 action types，为自动化转换使用 pipelines，为复杂实时逻辑使用 functions。

| 反模式 | 含义 | 对 lm 的风险 |
|---|---|---|
| System Silos | 按来源系统建对象类型 | 把 lab_* 表直接变成图谱会沦为 ER 图，无法支撑自然语言问答 |
| Kitchen Sink | 把所有技术字段都塞进属性 | 前端节点过多、标签重叠、用户无法理解业务含义 |
| God Object | 一个对象表达多类实体 | 检测记录、规则、规格、外观缺陷混在一起，会导致推理路径不可解释 |
| Golden Hammer | 所有问题都用一种工具解决 | 不能只依赖 SQL、只依赖 Neo4j 或只依赖 LLM，需要分层编排 |
| Misnomer | 名称模糊或误导 | 当前图上短标签如 A/B/C/C1/D/B1 对用户含义不足，需业务化命名 |

## Ontology-Augmented Generation 启发

来源：https://palantir.com/docs/foundry/ontology/ontology-augmented-generation/

Palantir 官方指出，LLM 应用于业务上下文时，第一步几乎总是找到相关上下文；上下文检索通常是 RAG 系统最困难部分。官方也强调不存在单一最优检索方式，方案取决于数据本身，可组合语义搜索、对象检索、函数和完整上下文输入。

对 lm 的启发是：知识图谱问答不应只返回图谱画布，而应形成“问题理解 → 对象定位 → 关系路径检索 → 数据/规则/文档证据组合 → 函数计算 → 可解释答案”的 Ontology-Augmented Generation 流程。
