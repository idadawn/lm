/**
 * 知识图谱数据转换 Composable
 *
 * 把后端 OntologyData 转成 G6 可用的 { nodes, edges, combos }
 * 支持：全量图、搜索子图、以规格为中心的局部子图
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
}

export interface G6GraphData {
  nodes: any[];
  edges: any[];
  combos: any[];
}

// ------------------------------------------------------------------
// 配色方案（参考金融图谱的鲜艳配色）
// ------------------------------------------------------------------
const COLORS = {
  spec: {
    fill: '#DBEAFE',
    stroke: '#2563EB',
    text: '#1E3A8A',
    shadow: '#60A5FA',
  },
  ruleQualified: {
    fill: '#DCFCE7',
    stroke: '#16A34A',
    text: '#14532D',
    shadow: '#4ADE80',
  },
  ruleUnqualified: {
    fill: '#FEE2E2',
    stroke: '#DC2626',
    text: '#7F1D1D',
    shadow: '#F87171',
  },
  formula: {
    fill: '#F3E8FF',
    stroke: '#9333EA',
    text: '#581C87',
    shadow: '#C084FC',
  },
  edge: {
    specRule: '#94A3B8',
    ruleFormula: '#A78BFA',
  },
};

export function useGraphData() {
  const ontology = ref<OntologyData | null>(null);

  const indexes = computed<GraphIndexes>(() => {
    if (!ontology.value) {
      return { rulesBySpec: {}, rulesByFormula: {}, formulaMap: {} };
    }
    return buildIndexes(ontology.value);
  });

  function setData(data: OntologyData) {
    ontology.value = data;
  }

  /**
   * 构建全量图数据（支持搜索过滤）
   */
  function buildGraphData(search?: string): G6GraphData {
    if (!ontology.value) return { nodes: [], edges: [], combos: [] };
    return _buildFullGraph(ontology.value, indexes.value, search);
  }

  /**
   * 构建以某个规格为中心的局部子图
   */
  function buildSpecSubgraph(specId: string): G6GraphData {
    if (!ontology.value) return { nodes: [], edges: [], combos: [] };
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

  for (const f of data.formulas) {
    formulaMap[f.id] = f;
  }
  for (const r of data.rules) {
    const specKey = r.product_spec_id || '_none';
    if (!rulesBySpec[specKey]) rulesBySpec[specKey] = [];
    rulesBySpec[specKey].push(r);
    if (r.formula_id) {
      if (!rulesByFormula[r.formula_id]) rulesByFormula[r.formula_id] = [];
      rulesByFormula[r.formula_id].push(r);
    }
  }

  return { rulesBySpec, rulesByFormula, formulaMap };
}

// ------------------------------------------------------------------
// 全量图（搜索过滤版）
// ------------------------------------------------------------------

