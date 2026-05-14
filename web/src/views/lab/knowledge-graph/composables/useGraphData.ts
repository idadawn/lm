/**
 * 知识图谱数据转换 Composable
 *
 * 把后端 OntologyData 转成前端图谱可用的 { nodes, edges, combos }。
 * 重点目标不是“把所有节点撒开”，而是围绕检测中心真实业务主线组织关系：
 * 带材 → 产品规格 → 判定规则 → 公式/指标依据，并保留叠片、单片、外观等检测对象入口。
 */

import { computed, ref } from 'vue';
import type {
  OntologyData,
  OntologyRule,
  OntologyFormula,
} from '../types/ontology';

export interface GraphIndexes {
  rulesBySpec: Record<string, OntologyRule[]>;
  rulesByFormula: Record<string, OntologyRule[]>;
  formulaMap: Record<string, OntologyFormula>;
  specMap: Record<string, any>;
}

export interface G6GraphData {
  nodes: any[];
  edges: any[];
  combos: any[];
  meta?: Record<string, any>;
}

const MAX_FULL_SPECS = 8;
const MAX_RULES_PER_SPEC = 3;
const MAX_FOCUS_RULES = 20;

export function useGraphData() {
  const ontology = ref<OntologyData | null>(null);

  const indexes = computed<GraphIndexes>(() => {
    if (!ontology.value) {
      return { rulesBySpec: {}, rulesByFormula: {}, formulaMap: {}, specMap: {} };
    }
    return buildIndexes(ontology.value);
  });

  function setData(data: OntologyData) {
    ontology.value = data;
  }

  function buildGraphData(search?: string): G6GraphData {
    if (!ontology.value) return { nodes: [], edges: [], combos: [], meta: { layout: 'business-flow' } };
    return _buildFullGraph(ontology.value, indexes.value, search);
  }

  function buildSpecSubgraph(specId: string): G6GraphData {
    if (!ontology.value) return { nodes: [], edges: [], combos: [], meta: { layout: 'business-flow' } };
    return _buildSpecCentricGraph(ontology.value, indexes.value, specId);
  }

  return {
    ontology,
    indexes,
    setData,
    buildGraphData,
    buildSpecSubgraph,
  };
}

// ------------------------------------------------------------------
// 索引构建
// ------------------------------------------------------------------

export function buildIndexes(data: OntologyData): GraphIndexes {
  const rulesBySpec: Record<string, OntologyRule[]> = {};
  const rulesByFormula: Record<string, OntologyRule[]> = {};
  const formulaMap: Record<string, OntologyFormula> = {};
  const specMap: Record<string, any> = {};

  for (const s of data.specs || []) specMap[s.id] = s;
  for (const f of data.formulas || []) formulaMap[f.id] = f;
  for (const r of data.rules || []) {
    const specKey = r.product_spec_id || '_none';
    if (!rulesBySpec[specKey]) rulesBySpec[specKey] = [];
    rulesBySpec[specKey].push(r);
    if (r.formula_id) {
      if (!rulesByFormula[r.formula_id]) rulesByFormula[r.formula_id] = [];
      rulesByFormula[r.formula_id].push(r);
    }
  }

  Object.values(rulesBySpec).forEach((rules) => {
    rules.sort((a, b) => Number(a.priority || 0) - Number(b.priority || 0));
  });

  return { rulesBySpec, rulesByFormula, formulaMap, specMap };
}

function textIncludes(value: unknown, q: string) {
  return String(value || '').toLowerCase().includes(q);
}

function formulaTypeLabel(f?: OntologyFormula) {
  if (!f) return '';
  if (f.formula_type === 'CALC' || f.formula_type === '1') return '计算公式';
  if (f.formula_type === 'JUDGE' || f.formula_type === '2') return '判定公式';
  return '公式';
}

function ruleStatusColor(status?: string) {
  return status === '合格' ? '#52C41A' : '#FA8C16';
}

function addEdge(edges: any[], source: string, target: string, label: string, relation: string) {
  if (!source || !target || source === target) return;
  const id = `${source}-${relation}-${target}`;
  if (edges.some((e) => e.id === id)) return;
  edges.push({ id, source, target, label, relation, dataType: relation });
}

function addBusinessObjectNodes(nodes: any[], edges: any[], rootId: string) {
  const objects = [
    { id: 'domain:lamination', type: 'LaminationData', label: '叠片数据', subtitle: '导入后计算检测明细', x: 100, y: 80 },
    { id: 'domain:single-sheet', type: 'SingleSheetPerformance', label: '单片性能', subtitle: 'Ps / Ss / Hc', x: 100, y: 150 },
    { id: 'domain:appearance', type: 'AppearanceFeature', label: '外观特性', subtitle: '缺陷、等级、分类', x: 100, y: 220 },
    { id: 'domain:metric-judge', type: 'MetricJudgement', label: '指标判定', subtitle: '统计计算与等级结果', x: 520, y: 200 },
  ];
  for (const obj of objects) {
    nodes.push({
      ...obj,
      dataType: obj.type,
      rawData: { name: obj.label, description: obj.subtitle },
      isDomainNode: true,
    });
  }
  addEdge(edges, rootId, 'domain:lamination', '检测形成', 'HAS_LAMINATION_DATA');
  addEdge(edges, rootId, 'domain:single-sheet', '检测形成', 'HAS_SINGLE_SHEET_PERF');
  addEdge(edges, rootId, 'domain:appearance', '记录外观', 'HAS_APPEARANCE');
  addEdge(edges, 'domain:lamination', 'domain:metric-judge', '参与统计', 'FEEDS_METRIC');
  addEdge(edges, 'domain:single-sheet', 'domain:metric-judge', '参与统计', 'FEEDS_METRIC');
}

