/**
 * 知识图谱 Ontology 统一类型定义
 *
 * 前后端共用语义，避免在组件里直接拼 G6 数据。
 */

// ------------------------------------------------------------------
// 基础实体类型
// ------------------------------------------------------------------

export type OntologyNodeType =
  | 'InspectionRecord'
  | 'ProductSpec'
  | 'Metric'
  | 'JudgmentRule'
  | 'RuleCondition'
  | 'DefectFeature'
  | 'ReportConfig';

export type EdgeStatus = 'active' | 'muted' | 'failed';
export type NodeStatus = 'ok' | 'warning' | 'error' | 'unknown';

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
  relation: string;
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
  type: string;
  id: string;
  label: string;
}

export interface EdgeRef {
  source: string;
  target: string;
  relation: string;
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

export interface OntologyData {
  specs: OntologySpec[];
  rules: OntologyRule[];
  formulas: OntologyFormula[];
}

// ------------------------------------------------------------------
// 面板数据（详情面板统一 Props）
// ------------------------------------------------------------------

export type PanelType = 'spec' | 'ruleCombo' | 'rule' | 'formula' | 'ribbon' | 'lamination' | 'singleSheet' | 'appearance';

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
}

// ------------------------------------------------------------------
// 视图模式
// ------------------------------------------------------------------

export type ViewMode = 'grid' | 'graph';