function _buildFullGraph(
  data: OntologyData,
  idx: GraphIndexes,
  search?: string
): G6GraphData {
  const { rulesBySpec, rulesByFormula, formulaMap } = idx;
  const nodes: any[] = [];
  const edges: any[] = [];
  const combos: any[] = [];
  const searchLower = (search || '').toLowerCase();
  const hasSearch = !!searchLower;

  // 搜索匹配
  const matchedSpecs = new Set<string>();
  const matchedFormulas = new Set<string>();
  const matchedRules = new Set<string>();

  if (hasSearch) {
    for (const s of data.specs) {
      if (
        s.name.toLowerCase().includes(searchLower) ||
        s.code.toLowerCase().includes(searchLower)
      )
        matchedSpecs.add(s.id);
    }
    for (const f of data.formulas) {
      if (
        (f.formula_name || '').toLowerCase().includes(searchLower) ||
        (f.column_name || '').toLowerCase().includes(searchLower)
      )
        matchedFormulas.add(f.id);
    }
    for (const r of data.rules) {
      if (
        (r.name || '').toLowerCase().includes(searchLower) ||
        (r.quality_status || '').toLowerCase().includes(searchLower)
      )
        matchedRules.add(r.id);
    }
  }

  // 可见性计算
  const visibleSpecIds = new Set<string>();
  const visibleFormulaIds = new Set<string>();

  if (hasSearch) {
    for (const r of data.rules) {
      if (matchedRules.has(r.id)) {
        if (r.product_spec_id) visibleSpecIds.add(r.product_spec_id);
        if (r.formula_id) visibleFormulaIds.add(r.formula_id);
      }
    }
    for (const id of matchedSpecs) visibleSpecIds.add(id);
    for (const id of matchedFormulas) visibleFormulaIds.add(id);
  }

  // --- Spec nodes（大圆，蓝色） ---
  for (const spec of data.specs) {
    if (hasSearch && !visibleSpecIds.has(spec.id)) continue;

    const specRules = rulesBySpec[spec.id] || [];
    const ruleCount = specRules.length;
    const formulaIds = new Set(specRules.map((r) => r.formula_id).filter(Boolean));

    nodes.push({
      id: `spec:${spec.id}`,
      label: spec.name || spec.code,
      type: 'circle',
      size: 72,
      style: {
        fill: COLORS.spec.fill,
        stroke: COLORS.spec.stroke,
        lineWidth: 3,
        shadowColor: COLORS.spec.shadow,
        shadowBlur: 12,
        cursor: 'pointer',
      },
      labelCfg: {
        position: 'center',
        style: {
          fill: COLORS.spec.text,
          fontSize: 14,
          fontWeight: 700,
        },
      },
      dataType: 'spec',
      rawData: spec,
      ruleCount,
      formulaCount: formulaIds.size,
    });

    // 按状态分组创建 Combo
    const statusGroups: Record<string, OntologyRule[]> = {};
    for (const r of specRules) {
      const key = r.quality_status || '未知';
      if (!statusGroups[key]) statusGroups[key] = [];
      statusGroups[key].push(r);
    }

    for (const [status, rules] of Object.entries(statusGroups)) {
      if (hasSearch && !rules.some((r) => matchedRules.has(r.id))) continue;

      const comboId = `combo:${spec.id}:${status}`;
      const isQualified = status === '合格';
      const c = isQualified ? COLORS.ruleQualified : COLORS.ruleUnqualified;

      combos.push({
        id: comboId,
        label: `${status} (${rules.length})`,
        style: {
          fill: 'rgba(255,255,255,0.6)',
          stroke: c.stroke,
          lineWidth: 2,
          lineDash: [4, 2],
          radius: 16,
        },
        labelCfg: {
          position: 'top',
          style: { fill: c.stroke, fontSize: 12, fontWeight: 600 },
        },
        dataType: 'ruleCombo',
        specId: spec.id,
        specName: spec.name || spec.code,
        qualityStatus: status,
        rules,
      });

      // 规格 → Combo 的边
      edges.push({
        source: `spec:${spec.id}`,
        target: comboId,
        label: '包含',
        style: {
          stroke: COLORS.edge.specRule,
          lineWidth: 1.5,
          endArrow: { path: 'M 0,0 L 8,4 L 8,-4 Z', fill: COLORS.edge.specRule },
        },
        labelCfg: {
          style: { fill: '#64748B', fontSize: 10, background: '#fff' },
        },
        dataType: 'spec-combo',
      });

      for (const r of rules) {
        nodes.push({
          id: `rule:${r.id}`,
          label: r.name,
          comboId: comboId,
          type: 'circle',
          size: 44,
          style: {
            fill: c.fill,
            stroke: c.stroke,
            lineWidth: 2,
            shadowColor: c.shadow,
            shadowBlur: 6,
            cursor: 'pointer',
          },
          labelCfg: {
            position: 'center',
            style: {
              fill: c.text,
              fontSize: 10,
              fontWeight: 600,
            },
          },
          dataType: 'rule',
          rawData: r,
        });

        if (r.formula_id) {
          const showFormula = !hasSearch || visibleFormulaIds.has(r.formula_id);
          if (showFormula) {
            edges.push({
              source: `rule:${r.id}`,
              target: `formula:${r.formula_id}`,
              label: '使用',
              style: {
                stroke: COLORS.edge.ruleFormula,
                lineWidth: 1.5,
                endArrow: { path: 'M 0,0 L 8,4 L 8,-4 Z', fill: COLORS.edge.ruleFormula },
              },
              labelCfg: {
                style: { fill: '#7C3AED', fontSize: 10, background: '#fff' },
              },
              dataType: 'rule-formula',
            });
          }
        }
      }
    }
  }

  // --- Formula nodes（大圆，紫色） ---
  for (const f of data.formulas) {
    if (hasSearch && !visibleFormulaIds.has(f.id)) continue;

    const linkedRuleCount = rulesByFormula[f.id]?.length || 0;
    const typeLabel =
      f.formula_type === 'CALC'
        ? '计算'
        : f.formula_type === 'JUDGE' || f.formula_type === '2'
          ? '判定'
          : '';

    nodes.push({
      id: `formula:${f.id}`,
      label: f.formula_name || f.column_name,
      type: 'circle',
      size: 56,
      style: {
        fill: COLORS.formula.fill,
        stroke: COLORS.formula.stroke,
        lineWidth: 3,
        shadowColor: COLORS.formula.shadow,
        shadowBlur: 10,
        cursor: 'pointer',
      },
      labelCfg: {
        position: 'center',
        style: {
          fill: COLORS.formula.text,
          fontSize: 11,
          fontWeight: 600,
        },
      },
      dataType: 'formula',
      rawData: f,
      ruleCount: linkedRuleCount,
      typeLabel,
    });
  }

  return { nodes, edges, combos };
}

