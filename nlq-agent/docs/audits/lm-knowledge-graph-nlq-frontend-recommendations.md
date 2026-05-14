# lm 项目知识图谱问答与前端展示优化建议

作者：**Manus AI**  
日期：2026-05-14

## 一、结论摘要

`lm` 项目已经具备知识图谱问答的雏形：`nlq-agent` 中存在 Neo4j 知识图谱构建、KG API、`RootCauseAgent` 的多跳判定路径，以及 Chat2SQL 的可视化推理步骤；主 Web 应用的 `/lab/knowledge-graph` 页面也已经通过 AntV G6 展示规格、规则、公式和属性节点。然而，当前实现更像一个“数据库关系图浏览器”，还没有演进成类似 **Palantir Ontology** 的业务对象层、决策逻辑层和操作层。因此，当前问答能力容易被拆成 SQL、Neo4j、LLM 三套独立通道，前端画布也会因为节点密集、标签过短、布局无层次而出现用户截图中的“红圈堆叠”和“紫色公式遮挡”。

我建议将目标定义为：**先用知识图谱完成对象定位、路径取证和规则解释，再按需要调用 SQL/统计函数完成数据计算，最后用可视化推理链与局部图谱共同呈现答案**。这个方向与 Palantir 官方对 Ontology 的定义一致：Ontology 是组织的 operational layer，位于数据集、虚拟表和模型之上，把数据资产连接到现实业务对象，并同时包含 semantic elements 与 kinetic elements。[1] Palantir 的核心概念也明确把数据与模型映射为 object types、properties、link types 和 action types。[2]

> Palantir 官方将 Ontology 描述为组织的 operational layer，既包含 objects、properties、links 等语义元素，也包含 actions、functions、dynamic security 等动能元素。[1]

| 主题 | 当前状态 | 核心问题 | 推荐方向 |
|---|---|---|---|
| `nlq-agent` | 已有 `query_agent`、`root_cause_agent`、`chat2sql_agent` 与 KG 工具 | KG、SQL、LLM 仍偏分离，主问答未形成统一 Ontology planner | 引入“意图识别 → 对象解析 → 图谱路径 → 函数/SQL → 答案合成”的统一编排 |
| 图谱模型 | Neo4j 中已有 `ProductSpec`、`Metric`、`JudgmentRule`、`SpecAttribute`、外观特性、报表配置等节点 | 更接近表结构映射，缺少对象类型、关系类型、规则条件、版本、动作的本体治理 | 从“表驱动图”升级为“业务对象本体” |
| `/lab/knowledge-graph` | Vue + AntV G6，展示 specs/rules/formulas/attributes | 全量力导向图导致严重重叠，缺少聚合、分层、详情上下文和业务视角 | 改成“Ontology Explorer + Reasoning Workspace”双视图 |
| 问答体验 | `RootCauseAgent` 已能输出 `reasoning_step`；Chat2SQL 也有分步链路 | 推理链没有与图谱画布打通，用户看不到“答案如何从图上来” | 将推理步骤映射为高亮路径、证据卡片、条件表、可追溯节点 |

## 二、对 `nlq-agent` 的优化建议

### 2.1 从“多个 Agent 工具”升级为“Ontology-aware Planner”

当前 `RootCauseAgent` 已经做得比较好：它从自然语言中抽取炉号、批次、目标等级，然后调用 `traverse_judgment_path`，按 `record → spec → rule → condition → grade` 输出有序推理步骤，并通过 SSE 推送给前端。这个模式值得保留并扩大到所有知识图谱问答场景。问题在于，当前只有单条记录的判级根因解释走了相对完整的图谱路径，其他问题仍容易退回 SQL 工具或表字段查询。

建议新增一个统一的 **Ontology Planner**，它不直接回答，而是先把用户问题转换成一份结构化计划。计划中至少包含 `intent`、`anchor_objects`、`required_paths`、`required_functions`、`required_sql`、`evidence_policy` 和 `visualization_hint`。这样可以让系统先判断问题属于规则解释、规格查询、缺陷归因、统计分析、趋势分析还是配置追问，再选择图谱、SQL、向量检索或函数计算。

