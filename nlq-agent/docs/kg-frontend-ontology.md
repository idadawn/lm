# 知识图谱前端 Ontology 规格文档

> 本文档描述前端知识图谱（relation-graph-vue3）中所有节点类型、数据来源、连接关系及面板展示规则。
> 对应后端接口：`GET /api/v1/kg/ontology`
> 最后更新：2026-05-16

---

## 1. 节点类型总览

| 节点类型 | 显示名称 | 数据来源 | 节点形状 | 尺寸(wh) | 颜色 |
|---------|---------|---------|---------|---------|------|
| Ribbon | 带材 | 虚拟根节点 | 圆形 | 110×110 | `#E6F4FF` / `#1677FF` |
| ProductSpec | 产品规格 | `lab_product_spec` | 圆形 | 90×90 | `#F0F5FF` / `#2F54EB` |
| SpecAttribute | 产品属性 | `lab_product_spec_attribute` | 圆形 | 85×85 | `#FFF0F6` / `#EB2F96` |
| RawDataImport | 叠片数据 | `lab_raw_data` | 圆形 | 95×95 | `#ECFDF5` / `#10B981` |
| MagneticDataImport | 单片性能 | `lab_magnetic_raw_data` | 圆形 | 95×95 | `#FEF3C7` / `#F59E0B` |
| IntermediateData | 中间数据 | `lab_intermediate_data` | 圆形 | 90×90 | `#E0F2FE` / `#0EA5E9` |
| TemplateField | 字段映射 | 各表 `INFORMATION_SCHEMA.COLUMNS` | 圆形 | 70×70 | `#F3F4F6` / `#9CA3AF` |
| FurnaceNoInput | 原始炉号 | 炉号解析规则 | 圆形 | 90×90 | `#FEF3C7` / `#D97706` |
| FurnaceNoParsed | 炉号 | 炉号解析规则 | 圆形 | 85×85 | `#ECFDF5` / `#059669` |
| FurnaceNoField | 炉号字段 | 炉号解析规则 | 圆形 | 70×70 | `#EFF6FF` / `#3B82F6` |

> 所有节点 `nodeShape: 0`，`borderRadius: 50%`，使用力导向布局（`layoutName: 'force'`）。

---

## 2. 节点详细规格

### 2.1 Ribbon（带材）— 业务根节点

| 属性 | 值 |
|-----|-----|
| ID | `domain:ribbon` |
| 显示文本 | 带材 |
| subtitle | 检测中心业务根节点 |
| rawData | `{ name: '带材', description: '以炉号为业务入口，连接规格、检测数据、公式和判定规则。' }` |

**子节点连线：**
- `contains` → ProductSpec（包含产品）
- `importsVia` → RawDataImport（叠片导入）
- `importsVia` → MagneticDataImport（单片导入）
- `hasIdentifier` → FurnaceNoInput（原始炉号）

**面板类型：** `ribbonRoot`
- 折叠面板：产品规格列表 + 产品扩展信息 + 叠片数据字段 + 单片性能字段

---

### 2.2 ProductSpec（产品规格）

| 属性 | 值 |
|-----|-----|
| ID | `spec:{F_Id}` |
| 显示文本 | `F_NAME` 或 `F_CODE` |
| subtitle | `F_CODE` |
| rawData | 完整 `lab_product_spec` 行数据 |

**数据来源：**
```sql
SELECT F_Id as id, F_CODE as code, F_NAME as name, F_DETECTION_COLUMNS as detection_columns
FROM lab_product_spec
WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0) AND F_ENABLEDMARK = 1
```

**父级：** Ribbon（`contains`）

**子节点连线：**
- `hasAttribute` → SpecAttribute（扩展信息）

**面板类型：** `spec`
- 编码/名称 + 扩展信息列表（来自 `lab_product_spec_attribute`）

---

### 2.3 SpecAttribute（产品属性 / 扩展属性）

