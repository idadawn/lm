/**
 * 知识图谱管理 API — 连接 nlq-agent 的管理端点
 */

import type { ReasoningStep, OntologyGraphDTO, OntologyNode, OntologyEdge, OntologyCombo } from '/@/views/lab/knowledge-graph/types/ontology';

// 安全兜底：若环境变量未生效，强制使用 /nlq-agent（与 vite.config proxy 前缀一致）
const BASE_URL = import.meta.env?.VITE_NLQ_AGENT_API_BASE || '/nlq-agent';

export interface CollectionInfo {
  name: string;
  display_name: string;
  points_count: number;
  vector_count: number;
  status: string;
}

export interface KgDocument {
  id: number | string;
  doc_id: string;
  text: string;
  metadata: Record<string, unknown>;
  collection?: string;
}

export interface SearchHit extends KgDocument {
  score: number;
  collection?: string;
}

export async function getCollections(): Promise<CollectionInfo[]> {
  const res = await fetch(`${BASE_URL}/api/v1/kg/collections`);
  if (!res.ok) throw new Error(`Failed to fetch collections: ${res.status}`);
  const data = await res.json();
  return data.collections ?? [];
}

export async function getDocuments(
  collection: string,
  limit = 20,
  offset = 0,
): Promise<{ documents: KgDocument[]; next_offset: number | null; count: number }> {
  const res = await fetch(
    `${BASE_URL}/api/v1/kg/collections/${encodeURIComponent(collection)}/documents?limit=${limit}&offset=${offset}`,
  );
  if (!res.ok) throw new Error(`Failed to fetch documents: ${res.status}`);
  return res.json();
}

export async function searchKnowledge(
  query: string,
  collection?: string,
  topK = 10,
): Promise<SearchHit[]> {
  const res = await fetch(`${BASE_URL}/api/v1/kg/search`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ query, collection, top_k: topK }),
  });
  if (!res.ok) throw new Error(`Search failed: ${res.status}`);
  const data = await res.json();
  return data.results ?? [];
}

export interface OntologySpec {
  id: string;
  code: string;
  name: string;
  description: string;                // 兼容旧字段（值 = detection_columns）
  detection_columns?: string;         // F_DETECTION_COLUMNS 叠片检测数据列
  version?: string;                   // 当前版本号 F_VERSION（来自 lab_product_spec_version）
  attributes: Array<{ key: string; name: string; value: string; unit: string }>;
}

export interface OntologyRule {
  id: string;
  code: string;                    // F_CODE 等级代码
  name: string;                    // F_NAME 等级名称
  formula_id: string;              // F_FORMULA_ID 关联的判定公式
  formula_name: string;            // F_FORMULA_NAME 冗余公式名
  product_spec_id: string;         // F_PRODUCT_SPEC_ID 产品规格 ID
  product_spec_name: string;       // 规格名（来自 JOIN）
  product_spec_code?: string;      // 规格代码
  quality_status: string;          // F_QUALITY_STATUS 合格/不合格/其他
  priority: number;                // F_PRIORITY 优先级
  color?: string;                  // F_COLOR 展示颜色
  is_statistic?: boolean;          // F_IS_STATISTIC 是否参与统计
  is_default?: boolean;            // F_IS_DEFAULT 是否兜底
  description?: string;            // F_DESCRIPTION 业务说明
  conditionJson?: string;          // F_CONDITION 判定条件 JSON
}

export interface OntologyFormula {
  id: string;
  formula_name: string;
  column_name: string;            // F_COLUMN_NAME
  formula: string;                // F_FORMULA
  formula_type: string;           // F_FORMULA_TYPE 归一化为 'CALC'|'JUDGE'|'NO'
  unit_name: string;              // F_UNIT_NAME
  formula_language?: string;      // F_FORMULA_LANGUAGE 'EXCEL'|'MATH'
  table_name?: string;            // F_TABLE_NAME
  source_type?: string;           // F_SOURCE_TYPE 'SYSTEM'|'CUSTOM'
  precision_val?: number | null;  // F_PRECISION
  sort_order?: number;            // F_SORT_ORDER
  is_enabled?: boolean;           // F_IS_ENABLED
  default_value?: string;         // F_DEFAULT_VALUE
  remark?: string;                // F_REMARK
  aliases?: string[];             // NLQ 同义词
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
  aliases?: string[];            // NLQ 同义词
}

