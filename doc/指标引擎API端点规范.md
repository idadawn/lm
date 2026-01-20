# 指标引擎API端点规范

## ⚠️ 重要说明：Indicator 和 Metric 整合

**更新日期**：2026-01-27

Indicator 和 Metric 系统已整合，统一使用 **Metric** 系统。所有 API 端点均使用 Metric 命名。

- ✅ 统一使用 `/api/lab/metric-definitions` 端点
- ✅ 统一使用 `MetricDefinition` 相关 DTO
- ✅ Indicator 相关 API 已删除

## 概述
本文档定义了指标引擎系统的RESTful API端点规范。遵循项目现有的API设计模式和命名约定。

**注意**：本文档中如有提到 Indicator，均指 Metric 系统。

## 基础信息
- **基础路径**: `/api/lab`
- **认证**: JWT Bearer Token
- **版本**: v1
- **内容类型**: application/json

## 指标定义管理 (Metric Definition)

### 1. 获取指标定义列表
获取指标定义的分页列表，支持搜索和过滤。

**端点**: `GET /api/lab/metric-definitions`

**请求参数** (Query Parameters):
```typescript
interface MetricDefinitionQuery {
  keyword?: string;          // 关键词搜索（名称、代码、描述）
  category?: string;         // 分类筛选
  status?: number;          // 状态筛选（1-启用，0-禁用）
  creatorTimeStart?: string; // 创建时间开始
  creatorTimeEnd?: string;   // 创建时间结束
  currentPage?: number;     // 当前页码，默认1
  pageSize?: number;       // 每页条数，默认50
  sidx?: string;           // 排序字段
  sort?: string;           // 排序方式（asc/desc）
}
```

**响应**:
```typescript
interface MetricDefinitionListResponse {
  total: number;           // 总记录数
  list: MetricDefinitionDto[]; // 指标定义列表
}
```

**示例**:
```bash
GET /api/lab/metric-definitions?category=物理性能&status=1&currentPage=1&pageSize=20
```

### 2. 获取指标定义详情
获取指定ID的指标定义详情。

**端点**: `GET /api/lab/metric-definitions/{id}`

**路径参数**:
- `id`: 指标定义ID

**响应**: `MetricDefinitionDto`

**示例**:
```bash
GET /api/lab/metric-definitions/1234567890abcdef
```

### 3. 创建指标定义
创建新的指标定义。

**端点**: `POST /api/lab/metric-definitions`

**请求体**: `MetricDefinitionInput`

**响应**: `MetricDefinitionDto` (包含生成的ID)

**示例**:
```json
{
  "name": "抗拉强度",
  "code": "TS",
  "description": "材料的抗拉强度指标",
  "formula": "force / area",
  "unitId": "kgf_mm2",
  "category": "机械性能",
  "status": 1,
  "sortOrder": 10,
  "remark": "重要质量指标"
}
```

### 4. 更新指标定义
更新指定ID的指标定义。

**端点**: `PUT /api/lab/metric-definitions/{id}`

**路径参数**:
- `id`: 指标定义ID

**请求体**: `MetricDefinitionInput`

**响应**: `MetricDefinitionDto`

**示例**:
```bash
PUT /api/lab/metric-definitions/1234567890abcdef
```

### 5. 删除指标定义
删除指定ID的指标定义。

**端点**: `DELETE /api/lab/metric-definitions/{id}`

**路径参数**:
- `id`: 指标定义ID

**响应**: 无内容（HTTP 204）

**示例**:
```bash
DELETE /api/lab/metric-definitions/1234567890abcdef
```

### 6. 更新指标定义状态
更新指标定义的启用/禁用状态。

**端点**: `PUT /api/lab/metric-definitions/{id}/status`

**路径参数**:
- `id`: 指标定义ID

**请求体**:
```typescript
interface UpdateStatusInput {
  status: number; // 1-启用，0-禁用
}
```

**响应**: `MetricDefinitionDto`

**示例**:
```bash
PUT /api/lab/metric-definitions/1234567890abcdef/status
Content-Type: application/json

{
  "status": 0
}
```

## 公式验证 (Formula Validation)

### 7. 验证公式语法
验证公式的语法正确性和变量有效性。

**端点**: `POST /api/lab/metrics/validate-formula`

**请求体**: `FormulaValidationInput`

**响应**: `FormulaValidationResult`

**示例**:
```json
{
  "formula": "(width * thickness * density) / 1000",
  "availableVariables": {
    "width": "number",
    "thickness": "number",
    "density": "number"
  }
}
```