| 属性 | 值 |
|-----|-----|
| ID | `attr:{F_ATTRIBUTE_KEY}` 或 `attr:{F_ATTRIBUTE_NAME}` |
| 显示文本 | `F_ATTRIBUTE_NAME` 或 `F_ATTRIBUTE_KEY` |
| subtitle | `F_VALUE_TYPE` |
| rawData | 完整 `lab_product_spec_attribute` 行数据 |

**数据来源：**
```sql
SELECT a.*, v.F_IS_CURRENT
FROM lab_product_spec_attribute a
INNER JOIN lab_product_spec_version v
  ON a.F_PRODUCT_SPEC_ID = v.F_PRODUCT_SPEC_ID AND a.F_VERSION = v.F_VERSION
WHERE v.F_IS_CURRENT = 1
  AND (a.F_DeleteMark IS NULL OR a.F_DeleteMark = 0)
  AND (v.F_DELETE_MARK IS NULL OR v.F_DELETE_MARK = 0)
```

**去重策略：** 全局按 `name || attr_key` 去重，同名属性复用同一节点。

**父级：** ProductSpec（`hasAttribute`）

> 旧版本直接连到 Ribbon，现已调整为 ProductSpec 的子节点。

---

### 2.4 RawDataImport（叠片数据）

| 属性 | 值 |
|-----|-----|
| ID | `tmpl:{F_Id}`（模板ID） |
| 显示文本 | **叠片数据**（中文业务名） |
| subtitle | `F_TEMPLATE_NAME` |
| rawData.targetTable | `lab_raw_data` |

**数据来源：**
```sql
SELECT F_Id, F_TEMPLATE_NAME, F_TEMPLATE_CODE, F_CONFIG_JSON
FROM LAB_EXCEL_IMPORT_TEMPLATE
WHERE F_TEMPLATE_CODE = 'RawDataImport'
```

**父级：** Ribbon（`importsVia`，标签"叠片导入"）

**子节点：**
- `hasField` → TemplateField（lab_raw_data 表字段，排除隐藏字段和炉号字段）
- `produces` → IntermediateData（计算产生）

**面板类型：** `spec`
- 目标表名 + 表字段列表（数据库字段 \| C#属性 \| 中文注释）+ 导入模板字段映射

---

### 2.5 MagneticDataImport（单片性能）

| 属性 | 值 |
|-----|-----|
| ID | `tmpl:{F_Id}` |
| 显示文本 | **单片性能**（中文业务名） |
| subtitle | `F_TEMPLATE_NAME` |
| rawData.targetTable | `lab_magnetic_raw_data` |

**数据来源：** 同 RawDataImport，但 `F_TEMPLATE_CODE = 'MagneticDataImport'`

**父级：** Ribbon（`importsVia`，标签"单片导入"）

**子节点：**
- `hasField` → TemplateField（lab_magnetic_raw_data 表字段）
- `references` → FurnaceNoInput（原始炉号）
- `references` → FurnaceNoParsed（炉号）

**面板类型：** `spec`
- 同 RawDataImport

---

### 2.6 IntermediateData（中间数据）

| 属性 | 值 |
|-----|-----|
| ID | `domain:intermediate-data` |
| 显示文本 | 中间数据 |
| subtitle | 叠片数据计算结果 |
| rawData.targetTable | `lab_intermediate_data` |

**数据来源：** `INFORMATION_SCHEMA.COLUMNS` 查询 `lab_intermediate_data` 字段

**父级：** RawDataImport（`produces`，标签"计算产生"）

**子节点：**
- `hasField` → TemplateField（lab_intermediate_data 表字段）

**面板类型：** `spec`
- 目标表名 + 表字段列表

> 中间数据是叠片数据经公式计算后的结果，供判定规则使用。

---

### 2.7 TemplateField（字段映射）

