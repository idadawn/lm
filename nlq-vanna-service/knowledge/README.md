# knowledge/ 业务方编辑指南

本目录存放注入 Qdrant 向量知识库的业务内容文件。
启动时由 `app/knowledge/bootstrap.py` 读取并写入向量库。

---

## 文件结构

```
knowledge/
├── README.md             本指南
├── ddl.md                可选：为自动 DDL 补充人工注释
├── terminology.md        业务术语定义（每条 ## 标题 + 说明）
├── judgment_rules.md     判级规则 + F_CONDITION JSON Schema 说明
└── qa_examples.yaml      JSON_EXTRACT few-shot 示例（{question, sql}）
```

---

## 何时需要 bump KNOWLEDGE_VERSION

修改以下任意文件后，**必须** 在 `.env` 或环境变量中更新 `KNOWLEDGE_VERSION`（例如从 `v1` 改为 `v2`），
以触发下次启动时的自动重索引：

- `terminology.md`
- `judgment_rules.md`
- `qa_examples.yaml`
- `ddl.md`（若有内容变更）

**不 bump 版本时**，Qdrant 中的旧向量不会被更新，新内容不生效。

```bash
# .env 示例
KNOWLEDGE_VERSION=v2
```

也可以用 CLI 强制重索引（跳过版本检查）：

```bash
python -m app.cli reindex
```

---

## 术语文件写法约定（terminology.md）

每条术语用二级标题 `## 术语名` 开头，后接字段名 + 一句话说明。例如：

```markdown
## 叠片系数

字段：F_LAMINATION_COEFFICIENT（LAB_INTERMEDIATE_DATA 表）
达标阈值：≥ 0.95
说明：硅钢片叠压后的填充率指标，反映铁芯加工质量。
```

bootstrap 会按 `\n\n## ` 分段，每段作为独立向量点存入 Qdrant。
**分段颗粒度越细，检索精度越高**——建议每条术语单独一个 `## ` 段。

---

## 判级规则文件写法约定（judgment_rules.md）

参见文件内 `## F_CONDITION JSON Schema 字段说明` 子段。
每条规则用 `## ` 二级标题分隔，写清楚条件字段、比较符和阈值。

---

## QA 示例文件写法约定（qa_examples.yaml）

每条示例必须包含 `question` 和 `sql` 两个字段：

```yaml
- question: "120 规格 Ps 铁损超标的炉号有哪些？"
  sql: >
    SELECT ...
```

建议覆盖 5 种 JSON_EXTRACT 模式（见文件内注释），帮助模型学习如何查询 F_CONDITION 字段。