// ------ 外观特性相关类型 ------

export interface OntologyAppearanceCategory {
  id: string;
  name: string;
  description?: string;
  parent_id?: string;          // F_PARENTID
  root_id?: string;            // F_ROOTID
  path?: string;               // F_PATH "rootId,parentId,currentId"
  sort_code?: number;
}

export interface OntologyAppearanceFeature {
  id: string;
  name: string;                // F_NAME 特性名（如"脆"）
  category_id: string;         // F_CATEGORY_ID
  severity_level_id?: string;  // F_SEVERITY_LEVEL_ID
  level_name?: string;         // JOIN 出来：等级名称（如"中等"）
  level_is_default?: boolean;
  keywords: string[];          // 已解析的 JSON 数组
  description?: string;
  sort_code?: number;
  aliases?: string[];          // NLQ 同义词
}

export interface OntologyAppearanceLevel {
  id: string;
  name: string;                // 等级名（微/轻微/中等/严重/超级）
  description?: string;
  sort_code?: number;
  enabled?: boolean;
  is_default?: boolean;
}

export interface OntologyReportConfig {
  id: string;
  name: string;                  // F_NAME 统计名称
  formula_id: string;            // F_FORMULA_ID 关联 JUDGE 公式
  level_names: string[];         // F_LEVEL_NAMES 已解析的 JSON 数组
  description?: string;
  sort_order?: number;
  is_system?: boolean;
  is_header?: boolean;           // 头部展示
  is_percentage?: boolean;       // 仅占比
  is_show_in_report?: boolean;
  is_show_ratio?: boolean;
  aliases?: string[];            // NLQ 同义词
}

export interface OntologySqlTemplate {
  id: string;
  name: string;
  description?: string;
  applicable_metric_id?: string;    // '*' = 通用模板
  parameters?: Array<{
    name: string;
    type: string;
    required?: boolean;
    desc?: string;
  }>;
  sql_template: string;
  output_columns?: Array<{ name: string; desc?: string }>;
  sample_questions?: string[];
}

// 维度元数据（NLQ 用：自然语言→SQL 条件的解析依据）
export interface OntologyTimeRange {
  expr: string;
  aliases: string[];
  start: string;
  end: string;
}

export interface OntologyShiftValue {
  code: string;
  numeric?: number;
  aliases: string[];
}

export interface OntologyDimensions {
  time?: {
    field: string;
    aliases?: string[];
    format?: string;
    min_date?: string;
    max_date?: string;
    row_count?: number;
    common_ranges?: OntologyTimeRange[];
    sql_helpers?: Record<string, string>;
  };
  shift?: {
    field: string;
    secondary_field?: string;
    aliases?: string[];
    values?: OntologyShiftValue[];
    values_from_data?: string[];
  };
  line?: {
    field: string;
    aliases?: string[];
    value_aliases?: Record<string, string[]>;
    values_from_data?: string[];
  };
  product_spec?: {
    field: string;
    aliases?: string[];
    kg_ref?: string;
    values_from_data?: string[];
  };
  furnace_no?: {
    field: string;
    aliases?: string[];
    raw_field?: string;
    raw_aliases?: string[];
  };
}

export interface OntologyData {
  specs: OntologySpec[];
  rules: OntologyRule[];
  formulas: OntologyFormula[];
  spec_attributes?: OntologySpecAttribute[];
  excel_templates?: OntologyExcelTemplate[];
  table_fields?: Record<string, OntologyTableField[]>;
  appearance_categories?: OntologyAppearanceCategory[];
  appearance_features?: OntologyAppearanceFeature[];
  appearance_levels?: OntologyAppearanceLevel[];
  report_configs?: OntologyReportConfig[];
  sql_templates?: OntologySqlTemplate[];
  sql_placeholder_format?: Record<string, string>;
  sql_table_context?: Record<string, Record<string, unknown>>;
  dimensions?: OntologyDimensions;
}

export async function getOntology(): Promise<OntologyData> {
  const res = await fetch(`${BASE_URL}/api/v1/kg/ontology`);
  if (!res.ok) throw new Error(`Failed to fetch ontology: ${res.status}`);
  return res.json();
}