| 属性 | 值 |
|-----|-----|
| ID | `field:{tmplId}:{column_name}` 或 `field:intermediate:{column_name}` |
| 显示文本 | `COLUMN_COMMENT` 或 `COLUMN_NAME` |
| subtitle | `COLUMN_NAME` |
| rawData | `{ column_name, column_comment, mapped_field, mapped_label, parentTemplate, targetTable }` |

**数据来源：**
- `lab_raw_data` / `lab_magnetic_raw_data` / `lab_intermediate_data` 的 `INFORMATION_SCHEMA.COLUMNS`

**父级：**
- RawDataImport / MagneticDataImport / IntermediateData（`hasField`）

**全局隐藏字段（大小写不敏感，不创建节点）：**
```
F_CREATOR_TIME, F_CREATOR_USER_ID, F_LAST_MODIFY_TIME, F_LAST_MODIFY_USER_ID,
F_DELETE_MARK, F_DELETE_TIME, F_DELETE_USER_ID, F_ERROR_MESSAGE, F_SORTCODE,
F_CREATORTIME, F_CREATORUSERID, F_ENABLEDMARK,
F_LastModifyTime, F_LastModifyUserId, F_DeleteMark, F_DeleteTime, F_DeleteUserId,
F_TENANTID, F_TenantId, F_IMPORT_SESSION_ID, F_ROW_INDEX,
F_IS_VALID, F_IS_VALID_DATA, F_SOURCE_FILE_ID,
F_APPEARANCE_FEATURE_IDS, F_APPEARANCE_FEATURE_CATEGORY_IDS, F_APPEARANCE_FEATURE_LEVEL_IDS,
F_MATCH_CONFIDENCE, F_IMPORT_ERROR, F_IMPORT_STATUS
```

**炉号解析字段（不创建独立节点，直接指向炉号节点）：**

| 数据库字段 | 目标节点ID | 中文注释 |
|-----------|-----------|---------|
| `F_FURNACE_NO` | `domain:furnace-no-input` | 原始炉号 |
| `F_ORIGINAL_FURNACE_NO` | `domain:furnace-no-input` | 原始炉号 |
| `F_FURNACE_NO_FORMATTED` | `domain:furnace-no-parsed` | 炉号（格式化） |
| `F_FURNACE_NO_PARSED` | `domain:furnace-no-parsed` | 炉号（解析后） |
| `F_PROD_DATE` | `field:furnace:prod-date` | 生产日期 |
| `F_LINE_NO` | `field:furnace:line-no` | 产线号 |
| `F_SHIFT` / `F_SHIFT_NUMERIC` | `field:furnace:shift` | 班次 |
| `F_FURNACE_BATCH_NO` | `field:furnace:furnace-batch` | 炉次号 |
| `F_COIL_NO` | `field:furnace:coil-no` | 卷号 |
| `F_SUBCOIL_NO` | `field:furnace:subcoil-no` | 分卷号 |
| `F_SPECIAL_MARKER` | `field:furnace:special-marker` | 特殊标记（W/w） |
| `F_FEATURE_SUFFIX` | `field:furnace:feature-suffix` | 特性描述（脆/硬） |

**面板类型：** `spec`
- 数据库字段 \| C#属性 \| 中文注释

---

### 2.8 FurnaceNoInput（原始炉号）

| 属性 | 值 |
|-----|-----|
| ID | `domain:furnace-no-input` |
| 显示文本 | 原始炉号 |
| subtitle | Excel导入的原始炉号字符串 |
| rawData | `{ description: '原始炉号字符串，如：1甲20251101-1-4-1W脆' }` |

**父级：** Ribbon（`hasIdentifier`，标签"原始炉号"）

**子节点：**
- `parsesTo` → FurnaceNoParsed（解析）

**面板类型：** `furnaceNo`
- 格式规则 + 正则表达式 + 解析字段（正则分组）+ 忽略后缀 + 磁性数据炉号 + **数据库字段**

