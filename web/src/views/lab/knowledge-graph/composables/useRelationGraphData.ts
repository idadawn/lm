/**
 * 知识图谱 Relation Graph 数据转换
 *
 * 将后端 OntologyData / 带子图数据转换为 relation-graph-vue3 可用的 RGJsonData。
 * 核心布局：带材(Ribbon) 为根节点居中/居左，向右展开产品规格 → 判定规则 → 公式/指标。
 */

import { computed, ref } from 'vue';
import type { RGJsonData } from 'relation-graph-vue3';
import type {
  OntologyData,
  OntologyRule,
  OntologyFormula,
  OntologySpecAttribute,
  RibbonSubgraphResponse,
} from '../types/ontology';
import { buildIndexes, type GraphIndexes } from './useGraphData';

export type RgGraphData = RGJsonData;

const MAX_FULL_SPECS = 8;
const MAX_RULES_PER_SPEC = 3;
const MAX_FOCUS_RULES = 20;

// ------------------------------------------------------------------
// 节点样式映射（与现有 ECharts 样式保持一致）
// ------------------------------------------------------------------

const NODE_STYLE_MAP: Record<
  string,
  { color: string; borderColor: string; fontColor: string; width: number; height: number }
> = {
  // 等宽高 = 圆形节点 (nodeShape: 0)
  Ribbon:              { color: '#E6F4FF', borderColor: '#1677FF', fontColor: '#0958D9', width: 110, height: 110 },
  ProductSpec:         { color: '#F0F5FF', borderColor: '#2F54EB', fontColor: '#1D39C4', width: 90, height: 90 },
  spec:                { color: '#F0F5FF', borderColor: '#2F54EB', fontColor: '#1D39C4', width: 90, height: 90 },
  LaminationData:      { color: '#F6FFED', borderColor: '#52C41A', fontColor: '#237804', width: 80, height: 80 },
  SingleSheetPerf:     { color: '#FFF7E6', borderColor: '#FA8C16', fontColor: '#AD4E00', width: 80, height: 80 },
  AppearanceFeature:   { color: '#FFF1F0', borderColor: '#FF4D4F', fontColor: '#A8071A', width: 80, height: 80 },
  AppearanceCategory:  { color: '#FFF7E6', borderColor: '#FA8C16', fontColor: '#AD4E00', width: 75, height: 75 },
  AppearanceLevel:     { color: '#F9F0FF', borderColor: '#9254DE', fontColor: '#531DAB', width: 60, height: 60 },
  ReportConfig:        { color: '#FFFBE6', borderColor: '#FAAD14', fontColor: '#AD6800', width: 80, height: 80 },
  JudgmentRule:        { color: '#F9F0FF', borderColor: '#722ED1', fontColor: '#391085', width: 90, height: 90 },
  rule:                { color: '#F9F0FF', borderColor: '#722ED1', fontColor: '#391085', width: 90, height: 90 },
  Formula:             { color: '#E6FFFB', borderColor: '#13C2C2', fontColor: '#006D75', width: 90, height: 90 },
  formula:             { color: '#E6FFFB', borderColor: '#13C2C2', fontColor: '#006D75', width: 90, height: 90 },
  SpecAttribute:       { color: '#FFF0F6', borderColor: '#EB2F96', fontColor: '#9B106A', width: 85, height: 85 },
  RawDataImport:       { color: '#ECFDF5', borderColor: '#10B981', fontColor: '#047857', width: 95, height: 95 },
  MagneticDataImport:  { color: '#FEF3C7', borderColor: '#F59E0B', fontColor: '#B45309', width: 95, height: 95 },
  IntermediateData:    { color: '#E0F2FE', borderColor: '#0EA5E9', fontColor: '#0369A1', width: 90, height: 90 },
  TemplateField:       { color: '#F3F4F6', borderColor: '#9CA3AF', fontColor: '#4B5563', width: 56, height: 56 },
  FurnaceNoInput:      { color: '#FEF3C7', borderColor: '#D97706', fontColor: '#92400E', width: 90, height: 90 },
  FurnaceNoParsed:     { color: '#ECFDF5', borderColor: '#059669', fontColor: '#065F46', width: 85, height: 85 },
  FurnaceNoField:      { color: '#EFF6FF', borderColor: '#3B82F6', fontColor: '#1E40AF', width: 56, height: 56 },
  MetricJudge:         { color: '#FFFBE6', borderColor: '#FAAD14', fontColor: '#AD6800', width: 80, height: 80 },
};

// ── 全局隐藏字段列表（所有节点统一过滤，大小写不敏感）──
const GLOBAL_HIDDEN_FIELDS_RAW = [
  'F_CREATOR_TIME', 'F_CREATOR_USER_ID', 'F_LAST_MODIFY_TIME', 'F_LAST_MODIFY_USER_ID',
  'F_DELETE_MARK', 'F_DELETE_TIME', 'F_DELETE_USER_ID', 'F_ERROR_MESSAGE', 'F_SORTCODE',
  'F_CREATORTIME', 'F_CREATORUSERID', 'F_ENABLEDMARK',
  'F_LastModifyTime', 'F_LastModifyUserId', 'F_DeleteMark', 'F_DeleteTime', 'F_DeleteUserId',
  'F_TENANTID', 'F_TenantId', 'F_IMPORT_SESSION_ID', 'F_ROW_INDEX',
  'F_IS_VALID', 'F_IS_VALID_DATA', 'F_SOURCE_FILE_ID',
  'F_APPEARANCE_FEATURE_IDS', 'F_APPEARANCE_FEATURE_CATEGORY_IDS', 'F_APPEARANCE_FEATURE_LEVEL_IDS',
  'F_MATCH_CONFIDENCE', 'F_IMPORT_ERROR', 'F_IMPORT_STATUS',
];
const GLOBAL_HIDDEN_FIELDS = new Set(GLOBAL_HIDDEN_FIELDS_RAW);
const GLOBAL_HIDDEN_FIELDS_UPPER = new Set(GLOBAL_HIDDEN_FIELDS_RAW.map(f => f.toUpperCase()));