export async function resyncNow(): Promise<Record<string, unknown>> {
  const res = await fetch(`${BASE_URL}/api/v1/sync/resync-now`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
  });
  if (!res.ok) {
    if (res.status === 401) throw new Error('需要管理员 Token 才能执行全量重建');
    throw new Error(`Resync failed: ${res.status}`);
  }
  return res.json();
}

// ------------------------------------------------------------------
// Phase 2 新增接口：问答解释、子图查询、实体解析
// ------------------------------------------------------------------

export interface ExplainRequest {
  question: string;
  session_id?: string;
  context?: Record<string, unknown>;
}

export interface ExplainResponse {
  answer: string;
  answer_card?: Record<string, unknown>;
  reasoning_steps: ReasoningStep[];
  subgraph?: OntologyGraphDTO;
  evidence_table?: Array<Record<string, unknown>>;
  suggested_actions?: Array<{ label: string; action: string; params?: Record<string, unknown> }>;
}

export async function explainQuestion(request: ExplainRequest): Promise<ExplainResponse> {
  const res = await fetch(`${BASE_URL}/api/v1/kg/explain`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!res.ok) throw new Error(`Explain failed: ${res.status}`);
  return res.json();
}

export interface SubgraphRequest {
  anchor_type: string;
  anchor_id: string;
  depth?: number;
  relation_filter?: string;
}

export async function getSubgraph(request: SubgraphRequest): Promise<OntologyGraphDTO> {
  const params = new URLSearchParams();
  params.set('anchor_type', request.anchor_type);
  params.set('anchor_id', request.anchor_id);
  if (request.depth) params.set('depth', String(request.depth));
  if (request.relation_filter) params.set('relation_filter', request.relation_filter);

  const res = await fetch(`${BASE_URL}/api/v1/kg/subgraph?${params.toString()}`);
  if (!res.ok) throw new Error(`Subgraph query failed: ${res.status}`);
  return res.json();
}

export interface ResolveRequest {
  phrase: string;
  context?: Record<string, unknown>;
}

export interface ResolvedEntity {
  type: string;
  id: string;
  label: string;
  confidence: number;
  meta?: Record<string, unknown>;
}

export async function resolveEntities(request: ResolveRequest): Promise<ResolvedEntity[]> {
  const res = await fetch(`${BASE_URL}/api/v1/kg/resolve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!res.ok) throw new Error(`Resolve failed: ${res.status}`);
  return res.json();
}

// ------------------------------------------------------------------
// Phase 3 新增：带材中心图谱
// ------------------------------------------------------------------

export interface RibbonSearchResult {
  id: string;
  furnace_no: string;
  furnace_no_formatted: string | null;
  prod_date: string | null;
  detection_date: string | null;
  spec_code: string | null;
  spec_name: string | null;
  labeling: string | null;
  detection_status: string;
}

export interface RibbonNode {
  id: string;
  type: string;
  label: string;
  furnace_no: string;
  furnace_no_formatted: string | null;
  prod_date: string | null;
  detection_date: string | null;
  spec_code: string | null;
  spec_name: string | null;
  labeling: string | null;
  detection_status: string;
  raw?: Record<string, unknown>;
}

export interface RibbonSubgraphResponse {
  ribbon: RibbonNode;
  nodes: OntologyNode[];
  edges: OntologyEdge[];
  combos: OntologyCombo[] | null;
}

export async function searchRibbon(query: string, limit = 20): Promise<RibbonSearchResult[]> {
  const res = await fetch(
    `${BASE_URL}/api/v1/kg/ribbon/search?q=${encodeURIComponent(query)}&limit=${limit}`,
  );
  if (!res.ok) throw new Error(`Search failed: ${res.status}`);
  return res.json();
}

export async function getRibbonSubgraph(furnaceNo: string, depth = 2): Promise<RibbonSubgraphResponse> {
  const res = await fetch(
    `${BASE_URL}/api/v1/kg/ribbon/${encodeURIComponent(furnaceNo)}/subgraph?depth=${depth}`,
  );
  if (!res.ok) throw new Error(`Subgraph failed: ${res.status}`);
  return res.json();
}