**数据库字段：**
- `F_FURNACE_NO` — 原始炉号
- `F_ORIGINAL_FURNACE_NO` — 原始炉号

---

### 2.9 FurnaceNoParsed（炉号）

| 属性 | 值 |
|-----|-----|
| ID | `domain:furnace-no-parsed` |
| 显示文本 | 炉号 |
| subtitle | 解析后的标准炉号 |
| rawData | `{ description: '解析后的标准炉号，如：1甲20251101-1-4-1' }` |

**父级：** FurnaceNoInput（`parsesTo`，标签"解析"）

**子节点：**
- `hasComponent` → FurnaceNoField（各组成部分）

**面板类型：** `furnaceNo`
- 炉号组成部分 + 字符含义 + 辅助方法 + **数据库字段**

**数据库字段：**
- `F_FURNACE_NO_FORMATTED` — 炉号（格式化）
- `F_FURNACE_NO_PARSED` — 炉号（解析后）

---

### 2.10 FurnaceNoField（炉号字段）

| 属性 | 值 |
|-----|-----|
| ID | `field:furnace:{id}` |
| 显示文本 | 中文含义（产线号/班次/生产日期/炉次号/卷号/分卷号/特殊标记/特性描述） |
| subtitle | C# 属性名 |
| rawData | `{ id, label, field, example, parentType: 'FurnaceNoParsed' }` |

**字段列表：**

| ID | 中文 | C#属性 | 示例 | 正则分组 |
|----|------|--------|------|---------|
| `line-no` | 产线号 | `LineNo` | 1, 2, 3... | Group 1 |
| `shift` | 班次 | `Shift` | 甲=1, 乙=2, 丙=3 | Group 2 |
| `prod-date` | 生产日期 | `ProdDate` | 2025-11-01 | Group 3 |
| `furnace-batch` | 炉次号 | `FurnaceBatchNo` | 1, 2... | Group 4 |
| `coil-no` | 卷号 | `CoilNo` | 4（支持小数） | Group 5 |
| `subcoil-no` | 分卷号 | `SubcoilNo` | 1（支持小数） | Group 6 |
| `special-marker` | 特殊标记 | `SpecialMarker` | W 或 w | Group 7 |
| `feature-suffix` | 特性描述 | `FeatureSuffix` | 脆, 硬... | Group 8 |

**父级：** FurnaceNoParsed（`hasComponent`）

**面板类型：** `furnaceNo`
- 中文名称 / C#属性 / 示例值 / 正则分组 + **数据库字段映射**

---

## 3. 连接关系（Palantir Ontology 命名）

| 关系名 | 颜色 | 说明 | 示例 |
|--------|------|------|------|
| `contains` | `#1677FF` | 包含 | Ribbon → ProductSpec |
| `hasAttribute` | `#EB2F96` | 拥有属性 | ProductSpec → SpecAttribute |
| `importsVia` | `#10B981` | 通过...导入 | Ribbon → RawDataImport / MagneticDataImport |
| `hasField` | `#9CA3AF` | 包含字段 | 导入模板/中间数据 → TemplateField |
| `derivedFrom` | `#3B82F6` | 派生自 | 字段 → 炉号节点 |
| `references` | `#D97706` | 引用 | MagneticDataImport → 炉号节点 |
| `hasIdentifier` | `#D97706` | 拥有标识符 | Ribbon → FurnaceNoInput |
| `parsesTo` | `#059669` | 解析为 | FurnaceNoInput → FurnaceNoParsed |
| `hasComponent` | `#3B82F6` | 包含组件 | FurnaceNoParsed → FurnaceNoField |
| `produces` | `#0EA5E9` | 计算产生 | RawDataImport → IntermediateData |

---

## 4. 面板类型规格

