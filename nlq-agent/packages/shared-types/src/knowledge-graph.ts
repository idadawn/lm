/**
 * 知识图谱类型定义
 */

// ============================================================================
// 产品规格类型
// ============================================================================

/** 产品规格属性（简化版） */
export interface SpecAttribute {
  name: string;
  value: string;
}

/** 产品规格 */
export interface ProductSpec {
  id: string;
  code: string;
  name: string;
  attributes: SpecAttribute[];
}

/** 产品规格详情 */
export interface ProductSpecDetail {
  code: string;
  attributes: SpecAttribute[];
  judgment_types: JudgmentType[];
}

// ============================================================================
// 指标类型
// ============================================================================

/** 指标公式 */
export interface MetricFormula {
  id: string;
  name: string;
  columnName: string;
  formula: string;
  unit: string;
  formulaType: string;
  description?: string;
}

// ============================================================================
// 判定规则类型
// ============================================================================

/** 判定类型 */
export interface JudgmentType {
  formula_id: string;
  name: string;
  rule_count: number;
}

/** 判定规则 */
export interface JudgmentRule {
  id: string;
  formulaId: string;
  name: string;
  priority: number;
  qualityStatus: string;
  color: string;
  isDefault: boolean;
  conditionJson?: string;
  spec_code?: string;
  spec_name?: string;
}

// ============================================================================
// 报表配置类型
// ============================================================================

/** 一次交检配置 */
export interface FirstInspectionConfig {
  grades: string[];
  description: string;
}

/** 报表配置 */
export interface ReportConfig {
  id: string;
  code: string;
  name: string;
  value: string;
  description?: string;
}

// ============================================================================
// 知识图谱健康状态
// ============================================================================

/** 知识图谱健康状态 */
export interface KnowledgeGraphHealth {
  ready: boolean;
  backend: "neo4j" | "networkx";
}

// ============================================================================
// API 响应类型
// ============================================================================

/** 刷新响应 */
export interface RefreshResponse {
  message: string;
}

/** 规则搜索结果 */
export interface RuleSearchResult extends JudgmentRule {
  spec_code?: string;
  spec_name?: string;
}

// ============================================================================
// 外观特性类型
// ============================================================================

/** 外观特性等级 */
export interface AppearanceFeatureLevel {
  id: string;
  name: string;
  description?: string;
}

/** 外观特性大类 */
export interface AppearanceFeatureCategory {
  id: string;
  name: string;
  description?: string;
  parentId?: string;
}

/** 外观特性 */
export interface AppearanceFeature {
  id: string;
  name: string;
  keywords?: string;
  description?: string;
  category: AppearanceFeatureCategory;
  level: AppearanceFeatureLevel;
}

// ============================================================================
// 报表配置类型
// ============================================================================

/** 报表配置 */
export interface ReportConfig {
  id: string;
  name: string;
  description?: string;
  formulaId?: string;
  levelNames?: string[];
  isSystem: boolean;
  sortOrder: number;
  isHeader: boolean;
  isPercentage: boolean;
  isShowInReport: boolean;
  isShowRatio: boolean;
  metric?: MetricFormula;
}