| 层次 | 输入 | 输出 | 说明 |
|---|---|---|---|
| Intent Router | 用户问题、上下文 | `root_cause`、`metric_query`、`rule_explain`、`spec_compare`、`trend_analysis` 等 | 不再只做粗粒度路由，而是输出业务意图与缺失槽位 |
| Entity Resolver | 炉号、批次、规格、指标、缺陷词、等级词 | 本体对象 ID 与置信度 | 同义词、中文简称、字段名、历史上下文都在这里统一处理 |
| Path Planner | 意图与对象 | Neo4j 路径模板 | 例如 `FurnaceRecord -> ProductSpec -> JudgmentRule -> Condition -> Metric` |
| Evidence Collector | 路径模板、SQL/函数需求 | 证据包 | 包含图谱节点、边、实际值、期望阈值、SQL 结果、文档片段 |
| Answer Composer | 证据包 | Markdown 答案、推理链、前端高亮图 | 输出必须可追溯，不能只给自然语言结论 |

这种架构能避免“LLM 猜答案”，也能避免“SQL 结果没有业务解释”。Palantir 的 Ontology-Augmented Generation 文档指出，业务 LLM 任务的关键第一步通常是找到相关上下文，而且上下文检索往往是 RAG 系统最困难的部分。[4] 对 `lm` 来说，**相关上下文不应只是文档 chunk，而应优先是本体对象、关系路径、规则条件和真实检测数据**。

### 2.2 把知识图谱扩展为“检测领域本体”，而不是 Neo4j 表映射

`Neo4jKnowledgeGraph` 当前从 MySQL 构建 `ProductSpec`、`Metric`、`JudgmentRule`、`SpecAttribute`、`AppearanceFeature`、`AppearanceFeatureCategory`、`AppearanceFeatureLevel` 和 `ReportConfig` 等节点。这个方向正确，但现在主要还停留在“把表变成节点”的阶段。Palantir 的最佳实践强调 **model reality, not systems**，即对象类型应该代表现实世界实体，而不是源系统或部门视图。[3]

建议补充以下业务对象类型，并让问答优先面向这些对象，而不是直接面向数据库表字段。

| 对象类型 | 当前是否具备 | 建议属性 | 关键关系 |
|---|---:|---|---|
| `InspectionRecord` 检测记录 | 不完整，主要仍在 SQL 表中 | 炉号、批次、检测日期、班次、规格、标注等级、一次交检结果 | `OBSERVED_AS` 指向指标值，`USES_SPEC` 指向规格，`JUDGED_BY` 指向规则 |
| `ProductSpec` 产品规格 | 已具备 | 规格编码、名称、状态、版本、有效期 | `HAS_ATTRIBUTE`、`HAS_RULE`、`HAS_VERSION` |
| `Metric` 指标/公式 | 已具备 | 中文名、列名、单位、公式、来源、口径说明 | `DERIVED_FROM`、`EVALUATED_BY`、`USED_IN_REPORT` |
| `JudgmentRule` 判定规则 | 已具备 | 等级、优先级、质量状态、条件 JSON、适用规格、版本 | `HAS_CONDITION`、`EVALUATES`、`APPLIES_TO` |
| `RuleCondition` 规则条件 | 建议新增 | 字段、操作符、阈值、单位、是否结构化、条件组 | `CONSTRAINS` 指向指标，`PART_OF` 指向规则 |
| `DefectFeature` 外观/缺陷特性 | 部分具备 | 关键词、严重等级、类别、同义词 | `BELONGS_TO`、`HAS_LEVEL`、`MENTIONED_IN` |
| `ReportConfig` 报表配置 | 已具备 | 指标口径、是否表头、是否百分比、排序 | `USES_METRIC`、`GROUPS_LEVELS` |
| `Action` 业务动作 | 未具备 | 调整规则、复核异常、发布规格、模拟阈值 | `MODIFIES` 对象、`TRIGGERED_BY` 问答或用户操作 |

特别建议把 `JudgmentRule.conditionJson` 拆成可查询的 `RuleCondition` 节点或属性集合。当前 `graph_tools.py` 已经在代码中解析条件并输出 `condition` 步骤，但这些条件还没有成为稳定本体对象，导致前端无法天然展示“哪条条件满足、哪条不满足”，也不利于搜索“哪些规则包含脆边阈值”。