function buildSpecNode(spec: any, x: number, y: number, ruleCount: number, formulaCount: number) {
  return {
    id: `spec:${spec.id}`,
    label: spec.name || spec.code || '产品规格',
    subtitle: spec.code,
    type: 'ProductSpec',
    dataType: 'spec',
    x,
    y,
    rawData: spec,
    ruleCount,
    formulaCount,
  };
}

function buildRuleNode(rule: OntologyRule, x: number, y: number) {
  return {
    id: `rule:${rule.id}`,
    label: rule.name || rule.quality_status || '判定规则',
    subtitle: rule.quality_status || '规则',
    type: 'JudgementRule',
    dataType: 'rule',
    x,
    y,
    rawData: rule,
    statusColor: ruleStatusColor(rule.quality_status),
  };
}

function buildFormulaNode(formula: OntologyFormula, x: number, y: number, linkedRuleCount: number) {
  return {
    id: `formula:${formula.id}`,
    label: formula.formula_name || formula.column_name || '公式',
    subtitle: formulaTypeLabel(formula),
    type: 'Formula',
    dataType: 'formula',
    x,
    y,
    rawData: formula,
    typeLabel: formulaTypeLabel(formula),
    ruleCount: linkedRuleCount,
  };
}

function distributeY(index: number, total: number, min = 360, gap = 32) {
  const centerOffset = (total - 1) / 2;
  return min + (index - centerOffset) * gap;
}

function collectFocusSets(data: OntologyData, idx: GraphIndexes, search?: string) {
  const q = (search || '').trim().toLowerCase();
  const specIds = new Set<string>();
  const ruleIds = new Set<string>();
  const formulaIds = new Set<string>();

  if (!q) {
    const rankedSpecs = [...(data.specs || [])]
      .map((s) => ({ spec: s, rules: idx.rulesBySpec[s.id] || [] }))
      .filter((item) => item.rules.length > 0)
      .sort((a, b) => b.rules.length - a.rules.length)
      .slice(0, MAX_FULL_SPECS);

    for (const item of rankedSpecs) {
      specIds.add(item.spec.id);
      for (const r of item.rules.slice(0, MAX_RULES_PER_SPEC)) {
        ruleIds.add(r.id);
        if (r.formula_id) formulaIds.add(r.formula_id);
      }
    }
    return { specIds, ruleIds, formulaIds, mode: 'overview' };
  }

  for (const s of data.specs || []) {
    if (textIncludes(s.name, q) || textIncludes(s.code, q)) {
      specIds.add(s.id);
      for (const r of (idx.rulesBySpec[s.id] || []).slice(0, MAX_FOCUS_RULES)) {
        ruleIds.add(r.id);
        if (r.formula_id) formulaIds.add(r.formula_id);
      }
    }
  }

  for (const r of data.rules || []) {
    if (textIncludes(r.name, q) || textIncludes(r.quality_status, q) || textIncludes(r.conditionJson, q)) {
      ruleIds.add(r.id);
      if (r.product_spec_id) specIds.add(r.product_spec_id);
      if (r.formula_id) formulaIds.add(r.formula_id);
    }
  }

  for (const f of data.formulas || []) {
    if (textIncludes(f.formula_name, q) || textIncludes(f.column_name, q) || textIncludes(f.formula, q)) {
      formulaIds.add(f.id);
      for (const r of (idx.rulesByFormula[f.id] || []).slice(0, MAX_FOCUS_RULES)) {
        ruleIds.add(r.id);
        if (r.product_spec_id) specIds.add(r.product_spec_id);
      }
    }
  }

  return { specIds, ruleIds, formulaIds, mode: 'focus' };
}

// ------------------------------------------------------------------
// 全量/搜索图：带材业务主线，而非星爆块状图
// ------------------------------------------------------------------

