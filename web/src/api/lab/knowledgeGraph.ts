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
  description: string;
  attributes: Array<{ key: string; name: string; value: string; unit: string }>;
}

export interface OntologyRule {
  id: string;
  name: string;
  code: string;
  quality_status: string;
  priority: number;
  description: string;
  product_spec_id: string;
  product_spec_name: string;
  formula_name: string;
  formula_id: string;
}

export interface OntologyFormula {
  id: string;
  formula_name: string;
  column_name: string;
  formula: string;
  formula_type: string;
  unit_name: string;
}

export interface OntologyData {
  specs: OntologySpec[];
  rules: OntologyRule[];
  formulas: OntologyFormula[];
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
