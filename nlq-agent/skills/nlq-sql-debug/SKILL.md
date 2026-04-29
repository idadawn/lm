---
name: nlq-sql-debug
description: 路美 NLQ Agent SQL 生成调试——诊断 Stage 2 的 NL2SQL 错误，修正 SQL 模板，优化 Prompt
argument-hint: "<SQL 生成问题描述，例如：合格率查询 SQL 字段名错误>"
level: 3
---

<Purpose>
本 Skill 专注于诊断和修复路美 NLQ Agent **Stage 2（Data & SQL Agent）**中的 SQL 生成问题。

Stage 2 的 SQL 生成流程：
1. 接收 `AgentContext`（含业务解释和过滤条件）
2. 优先匹配预定义 SQL 模板（`ddl.py` 中的 `PREDEFINED_SQL_TEMPLATES`）
3. 若无匹配模板，调用 LLM 动态生成 SQL（`SQL_GENERATION_PROMPT`）
4. 执行安全检查（白名单 + LIMIT 注入）
5. 执行 SQL，若失败则进入修正循环（最多 2 次，`SQL_CORRECTION_PROMPT`）
6. 回填 `condition` 步骤的 `actual` 和 `satisfied` 字段
</Purpose>

<Use_When>
- SQL 执行报错（语法错误、字段不存在、表名错误）
- SQL 生成结果不符合业务预期（查询了错误的字段或表）
- 预定义模板未能匹配到正确的查询意图
- SQL 修正循环失败（超过 2 次仍报错）
- 需要添加新的预定义 SQL 模板
- 需要优化 SQL 生成 Prompt
</Use_When>

<Database_Schema>
路美项目核心数据表（`src/models/ddl.py` 中的完整 DDL）：

```sql
-- 核心检测数据表（最常用）
LAB_INTERMEDIATE_DATA
  F_ID                    -- 主键
  F_BATCH_NO              -- 批次号
  F_PRODUCT_MODEL         -- 产品型号（如 50W470）
  F_DETECTION_DATE        -- 检测日期
  F_SHIFT                 -- 班次（早/中/晚）
  F_DETECTION_WEIGHT      -- 检测重量（kg）
  F_IRON_LOSS_P17_50      -- 铁损 P17/50（W/kg）
  F_MAGNETIC_INDUCTION_B50 -- 磁感 B50（T）
  F_STACKING_FACTOR       -- 叠片系数
  F_JUDGMENT_LEVEL_CODE   -- 判定等级代码（A/B/C/D）
  F_JUDGMENT_LEVEL_NAME   -- 判定等级名称
  F_IS_QUALIFIED          -- 是否合格（1=合格，0=不合格）
  F_CREATED_AT            -- 创建时间

-- 判定等级定义表
LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL
  F_ID, F_LEVEL_CODE, F_LEVEL_NAME
  F_IRON_LOSS_MAX, F_MAGNETIC_INDUCTION_MIN, F_STACKING_FACTOR_MIN
  F_PRODUCT_MODEL, F_IS_ACTIVE

-- 产品规格表
LAB_PRODUCT_SPEC
  F_ID, F_MODEL_NAME, F_SPEC_CODE, F_IS_ACTIVE

-- 产品规格属性表
LAB_PRODUCT_SPEC_ATTRIBUTE
  F_ID, F_SPEC_ID, F_ATTRIBUTE_NAME, F_STANDARD_VALUE, F_UNIT, F_TOLERANCE
```

**时间过滤约定**：
- 月度查询：`DATE_FORMAT(F_DETECTION_DATE, '%Y-%m') = '{year_month}'`
- 日期范围：`F_DETECTION_DATE BETWEEN '{start}' AND '{end}'`
- 最近 N 天：`F_DETECTION_DATE >= DATE_SUB(NOW(), INTERVAL {n} DAY)`
</Database_Schema>

<Predefined_Templates>
`src/models/ddl.py` 中的预定义 SQL 模板（高频统计查询）：

```python
PREDEFINED_SQL_TEMPLATES = {
    "monthly_pass_rate": """
        SELECT
            DATE_FORMAT(F_DETECTION_DATE, '%Y-%m') AS month,
            COUNT(*) AS total_count,
            SUM(CASE WHEN F_IS_QUALIFIED = 1 THEN 1 ELSE 0 END) AS qualified_count,
            ROUND(SUM(CASE WHEN F_IS_QUALIFIED = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 2) AS pass_rate
        FROM LAB_INTERMEDIATE_DATA
        WHERE DATE_FORMAT(F_DETECTION_DATE, '%Y-%m') = '{month}'
        GROUP BY month
        LIMIT 100
    """,
    "level_distribution": """
        SELECT
            F_JUDGMENT_LEVEL_CODE AS level_code,
            F_JUDGMENT_LEVEL_NAME AS level_name,
            COUNT(*) AS count,
            ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) AS percentage
        FROM LAB_INTERMEDIATE_DATA
        WHERE DATE_FORMAT(F_DETECTION_DATE, '%Y-%m') = '{month}'
        GROUP BY F_JUDGMENT_LEVEL_CODE, F_JUDGMENT_LEVEL_NAME
        ORDER BY F_JUDGMENT_LEVEL_CODE
        LIMIT 100
    """,
    "iron_loss_stats": """
        SELECT
            ROUND(AVG(F_IRON_LOSS_P17_50), 4) AS avg_iron_loss,
            ROUND(MIN(F_IRON_LOSS_P17_50), 4) AS min_iron_loss,
            ROUND(MAX(F_IRON_LOSS_P17_50), 4) AS max_iron_loss,
            ROUND(STDDEV(F_IRON_LOSS_P17_50), 4) AS stddev_iron_loss
        FROM LAB_INTERMEDIATE_DATA
        WHERE DATE_FORMAT(F_DETECTION_DATE, '%Y-%m') = '{month}'
          AND F_IRON_LOSS_P17_50 IS NOT NULL
        LIMIT 1
    """,
    "monthly_production": """
        SELECT
            DATE_FORMAT(F_DETECTION_DATE, '%Y-%m') AS month,
            ROUND(SUM(F_DETECTION_WEIGHT) / 1000, 2) AS total_weight_tons,
            COUNT(*) AS batch_count
        FROM LAB_INTERMEDIATE_DATA
        WHERE DATE_FORMAT(F_DETECTION_DATE, '%Y-%m') = '{month}'
        LIMIT 1
    """
}
```
</Predefined_Templates>

