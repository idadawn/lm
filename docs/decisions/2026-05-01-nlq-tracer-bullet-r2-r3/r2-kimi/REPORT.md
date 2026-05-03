# KIMI Round-2 Report — 诊断 SELECT 泛化 + F2 短路化

## 改动 #4 — 诊断 SELECT 泛化（metadata-driven 注册）

**状态：** 完成

**文件：** `nlq-agent/src/pipelines/stage2/data_sql_agent.py`

**变更摘要：**
- 引入 `DiagnosticSpec`（`frozen=True` dataclass），包含 `metric_template`、`sql_expression`、`alias`、`canonical_key` 四个字段。
- 新增 `DIAGNOSTIC_FIELD_REGISTRY: dict[str, DiagnosticSpec]`，注册中英文两种键（合格率/qualified_rate、抽样数量/sample_count）。
- 新增 `canonical_field_key()` 归一化函数，将任何注册键映射到中文 canonical key。
- `_diagnostic_select_for_condition` 完全改为注册表驱动：
  - 按 `metric_template` 分组收集 `DiagnosticSpec`
  - 动态构建 `SELECT` 列（`spec.sql_expression AS spec.alias`）
  - 结果扇出时通过 `spec.alias` 和 `spec.canonical_key` 匹配回填
  - 保留 `validate_sql` 安全校验和 `METRIC_SQL_TEMPLATES` 检查
- 移除旧的 `_DIAGNOSTIC_FIELD_MAP` 硬编码常量。

**设计决策：** 选择 `dataclass(frozen=True)` 而非 NamedTuple 或 Pydantic，原因：
1. 轻量——不需要 Pydantic 的验证/序列化开销（纯内部常量表）。
2. 可读——字段名自解释，比 tuple 索引清晰。
3. 可扩展——未来加字段（如 `description`、`unit`）只需改 class 定义，不影响调用方。
4. `frozen=True` 保证注册表在运行时不被意外修改。

## 改动 #7 — F2 短路化（性能优化）

**状态：** 完成

**文件：** `nlq-agent/src/pipelines/stage2/data_sql_agent.py`

**变更摘要：**
- 在 `_backfill_conditions` 中，调用 `_diagnostic_select_for_condition` 之前，先遍历 `context.filters`。
- 对每个 filter，用 `canonical_field_key()` 检查 `first_row` 是否已包含该 canonical 字段。
- 若命中：直接调用 `update_condition_step` + `emit_reasoning_step` 回填，不发诊断 query；记录 `logger.debug`。
- 若所有注册表字段均已短路，直接返回 `sse_events`，跳过诊断 SELECT 和降级路径。
- 若仍有未短路字段，继续走诊断 SELECT（结果与短路事件合并返回）。

**行为等价性：** 原来发 0~1 次诊断 query；现在最多 1 次（未短路时），可能 0 次（全部短路时）。

## 测试结果

```bash
$ uv run pytest tests/unit/ -m "not live_llm" --tb=short
============================== 43 passed in 0.62s ==============================
```

- 原有 34 个测试全部通过（test_evaluate_condition 23 + test_doc_type_to_step_kind 11）
- 新增 9 个测试全部通过：
  - `test_diagnostic_registry.py`：6 个（canonical key ×3、registry ×3、select driven ×3）
  - `test_backfill_short_circuit.py`：3 个（short-circuit、diagnostic query、partial mixed）

**ruff check：** `All checks passed!`

## 已知限制 / TODO

1. **canonical_field_key 的英文映射硬编码：** 当前 `canonical_field_key` 依赖注册表的 `spec.canonical_key` 属性返回中文键。若未来添加新的中英文别名对，需在注册表中同时加入中文键和英文键两条记录。更优雅的方式是分离“canonical 注册表”和“别名映射表”，但当前 4 个键的规模下over-engineering。
2. **短路检查仅支持注册表字段：** 如果主查询行包含非注册表字段（如 `F_PERF_PS_LOSS`），仍走降级路径，不会短路。这是因为只有注册表字段才有诊断 query 的需求；非注册表字段原来也不走诊断 SELECT，行为未变。
3. **诊断 SQL 的聚合表达式仍与 合格率 metric 强耦合：** `DiagnosticSpec.sql_expression` 中的 `SUM(sample_count)`、`ROUND(SUM(qualified_count) ...)` 等表达式假设底层 CTE 有 `sample_count`、`qualified_count`、`total_count` 列。未来若新增 metric template（如“产量统计”），需要确保 CTE 列名与 `sql_expression` 一致。
4. **e2e 测试未运行：** 本次仅跑 unit test，未启动 docker-compose 跑 e2e（符合约束：不准跑外部服务）。e2e 验证留给 CI 或 GLM 工人。
