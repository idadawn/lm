# 知识图谱功能验证指南

本文档介绍如何验证知识图谱功能。

## 📋 功能概览

知识图谱模块已实现以下功能：

### 后端 API (services/agent-api/app/api/kg.py)

| 端点 | 方法 | 描述 |
|------|------|------|
| `/api/v1/kg/health` | GET | 检查知识图谱健康状态 |
| `/api/v1/kg/refresh` | POST | 刷新知识图谱数据 |
| `/api/v1/kg/specs` | GET | 获取所有产品规格 |
| `/api/v1/kg/specs/{code}` | GET | 获取产品规格详情 |
| `/api/v1/kg/specs/{code}/rules` | GET | 获取产品规格的判定规则 |
| `/api/v1/kg/metrics` | GET | 获取所有指标公式 |
| `/api/v1/kg/metrics/{name}` | GET | 获取指标详情 |
| `/api/v1/kg/first-inspection/config` | GET | 获取一次交检配置 |
| `/api/v1/kg/rules/search` | GET | 根据关键词搜索判定规则 |

### 前端页面 (apps/web/app/kg/page.tsx)

| 标签页 | 功能 |
|--------|------|
| 概览 | 知识图谱概览和快速入口 |
| 产品规格 | 浏览所有产品规格及其属性、判定类型 |
| 指标公式 | 查看所有指标的计算公式和单位 |
| 判定规则 | 按规格或关键词搜索判定规则 |
| 报表配置 | 查看一次交检合格率配置 |

## 🚀 快速开始

### 1. 启动后端服务

```bash
cd services/agent-api

# 确保环境变量已配置（Neo4j 连接信息）
# .env 文件应包含:
# NEO4J_URI=bolt://localhost:7687
# NEO4J_USER=neo4j
# NEO4J_PASSWORD=Ds@20256

# 启动 API 服务
uv run uvicorn app.main:app --reload --port 8000
```

### 2. 启动前端服务

```bash
# 在新终端窗口
pnpm --filter web dev
```

### 3. 访问知识图谱浏览器

打开浏览器访问: http://localhost:3000/kg

## 🧪 自动化测试

运行测试脚本验证所有 API 端点:

```bash
cd services/agent-api
uv run python scripts/test_kg_api.py
```

测试脚本会执行以下操作:
1. ✅ 检查知识图谱健康状态
2. ✅ 刷新知识图谱数据（如需要）
3. ✅ 获取产品规格列表
4. ✅ 获取产品规格详情
5. ✅ 获取判定规则
6. ✅ 获取指标公式
7. ✅ 获取报表配置
8. ✅ 搜索判定规则

## 📊 数据结构

### 产品规格 (ProductSpec)

```typescript
{
  id: string;           // 规格ID
  code: string;         // 规格代码 (如 "120")
  name: string;         // 规格名称
  attributes: [         // 扩展属性
    {
      name: string;     // 属性名
      value: string;    // 属性值
      dataType: string; // 数据类型
    }
  ];
}
```

### 指标公式 (MetricFormula)

```typescript
{
  id: string;           // 指标ID
  name: string;         // 指标名称
  columnName: string;   // 数据库列名
  formula: string;      // 计算公式
  unit: string;         // 单位
  formulaType: string;  // 公式类型 (CALC/JUDGE/NO)
  description?: string; // 描述
}
```

### 判定规则 (JudgmentRule)

```typescript
{
  id: string;           // 规则ID
  formulaId: string;    // 公式ID
  name: string;         // 规则名称
  priority: number;     // 优先级
  qualityStatus: string;// 质量状态
  color: string;        // 颜色标识
  isDefault: boolean;   // 是否默认
  conditionJson?: string; // 条件JSON
  spec_code?: string;   // 产品规格代码
  spec_name?: string;   // 产品规格名称
}
```

## 🔍 使用示例

### 示例 1: 查看产品规格详情

1. 访问 http://localhost:3000/kg
2. 点击"产品规格"标签页
3. 在左侧列表选择一个规格（如 "120"）
4. 右侧显示该规格的属性和判定类型

### 示例 2: 搜索判定规则

1. 访问 http://localhost:3000/kg
2. 点击"判定规则"标签页
3. 在搜索框输入关键词（如 "带厚"）
4. 点击"搜索"按钮查看相关规则

### 示例 3: 查看指标公式

1. 访问 http://localhost:3000/kg
2. 点击"指标公式"标签页
3. 浏览所有指标的计算公式和单位

## 🛠️ 故障排查

### 问题 1: 知识图谱未初始化

**症状**: 页面显示 "● 未初始化"

**解决方案**:
1. 确认 Neo4j 服务正在运行:
   ```bash
   # 检查 Neo4j 进程
   ps aux | grep neo4j

   # 或访问 Neo4j 浏览器
   http://localhost:7474
   ```

2. 确认环境变量配置正确:
   ```bash
   # 检查 .env 文件
   cat .env | grep NEO4J
   ```

3. 手动刷新知识图谱:
   - 点击页面右上角"刷新图谱"按钮
   - 或调用 API: `POST /api/v1/kg/refresh`

### 问题 2: 无法连接到 API

**症状**: 前端页面显示连接错误

**解决方案**:
1. 确认后端服务正在运行:
   ```bash
   curl http://localhost:8000/api/v1/kg/health
   ```

2. 检查 CORS 配置

3. 查看后端日志排查错误

### 问题 3: 数据为空

**症状**: API 返回空数组

**解决方案**:
1. 确认 MySQL 数据库有数据
2. 检查知识图谱构建日志
3. 重新刷新知识图谱

## 📝 注意事项

1. **首次初始化**: 首次启动时会自动从 MySQL 构建知识图谱，可能需要几秒钟
2. **数据刷新**: 修改 MySQL 数据后需要手动刷新知识图谱
3. **性能**: 大量数据时查询可能较慢，建议添加索引
4. **安全性**: 生产环境应配置认证和授权

## 🔗 相关文件

- 后端 API: `services/agent-api/app/api/kg.py`
- 知识图谱查询: `services/agent-api/app/knowledge_graph/queries.py`
- Neo4j 实现: `services/agent-api/app/knowledge_graph/neo4j_graph.py`
- 前端页面: `apps/web/app/kg/page.tsx`
- 类型定义: `packages/shared-types/src/knowledge-graph.ts`
- 测试脚本: `services/agent-api/scripts/test_kg_api.py`

## 📚 下一步

- [ ] 添加知识图谱可视化（使用 Cytoscape.js 或 D3.js）
- [ ] 实现图谱搜索和路径查询
- [ ] 添加数据导出功能
- [ ] 优化查询性能
- [ ] 添加单元测试和集成测试
