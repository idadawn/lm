# LightRAG 检索增强集成文档

> 状态：v1.0 集成完成 · 最后更新 2026-05-17
> 责任人：nlq-agent / knowledge_graph 维护者
> 适用版本：nlq-agent ≥ 0.3.x · lightrag-hku ≥ 1.4.x

---

## 1. 这是什么 / 为什么需要

### 痛点
- 用户问「炉号是怎么组成的」，nlq-agent 之前没有任何方式从 KG 里取出图谱里那张"FurnaceNoParsed → hasComponent × 8"的拓扑，只能让 LLM 拍脑袋编。
- 用户问「上月各班组质量分布」，chat2sql 写 SQL 时不知道 `F_LABELING` 和 `F_FIRST_INSPECTION` 的区别，LLM 把 A 类卷重当作"一次交检合格率"返回。
- 已有的 `knowledge_base.json` 是手工补丁，不可持续。

### 解决思路
LightRAG（HKUDS · EMNLP 2025）作为**检索增强层**叠在 nlq-agent 现有路由前面：

```
用户问题 → LightRAG 检索（语义召回 KG 节点 / 配置 / 文档）
   ├─ 高置信度（≥0.85）：直接返回答案 + citation
   ├─ 中等置信度：把召回片段注入下游 agent（chat2sql / query_agent）的 prompt
   └─ 无召回：走原有路由
```

LightRAG 内部用本地 BGE 中文 embedding，**不依赖外网**，索引数据存本地 JSON + NanoVectorDB。

---

## 2. 核心组件

| 组件 | 路径 | 作用 |
|---|---|---|
| LightRAG 管理器 | `app/knowledge_graph/lightrag_index.py` | 单例 + insert / query 入口 + citation 解析 |
| 知识源收集器 | `app/knowledge_graph/knowledge_sources.py` | 8 个 collector：把 Neo4j / JSON / DB / docs 序列化成 doc 片段 |
| 智能查找 | `app/knowledge_graph/kb_lookup.py` | `lookup_kb_smart()` 走 LightRAG，失败降级到静态 KB |
| 路由集成 | `app/agents/query_agent.py` 顶部 + `app/agents/chat2sql_agent.py` step3 | 顶层 KB lookup + 下游 prompt 注入 |
| 重建脚本 | `scripts/lightrag_reindex.py` | 手动重建索引 |
| SLA 评估 | `scripts/lightrag_eval.py` | 10 个客户验收用例的回归测试 |
| 部署 | `Dockerfile.dev` + `docker-compose.lightrag.yml` | 预下载模型 + 卷持久化 |

---

## 3. 配置项（`.env` / `settings`）

| 变量 | 默认 | 说明 |
|---|---|---|
| `LIGHTRAG_ENABLED` | `false` | 总开关，false 时所有功能降级到静态 KB |
| `LIGHTRAG_WORKING_DIR` | `<repo>/data/lightrag` | 索引数据存放目录（容器里挂卷） |
| `LIGHTRAG_EMBEDDING_MODEL` | `BAAI/bge-small-zh-v1.5` | 本地 embedding 模型；可换 BGE-large、E5、OpenAI 等 |
| `LIGHTRAG_EMBEDDING_DIM` | `512` | 必须匹配模型实际维度 |
| `LIGHTRAG_EMBEDDING_DEVICE` | `cpu` | 有 GPU 改 `cuda` |
| `LIGHTRAG_CONFIDENCE_THRESHOLD` | `0.85` | ≥此值直接答；低于走原路由 |
| `LIGHTRAG_CONTEXT_TOP_K` | `5` | 注入下游 prompt 的片段数 |

---

## 4. 知识源（46 docs，分 8 类）