### 2.3 把 `RootCauseAgent` 的成功模式推广到全部问答

`RootCauseAgent` 现在已经具备一个非常有价值的模式：**状态优先、流式其次**。它把完整 `reasoning_steps` 写入 state，同时通过自定义事件流式推送步骤。建议将这个模式抽象成公共协议，所有 Agent 都输出相同结构的 `ReasoningStep`，而不是每个 Agent 自己拼文本。

建议的 `ReasoningStep` 结构如下：

```ts
interface ReasoningStep {
  id: string;
  kind: 'intent' | 'entity' | 'path' | 'record' | 'spec' | 'rule' | 'condition' | 'metric' | 'sql' | 'stat' | 'answer' | 'fallback';
  title: string;
  summary: string;
  status: 'pending' | 'running' | 'success' | 'warning' | 'failed';
  ontologyRefs?: Array<{ type: string; id: string; label: string }>;
  edgeRefs?: Array<{ source: string; target: string; relation: string }>;
  evidence?: Array<{ label: string; value: string | number; unit?: string; source?: string }>;
  confidence?: number;
  debug?: Record<string, unknown>;
}
```

这个结构可以同时服务聊天窗口、图谱高亮、右侧证据面板和后端调试。尤其是 `ontologyRefs` 与 `edgeRefs`，能让前端在回答过程中高亮真实图谱路径，而不是展示一张和回答无关的全量图。

### 2.4 统一 KG、Schema Cache 与 Chat2SQL

当前项目中有两套语义层：一套是 Neo4j 知识图谱，另一套是 Chat2SQL 使用的 `schema_loader` 缓存。后者会从 `information_schema` 加载表和列，并构建公式名到事实表列的 glossary。这说明项目已经意识到“表结构也需要语义化”，但这套能力没有纳入 Neo4j 本体。

建议把 schema cache 降级为底层技术索引，把 Ontology 作为上层唯一语义入口。具体做法是：`Metric.columnName` 与事实表字段建立映射；`InspectionRecord` 明确对应事实表；Chat2SQL 生成 SQL 前，必须先通过 Ontology Planner 获取业务对象与允许字段；SQL 执行后，结果再挂回本体证据包。这样可以避免 LLM 直接面对大量表字段，也能让用户问“脆边为什么影响 C 级？”时，系统先找到缺陷/指标/规则，再决定是否需要 SQL 统计。

### 2.5 增加“可追溯证据包”而不是只返回答案文本

问答接口建议不只返回 `response`，还要返回 `answer_card`、`reasoning_steps`、`subgraph` 和 `evidence_table`。这能让前端把回答拆成“结论、证据、路径、数据表、下一步动作”。

| 字段 | 用途 | 前端展示 |
|---|---|---|
| `response` | 自然语言答案 | 聊天文本 |
| `answer_card` | 结构化摘要 | 顶部结论卡片，显示等级、关键原因、置信度 |
| `reasoning_steps` | 过程可解释 | 左侧或聊天中的折叠推理链 |
| `subgraph` | 相关节点和边 | 图谱画布只渲染相关子图 |
| `evidence_table` | 条件/指标/SQL 结果 | 右侧证据表，可排序、可复制 |
| `suggested_actions` | 后续动作 | “查看规则”“模拟阈值”“打开检测记录”等按钮 |

## 三、前端 `/lab/knowledge-graph` 展示改造建议

### 3.1 当前页面为什么“难看”

主 Web 应用的 `/lab/knowledge-graph` 页面使用 `web/src/views/lab/knowledge-graph/index.vue` 实现。它将规格节点画成蓝色矩形、公式节点画成紫色菱形、规则节点画成红/绿圆形、属性节点画成青色小矩形，并使用 G6 的 `gForce` 力导向布局。截图中的问题基本由以下设计导致：