// ── 炉号解析字段 → 目标节点ID 映射表──
// 数据来源：MySQL INFORMATION_SCHEMA.COLUMNS + C# FurnaceNo.cs 解析规则
const FURNACE_NO_FIELD_MAP: Record<string, string> = {
  // 原始炉号（输入字符串，如：1甲20251101-1-4-1W脆）
  'F_ORIGINAL_FURNACE_NO': 'domain:furnace-no-input',
  'F_FURNACE_NO': 'domain:furnace-no-input',

  // 解析后的标准炉号（如：1甲20251101-1-4-1，不含特性描述）
  'F_FURNACE_NO_FORMATTED': 'domain:furnace-no-parsed',
  'F_FURNACE_NO_PARSED': 'domain:furnace-no-parsed',

  // 炉号组成部分（从原始炉号解析）
  'F_PROD_DATE': 'field:furnace:prod-date',               // 生产日期
  'F_LINE_NO': 'field:furnace:line-no',                   // 产线号
  'F_SHIFT': 'field:furnace:shift',                       // 班次（甲/乙/丙）
  'F_SHIFT_NUMERIC': 'field:furnace:shift',               // 班次数字（1/2/3）
  'F_FURNACE_BATCH_NO': 'field:furnace:furnace-batch',    // 炉次号
  'F_COIL_NO': 'field:furnace:coil-no',                   // 卷号（支持小数）
  'F_SUBCOIL_NO': 'field:furnace:subcoil-no',             // 分卷号（支持小数）
  'F_SPECIAL_MARKER': 'field:furnace:special-marker',     // 特殊标记（W/w）
  'F_FEATURE_SUFFIX': 'field:furnace:feature-suffix',     // 特性描述（脆/硬等）
};

// Palantir Ontology 风格的链接类型命名
const EDGE_COLOR_MAP: Record<string, string> = {
  contains: '#1677FF',           // 包含（带材→产品规格）
  hasAttribute: '#EB2F96',       // 拥有属性（带材→规格属性）
  importsVia: '#10B981',         // 通过...导入（带材→导入模板）
  hasField: '#9CA3AF',           // 包含字段（导入模板→字段）
  derivedFrom: '#3B82F6',        // 派生自（字段→炉号解析）
  references: '#D97706',         // 引用（模板→炉号节点）
  hasIdentifier: '#D97706',      // 拥有标识符（带材→原始炉号）
  parsesTo: '#059669',           // 解析为（原始炉号→炉号）
  hasComponent: '#3B82F6',       // 包含组件（炉号→解析字段）
  produces: '#0EA5E9',            // 计算产生（叠片数据→中间数据）
  hasAppearance: '#FF4D4F',       // 拥有外观特性（带材→外观特性聚合）
  belongsToCategory: '#FA8C16',   // 属于大类（特性→大类）
  parentCategory: '#D48806',      // 父级大类（大类树自引用）
  hasFormula: '#13C2C2',          // 拥有公式（带材→公式聚合根）
  sourcedFrom: '#10B981',         // 数据源（叠片数据→原始叠片导入/单片性能）
  computedBy: '#13C2C2',          // 由...计算（叠片数据→公式聚合根，引用边）
  hasJudgment: '#722ED1',         // 拥有判定（带材→判定等级聚合根）
  scopedBySpec: '#2F54EB',        // 限定规格（规格判定集→产品规格条目，引用边）
  basedOnFormula: '#13C2C2',      // 基于公式（等级→JUDGE 公式条目，引用边）
  hasMetric: '#FAAD14',           // 拥有指标（带材→指标聚合根）
  measuresBy: '#13C2C2',          // 度量基于（指标→JUDGE 公式条目，引用边）
  // 保留旧映射用于兼容
  BELONGS_TO_SPEC: '#1677FF',
  HAS_LAMINATION_DATA: '#52C41A',
  HAS_SINGLE_SHEET_PERF: '#FA8C16',
  HAS_APPEARANCE: '#FF4D4F',
  HAS_RULE: '#722ED1',
  USES_FORMULA: '#13C2C2',
  PRODUCES_RESULT: '#FAAD14',
  FEEDS_METRIC: '#8C8C8C',
};

