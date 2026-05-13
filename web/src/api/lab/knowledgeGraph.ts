/**
 * 知识图谱管理 API — 连接 nlq-agent 的管理端点
 */

const BASE_URL = import.meta.env?.VITE_NLQ_AGENT_API_BASE || '';

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