| 现象 | 代码原因 | 用户感知 |
|---|---|---|
| 大量红色圆形堆在中心 | 规则节点 `size: 28` 且数量多，`gForce` 只设置 `preventOverlap` 与固定 `nodeSize: 40` | 看不清规则等级和名称 |
| 紫色公式菱形互相压住文字 | 公式节点标签长、菱形尺寸固定 `[80, 50]` | 公式名称和节点边界冲突 |
| 线条像蜘蛛网 | 全量节点和边一次渲染，边没有按关系分层或透明度控制 | 用户无法理解路径 |
| 筛选粒度太粗 | 只有 `全部/规格/规则/公式` | 无法按规格、等级、缺陷、指标、规则状态探索 |
| 详情面板孤立 | 点击节点只显示 raw 信息 | 无法看到上下游、关联规则、影响记录和问答解释 |

因此，问题不只是“配色不好看”，而是**展示范式不对**：全量力导向图适合技术验证，不适合业务人员理解规则、规格和检测根因。

### 3.2 推荐改成三层界面：总览、探索、推理

建议将页面从单一画布改成三层工作台。默认不展示全量图，而展示可理解的业务总览；用户搜索或问答后，再进入局部图谱与推理路径。

| 层级 | 页面名称 | 主要内容 | 适合场景 |
|---|---|---|---|
| L1 | Ontology Overview | 规格数、规则数、指标数、缺陷数、报表配置数；按规格/等级统计的概览卡片 | 快速理解系统有哪些业务对象 |
| L2 | Ontology Explorer | 左侧对象树，中间局部图谱，右侧对象详情 | 浏览某个规格、指标、规则的上下游 |
| L3 | Reasoning Workspace | 问答输入、推理链、证据表、路径高亮、结论卡片 | 解释“为什么某炉号是 C 级”或“某缺陷影响哪些规则” |

对于 `/lab/knowledge-graph` 的第一版落地，建议保留 Vue + AntV G6，不必马上重写技术栈。只是将数据进入画布前做聚合、分层和子图过滤。

### 3.3 布局从 `gForce` 改成“业务分层 DAG + 局部展开”

当前图谱适合改为从左到右的业务路径：`检测记录/缺陷 → 指标公式 → 判定规则 → 产品规格 → 等级/报表`。如果没有检测记录，就展示 `产品规格 → 规则 → 指标 → 属性` 的本体结构。AntV G6 可以使用 `dagre`、`comboCombined` 或自定义布局；关键不是算法名称，而是让节点按业务层级排列。

| 视图 | 推荐布局 | 节点策略 | 边策略 |
|---|---|---|---|
| 全局总览 | Combo / Radial | 按类型聚合成大节点，不显示所有规则 | 只显示类型间关系和数量 |
| 规格详情 | DAG / CompactBox | 以一个规格为中心展开规则、指标、属性 | 边带关系名与数量 |
| 规则详情 | Flow / Dagre | `规格 → 规则 → 条件 → 指标` | 高亮满足/不满足条件 |
| 问答结果 | Path Highlight | 只显示本次回答相关节点 | 逐步动画播放推理路径 |

更重要的是，在默认视图中不要直接把所有 `rule` 节点铺开。规则应该按 `product_spec_id + formula_id + quality_status` 聚合成 combo，用户点击后再展开明细。这会立刻解决截图中红色规则圆圈拥挤的问题。

### 3.4 节点视觉应改成“业务卡片”而不是几何形状

建议把规则节点从红色小圆改成卡片式节点，展示等级、状态、优先级和条件数量；公式节点从紫色菱形改成指标卡，展示单位、来源和公式类型；规格节点展示编码、名称、属性数量和规则数量。对于长文本，节点内只显示短标题，完整信息进入 tooltip 与右侧详情。

| 节点类型 | 当前展示 | 推荐展示 |
|---|---|---|
| 产品规格 | 蓝色矩形，显示名称 | 蓝色业务卡片：规格编码、名称、属性数、规则数 |
| 判定规则 | 红/绿圆，显示 A/B/C/D 等短文本 | 等级卡片：等级、合格/不合格、优先级、条件数；不满足时红色强调 |
| 指标公式 | 紫色菱形，显示公式名 | 指标卡片：指标名、单位、公式类型、来源 |
| 属性 | 青色小矩形，显示键值 | 默认折叠到规格详情，不在全局画布展示全部属性 |
| 条件 | 当前没有节点 | 条件 pill：字段、操作符、阈值、实际值、满足状态 |

