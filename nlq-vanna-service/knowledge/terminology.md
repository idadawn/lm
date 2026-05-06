<!-- TODO: 业务方完善 -->
# 业务术语定义

本文件定义检测室数据分析系统的核心业务术语。
每条术语对应 Qdrant 中一个独立向量点，供 NLQ 检索时提供字段语义上下文。

---

## 叠片系数

字段：`F_LAMINATION_COEFFICIENT`（表：LAB_INTERMEDIATE_DATA）
单位：无量纲（0~1 之间的小数）
达标阈值：≥ 0.95 <!-- TODO: 业务方确认真实阈值 -->
说明：硅钢片叠压后铁芯的填充率指标，反映铁芯加工质量。叠片系数越高表示铁芯更致密，磁性能更好。

## Ps 铁损

字段：`F_PERF_PS_LOSS`（表：LAB_INTERMEDIATE_DATA）
单位：W/kg
说明：硅钢片在交变磁场下单位质量的铁损耗，是评价硅钢电磁性能的核心指标。数值越低越好。
<!-- TODO: 业务方补充各规格的合格阈值，例如 120 规格 Ps ≤ 1.30 W/kg -->

## 炉号

字段：`F_FURNACE_NO`（表：LAB_INTERMEDIATE_DATA）
类型：字符串
说明：唯一标识一炉钢的编号，是追溯批次质量的主键之一。同一炉号对应同一冶炼批次的钢材。

## 带宽

字段：`F_WIDTH`（表：LAB_INTERMEDIATE_DATA）
单位：mm
说明：硅钢带材的宽度尺寸，是产品规格的关键参数之一。判级时常作为尺寸条件参与筛选。
<!-- TODO: 业务方补充各规格的标称带宽与公差范围 -->

## 规格

字段：`F_NAME`（表：LAB_PRODUCT_SPEC）
示例值：`"120"`、`"135"` 等
说明：产品规格代码，对应一类硅钢产品的标准参数集合。判级规则通过 `F_PRODUCT_SPEC_ID` 关联到具体规格。
关联：LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL.F_PRODUCT_SPEC_ID → LAB_PRODUCT_SPEC.F_Id

## 批次号

字段：`F_BATCH_NO`（表：LAB_INTERMEDIATE_DATA）
类型：字符串
说明：生产批次的唯一标识符，一个批次可包含多炉钢。用于按批次汇总质量统计。

## 生产日期

字段：`F_PROD_DATE`（表：LAB_INTERMEDIATE_DATA）
类型：日期（DATE 或 DATETIME）
说明：该批次硅钢的生产日期。用于按时间范围查询历史质量数据。
<!-- TODO: 业务方确认字段实际类型（DATE/DATETIME/VARCHAR）及日期格式 -->

## 中间数据

表：`LAB_INTERMEDIATE_DATA`
说明：存储单批次硅钢关键测量值的核心表，包含叠片系数、铁损、带宽等多项性能指标。
每行对应一个生产批次（F_BATCH_NO）或炉次（F_FURNACE_NO）的测量结果集合。
关联表：
  - LAB_PRODUCT_SPEC（规格定义，通过 F_PRODUCT_SPEC_ID 关联）
  - LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL（判级规则，通过 F_PRODUCT_SPEC_ID 关联）
  - LAB_INTERMEDIATE_DATA_FORMULA（计算公式，通过 F_FORMULA_ID 关联）
