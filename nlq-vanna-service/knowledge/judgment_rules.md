# 判级规则（业务方完善）

<!-- TODO: 业务方完善真实判级规则 -->

本文件描述检测室数据分析系统的产品等级判定逻辑，
以及 `LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL` 表中 `F_CONDITION` 字段的 JSON Schema 结构。

---

## F_CONDITION JSON Schema 字段说明

`F_CONDITION` 字段（VARCHAR 4000）存储 JSON 格式的判定条件，每条判级规则可包含一个或多个条件对象。

### 单条件结构

```json
{
  "metric": "F_PERF_PS_LOSS",
  "operator": ">=" | "<=" | ">" | "<" | "between" | "in",
  "threshold": 1.30,
  "unit": "W/kg"
}
```

字段说明：
- `metric`（string，必填）：被比较的字段名，对应 LAB_INTERMEDIATE_DATA 中的列名，例如 `"F_PERF_PS_LOSS"`、`"F_WIDTH"`、`"F_LAMINATION_COEFFICIENT"`
- `operator`（string，必填）：比较运算符，支持：
  - `">="` / `"<="` / `">"` / `"<"`：单值比较
  - `"between"`：区间比较，配合 `range` 数组（见下）
  - `"in"`：枚举匹配，配合 `values` 数组（见下）
- `threshold`（number，单值比较时必填）：比较阈值
- `unit`（string，可选）：单位说明，例如 `"W/kg"`、`"mm"`
- `range`（array，`between` 时必填）：`[下限, 上限]`，例如 `[119.5, 120.5]`
- `values`（array，`in` 时必填）：枚举值列表，例如 `["A", "B", "C"]`

<!-- TODO: 业务方确认实际 F_CONDITION JSON 结构，可能与上述 schema 有差异 -->

### 多条件结构（AND / OR 组合）

```json
{
  "logic": "AND" | "OR",
  "conditions": [
    {"metric": "F_WIDTH", "operator": ">=", "threshold": 119.5, "unit": "mm"},
    {"metric": "F_PERF_PS_LOSS", "operator": "<=", "threshold": 1.30, "unit": "W/kg"}
  ]
}
```

<!-- TODO: 业务方确认多条件组合的实际 JSON 格式 -->

---

## SQL 查询 F_CONDITION 的方法

对 `F_CONDITION` 字段使用 MySQL `JSON_EXTRACT()` 函数：

```sql
-- 读取 metric 字段
JSON_EXTRACT(j.F_CONDITION, '$.metric')

-- 读取 threshold 并转为数值
CAST(JSON_EXTRACT(j.F_CONDITION, '$.threshold') AS DECIMAL(10,4))

-- 读取 range 数组的下限（between 类型）
CAST(JSON_EXTRACT(j.F_CONDITION, '$.range[0]') AS DECIMAL(10,4))

-- 检查 values 数组是否包含某值（in 类型）
JSON_CONTAINS(JSON_EXTRACT(j.F_CONDITION, '$.values'), JSON_QUOTE('A'))
```

---

## 判级示例规则（业务方参考模板）

<!-- TODO: 业务方完善真实判级规则 -->

### 示例：120 规格 C 级判定条件

条件描述：带宽 ≥ 119.5 mm 且 Ps 铁损 ≤ 1.30 W/kg

对应的 F_CONDITION 参考格式：

```json
{
  "logic": "AND",
  "conditions": [
    {"metric": "F_WIDTH", "operator": ">=", "threshold": 119.5, "unit": "mm"},
    {"metric": "F_PERF_PS_LOSS", "operator": "<=", "threshold": 1.30, "unit": "W/kg"}
  ]
}
```

查询该条件是否被满足的 SQL 示例（以 Ps 铁损单条件为例）：

```sql
SELECT j.F_NAME AS grade_name, j.F_PRIORITY
FROM LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL j
JOIN LAB_PRODUCT_SPEC s ON j.F_PRODUCT_SPEC_ID = s.F_Id
WHERE s.F_NAME = '120'
  AND JSON_EXTRACT(j.F_CONDITION, '$.metric') = 'F_PERF_PS_LOSS'
  AND JSON_EXTRACT(j.F_CONDITION, '$.operator') = '<='
  AND CAST(JSON_EXTRACT(j.F_CONDITION, '$.threshold') AS DECIMAL(10,4)) >= 1.30
ORDER BY j.F_PRIORITY DESC
LIMIT 1;
```

### 示例：优先级规则

`F_PRIORITY`（INT）值越大，优先级越高。当多条判级规则同时满足时，取优先级最高的等级。
`F_IS_DEFAULT`（BOOL）为 true 的规则作为兜底判定（当没有其他规则匹配时使用）。