function getNodeStyle(dataType: string) {
  return NODE_STYLE_MAP[dataType] || NODE_STYLE_MAP.Ribbon;
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

function truncateLabel(label: string, max = 14) {
  let text = String(label || '');
  // 去掉括号内的内容，如"生产日期（从炉号解析）"→"生产日期"
  text = text.replace(/[（(].*?[）)]/g, '');
  return text.length > max ? `${text.slice(0, max)}…` : text;
}

function textIncludes(value: unknown, q: string) {
  return String(value || '').toLowerCase().includes(q);
}

function distributeY(index: number, total: number, min = 360, gap = 32) {
  const centerOffset = (total - 1) / 2;
  return min + (index - centerOffset) * gap;
}

// ------------------------------------------------------------------
// 通用节点/连线构建
// ------------------------------------------------------------------

function makeNode(
  id: string,
  label: string,
  dataType: string,
  opts: { subtitle?: string; rawData?: any; statusColor?: string; ruleCount?: number; formulaCount?: number; typeLabel?: string } = {}
) {
  const style = getNodeStyle(dataType);
  const fontSize = dataType === 'Ribbon' ? 14 : 12;
  const fontWeight = dataType === 'Ribbon' || dataType === 'ProductSpec' || dataType === 'spec' ? 700 : 600;

  return {
    id,
    text: truncateLabel(label, dataType === 'JudgmentRule' || dataType === 'rule' ? 8 : 12),
    width: style.width,
    height: style.height,
    color: style.color,
    borderColor: opts.statusColor || style.borderColor,
    borderWidth: dataType === 'Ribbon' ? 1.8 : 2,
    fontColor: style.fontColor,
    fontSize,
    fontWeight,
    // force 布局下不设置 fixed/x/y，让引擎自动扩散
    data: {
      dataType,
      subtitle: opts.subtitle,
      rawData: opts.rawData,
      ruleCount: opts.ruleCount,
      formulaCount: opts.formulaCount,
      typeLabel: opts.typeLabel,
      statusColor: opts.statusColor,
    },
  };
}

function makeLine(from: string, to: string, relation: string, label?: string) {
  if (!from || !to || from === to) return null;
  const id = `${from}-${relation}-${to}`;
  return {
    id,
    from,
    to,
    text: label || relation,
    color: EDGE_COLOR_MAP[relation] || '#BFBFBF',
    lineWidth: relation === 'USES_FORMULA' ? 1.8 : 2,
    showEndArrow: true,
    data: { relation },
  };
}

function addLine(lines: any[], from: string, to: string, relation: string, label?: string) {
  const line = makeLine(from, to, relation, label);
  if (line && !lines.some((l) => l.id === line.id)) {
    lines.push(line);
  }
}

// ------------------------------------------------------------------
// 业务对象节点（叠片、单片、外观、指标判定）
// ------------------------------------------------------------------

function addBusinessObjectNodes(nodes: any[], lines: any[], rootId: string) {
  const objects = [
    { id: 'domain:lamination', dataType: 'LaminationData', label: '叠片数据', subtitle: '导入后计算检测明细' },
    { id: 'domain:single-sheet', dataType: 'SingleSheetPerf', label: '单片性能', subtitle: 'Ps / Ss / Hc' },
    { id: 'domain:appearance', dataType: 'AppearanceFeature', label: '外观特性', subtitle: '缺陷、等级、分类' },
    { id: 'domain:metric-judge', dataType: 'MetricJudge', label: '指标判定', subtitle: '统计计算与等级结果' },
  ];
  for (const obj of objects) {
    nodes.push(makeNode(obj.id, obj.label, obj.dataType, { subtitle: obj.subtitle, rawData: { name: obj.label, description: obj.subtitle } }));
  }
  addLine(lines, rootId, 'domain:lamination', 'HAS_LAMINATION_DATA', '检测形成');
  addLine(lines, rootId, 'domain:single-sheet', 'HAS_SINGLE_SHEET_PERF', '检测形成');
  addLine(lines, rootId, 'domain:appearance', 'HAS_APPEARANCE', '记录外观');
  addLine(lines, 'domain:lamination', 'domain:metric-judge', 'FEEDS_METRIC', '参与统计');
  addLine(lines, 'domain:single-sheet', 'domain:metric-judge', 'FEEDS_METRIC', '参与统计');
}

// ------------------------------------------------------------------
// 聚焦集合计算（复用原逻辑）
// ------------------------------------------------------------------

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
// 全量/搜索图 → RGJsonData
// ------------------------------------------------------------------

// 当前 Neo4j 构建步骤控制的节点类型（Step 2+3+4: Ribbon + ProductSpec + SpecAttribute + Excel导入模板 + 炉号）
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
  // 外观特性体系（LAB_APPEARANCE_FEATURE / _CATEGORY / _LEVEL）
  'AppearanceFeature',
  'AppearanceCategory',
  'AppearanceLevel',
  // 公式体系（lab_intermediate_data_formula）
  'Formula',
  // 判定等级体系（lab_intermediate_data_judgment_level）
  'JudgmentRule',
  // 统计指标体系（lab_report_config）
  'ReportConfig',
]);

