/**
 * 知识图谱 Ontology 统一类型定义
 *
 * 前后端共用语义，以带材(Ribbon)为根节点，向外展开产品规格、检测数据、判定规则、公式等业务节点。
 */

// ------------------------------------------------------------------
// 节点类型（与 Neo4j 节点标签、前端 dataType 严格对齐）
// ------------------------------------------------------------------

export type OntologyNodeType =
  | 'Ribbon'           // 带材 — 业务根节点（虚拟）
  | 'ProductSpec'      // 产品规格 — 带材分为的产品类型，来源 lab_product_spec
  | 'LaminationData'   // 叠片数据 — 原始数据导入后的检测明细
  | 'SingleSheetPerf'  // 单片性能 — Ps铁损 / Ss功率 / Hc
  | 'AppearanceFeature'// 外观特性 — 缺陷、等级、分类
  | 'AppearanceCategory'// 外观特性大类
  | 'AppearanceLevel'  // 外观特性等级
  | 'JudgmentRule'     // 判定规则 — 来源 lab_intermediate_data_judgment_level
  | 'Formula'          // 公式/指标 — 来源 lab_intermediate_data_formula（原 Metric 统一为 Formula）
  | 'MetricJudge'      // 指标判定 — 统计计算与等级结果汇总节点
  | 'ReportConfig'     // 报表配置 — 来源 lab_report_config
  | 'SpecAttribute';   // 规格扩展属性 — 来源 lab_product_spec_attribute

// ------------------------------------------------------------------
// 关系类型（与 Neo4j 关系类型、前端 edge.relation 严格对齐）
// ------------------------------------------------------------------

export type OntologyRelationType =
  | 'BELONGS_TO_SPEC'      // Ribbon → ProductSpec（适用规格）
  | 'HAS_LAMINATION_DATA'  // Ribbon → LaminationData（检测形成）
  | 'HAS_SINGLE_SHEET_PERF'// Ribbon → SingleSheetPerf（检测形成）
  | 'HAS_APPEARANCE'       // Ribbon → AppearanceFeature（记录外观）
  | 'HAS_RULE'             // ProductSpec → JudgmentRule（判定规则）
  | 'USES_FORMULA'         // JudgmentRule → Formula（依据公式）
  | 'PRODUCES_RESULT'      // Formula → MetricJudge（输出判定）
  | 'FEEDS_METRIC'         // LaminationData/SingleSheetPerf → MetricJudge（参与统计）
  | 'HAS_ATTRIBUTE'        // ProductSpec → SpecAttribute（扩展属性）
  | 'BELONGS_TO_CATEGORY'  // AppearanceFeature → AppearanceCategory（属于大类）
  | 'HAS_LEVEL'            // AppearanceFeature → AppearanceLevel（拥有等级）
  | 'USES_FORMULA_CONFIG'; // ReportConfig → Formula（关联公式）

// ------------------------------------------------------------------
// 状态类型
// ------------------------------------------------------------------

export type EdgeStatus = 'active' | 'muted' | 'failed';
export type NodeStatus = 'ok' | 'warning' | 'error' | 'unknown';

// ------------------------------------------------------------------
// 图谱节点/边/分组 DTO
// ------------------------------------------------------------------

export interface OntologyNode {
  id: string;
  type: OntologyNodeType;
  label: string;
  subtitle?: string;
  status?: NodeStatus;
  metrics?: Record<string, string | number>;
  badges?: string[];
  raw?: Record<string, unknown>;
}

export interface OntologyEdge {
  id: string;
  source: string;
  target: string;
  relation: OntologyRelationType;
  label?: string;
  status?: EdgeStatus;
}

export interface OntologyCombo {
  id: string;
  label: string;
  type: string;
  collapsed?: boolean;
  parentId?: string;
}

export interface OntologyGraphDTO {
  nodes: OntologyNode[];
  edges: OntologyEdge[];
  combos?: OntologyCombo[];
  highlights?: { nodeIds: string[]; edgeIds: string[] };
}

// ------------------------------------------------------------------
// 推理步骤（与后端 ReasoningStep 对齐）
// ------------------------------------------------------------------

export type ReasoningStepKind =
  | 'intent'
  | 'entity'
  | 'path'
  | 'record'
  | 'spec'
  | 'rule'
  | 'condition'
  | 'metric'
  | 'sql'
  | 'stat'
  | 'answer'
  | 'fallback';

export type StepStatus = 'pending' | 'running' | 'success' | 'warning' | 'failed';

export interface OntologyRef {
  type: OntologyNodeType;
  id: string;
  label: string;
}

export interface EdgeRef {
  source: string;
  target: string;
  relation: OntologyRelationType;
}