<Steps>
## Phase 1 — 复现问题

1. 读取相关文件：
   ```
   src/pipelines/stage2/data_sql_agent.py
   src/models/ddl.py
   src/utils/prompts.py
   ```

2. 获取失败的 SQL 和错误信息：
   ```bash
   # 查看最近的错误日志
   grep -n "SQL_ERROR\|sql_error\|SQLError" nlq-agent/logs/app.log | tail -20

   # 或直接测试同步接口
   curl -X POST http://localhost:18100/api/v1/query/sync \
     -H "Content-Type: application/json" \
     -d '{"question": "本月A类合格率是多少", "context": {}}'
   ```

3. 在 MySQL 中手动验证 SQL：
   ```sql
   -- 检查表和字段是否存在
   SHOW TABLES LIKE 'LAB_%';
   DESCRIBE LAB_INTERMEDIATE_DATA;

   -- 手动执行生成的 SQL（去掉 LIMIT 后验证逻辑）
   SELECT COUNT(*) FROM LAB_INTERMEDIATE_DATA
   WHERE DATE_FORMAT(F_DETECTION_DATE, '%Y-%m') = '2024-01';
   ```

## Phase 2 — 定位根因

**常见错误类型及修复方向**：

| 错误类型 | 症状 | 修复位置 |
|----------|------|----------|
| 字段名错误 | `Unknown column 'xxx'` | 更新 `ddl.py` DDL 或 `SQL_GENERATION_PROMPT` |
| 表名错误 | `Table 'xxx' doesn't exist` | 更新 `ddl.py` DDL |
| 语法错误 | `You have an error in your SQL syntax` | 优化 `SQL_GENERATION_PROMPT` 或修正模板 |
| 模板未匹配 | 走 LLM 生成但结果不准 | 在 `PREDEFINED_SQL_TEMPLATES` 中添加模板 |
| 修正循环失败 | `SQL correction failed after 2 attempts` | 优化 `SQL_CORRECTION_PROMPT` |

## Phase 3 — 修复

### 修复预定义模板
在 `src/models/ddl.py` 的 `PREDEFINED_SQL_TEMPLATES` 中修正或添加模板。

### 修复 SQL 生成 Prompt
在 `src/utils/prompts.py` 中修改 `SQL_GENERATION_PROMPT`，重点检查：
- DDL 描述是否准确（字段名、类型、注释）
- 示例 SQL 是否正确
- 时间过滤的 SQL 语法是否正确

### 修复 SQL 修正 Prompt
在 `src/utils/prompts.py` 中修改 `SQL_CORRECTION_PROMPT`，确保：
- 错误信息被完整传递给 LLM
- 修正指令足够具体（如"请将 `xxx` 字段名改为 `yyy`"）

## Phase 4 — 验证修复

```bash
# 重启服务
pkill -f "uvicorn src.main:app"
uvicorn src.main:app --reload --port 18100 &

# 测试修复后的查询
curl -X POST http://localhost:18100/api/v1/query/sync \
  -H "Content-Type: application/json" \
  -d '{"question": "本月A类合格率是多少", "context": {"month": "2024-01"}}'

# 运行测试套件
python -m pytest tests/test_pipeline.py -v -k "sql"
```
</Steps>

<SQL_Safety_Rules>
所有生成的 SQL 必须通过以下安全检查（`data_sql_agent.py` 中的 `_validate_sql_safety()`）：

```python
# 禁止的关键词（大小写不敏感）
FORBIDDEN_KEYWORDS = [
    "DROP", "DELETE", "INSERT", "UPDATE", "ALTER", "CREATE",
    "TRUNCATE", "EXEC", "EXECUTE", "GRANT", "REVOKE",
    "--", "/*", "*/", "xp_", "sp_"
]

# 必须包含 LIMIT（自动注入，最大 1000）
# 只允许查询 LAB_ 前缀的表
ALLOWED_TABLE_PREFIX = "LAB_"
```
</SQL_Safety_Rules>

<Final_Checklist>
- [ ] 修复后的 SQL 在 MySQL 中手动执行成功
- [ ] SQL 通过安全检查
- [ ] 相关的预定义模板已更新
- [ ] Prompt 模板中的 DDL 描述与实际表结构一致
- [ ] 测试用例通过（`python -m pytest tests/ -v -k "sql"`）
- [ ] 修正循环在 2 次内收敛
</Final_Checklist>