视觉上建议使用更柔和的专业配色：规格蓝 `#2563EB`、指标紫 `#7C3AED`、规则橙 `#F97316`、合格绿 `#16A34A`、不合格红 `#DC2626`、属性青 `#0891B2`。边线使用低饱和灰蓝，并在 hover 或推理播放时提高亮度。背景应保持浅灰网格或纯白，避免强渐变抢占注意力。

### 3.5 增加“问答驱动的图谱”体验

用户真正需要的不是看完整知识图谱，而是回答业务问题。因此 `/lab/knowledge-graph` 应该成为一个问答驱动的图谱工作台。页面顶部搜索框可以升级为自然语言输入框，例如“为什么炉号 1丙20260110-1 是 C 级？”、“规格 142 的脆边规则有哪些？”、“哪些指标影响一次交检合格率？”。

当用户提交问题后，前端接收 SSE 中的 `reasoning_step`，并同步做三件事：第一，聊天/推理链显示当前步骤；第二，画布逐步出现或高亮相关节点与边；第三，右侧证据面板更新条件表和实际值。这样用户看到的不再是静态混乱图，而是“答案在图上逐步生成”。

| 交互阶段 | 画布表现 | 证据面板 | 聊天区 |
|---|---|---|---|
| 识别实体 | 高亮炉号/批次或提示缺失槽位 | 显示解析到的实体 | “识别到炉号…” |
| 定位规格 | 展开检测记录到规格 | 显示规格代码、名称 | “该记录适用规格…” |
| 匹配规则 | 展开规格下规则分组 | 显示候选规则列表 | “命中 C 级规则…” |
| 评估条件 | 条件节点逐个变绿/红 | 显示实际值、阈值、满足状态 | “其中脆边条件不满足…” |
| 给出结论 | 高亮最终等级路径 | 显示结论卡 | “因此判为 C 级…” |

### 3.6 右侧详情面板改为“对象档案 + 上下游 + 动作”

当前详情面板只显示 raw 字段，建议改为对象档案模式。一个节点被点击后，右侧应显示对象摘要、关键属性、上下游关系、相关问答、可执行动作。Palantir Ontology 不只是看对象，还强调 action types 和 functions：action type 定义用户一次性对对象、属性和链接可做的变更，functions 则能接收对象或对象集合并读取属性，在动作和应用中复用。[2]

| 面板区块 | 内容 | 价值 |
|---|---|---|
| 对象摘要 | 名称、类型、状态、版本、更新时间 | 让用户先理解对象是什么 |
| 关键属性 | 单位、阈值、优先级、质量状态 | 避免 raw JSON 压迫感 |
| 上下游 | 父规格、关联规则、评估指标、报表配置 | 支撑业务探索 |
| 证据与历史 | 最近命中记录、变更历史、版本说明 | 支撑信任与审计 |
| 动作 | 查看规则、模拟阈值、发起复核、跳转报表 | 从“看图”进入“运营” |

## 四、建议的后端接口与前端数据结构

为了支撑漂亮且有用的前端，后端应新增局部子图接口，而不是让前端一次获取全部 `specs/rules/formulas` 后自己拼图。

| 接口 | 输入 | 输出 | 说明 |
|---|---|---|---|
| `GET /api/v1/kg/ontology/summary` | 无 | 类型统计、质量统计、更新状态 | 首页概览卡片 |
| `GET /api/v1/kg/subgraph` | `anchor_type`、`anchor_id`、`depth`、`relation_filter` | nodes、edges、combos | 局部图谱，避免全量混乱 |
| `POST /api/v1/kg/resolve` | 自然语言短语 | 对象候选列表 | 搜索框自动补全和实体解析 |
| `POST /api/v1/kg/explain` | 问题、上下文 | answer、steps、subgraph、evidence | 问答驱动图谱 |
| `GET /api/v1/kg/rules/{id}/conditions` | rule_id | 结构化条件 | 条件节点与证据表 |
| `POST /api/v1/kg/simulate` | rule_id、metric_values | 模拟结果 | 后续可做阈值模拟动作 |

前端图谱数据结构建议统一为 `OntologyGraphDTO`，不要在 Vue 组件里直接把 `OntologyData` 拼成 G6 节点。这样有利于后端控制聚合、权限、命名、颜色和高亮语义。