export function buildRelationGraphData(data: OntologyData, search?: string): RGJsonData {
  const idx = buildIndexes(data);
  const { specMap } = idx;
  const nodes: any[] = [];
  const lines: any[] = [];
  const q = (search || '').trim().toLowerCase();

  const rootId = 'domain:ribbon';

  // ── Ribbon 根节点 ──
  if (ACTIVE_NODE_TYPES.has('Ribbon')) {
    nodes.push(makeNode(rootId, '带材', 'Ribbon', {
      subtitle: '检测中心业务根节点',
      rawData: { name: '带材', description: '以炉号为业务入口，连接规格、检测数据、公式和判定规则。' },
    }));
  }

  // ── 产品规格聚合节点 ──
  // 聚合节点代表"产品规格"分类；下挂每个规格条目作为子节点
  const productSpecAggId = 'domain:product-spec';
  const specList = (data.specs || []).filter((s) => {
    if (!q) return true;
    return textIncludes(s.name, q) || textIncludes(s.code, q);
  });
  if (ACTIVE_NODE_TYPES.has('ProductSpec')) {
    nodes.push(makeNode(
      productSpecAggId,
      '产品规格',
      'ProductSpec',
      {
        subtitle: `共 ${specList.length} 项`,
        rawData: {
          isAggregate: true,
          description: '带材所属的产品规格分类',
          specs: specList,
        },
        typeLabel: '产品规格分类',
      }
    ));
    if (ACTIVE_NODE_TYPES.has('Ribbon')) {
      addLine(lines, rootId, productSpecAggId, 'contains', '包含产品');
    }

    // 每个产品规格作为聚合节点的子条目
    specList.forEach((spec) => {
      const specNodeId = `spec:${spec.id}`;
      nodes.push(makeNode(
        specNodeId,
        spec.name || spec.code || '产品规格',
        'ProductSpec',
        {
          subtitle: spec.code,
          rawData: spec,
          ruleCount: 0,
          formulaCount: 0,
        }
      ));
      addLine(lines, productSpecAggId, specNodeId, 'contains', '规格条目');
    });
  }

  // ── 产品扩展信息聚合节点（去重）──
  // 聚合节点作为产品规格聚合节点的子节点；下挂每个唯一扩展属性作为子条目
  if (ACTIVE_NODE_TYPES.has('SpecAttribute') && data.spec_attributes) {
    const uniqueAttrs = new Map<string, { attr: OntologySpecAttribute; specIds: Set<string> }>();
    for (const attr of data.spec_attributes) {
      const key = attr.name || attr.attr_key || attr.id;
      if (!uniqueAttrs.has(key)) {
        uniqueAttrs.set(key, { attr, specIds: new Set() });
      }
      uniqueAttrs.get(key)!.specIds.add(attr.spec_id);
    }
    const attrList = Array.from(uniqueAttrs.values()).map(({ attr, specIds }) => ({
      ...attr,
      specCount: specIds.size,
    }));

    const specAttrAggId = 'domain:spec-attribute';
    nodes.push(makeNode(
      specAttrAggId,
      '产品扩展信息',
      'SpecAttribute',
      {
        subtitle: `共 ${attrList.length} 项（去重）`,
        rawData: {
          isAggregate: true,
          description: '跨产品规格的扩展属性（按名称去重）',
          attributes: attrList,
        },
        typeLabel: '扩展信息分类',
      }
    ));
    if (ACTIVE_NODE_TYPES.has('ProductSpec')) {
      addLine(lines, productSpecAggId, specAttrAggId, 'hasAttribute', '扩展信息');
    } else if (ACTIVE_NODE_TYPES.has('Ribbon')) {
      addLine(lines, rootId, specAttrAggId, 'hasAttribute', '扩展信息');
    }

    // 每个唯一扩展属性作为聚合节点的子条目
    attrList.forEach((attr) => {
      const attrKey = attr.attr_key || attr.name || attr.id;
      const attrNodeId = `attr:${attrKey}`;
      nodes.push(makeNode(
        attrNodeId,
        attr.name || attr.attr_key || '属性',
        'SpecAttribute',
        {
          subtitle: attr.value_type || '扩展属性',
          rawData: attr,
          typeLabel: '扩展属性',
        }
      ));
      addLine(lines, specAttrAggId, attrNodeId, 'hasAttribute', '扩展属性');
    });
  }

  // ── Excel 导入模板节点（叠片数据 + 单片性能）──
  if (ACTIVE_NODE_TYPES.has('RawDataImport') && data.excel_templates) {
    for (const tmpl of data.excel_templates) {
      const isRaw = tmpl.template_code === 'RawDataImport';
      const isMagnetic = tmpl.template_code === 'MagneticDataImport';
      if (!isRaw && !isMagnetic) continue;

      const type = isRaw ? 'RawDataImport' : 'MagneticDataImport';
      const targetTable = isRaw ? 'lab_raw_data' : 'lab_magnetic_raw_data';
      const label = isRaw ? '原始叠片导入' : '单片性能'; // 业务命名
      const tmplNodeId = `tmpl:${tmpl.id}`;

      nodes.push(makeNode(
        tmplNodeId,
        label,
        type,
        {
          subtitle: tmpl.template_name,
          rawData: {
            ...tmpl,
            targetTable,
            route: isRaw ? '/lab/rawData' : '/lab/magneticData',
          },
          typeLabel: targetTable,
        }
      ));
      // 不再直连 Ribbon；由「叠片数据」节点通过 sourcedFrom 引用

      // 单片性能：增加指向炉号节点的引用连线
      if (isMagnetic && ACTIVE_NODE_TYPES.has('FurnaceNoInput')) {
        addLine(lines, tmplNodeId, 'domain:furnace-no-input', 'references', '原始炉号');
      }
      if (isMagnetic && ACTIVE_NODE_TYPES.has('FurnaceNoParsed')) {
        addLine(lines, tmplNodeId, 'domain:furnace-no-parsed', 'references', '炉号');
      }

      // 字段子节点（使用目标表的实际字段）
      const tableKey = isRaw ? 'lab_raw_data' : 'lab_magnetic_raw_data';
      const tableFieldList = data.table_fields?.[tableKey] || [];
      if (ACTIVE_NODE_TYPES.has('TemplateField') && tableFieldList.length > 0) {
        tableFieldList.forEach((tf) => {
          const colName = tf.column_name;

          // 1. 全局隐藏字段：跳过不显示（大小写不敏感）
          if (GLOBAL_HIDDEN_FIELDS.has(colName) || GLOBAL_HIDDEN_FIELDS_UPPER.has(colName.toUpperCase())) {
            return;
          }

          // 2. 炉号解析字段：指向对应的炉号解析节点，不创建独立子节点
          const furnaceTargetId = FURNACE_NO_FIELD_MAP[colName];
          if (furnaceTargetId) {
            const relationLabel = tf.column_comment || colName;
            addLine(lines, tmplNodeId, furnaceTargetId, 'derivedFrom', relationLabel);
            return;
          }

          // 3. 普通字段：创建 TemplateField 子节点
          const fieldNodeId = `field:${tmpl.id}:${colName}`;
          const fieldNameLower = colName.replace(/^F_/, '').toLowerCase();
          const mapping = tmpl.field_mappings?.find(
            (m) => m.field.toLowerCase() === fieldNameLower
          );
          nodes.push(makeNode(
            fieldNodeId,
            tf.column_comment || colName, // 节点显示中文注释
            'TemplateField',
            {
              subtitle: colName, // subtitle 显示数据库字段名
              rawData: {
                column_name: colName,
                column_comment: tf.column_comment,
                mapped_field: mapping?.field || '', // C# 属性名
                mapped_label: mapping?.label || '',
                parentTemplate: tmpl.template_code,
              },
              typeLabel: mapping?.data_type || '字段',
            }
          ));
          addLine(lines, tmplNodeId, fieldNodeId, 'hasField', tf.column_comment || colName);
        });
      }
    }
  }

  // ── 叠片数据节点（业务成品视图 = lab_intermediate_data）──
  // 数据流：lab_raw_data + lab_magnetic_raw_data → (CALC/JUDGE 公式) → lab_intermediate_data
  // 不画字段子节点；列定义在公式聚合根里，通过 computedBy 引用边表达
  if (ACTIVE_NODE_TYPES.has('IntermediateData')) {
    const laminationNodeId = 'domain:lamination-data';
    nodes.push(makeNode(
      laminationNodeId,
      '叠片数据',
      'IntermediateData',
      {
        subtitle: 'lab_intermediate_data',
        rawData: {
          targetTable: 'lab_intermediate_data',
          route: '/lab/intermediateData',
          description: '带材的叠片数据成品视图：原始叠片(lab_raw_data) + 单片性能(lab_magnetic_raw_data) 经计算/判定公式合成。',
        },
        typeLabel: '业务成品视图',
      }
    ));
    if (ACTIVE_NODE_TYPES.has('Ribbon')) {
      addLine(lines, rootId, laminationNodeId, 'contains', '叠片数据');
    }

    // sourcedFrom：连接两个数据源（原始叠片 + 单片性能），如果对应导入模板节点存在
    if (ACTIVE_NODE_TYPES.has('RawDataImport') && data.excel_templates) {
      for (const tmpl of data.excel_templates) {
        if (tmpl.template_code === 'RawDataImport' || tmpl.template_code === 'MagneticDataImport') {
          addLine(lines, laminationNodeId, `tmpl:${tmpl.id}`,
            'sourcedFrom',
            tmpl.template_code === 'RawDataImport' ? '原始叠片' : '单片性能');
        }
      }
    }

    // computedBy：引用公式聚合根（中间数据每列由公式定义）
    if (ACTIVE_NODE_TYPES.has('Formula')) {
      addLine(lines, laminationNodeId, 'domain:formula', 'computedBy', '由公式计算');
    }
  }

  // ── 炉号解析节点（原始炉号 → 炉号 → 解析字段）──
  if (ACTIVE_NODE_TYPES.has('FurnaceNoInput')) {
    const furnaceNoNodeId = 'domain:furnace-no-input';
    nodes.push(makeNode(
      furnaceNoNodeId,
      '原始炉号',
      'FurnaceNoInput',
      {
        subtitle: 'Excel导入的原始炉号字符串',
        rawData: {
          description: '原始炉号字符串，如：1甲20251101-1-4-1W脆',
        },
        typeLabel: '输入',
      }
    ));
    if (ACTIVE_NODE_TYPES.has('Ribbon')) {
      addLine(lines, rootId, furnaceNoNodeId, 'hasIdentifier', '原始炉号');
    }

    const parsedNodeId = 'domain:furnace-no-parsed';
    nodes.push(makeNode(
      parsedNodeId,
      '炉号',
      'FurnaceNoParsed',
      {
        subtitle: '解析后的标准炉号',
        rawData: {
          description: '解析后的标准炉号，如：1甲20251101-1-4-1',
        },
        typeLabel: '解析结果',
      }
    ));
    addLine(lines, furnaceNoNodeId, parsedNodeId, 'parsesTo', '解析');

    // 炉号解析字段（按 C# FurnaceNo.cs 正则分组顺序）
    if (ACTIVE_NODE_TYPES.has('FurnaceNoField')) {
      const parseFields = [
        { id: 'line-no', label: '产线号', field: 'LineNo', example: '1, 2, 3...' },
        { id: 'shift', label: '班次', field: 'Shift', example: '甲=1, 乙=2, 丙=3' },
        { id: 'prod-date', label: '生产日期', field: 'ProdDate', example: '2025-11-01' },
        { id: 'furnace-batch', label: '炉次号', field: 'FurnaceBatchNo', example: '1, 2...' },
        { id: 'coil-no', label: '卷号', field: 'CoilNo', example: '4（支持小数）' },
        { id: 'subcoil-no', label: '分卷号', field: 'SubcoilNo', example: '1（支持小数）' },
        { id: 'special-marker', label: '特殊标记', field: 'SpecialMarker', example: 'W 或 w' },
        { id: 'feature-suffix', label: '特性描述', field: 'FeatureSuffix', example: '脆, 硬...' },
      ];
      parseFields.forEach((pf) => {
        const fieldNodeId = `field:furnace:${pf.id}`;
        nodes.push(makeNode(
          fieldNodeId,
          pf.label, // 节点显示中文含义
          'FurnaceNoField',
          {
            subtitle: pf.field, // subtitle 显示 C# 属性名
            rawData: { ...pf, parentType: 'FurnaceNoParsed' },
            typeLabel: pf.field,
          }
        ));
        addLine(lines, parsedNodeId, fieldNodeId, 'hasComponent', pf.field);
      });
    }
  }

  // ── 外观特性体系（聚合根 + 大类聚合 + 特性聚合 + 等级聚合）──
  // 数据来源：LAB_APPEARANCE_FEATURE_CATEGORY / LAB_APPEARANCE_FEATURE / LAB_APPEARANCE_FEATURE_LEVEL
  if (ACTIVE_NODE_TYPES.has('AppearanceFeature')) {
    const categories = data.appearance_categories || [];
    const features = data.appearance_features || [];
    const levels = data.appearance_levels || [];

    // 聚合根：外观特性
    const appearanceRootId = 'domain:appearance';
    nodes.push(makeNode(
      appearanceRootId,
      '外观特性',
      'AppearanceFeature',
      {
        subtitle: `${features.length} 特性 / ${categories.length} 大类 / ${levels.length} 等级`,
        rawData: {
          isAggregate: true,
          isRoot: true,
          description: '带材的外观特性体系（大类 / 特性定义 / 等级）',
          categoryCount: categories.length,
          featureCount: features.length,
          levelCount: levels.length,
        },
        typeLabel: '外观特性聚合根',
      }
    ));
    if (ACTIVE_NODE_TYPES.has('Ribbon')) {
      addLine(lines, rootId, appearanceRootId, 'hasAppearance', '记录外观');
    }

    // 大类节点（不再有聚合节点；顶级大类直接挂在外观特性聚合根下，子级按 F_PARENTID 自连成树）
    // "无父级"信号：parent_id 为空 / '-1' / '0' / 引用了一个不存在的 ID
    if (ACTIVE_NODE_TYPES.has('AppearanceCategory')) {
      const categoryIdSet = new Set(categories.map((c) => c.id));
      categories.forEach((cat) => {
        const catNodeId = `appearance-cat:${cat.id}`;
        nodes.push(makeNode(
          catNodeId,
          cat.name || '大类',
          'AppearanceCategory',
          {
            subtitle: cat.description || '',
            rawData: cat,
            typeLabel: '特性大类',
          }
        ));
        const pid = cat.parent_id;
        const isTopLevel = !pid || pid === '-1' || pid === '0' || !categoryIdSet.has(pid);
        if (isTopLevel) {
          addLine(lines, appearanceRootId, catNodeId, 'contains', '大类');
        } else {
          addLine(lines, `appearance-cat:${pid}`, catNodeId, 'parentCategory', '子分类');
        }
      });
    }

    // 特性节点（不再有聚合节点；每条特性直接挂在所属大类节点下）
    features.forEach((feat) => {
      const featNodeId = `appearance-feat:${feat.id}`;
      // 标题只显示特性名；等级用 subtitle 体现，详细信息在面板里
      nodes.push(makeNode(
        featNodeId,
        feat.name,
        'AppearanceFeature',
        {
          subtitle: feat.level_name || '',
          rawData: feat,
          typeLabel: '外观特性',
        }
      ));
      // 优先挂在所属大类节点下；若大类未启用或缺失，兜底挂到聚合根
      if (feat.category_id && ACTIVE_NODE_TYPES.has('AppearanceCategory')) {
        addLine(lines, `appearance-cat:${feat.category_id}`, featNodeId, 'contains', '特性');
      } else {
        addLine(lines, appearanceRootId, featNodeId, 'contains', '特性');
      }
    });

    // 子聚合 3：特性等级
    if (ACTIVE_NODE_TYPES.has('AppearanceLevel') && levels.length > 0) {
      const levelAggId = 'domain:appearance-level';
      nodes.push(makeNode(
        levelAggId,
        '特性等级',
        'AppearanceLevel',
        {
          subtitle: `共 ${levels.length} 项`,
          rawData: { isAggregate: true, description: 'LAB_APPEARANCE_FEATURE_LEVEL 定义的特性严重等级', levels },
          typeLabel: '特性等级聚合',
        }
      ));
      addLine(lines, appearanceRootId, levelAggId, 'contains', '等级');

      // 每个等级作为子条目
      levels.forEach((lv) => {
        const lvNodeId = `appearance-lv:${lv.id}`;
        const display = lv.is_default ? `${lv.name}·默认` : lv.name;
        nodes.push(makeNode(
          lvNodeId,
          display,
          'AppearanceLevel',
          {
            subtitle: lv.description || '',
            rawData: lv,
            typeLabel: '特性等级',
          }
        ));
        addLine(lines, levelAggId, lvNodeId, 'contains', '等级条目');
      });
    }
  }

  // ── 判定等级体系（lab_intermediate_data_judgment_level）──
  // 拓扑：带材 → 判定等级聚合根 → 各规格判定集 → 单条等级 ⇒ basedOnFormula 引用 JUDGE 公式
  if (ACTIVE_NODE_TYPES.has('JudgmentRule')) {
    const ruleList = data.rules || [];

    if (ruleList.length > 0) {
      // 按 product_spec_id 分组（空值归为"通用判定"）
      const groupBySpec = new Map<string, typeof ruleList>();
      for (const rule of ruleList) {
        const key = rule.product_spec_id || '__shared__';
        if (!groupBySpec.has(key)) groupBySpec.set(key, []);
        groupBySpec.get(key)!.push(rule);
      }

      // 聚合根
      const judgmentRootId = 'domain:judgment-level';
      nodes.push(makeNode(
        judgmentRootId,
        '判定等级',
        'JudgmentRule',
        {
          subtitle: `${ruleList.length} 条 / ${groupBySpec.size} 规格`,
          rawData: {
            isAggregate: true,
            isRoot: true,
            description: 'lab_intermediate_data_judgment_level：每条等级 = (产品规格, JUDGE 公式) 二元组下的具象判定',
            totalCount: ruleList.length,
            specGroupCount: groupBySpec.size,
          },
          typeLabel: '判定等级聚合根',
        }
      ));
      if (ACTIVE_NODE_TYPES.has('Ribbon')) {
        addLine(lines, rootId, judgmentRootId, 'hasJudgment', '判定等级');
      }

      // 按规格分组节点
      const specMap = new Map((data.specs || []).map((s) => [s.id, s] as const));
      groupBySpec.forEach((rulesInSpec, specKey) => {
        const isShared = specKey === '__shared__';
        const specRef = isShared ? null : specMap.get(specKey);
        const specDisplayName = isShared
          ? '通用判定（无规格绑定）'
          : (specRef?.name || rulesInSpec[0].product_spec_name || rulesInSpec[0].product_spec_code || '未知规格');
        const groupNodeId = `judgment-spec:${specKey}`;

        nodes.push(makeNode(
          groupNodeId,
          specDisplayName,
          'JudgmentRule',
          {
            subtitle: `${rulesInSpec.length} 条等级`,
            rawData: {
              isSpecGroup: true,
              specId: isShared ? '' : specKey,
              specName: specDisplayName,
              specCode: specRef?.code || rulesInSpec[0].product_spec_code || '',
              ruleCount: rulesInSpec.length,
              rules: rulesInSpec,
            },
            typeLabel: '规格判定集',
          }
        ));
        addLine(lines, judgmentRootId, groupNodeId, 'contains', specDisplayName);

        // scopedBySpec 引用边连到产品规格条目（如果规格节点存在）
        if (!isShared && specRef && ACTIVE_NODE_TYPES.has('ProductSpec')) {
          addLine(lines, groupNodeId, `spec:${specKey}`, 'scopedBySpec', '适用规格');
        }

        // 每条等级条目
        rulesInSpec.forEach((rule) => {
          const ruleNodeId = `rule:${rule.id}`;
          nodes.push(makeNode(
            ruleNodeId,
            rule.name || rule.quality_status || '等级',
            'JudgmentRule',
            {
              subtitle: rule.formula_name || rule.quality_status || '',
              rawData: rule,
              statusColor: rule.color || ruleStatusColor(rule.quality_status),
              typeLabel: '判定等级',
            }
          ));
          addLine(lines, groupNodeId, ruleNodeId, 'contains', rule.name || '等级');

          // basedOnFormula 引用边连到 JUDGE 公式条目
          if (rule.formula_id && ACTIVE_NODE_TYPES.has('Formula')) {
            addLine(lines, ruleNodeId, `formula:${rule.formula_id}`, 'basedOnFormula', '基于公式');
          }
        });
      });
    }
  }

  // ── 统计指标体系（lab_report_config）──
  // 每条指标 = (JUDGE 公式 F_FORMULA_ID) + 包含等级名称数组 (F_LEVEL_NAMES) + 统计参数
  if (ACTIVE_NODE_TYPES.has('ReportConfig')) {
    const metricList = data.report_configs || [];

    if (metricList.length > 0) {
      const metricRootId = 'domain:metric';
      nodes.push(makeNode(
        metricRootId,
        '指标',
        'ReportConfig',
        {
          subtitle: `共 ${metricList.length} 项`,
          rawData: {
            isAggregate: true,
            isRoot: true,
            description: 'lab_report_config：统计指标定义，每个指标基于一个 JUDGE 公式 + 一组等级名称做聚合统计',
            totalCount: metricList.length,
            metrics: metricList,
          },
          typeLabel: '指标聚合根',
        }
      ));
      if (ACTIVE_NODE_TYPES.has('Ribbon')) {
        addLine(lines, rootId, metricRootId, 'hasMetric', '统计指标');
      }

      // 各指标条目
      metricList.forEach((m) => {
        const metricNodeId = `metric:${m.id}`;
        const lvlSummary = (m.level_names || []).length > 0 ? `[${m.level_names.join('/')}]` : '';
        nodes.push(makeNode(
          metricNodeId,
          m.name || '指标',
          'ReportConfig',
          {
            subtitle: lvlSummary,
            rawData: m,
            typeLabel: m.is_percentage ? '占比指标' : '汇总指标',
          }
        ));
        addLine(lines, metricRootId, metricNodeId, 'contains', m.name);

        // measuresBy 引用边连到 JUDGE 公式条目
        if (m.formula_id && ACTIVE_NODE_TYPES.has('Formula')) {
          addLine(lines, metricNodeId, `formula:${m.formula_id}`, 'measuresBy', '基于公式');
        }
      });
    }
  }

  // ── 公式体系（聚合根 + 3 类型分组节点 + 各公式条目）──
  // 数据来源：lab_intermediate_data_formula；F_FORMULA_TYPE 三个值 CALC/JUDGE/NO
  if (ACTIVE_NODE_TYPES.has('Formula')) {
    const formulaList = data.formulas || [];

    // 规范化 type；兼容 '1'/'2'/'3'
    const normalizeType = (t?: string): 'CALC' | 'JUDGE' | 'NO' | 'OTHER' => {
      const s = String(t || '').toUpperCase().trim();
      if (s === 'CALC' || s === '1') return 'CALC';
      if (s === 'JUDGE' || s === '2') return 'JUDGE';
      if (s === 'NO' || s === '3') return 'NO';
      return 'OTHER';
    };

    const calcList = formulaList.filter((f) => normalizeType(f.formula_type) === 'CALC');
    const judgeList = formulaList.filter((f) => normalizeType(f.formula_type) === 'JUDGE');
    const noList = formulaList.filter((f) => normalizeType(f.formula_type) === 'NO');

    // 聚合根
    const formulaRootId = 'domain:formula';
    nodes.push(makeNode(
      formulaRootId,
      '公式',
      'Formula',
      {
        subtitle: `${formulaList.length} 项（计算 ${calcList.length} / 判定 ${judgeList.length} / 只展示 ${noList.length}）`,
        rawData: {
          isAggregate: true,
          isRoot: true,
          description: 'lab_intermediate_data_formula 中间数据公式 / 判定 / 只展示列',
          totalCount: formulaList.length,
          calcCount: calcList.length,
          judgeCount: judgeList.length,
          noCount: noList.length,
        },
        typeLabel: '公式聚合根',
      }
    ));
    if (ACTIVE_NODE_TYPES.has('Ribbon')) {
      addLine(lines, rootId, formulaRootId, 'hasFormula', '中间数据公式');
    }

    // 三个类型分组节点
    const typeGroups: Array<{ key: 'CALC' | 'JUDGE' | 'NO'; label: string; list: typeof formulaList }> = [
      { key: 'CALC', label: '计算公式', list: calcList },
      { key: 'JUDGE', label: '判定公式', list: judgeList },
      { key: 'NO', label: '只展示', list: noList },
    ];

    typeGroups.forEach((grp) => {
      const groupNodeId = `domain:formula-type:${grp.key}`;
      nodes.push(makeNode(
        groupNodeId,
        grp.label,
        'Formula',
        {
          subtitle: `共 ${grp.list.length} 项`,
          rawData: {
            isTypeGroup: true,
            formulaType: grp.key,
            label: grp.label,
            description: grp.key === 'CALC'
              ? '通过表达式计算得到的列（F_FORMULA_TYPE = CALC）'
              : grp.key === 'JUDGE'
                ? '基于条件规则判定等级的列（F_FORMULA_TYPE = JUDGE）'
                : '仅前端展示、不参与计算与判定的列（F_FORMULA_TYPE = NO）',
            formulas: grp.list,
          },
          typeLabel: `${grp.label}类型组`,
        }
      ));
      addLine(lines, formulaRootId, groupNodeId, 'contains', grp.label);

      // 各公式条目
      grp.list.forEach((f) => {
        const fNodeId = `formula:${f.id}`;
        nodes.push(makeNode(
          fNodeId,
          f.formula_name || f.column_name || '公式',
          'Formula',
          {
            subtitle: f.unit_name || f.column_name || '',
            rawData: f,
            typeLabel: grp.label,
            statusColor: f.is_enabled === false ? '#9CA3AF' : undefined,
          }
        ));
        addLine(lines, groupNodeId, fNodeId, 'contains', f.formula_name || f.column_name);
      });
    });
  }

  return {
    rootId,
    nodes,
    lines,
  };
}