export interface EvidenceItem {
  label: string;
  value?: string | number | null;
  unit?: string;
  source?: string;
}

export interface ReasoningStep {
  id: string;
  kind: ReasoningStepKind;
  title: string;
  summary?: string;
  status: StepStatus;
  ontologyRefs?: OntologyRef[];
  edgeRefs?: EdgeRef[];
  evidence?: EvidenceItem[];
  confidence?: number;
  // 兼容旧字段
  meta?: Record<string, unknown>;
  field?: string;
  expected?: string;
  actual?: string | number;
  satisfied?: boolean | null;
  label?: string;
  detail?: string;
}

// ------------------------------------------------------------------
// 后端原始数据类型（/api/v1/kg/ontology 返回）
// ------------------------------------------------------------------

export interface OntologySpec {
  id: string;
  code: string;
  name: string;
  description?: string;
  detection_columns?: number;
  attributes?: Array<{ key: string; name: string; value: string; unit: string }>;
}

export interface OntologyRule {
  id: string;
  name: string;
  code?: string;
  quality_status: string;
  priority: number;
  description?: string;
  product_spec_id?: string;
  product_spec_name?: string;
  formula_name?: string;
  formula_id?: string;
  conditionJson?: string;
}

export interface OntologyFormula {
  id: string;
  formula_name?: string;
  column_name?: string;
  formula?: string;
  formula_type?: string;
  unit_name?: string;
}

export interface OntologySpecAttribute {
  id: string;
  spec_id: string;
  name: string;
  attr_key?: string;
  value?: string;
  value_type?: string;
  unit?: string;
  precision_val?: number | string;
  version?: string;
}

export interface OntologyTemplateField {
  field: string;
  label?: string;
  data_type?: string;
  required?: boolean;
}

export interface OntologyExcelTemplate {
  id: string;
  template_name: string;
  template_code: string;
  field_mappings: OntologyTemplateField[];
}

export interface OntologyTableField {
  column_name: string;
  column_comment: string;
}

export interface OntologyData {
  specs: OntologySpec[];
  rules: OntologyRule[];
  formulas: OntologyFormula[];
  spec_attributes?: OntologySpecAttribute[];
  excel_templates?: OntologyExcelTemplate[];
  table_fields?: Record<string, OntologyTableField[]>;
}

// ------------------------------------------------------------------
// 面板数据（详情面板统一 Props）
// ------------------------------------------------------------------

export type PanelType =
  | 'spec'
  | 'ruleCombo'
  | 'rule'
  | 'formula'
  | 'ribbon'
  | 'lamination'
  | 'singleSheet'
  | 'appearance'
  | 'reportConfig'
  | 'ribbonRoot'
  | 'furnaceNo'
  | 'productSpecList'              // 产品规格聚合节点：展示所有规格条目
  | 'specAttributeList'            // 产品扩展信息聚合节点：展示去重后的扩展属性
  | 'specAttributeItem'            // 单条扩展属性条目（聚合节点的子节点）
  | 'appearanceFeatureRoot'        // 外观特性聚合根（带材子节点）
  | 'appearanceCategoryItem'       // 单个特性大类（含旗下特性列表）
  | 'appearanceFeatureItem'        // 单条外观特性
  | 'appearanceLevelList'          // 特性等级聚合：展示全部等级
  | 'appearanceLevelItem'          // 单个特性等级
  | 'formulaRoot'                  // 公式聚合根（带材子节点）
  | 'formulaTypeGroup'             // 公式类型分组节点（CALC/JUDGE/NO）
  | 'formulaItem'                  // 单条公式
  | 'laminationDataView'           // 叠片数据成品视图（lab_intermediate_data，合并原 RawDataImport 直挂 + 中间数据）
  | 'judgmentLevelRoot'            // 判定等级聚合根（带材子节点）
  | 'judgmentLevelSpecGroup'       // 按产品规格分组（聚合根 → 规格判定集）
  | 'judgmentLevelItem'            // 单条判定等级
  | 'metricRoot'                   // 指标聚合根（带材子节点）
  | 'metricItem';                  // 单条指标

export interface PanelData {
  type: PanelType;
  typeLabel: string;
  label: string;
  color: string;
  raw?: Record<string, unknown>;
  ruleCount?: number;
  formulaCount?: number;
  specName?: string;
  qualityStatus?: string;
  rules?: OntologyRule[];
  linkedRules?: OntologyRule[];
  specs?: Array<{ id: string; name: string; code?: string; description?: string }>;
  products?: Array<{ id: string; name: string; code?: string; description?: string }>;
}

// ------------------------------------------------------------------
// 视图模式
// ------------------------------------------------------------------

export type ViewMode = 'grid' | 'graph';