// ------------------------------------------------------------------
// 规格中心子图（局部展开）
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
  if (!spec) return { nodes: [], edges: [], combos: [] };

  const specRules = rulesBySpec[specId] || [];
  const formulaIds = new Set(specRules.map((r) => r.formula_id).filter(Boolean));

  // Spec 节点（中心大圆）
  nodes.push({
    id: `spec:${spec.id}`,
    label: spec.name || spec.code,
    type: 'circle',
    size: 88,
    style: {
      fill: COLORS.spec.fill,
      stroke: COLORS.spec.stroke,
      lineWidth: 4,
      shadowColor: COLORS.spec.shadow,
      shadowBlur: 16,
      cursor: 'pointer',
    },
    labelCfg: {
      position: 'center',
      style: {
        fill: COLORS.spec.text,
        fontSize: 16,
        fontWeight: 700,
      },
    },
    dataType: 'spec',
    rawData: spec,
    ruleCount: specRules.length,
    formulaCount: formulaIds.size,
  });

  // 按 formula_id + quality_status 二级 Combo 聚合
  const groupKey = (r: OntologyRule) => `${r.formula_id || '_none'}|${r.quality_status || '未知'}`;
  const groups: Record<string, { formulaId: string; status: string; rules: OntologyRule[] }> = {};

  for (const r of specRules) {
    const key = groupKey(r);
    if (!groups[key]) {
      groups[key] = { formulaId: r.formula_id || '_none', status: r.quality_status || '未知', rules: [] };
    }
    groups[key].rules.push(r);
  }

  for (const [, group] of Object.entries(groups)) {
    const isQualified = group.status === '合格';
    const c = isQualified ? COLORS.ruleQualified : COLORS.ruleUnqualified;
    const formulaName = group.formulaId !== '_none'
      ? (formulaMap[group.formulaId]?.formula_name || formulaMap[group.formulaId]?.column_name || group.formulaId)
      : '通用';
    const comboId = `combo:${specId}:${group.formulaId}:${group.status}`;

    combos.push({
      id: comboId,
      label: `${formulaName} · ${group.status}`,
      style: {
        fill: 'rgba(255,255,255,0.5)',
        stroke: c.stroke,
        lineWidth: 2,
        lineDash: [4, 2],
        radius: 16,
      },
      labelCfg: {
        position: 'top',
        style: { fill: c.stroke, fontSize: 12, fontWeight: 600 },
      },
      dataType: 'ruleCombo',
      specId,
      specName: spec.name || spec.code,
      qualityStatus: group.status,
      rules: group.rules,
      collapsed: true,
    });

    edges.push({
      source: `spec:${spec.id}`,
      target: comboId,
      label: '包含',
      style: {
        stroke: COLORS.edge.specRule,
        lineWidth: 2,
        endArrow: { path: 'M 0,0 L 8,4 L 8,-4 Z', fill: COLORS.edge.specRule },
      },
      labelCfg: {
        style: { fill: '#64748B', fontSize: 10, background: '#fff' },
      },
      dataType: 'spec-combo',
    });

    // 只展开前 5 个规则作为预览
    const previewRules = group.rules.slice(0, 5);
    for (const r of previewRules) {
      nodes.push({
        id: `rule:${r.id}`,
        label: r.name,
        comboId,
        type: 'circle',
        size: 40,
        style: {
          fill: c.fill,
          stroke: c.stroke,
          lineWidth: 2,
          shadowColor: c.shadow,
          shadowBlur: 6,
          cursor: 'pointer',
        },
        labelCfg: {
          position: 'center',
          style: {
            fill: c.text,
            fontSize: 10,
            fontWeight: 600,
          },
        },
        dataType: 'rule',
        rawData: r,
      });

      if (r.formula_id) {
        edges.push({
          source: `rule:${r.id}`,
          target: `formula:${r.formula_id}`,
          label: '使用',
          style: {
            stroke: COLORS.edge.ruleFormula,
            lineWidth: 1.5,
            endArrow: { path: 'M 0,0 L 8,4 L 8,-4 Z', fill: COLORS.edge.ruleFormula },
          },
          labelCfg: {
            style: { fill: '#7C3AED', fontSize: 10, background: '#fff' },
          },
          dataType: 'rule-formula',
        });
      }
    }
  }

  // Formula 节点（只显示该规格相关的）
  for (const fid of formulaIds) {
    const f = formulaMap[fid];
    if (!f) continue;
    const linkedRuleCount = rulesByFormula[fid]?.length || 0;
    const typeLabel =
      f.formula_type === 'CALC'
        ? '计算'
        : f.formula_type === 'JUDGE' || f.formula_type === '2'
          ? '判定'
          : '';

    nodes.push({
      id: `formula:${f.id}`,
      label: f.formula_name || f.column_name,
      type: 'circle',
      size: 56,
      style: {
        fill: COLORS.formula.fill,
        stroke: COLORS.formula.stroke,
        lineWidth: 3,
        shadowColor: COLORS.formula.shadow,
        shadowBlur: 10,
        cursor: 'pointer',
      },
      labelCfg: {
        position: 'center',
        style: {
          fill: COLORS.formula.text,
          fontSize: 11,
          fontWeight: 600,
        },
      },
      dataType: 'formula',
      rawData: f,
      ruleCount: linkedRuleCount,
      typeLabel,
    });
  }

  return { nodes, edges, combos };
}