## 指标计算 (Metric Calculation)

### 8. 单条数据计算
执行单条数据的指标计算。

**端点**: `POST /api/lab/metrics/calculate`

**请求体**: `MetricCalculationInput`

**响应**: `MetricCalculationResult`

**示例**:
```json
{
  "metricDefinitionId": "1234567890abcdef",
  "contextData": {
    "width": 100,
    "thickness": 5,
    "density": 7.85
  },
  "rawDataId": "raw_data_id_123"
}
```

### 9. 批量计算（阶段二）
批量执行指标计算（支持进度跟踪）。

**端点**: `POST /api/lab/metrics/batch-calculate`

**请求体**:
```typescript
interface BatchCalculationInput {
  metricDefinitionIds: string[]; // 指标定义ID列表
  dataSource: {
    type: 'raw_data' | 'intermediate_data' | 'custom';
    filters?: Record<string, any>; // 数据筛选条件
    dataIds?: string[]; // 指定数据ID列表
  };
  options?: {
    useCache?: boolean; // 是否使用缓存
    parallel?: boolean; // 是否并行计算
  };
}
```

**响应**:
```typescript
interface BatchCalculationResponse {
  taskId: string;          // 任务ID，用于查询进度
  totalCount: number;      // 总计算数量
  estimatedTime?: number;  // 预计耗时（毫秒）
}
```

## 分类管理 (Category Management)

### 10. 获取分类列表
获取所有指标分类的列表。

**端点**: `GET /api/lab/metrics/categories`

**响应**:
```typescript
interface CategoryResponse {
  categories: string[]; // 分类名称列表
}
```

**示例**:
```bash
GET /api/lab/metrics/categories
```

## 批量操作 (Batch Operations)

### 11. 批量更新状态
批量更新多个指标定义的状态。

**端点**: `PUT /api/lab/metric-definitions/batch-status`

**请求体**:
```typescript
interface BatchUpdateStatusInput {
  ids: string[]; // 指标定义ID列表
  status: number; // 目标状态
}
```

**响应**:
```typescript
interface BatchUpdateResponse {
  successCount: number; // 成功数量
  failCount: number;   // 失败数量
  errors?: Array<{ id: string; error: string }>; // 错误详情
}
```

### 12. 批量删除
批量删除多个指标定义。

**端点**: `DELETE /api/lab/metric-definitions/batch-delete`

**请求体**:
```typescript
interface BatchDeleteInput {
  ids: string[]; // 指标定义ID列表
}
```

**响应**: `BatchUpdateResponse`

## 错误码定义

| 错误码 | 描述 | HTTP状态码 |
|--------|------|------------|
| METRIC_001 | 指标定义不存在 | 404 |
| METRIC_002 | 指标代码已存在 | 409 |
| METRIC_003 | 公式语法错误 | 400 |
| METRIC_004 | 变量未定义 | 400 |
| METRIC_005 | 计算错误 | 422 |
| METRIC_006 | 单位转换失败 | 400 |
| METRIC_007 | 批量计算任务不存在 | 404 |

## 接口约定

### 分页响应格式
所有分页接口使用统一响应格式：
```typescript
interface PaginatedResponse<T> {
  total: number;      // 总记录数
  list: T[];         // 当前页数据列表
  currentPage: number; // 当前页码
  pageSize: number;   // 每页条数
  totalPages: number; // 总页数
}
```

### 时间格式
- 请求参数：ISO 8601格式字符串，如 `2026-01-20T10:30:00Z`
- 响应字段：ISO 8601格式字符串

### 排序参数
- `sidx`: 排序字段名，如 `name`, `creatorTime`
- `sort`: 排序方向，`asc` 或 `desc`

### 单位处理
- 所有数值计算支持单位转换
- 计算结果包含单位ID和单位名称
- 公式中的变量应明确单位或使用基准单位

## 扩展说明

### 阶段一实现范围
阶段一主要实现以下端点：
1. 指标定义管理（CRUD）：端点1-6
2. 公式验证：端点7
3. 单条数据计算：端点8
4. 分类管理：端点10

### 阶段二扩展
阶段二将增加：
1. 批量计算：端点9
2. 批量操作：端点11-12
3. 变量绑定系统API
4. 版本管理API

### 与现有系统集成
- 与`IntermediateDataService`集成，扩展中间数据生成
- 与`UnitConversionService`集成，支持单位转换
- 使用现有异常处理机制（`Oops`）

---

**文档版本**: v1.0
**最后更新**: 2026-01-20
**负责人**: Claude (项目经理/架构师)