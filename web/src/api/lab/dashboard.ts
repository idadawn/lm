import { defHttp } from '/@/utils/http/axios';

// 查询参数
export interface DashboardQueryParams {
  startDate: string;
  endDate: string;
  shift?: string;
}

// KPI卡片数据
export interface DashboardKpiDto {
  totalWeight: number;
  qualifiedRate: number;
  laminationFactorAvg: number;
  laminationFactorTrend: number[];
  warnings: DashboardWarning[];
}

export interface DashboardWarning {
  type: 'quality' | 'process' | 'device';
  message: string;
  level: 'info' | 'warning' | 'error';
}

// 质量分布数据
export interface QualityDistributionDto {
  category: string;  // A级、B级、性能不合、其他不合
  count: number;
  weight: number;
  rate: number;
  color?: string;
}

// 叠片系数趋势数据
export interface LaminationTrendData {
  date: string;
  value: number;
  min: number;
  max: number;
}

// 缺陷Top5数据
export interface DefectTopData {
  category: string;
  count: number;
  weight: number;
}

// 生产热力图数据
export interface HeatmapData {
  dayOfWeek: number;  // 0-6 (周一到周日)
  hour: number;       // 0-23
  value: number;      // 合格率或叠片系数
  count: number;      // 样本数量
}

// 厚度-叠片系数关联数据
export interface ScatterData {
  thickness: number;
  laminationFactor: number;
  qualityLevel: string;  // A级、B级等
  id: string;
}

// API接口
export function getDashboardKpi(params: DashboardQueryParams) {
  return defHttp.get<DashboardKpiDto>({
    url: '/api/lab/dashboard/kpi',
    params,
  });
}

export function getQualityDistribution(params: DashboardQueryParams) {
  return defHttp.get<QualityDistributionDto[]>({
    url: '/api/lab/dashboard/quality-distribution',
    params,
  });
}

export function getLaminationTrend(params: DashboardQueryParams) {
  return defHttp.get<LaminationTrendData[]>({
    url: '/api/lab/dashboard/lamination-trend',
    params,
  });
}

export function getDefectTop5(params: DashboardQueryParams) {
  return defHttp.get<DefectTopData[]>({
    url: '/api/lab/dashboard/defect-top5',
    params,
  });
}

export function getProductionHeatmap(params: DashboardQueryParams) {
  return defHttp.get<HeatmapData[]>({
    url: '/api/lab/dashboard/production-heatmap',
    params,
  });
}

export function getThicknessCorrelation(params: DashboardQueryParams) {
  return defHttp.get<ScatterData[]>({
    url: '/api/lab/dashboard/thickness-correlation',
    params,
  });
}
