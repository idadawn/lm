/**
 * G6 图样式常量与构建函数
 */

import type { OntologyNodeType, NodeStatus, EdgeStatus } from '../types/ontology';

// ------------------------------------------------------------------
// 颜色常量（与文档推荐一致）
// ------------------------------------------------------------------

export const COLORS = {
  spec: {
    fill: '#EFF6FF',
    stroke: '#2563EB',
    text: '#2563EB',
    badge: '#3B82F6',
  },
  metric: {
    fill: '#FAF5FF',
    stroke: '#7C3AED',
    text: '#7C3AED',
    badge: '#8B5CF6',
  },
  rule: {
    qualified: {
      fill: '#F0FDF4',
      stroke: '#16A34A',
      text: '#16A34A',
    },
    unqualified: {
      fill: '#FEF2F2',
      stroke: '#DC2626',
      text: '#DC2626',
    },
  },
  condition: {
    fill: '#FFFBEB',
    stroke: '#F59E0B',
    text: '#B45309',
  },
  record: {
    fill: '#ECFEFF',
    stroke: '#0891B2',
    text: '#0E7490',
  },
  edge: {
    default: '#CBD5E1',
    active: '#60A5FA',
    failed: '#F87171',
    muted: '#E2E8F0',
  },
  combo: {
    qualifiedFill: '#F0FDF4',
    qualifiedStroke: '#16A34A',
    unqualifiedFill: '#FEF2F2',
    unqualifiedStroke: '#DC2626',
  },
} as const;

// ------------------------------------------------------------------
// 节点样式构建器
// ------------------------------------------------------------------

export function getNodeColorByType(type: OntologyNodeType, status?: NodeStatus) {
  switch (type) {
    case 'ProductSpec':
      return COLORS.spec;
    case 'Metric':
      return COLORS.metric;
    case 'JudgmentRule':
      return status === 'ok' || status === 'unknown'
        ? COLORS.rule.qualified
        : COLORS.rule.unqualified;
    case 'RuleCondition':
      return COLORS.condition;
    case 'InspectionRecord':
      return COLORS.record;
    default:
      return COLORS.spec;
  }
}

export function buildNodeStyle(
  type: OntologyNodeType,
  status?: NodeStatus,
  options?: { size?: [number, number]; radius?: number; lineWidth?: number }
) {
  const color = getNodeColorByType(type, status);
  return {
    fill: color.fill,
    stroke: color.stroke,
    lineWidth: options?.lineWidth ?? 2,
    radius: options?.radius ?? 8,
  };
}

export function buildNodeLabelCfg(
  type: OntologyNodeType,
  status?: NodeStatus,
  options?: { fontSize?: number; position?: string; offset?: number }
) {
  const color = getNodeColorByType(type, status);
  return {
    style: {
      fill: color.text,
      fontSize: options?.fontSize ?? (type === 'JudgmentRule' ? 9 : 12),
      fontWeight: 600,
    },
    position: options?.position ?? 'top',
    offset: options?.offset ?? 30,
  };
}

// ------------------------------------------------------------------
// 边样式构建器
// ------------------------------------------------------------------

export function buildEdgeStyle(status?: EdgeStatus) {
  switch (status) {
    case 'active':
      return { stroke: COLORS.edge.active, lineWidth: 2, endArrow: true };
    case 'failed':
      return { stroke: COLORS.edge.failed, lineWidth: 2, endArrow: true };
    case 'muted':
      return { stroke: COLORS.edge.muted, lineWidth: 1 };
    default:
      return { stroke: COLORS.edge.default, lineWidth: 1 };
  }
}

// ------------------------------------------------------------------
// Combo 样式构建器
// ------------------------------------------------------------------

export function buildComboStyle(qualityStatus: string) {
  const isQualified = qualityStatus === '合格';
  return {
    fill: isQualified ? COLORS.combo.qualifiedFill : COLORS.combo.unqualifiedFill,
    stroke: isQualified ? COLORS.combo.qualifiedStroke : COLORS.combo.unqualifiedStroke,
    lineWidth: 1,
    radius: 6,
  };
}

export function buildComboLabelCfg(qualityStatus: string) {
  const isQualified = qualityStatus === '合格';
  return {
    style: {
      fill: isQualified ? COLORS.combo.qualifiedStroke : COLORS.combo.unqualifiedStroke,
      fontSize: 11,
    },
    position: 'top' as const,
  };
}