| 面板类型 | 触发节点 | 展示内容 |
|---------|---------|---------|
| `ribbonRoot` | Ribbon | 折叠面板：产品规格列表 + 扩展属性 + 叠片数据字段 + 单片性能字段 |
| `spec` | ProductSpec / RawDataImport / MagneticDataImport / IntermediateData / TemplateField | 编码/名称/目标表 + 表字段列表（数据库字段 \| C#属性 \| 中文注释）+ 导入模板字段映射 |
| `furnaceNo` | FurnaceNoInput / FurnaceNoParsed / FurnaceNoField | 原始炉号：格式规则+正则+分组+数据库字段；炉号：组成部分+字符含义+方法+数据库字段；字段：中文/C#属性/示例/分组+数据库字段 |
| `ruleCombo` | 规则组 | 规格/状态/规则列表 |
| `rule` | JudgmentRule | 等级/代码/质量状态/优先级/条件表格 |
| `formula` | Formula | 名称/类型/列名/单位/公式/关联规则 |

---

## 5. 后端数据接口

### 5.1 主接口

```
GET /api/v1/kg/ontology
```

**返回结构：**
```json
{
  "specs": [...],
  "rules": [...],
  "formulas": [...],
  "spec_attributes": [...],
  "excel_templates": [...],
  "table_fields": {
    "lab_raw_data": [...],
    "lab_magnetic_raw_data": [...],
    "lab_intermediate_data": [...]
  }
}
```

### 5.2 数据来源说明

| 返回字段 | 来源表 | 说明 |
|---------|--------|------|
| `specs` | `lab_product_spec` | 产品规格 |
| `rules` | `lab_intermediate_data_judgment_level` + `lab_product_spec` | 判定规则 |
| `formulas` | `lab_intermediate_data_formula` | 公式/指标 |
| `spec_attributes` | `lab_product_spec_attribute` + `lab_product_spec_version` | 扩展属性（当前版本） |
| `excel_templates` | `LAB_EXCEL_IMPORT_TEMPLATE` | Excel导入模板（解析 `F_CONFIG_JSON`） |
| `table_fields` | `INFORMATION_SCHEMA.COLUMNS` | 各表字段及注释 |

---

## 6. 前端渲染白名单

通过 `ACTIVE_NODE_TYPES` Set 控制当前渲染的节点类型：

```ts
export const ACTIVE_NODE_TYPES = new Set([
  'Ribbon',
  'ProductSpec',
  'SpecAttribute',
  'RawDataImport',
  'MagneticDataImport',
  'IntermediateData',
  'TemplateField',
  'FurnaceNoInput',
  'FurnaceNoParsed',
  'FurnaceNoField',
]);
```

> 未在此白名单中的节点类型（如 `JudgmentRule`、`Formula`）不会渲染，但数据已随 `get_ontology()` 返回，可随时启用。

---

## 7. 布局配置

```ts
const graphOptions: RGOptions = {
  defaultNodeShape: 0,        // 0=圆形
  defaultLineShape: 1,
  defaultJunctionPoint: 'ltrb',
  layout: { layoutName: 'force' },  // 力导向布局
  moveToCenterWhenRefresh: true,
  zoomToFitWhenRefresh: true,
  useAnimationWhenRefresh: true,
};
```

---

## 8. 待构建节点（后续步骤）

以下节点类型数据已随 `get_ontology()` 返回，但尚未加入 `ACTIVE_NODE_TYPES`：

| 节点类型 | 数据来源 | 计划连接关系 |
|---------|---------|------------|
| `JudgmentRule` | `lab_intermediate_data_judgment_level` | ProductSpec → JudgmentRule（`HAS_RULE`） |
| `Formula` | `lab_intermediate_data_formula` | JudgmentRule → Formula（`USES_FORMULA`） |
| `AppearanceFeature` | `lab_appearance_feature` | Ribbon → AppearanceFeature（`HAS_APPEARANCE`） |
| `MetricJudge` | 虚拟节点 | LaminationData/SingleSheetPerf → MetricJudge（`FEEDS_METRIC`） |