```ts
interface OntologyGraphDTO {
  nodes: Array<{
    id: string;
    type: 'InspectionRecord' | 'ProductSpec' | 'Metric' | 'JudgmentRule' | 'RuleCondition' | 'DefectFeature' | 'ReportConfig';
    label: string;
    subtitle?: string;
    status?: 'ok' | 'warning' | 'error' | 'unknown';
    metrics?: Record<string, string | number>;
    badges?: string[];
    raw?: Record<string, unknown>;
  }>;
  edges: Array<{
    id: string;
    source: string;
    target: string;
    relation: string;
    label?: string;
    status?: 'active' | 'muted' | 'failed';
  }>;
  combos?: Array<{ id: string; label: string; type: string; collapsed?: boolean }>;
  highlights?: { nodeIds: string[]; edgeIds: string[] };
}
```

## 五、落地路线图

### 5.1 一周内可做的 UI 快速优化

第一阶段不需要大改后端，只改 `/lab/knowledge-graph/index.vue` 的图谱渲染策略即可明显改善观感。建议先做四件事：默认按规格筛选，不进入全量图；规则节点按规格或公式聚合；属性节点默认不进入画布，只进入详情面板；布局从 `gForce` 改为分层布局。这样可以快速解决截图中最严重的重叠问题。

| 优先级 | 改动 | 预计效果 |
|---|---|---|
| P0 | 默认不显示全部属性节点，属性进入右侧详情 | 节点数量减少，画布立刻清爽 |
| P0 | 规则按规格/公式 combo 聚合，点击展开 | 红色规则节点不再堆叠 |
| P0 | 使用 dagre 分层布局替代全局 gForce | 关系方向更清晰 |
| P1 | 节点 label 截断 + tooltip + 右侧完整信息 | 长文本不再压住节点 |
| P1 | 搜索命中后只展示相关一跳/两跳子图 | 搜索变成“定位”而不是“过滤后仍混乱” |
| P1 | 边标签默认隐藏，hover 时显示 | 减少蜘蛛网文字噪音 |

### 5.2 两到四周内完成的问答闭环

第二阶段把 `RootCauseAgent` 的推理步骤与图谱页面打通。后端在回答中返回 `subgraph` 和 `ontologyRefs`，前端根据 SSE 步骤高亮节点。此阶段的目标不是覆盖所有问题，而是把“为什么某炉号是某等级”打磨成标杆体验。

| 模块 | 任务 | 验收标准 |
|---|---|---|
| 后端 | `ReasoningStep` 结构统一 | `RootCauseAgent` 与 Chat2SQL 都输出同协议 |
| 后端 | `traverse_judgment_path` 返回相关节点/边 ID | 前端能高亮真实图谱路径 |
| 前端 | 增加问答输入与推理链面板 | 提问后可逐步看到 record/spec/rule/condition/grade |
| 前端 | 增加条件证据表 | 条件、实际值、期望值、满足状态可读 |
| 产品 | 固化 5 个样例问题 | 每个问题都能返回可追溯答案 |

### 5.3 一到两个月内演进为 Palantir-like Ontology

第三阶段重点是本体治理与动作化。把规则条件、检测记录、缺陷特性、报表配置、规格版本纳入统一对象模型，并增加对象级动作，例如规则调整、规格版本发布、异常复核和阈值模拟。Palantir 的最佳实践提醒，应根据任务选择合适工具：人工或 Agent 决策适合 action types，自动化转换适合 pipelines，复杂实时逻辑适合 functions。[3]

| 能力 | 实现内容 | 业务价值 |
|---|---|---|
| 本体治理 | 对象类型、属性、关系、命名、版本文档化 | 避免图谱变成不可维护的表复制 |
| 条件对象化 | `RuleCondition` 成为可检索对象 | 支持“哪些规则包含某阈值”的问答 |
| 检测记录对象化 | `InspectionRecord` 与指标值、规格、规则连接 | 支持炉号级根因解释和统计分析 |
| 动作化 | 模拟阈值、复核异常、调整规则 | 从问答走向运营闭环 |
| 权限与审计 | 规则变更、数据来源、答案证据留痕 | 支撑生产环境可信使用 |

## 六、推荐的 MVP 用户故事