// ------------------------------------------------------------------
// 规格中心子图 → RGJsonData
// ------------------------------------------------------------------

export function buildSpecSubgraph(data: OntologyData, specId: string): RGJsonData {
  const idx = buildIndexes(data);
  const { rulesBySpec, rulesByFormula, formulaMap } = idx;
  const nodes: any[] = [];
  const lines: any[] = [];

  const spec = data.specs.find((s) => s.id === specId);
  if (!spec) return { rootId: 'domain:ribbon', nodes: [], lines: [] };

  const specRules = (rulesBySpec[specId] || []).slice(0, 48);
  const formulaIds = new Set<string>(specRules.map((r) => r.formula_id).filter((id): id is string => Boolean(id)));
  const rootId = 'domain:ribbon';

  nodes.push(makeNode(rootId, '带材', 'Ribbon', { subtitle: '业务根节点', rawData: { name: '带材' } }));
  nodes.push(makeNode(
    `spec:${spec.id}`,
    spec.name || spec.code || '产品规格',
    'ProductSpec',
    { subtitle: spec.code, rawData: spec, ruleCount: specRules.length, formulaCount: formulaIds.size }
  ));
  addLine(lines, rootId, `spec:${spec.id}`, 'BELONGS_TO_SPEC', '适用规格');

  specRules.forEach((rule, i) => {
    nodes.push(makeNode(
      `rule:${rule.id}`,
      rule.name || rule.quality_status || '判定规则',
      'JudgmentRule',
      { subtitle: rule.quality_status || '规则', rawData: rule, statusColor: ruleStatusColor(rule.quality_status) }
    ));
    addLine(lines, `spec:${spec.id}`, `rule:${rule.id}`, 'HAS_RULE', '判定规则');
  });

  const formulas = Array.from(formulaIds).map((fid) => formulaMap[fid]).filter(Boolean);
  formulas.forEach((formula, i) => {
    nodes.push(makeNode(
      `formula:${formula.id}`,
      formula.formula_name || formula.column_name || '公式',
      'Formula',
      { subtitle: formulaTypeLabel(formula), rawData: formula, typeLabel: formulaTypeLabel(formula), ruleCount: rulesByFormula[formula.id]?.length || 0 }
    ));
  });

  for (const rule of specRules) {
    if (rule.formula_id && formulaMap[rule.formula_id]) {
      addLine(lines, `rule:${rule.id}`, `formula:${rule.formula_id}`, 'USES_FORMULA', '依据公式');
    }
  }

  return { rootId, nodes, lines };
}

