/**
 * 月度驾驶舱共享类型定义
 *
 * Web 与 mobile 共享同一份数据协议，图表渲染层各自实现。
 */

import type { ChartDescriptor } from "./index";

/** KPI 卡片 */
export interface DashboardKpiCard {
  title: string;
  value: string;
  unit: string;
  note?: string;
  accent: string;
  icon: string;
}

/** 明细摘要行 */
export interface DashboardDetailRow {
  date: string;
  shift: string;
  totalWeight: string;
  qualifiedRate: string;
  focus: string;
}

/** 班次摘要 */
export interface DashboardShiftSummary {
  label: string;
  score: string;
  note: string;
}

/** 月度驾驶舱载荷 */
export interface MonthlyDashboardPayload {
  title: string;
  subtitle: string;
  dateRange: {
    start: string;
    end: string;
  };
  hint?: string;
  kpiCards: DashboardKpiCard[];
  laminationTrend: ChartDescriptor;
  qualityDistribution: ChartDescriptor;
  shiftComparison: ChartDescriptor;
  qualityTrend: ChartDescriptor;
  thicknessCorrelation: ChartDescriptor;
  unqualifiedTop5: ChartDescriptor;
  shiftSummaries: DashboardShiftSummary[];
  detailRows: DashboardDetailRow[];
}