为了避免范围过大，建议先围绕生产驾驶舱中最容易产生价值的问答场景做 MVP。用户已经明确希望“基于知识图谱的问答”，因此 MVP 应先验证知识图谱优先、统计分析其次的闭环。

| 用户问题 | 系统应做什么 | 展示方式 |
|---|---|---|
| “为什么炉号 X 是 C 级？” | 解析炉号 → 找检测记录 → 找规格 → 找规则 → 评估条件 → 输出根因 | 结论卡 + 推理链 + 高亮路径 + 条件表 |
| “规格 142 有哪些不合格规则？” | 定位规格 → 展开规则 → 按等级/公式聚合 | 局部图谱 + 规则列表 |
| “脆边影响哪些判定？” | 解析缺陷/指标同义词 → 搜索规则条件与公式 → 返回相关规格和等级 | 缺陷中心网络 + 影响面表格 |
| “最近一周 C 级主要原因是什么？” | 先用 KG 定义 C 级规则与条件，再用 SQL 统计命中分布 | Pareto 图 + 根因表 + 规则解释 |
| “如果把某阈值放宽会怎样？” | 读取规则条件 → 调用模拟函数 → 返回等级变化统计 | 模拟结果卡 + 影响记录列表 |

## 七、给代码层面的直接建议

对 `web/src/views/lab/knowledge-graph/index.vue`，建议短期重构为三个文件：`graphTransform.ts` 负责把后端 DTO 转成 G6 数据；`KnowledgeGraphCanvas.vue` 负责画布；`KnowledgeGraphDetail.vue` 负责右侧详情。当前组件同时负责 toolbar、数据转换、G6 初始化、事件处理、详情面板和样式，后续很难继续演进。

| 文件 | 职责 |
|---|---|
| `index.vue` | 页面布局、状态协调、API 调用 |
| `components/KgToolbar.vue` | 搜索、过滤、重建、视图切换 |
| `components/KgCanvas.vue` | G6 初始化、布局、交互事件 |
| `components/KgDetailPanel.vue` | 对象档案、上下游、动作 |
| `components/KgReasoningPanel.vue` | SSE 推理步骤与证据表 |
| `utils/graphTransform.ts` | 聚合、截断、颜色、combo、边过滤 |
| `types/ontology.ts` | 统一 DTO 与节点类型 |

对 `nlq-agent`，建议把 `traverse_judgment_path` 继续作为标杆工具，但不要让它只服务 `RootCauseAgent`。可以增加 `explain_rule_path`、`find_impacted_rules`、`summarize_grade_causes` 三类图谱工具，并统一返回 `ReasoningStep + Evidence + Subgraph`。这样前端不用理解每个 Agent 的内部差异。

## 八、最终建议

如果只改 UI，页面会变漂亮，但问答价值有限；如果只改 Agent，用户仍然会觉得图谱不可理解。最佳路线是同时推进：**后端把问答输出改成结构化推理证据，前端把全量图改成局部图谱与推理工作台**。Palantir Ontology 给 `lm` 的启发不是“画一个更大的图”，而是把生产检测领域中的规格、指标、规则、缺陷、记录、报表和动作统一成可查询、可解释、可操作的业务对象层。

优先级上，我建议先做“炉号判级根因解释”这一条黄金路径。因为项目中 `RootCauseAgent` 和 `traverse_judgment_path` 已经具备基础，前端只需要把 `reasoning_steps` 与局部图谱打通，就能做出明显区别于普通 Chat2SQL 的体验。一旦这条路径打磨好，再扩展到规格规则查询、缺陷影响面分析、周/月统计根因和阈值模拟，就会自然形成 `lm` 自己的生产质量 Ontology。

## References

[1]: https://palantir.com/docs/foundry/ontology/overview/ "Palantir Documentation: Ontology building overview"
[2]: https://palantir.com/docs/foundry/ontology/core-concepts/ "Palantir Documentation: Ontology core concepts"
[3]: https://palantir.com/docs/foundry/ontology/ontology-best-practices-and-anti-patterns/ "Palantir Documentation: Ontology design best practices and anti-patterns"
[4]: https://palantir.com/docs/foundry/ontology/ontology-augmented-generation/ "Palantir Documentation: Ontology-augmented generation"
