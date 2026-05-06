# 知识图谱使用文档

## 概述

NLQ-Agent 使用 Neo4j 作为知识图谱后端，存储和管理实验室数据的元数据关系。

## 数据模型

### 实体类型

| 实体类型 | 说明 | 来源表 |
|---------|------|--------|
| ProductSpec | 产品规格 | lab_product_spec |
| Metric | 指标公式 | lab_intermediate_data_formula |
| JudgmentRule | 判定规则 | lab_intermediate_data_judgment_level |
| SpecAttribute | 规格扩展属性 | lab_product_spec_attribute |
| ReportConfig | 报表配置 | lab_report_config |

### 关系类型

| 关系类型 | 说明 | 示例 |
|---------|------|------|
| HAS_RULE | 规格拥有判定规则 | (ProductSpec)-[:HAS_RULE]->(JudgmentRule) |
| EVALUATES | 规则评估指标 | (JudgmentRule)-[:EVALUATES]->(Metric) |
| HAS_ATTRIBUTE | 规格有扩展属性 | (ProductSpec)-[:HAS_ATTRIBUTE {value: "1000"}]->(SpecAttribute) |

## 配置

### 环境变量

```bash
# .env 文件
NEO4J_URI=bolt://localhost:7687
NEO4J_USER=neo4j
NEO4J_PASSWORD=password
NEO4J_ENABLED=true  # 启用知识图谱
```

### Docker 部署 Neo4j

```bash
docker run -d \
  --name neo4j \
  -p 7474:7474 -p 7687:7687 \
  -e NEO4J_AUTH=neo4j/password \
  -v neo4j_data:/data \
  neo4j:5.27
```

## API 接口

### 健康检查
```bash
GET /api/v1/kg/health
```

### 刷新知识图谱
```bash
POST /api/v1/kg/refresh
```

### 获取所有产品规格
```bash
GET /api/v1/kg/specs
```

### 获取规格详情
```bash
GET /api/v1/kg/specs/{spec_code}
```

### 获取规格判定规则
```bash
GET /api/v1/kg/specs/{spec_code}/rules
```

### 获取所有指标
```bash
GET /api/v1/kg/metrics
```

### 获取指标详情
```bash
GET /api/v1/kg/metrics/{metric_name}
```

### 获取一次交检配置
```bash
GET /api/v1/kg/first-inspection/config
```

### 搜索判定规则
```bash
GET /api/v1/kg/rules/search?keyword=带厚
```

## 使用示例

### 查询产品规格的判定规则

```python
from app.knowledge_graph.manager import get_knowledge_graph
from app.knowledge_graph.queries import get_spec_judgment_rules

graph = get_knowledge_graph()
rules = await get_spec_judgment_rules(graph, "120")
print(rules)
```

### 查询规格相关的所有指标

```python
from app.knowledge_graph.queries import get_related_metrics_by_spec

metrics = await get_related_metrics_by_spec(graph, "142")
print(metrics)
```

## 维护

### 每日刷新

知识图谱应在低峰期每日刷新一次：

```bash
curl -X POST http://localhost:8000/api/v1/kg/refresh
```

### 监控

- 监控 Neo4j 内存使用
- 检查图谱构建日志
- 验证数据一致性

## 故障排查

### 知识图谱未初始化

检查日志中的错误信息，确认 Neo4j 连接配置正确。

### 数据不一致

执行刷新操作重建图谱：
```bash
POST /api/v1/kg/refresh
```