| 类别 | 来源 | doc 数量 | 文件 |
|---|---|---|---|
| `data_format` | knowledge_base.json | 1 | 炉号格式 |
| `metric_definition` | knowledge_base.json + lab_report_config | 2+N | 一次交检合格率、质量等级分布等 |
| `field_distinction` | knowledge_base.json | 1 | F_LABELING vs F_FIRST_INSPECTION |
| `dimension` | dimensions_meta.json | 5 | 时间/班次/产线/规格/炉号 |
| `term`/`term_alias` | knowledge_base.json + aliases.json | 4+3 | 卷重、批次号、外观特性、术语词典 |
| `sql_template` | sql_templates.json | 6 | 单指标聚合 / 占比 / 班次对比 / 日趋势 / 不合格分类 / 明细 |
| `ontology_node` | Neo4j (启用时) | 数十~数百 | 全部白名单节点 |
| `documentation` | docs/*.md | 22 | architecture / development guide / ontology |

**变更触发**：
- 修改 JSON 字典：`uv run python scripts/lightrag_reindex.py --only knowledge_base`
- Neo4j refresh：上游 `manager.refresh_knowledge_graph()` 完成后调 reindex
- lab_report_config 改了配置：业务后台 webhook 触发 reindex

---

## 5. 部署 SOP

### 5.1 开发环境（本地）

```bash
cd nlq-agent/services/agent-api
uv sync                       # 装 lightrag-hku + sentence-transformers + 依赖
export LIGHTRAG_ENABLED=true
uv run python scripts/lightrag_poc.py        # ① 验证 LightRAG 起得来
uv run python scripts/lightrag_reindex.py --full   # ② 全量索引（首次 ~10-30 分钟，看文档数）
uv run python scripts/lightrag_eval.py       # ③ 跑 10 个 SLA 用例验收
uv run uvicorn app.main:app --reload --port 8000   # ④ 启动服务
```

### 5.2 生产环境（Docker）

```bash
cd nlq-agent/services/agent-api
# 主 docker-compose.yml 之外，叠 lightrag override
docker compose -f docker-compose.yml -f docker-compose.lightrag.yml up -d

# 首次启动后手动跑一次全量索引（容器内）
docker compose exec nlq-agent uv run python scripts/lightrag_reindex.py --full

# 跑 SLA 回归
docker compose exec nlq-agent uv run python scripts/lightrag_eval.py
```

**关键点**：
- `lightrag_data` 卷持久化索引数据（容器删了重建也保留）
- `hf_cache` 卷缓存 BGE 模型（避免重复下载）
- 镜像构建时已经预拉了 BGE 模型，离线环境也能跑

### 5.3 CI 集成

```yaml
# .github/workflows/lightrag-eval.yml
- name: LightRAG SLA Regression
  run: |
    docker compose exec -T nlq-agent uv run python scripts/lightrag_eval.py \
      --json eval-report.json \
      --threshold 0.8
- name: Upload eval report
  uses: actions/upload-artifact@v3
  with:
    name: lightrag-eval-report
    path: eval-report.json
```

---

## 6. 客户验收标准（10 个 SLA case）

| # | 问题 | 期望 | 通过线 |
|---|---|---|---|
| 1 | 炉号是怎么组成的 | 8 段结构 + 数据库字段 + 示例 | conf ≥ 0.55，含「产线/班次/生产日期/炉次号/卷号」 |
| 2 | 一次交检合格率怎么算 | 公式 + F_FIRST_INSPECTION + F_SINGLE_COIL_WEIGHT | conf ≥ 0.55，**不含** F_LABELING |
| 3 | F_LABELING 和 F_FIRST_INSPECTION 区别 | 两列对照表 + 用途差异 | conf ≥ 0.55 |
| 4 | 班次有哪些 | 甲乙丙 + 数字代号 + 时段 | conf ≥ 0.50 |
| 5 | 检测中心有几条生产线 | 4 条 + 产线号 | conf ≥ 0.50 |
| 6 | 质量等级分布是怎么算的 | F_LABELING 分组 + **不含 F_FIRST_INSPECTION** | conf ≥ 0.55 |
| 7 | 批次号是什么 | F_FURNACE_BATCH_NO + 区别炉号 | conf ≥ 0.50 |
| 8 | 外观特性有哪些 | 脆/划/麻点... 列表 | conf ≥ 0.50 |
| 9 | 卷重是什么字段 | F_SINGLE_COIL_WEIGHT | conf ≥ 0.45 |
| 10 | 本月有什么新闻头条（无关问题） | **必须 disclaimer**（"我没有足够的信息"），不能瞎编 | 含 disclaimer |

跑回归：`uv run python scripts/lightrag_eval.py --verbose`

---

## 7. 监控 & 故障排查

### 7.1 常用指标
- 单次 query 延迟（embed + LLM 综合）：典型 300-800ms
- 索引大小：46 docs 约 5-10MB（`data/lightrag/vdb_*.json`）
- BGE 模型加载常驻内存：~400MB

### 7.2 故障树

| 症状 | 排查 |
|---|---|
| `get_lightrag()` 返回 None | 看启动日志「LightRAG initialization failed」；确认 LIGHTRAG_ENABLED=true |
| 索引为空（query 返回 disclaimer） | 跑 `scripts/lightrag_reindex.py --full` |
| BGE 模型加载失败 | 检查 `HF_HOME` 路径是否可写；离线环境确认镜像预下载已完成 |
| query 超慢（>5s） | 看是否每次都在重 init 单例；确认 `_instance` 缓存生效 |
| 答案幻觉 / 引用错误字段 | 调高 `LIGHTRAG_CONFIDENCE_THRESHOLD`；或往 knowledge_base.json 加更精确的条目（手工覆盖） |
| SLA case fail | 看 `--verbose` 输出，确认是知识缺失还是 prompt 问题；缺失就写新 doc reindex；prompt 问题改 chat2sql_agent.py 的 step3 prompt |

### 7.3 降级路径
- `LIGHTRAG_ENABLED=false` → 完全关掉 LightRAG，nlq-agent 退化到静态 knowledge_base.json + 原有路由
- LightRAG 初始化失败 → 自动设 `_init_failed=True`，后续不再尝试，所有请求走原路由
- 单次 query 异常 → 返回空 result，调用方 fallback 到下一级路由

---

## 8. 后续工作

- [ ] **citation 解析**：LightRAG context 输出格式因 mode 不同而异，目前只覆盖 JSON / CSV 块，复杂场景可能丢 source；可考虑 fork LightRAG 加 metadata 直传接口
- [ ] **rerank 模型**：当前未启用 reranker（PoC 跑出来有 warning），加 bge-reranker-base 可进一步提升 top-K 准确率
- [ ] **增量索引**：现在 reindex 是全量，下次可以基于 `doc.id` 做去重，只更新变化的部分
- [ ] **citation 前端展示**：response_formatter 把 `citations` 单独作为 SSE 事件推给前端，UI 在答案旁边显示"📎 来源"按钮
- [ ] **GPU 加速**：BGE 改 `device=cuda` 后 embedding 速度提升 10×，索引时间从 30min 降到 3min

---

## 9. 风险登记

| 风险 | 级别 | 缓解 |
|---|---|---|
| LightRAG 自动抽取出错误关系（如把 F_LABELING 和 F_FIRST_INSPECTION 当同义词） | 中 | 在 knowledge_base.json 加权威条目覆盖；SLA case 6/2 专门防这个 |
| 业务后台改了 lab_report_config 但没触发 reindex | 高 | 加 cron 每天凌晨 rebuild；或加业务后台 webhook |
| 索引太大导致 query 慢 | 低 | 目前 46 docs 没问题；超过 1000 docs 时考虑切换 PostgreSQL pgvector |
| BGE 模型版本变更导致已有 embedding 失效 | 低 | 升级模型时必须 rebuild；模型版本在镜像里 pin 死 |

---

## 10. 参考

- LightRAG 论文：[arXiv:2410.05779](https://arxiv.org/abs/2410.05779)
- LightRAG 仓库：[HKUDS/LightRAG](https://github.com/HKUDS/LightRAG)
- BGE 模型卡片：[BAAI/bge-small-zh-v1.5](https://huggingface.co/BAAI/bge-small-zh-v1.5)
- 设计讨论：本仓库 `chat/2026-05-17-lightrag-integration` session