// ------------------------------------------------------------------
// 带子图 → RGJsonData
// ------------------------------------------------------------------

function typeRank(type: string) {
  const order: Record<string, number> = {
    Ribbon: 0,
    ProductSpec: 1,
    LaminationData: 2,
    SingleSheetPerf: 3,
    AppearanceFeature: 4,
    JudgmentRule: 5,
    Formula: 6,
  };
  return order[type] ?? 9;
}

export function buildRibbonRelationGraph(response: RibbonSubgraphResponse): RGJsonData {
  const rawNodes = [...(response.nodes || [])].sort((a: any, b: any) => typeRank(a.type) - typeRank(b.type));
  const typeCounters: Record<string, number> = {};
  const typeTotals = rawNodes.reduce((acc: Record<string, number>, n: any) => {
    acc[n.type] = (acc[n.type] || 0) + 1;
    return acc;
  }, {});

  const nodes: any[] = [];
  const lines: any[] = [];

  rawNodes.forEach((node: any) => {
    const t = node.type || node.dataType;
    const index = typeCounters[t] || 0;
    typeCounters[t] = index + 1;
    const total = typeTotals[t] || 1;

    const baseRaw = node.raw || {};
    const enrichedRaw = t === 'Ribbon'
      ? { ...(response.ribbon as any), ...((response.ribbon as any)?.raw || {}), ...baseRaw }
      : baseRaw;

    nodes.push(makeNode(
      node.id,
      node.label || node.name || '',
      t,
      {
        subtitle: enrichedRaw.subtitle || enrichedRaw.code || '',
        rawData: enrichedRaw,
      }
    ));
  });

  (response.edges || []).forEach((e: any) => {
    addLine(lines, e.source, e.target, e.relation || e.dataType || 'RELATED', e.label);
  });

  return {
    rootId: response.ribbon?.id || 'domain:ribbon',
    nodes,
    lines,
  };
}