function _buildFullGraph(
  data: OntologyData,
  idx: GraphIndexes,
  search?: string
): G6GraphData {
  const { rulesBySpec, rulesByFormula, formulaMap, specMap } = idx;
  const nodes: any[] = [];
  const edges: any[] = [];
  const combos: any[] = [];
  const focus = collectFocusSets(data, idx, search);

  const rootId = 'domain:ribbon';
  nodes.push({
    id: rootId,
    label: '带材',
    subtitle: '检测中心业务根节点',
    type: 'Ribbon',
    dataType: 'Ribbon',
    x: 60,
    y: 260,
    isDomainNode: true,
    rawData: {
      name: '带材',
      description: '以炉号为业务入口，连接规格、检测数据、公式和判定规则。',
    },
  });

  addBusinessObjectNodes(nodes, edges, rootId);

  const specList = Array.from(focus.specIds)
    .map((id) => specMap[id])
    .filter(Boolean);

  specList.forEach((spec, i) => {
    const specRules = (rulesBySpec[spec.id] || []).filter((r) => focus.ruleIds.has(r.id));
    const formulaIds = new Set<string>(specRules.map((r) => r.formula_id).filter((id): id is string => Boolean(id)));
    nodes.push(buildSpecNode(spec, 180, distributeY(i, specList.length, 300, 32), specRules.length, formulaIds.size));
    addEdge(edges, rootId, `spec:${spec.id}`, '适用规格', 'BELONGS_TO_SPEC');
  });

  const ruleList = (data.rules || []).filter((r) => focus.ruleIds.has(r.id));
  ruleList.forEach((rule, i) => {
    nodes.push(buildRuleNode(rule, 340, distributeY(i, ruleList.length, 180, 34)));
    if (rule.product_spec_id && focus.specIds.has(rule.product_spec_id)) {
      addEdge(edges, `spec:${rule.product_spec_id}`, `rule:${rule.id}`, '判定规则', 'USES_RULE');
    } else {
      addEdge(edges, rootId, `rule:${rule.id}`, '判定规则', 'USES_RULE');
    }
  });

  const formulaList = Array.from(focus.formulaIds)
    .map((id) => formulaMap[id])
    .filter(Boolean);
  formulaList.forEach((formula, i) => {
    nodes.push(buildFormulaNode(formula, 520, distributeY(i, formulaList.length, 220, 30), rulesByFormula[formula.id]?.length || 0));
  });

  for (const rule of ruleList) {
    if (rule.formula_id && focus.formulaIds.has(rule.formula_id)) {
      addEdge(edges, `rule:${rule.id}`, `formula:${rule.formula_id}`, '依据公式', 'USES_FORMULA');
      addEdge(edges, `formula:${rule.formula_id}`, 'domain:metric-judge', '输出判定', 'PRODUCES_RESULT');
    }
  }

  return {
    nodes,
    edges,
    combos,
    meta: {
      layout: 'business-flow',
      mode: focus.mode,
      search: search || '',
      summary: {
        specs: specList.length,
        rules: ruleList.length,
        formulas: formulaList.length,
      },
    },
  };
}

// ------------------------------------------------------------------
// 规格中心子图：保持接口，改为有方向的“规格 → 规则 → 公式”链路
// ------------------------------------------------------------------

function _buildSpecCentricGraph(
  data: OntologyData,
  idx: GraphIndexes,
  specId: string
): G6GraphData {
  const { rulesBySpec, rulesByFormula, formulaMap } = idx;
  const nodes: any[] = [];
  const edges: any[] = [];
  const combos: any[] = [];

  const spec = data.specs.find((s) => s.id === specId);
  if (!spec) return { nodes: [], edges: [], combos: [], meta: { layout: 'business-flow' } };

  const specRules = (rulesBySpec[specId] || []).slice(0, 48);
  const formulaIds = new Set<string>(specRules.map((r) => r.formula_id).filter((id): id is string => Boolean(id)));
  const rootId = 'domain:ribbon';

  nodes.push({
    id: rootId,
    label: '带材',
    subtitle: '业务根节点',
    type: 'Ribbon',
    dataType: 'Ribbon',
    x: 60,
    y: 260,
    isDomainNode: true,
    rawData: { name: '带材' },
  });
  nodes.push(buildSpecNode(spec, 180, 260, specRules.length, formulaIds.size));
  addEdge(edges, rootId, `spec:${spec.id}`, '适用规格', 'BELONGS_TO_SPEC');

  specRules.forEach((rule, i) => {
    nodes.push(buildRuleNode(rule, 340, distributeY(i, specRules.length, 180, 34)));
    addEdge(edges, `spec:${spec.id}`, `rule:${rule.id}`, '判定规则', 'USES_RULE');
  });

  const formulas = Array.from(formulaIds).map((fid) => formulaMap[fid]).filter(Boolean);
  formulas.forEach((formula, i) => {
    nodes.push(buildFormulaNode(formula, 520, distributeY(i, formulas.length, 220, 30), rulesByFormula[formula.id]?.length || 0));
  });

  for (const rule of specRules) {
    if (rule.formula_id && formulaMap[rule.formula_id]) {
      addEdge(edges, `rule:${rule.id}`, `formula:${rule.formula_id}`, '依据公式', 'USES_FORMULA');
    }
  }

  return { nodes, edges, combos, meta: { layout: 'business-flow', mode: 'spec', specId } };
}
