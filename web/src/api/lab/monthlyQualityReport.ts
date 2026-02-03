import { defHttp } from '/@/utils/http/axios';

enum Api {
    Prefix = '/api/lab/monthly-quality-report',
}

/** 查询条件 */
export interface MonthlyReportQueryParams {
    startDate: string;
    endDate: string;
    shift?: string;
    shiftNo?: string;
    productSpecCode?: string;
    lineNo?: number;
}

/** 汇总指标 */
export interface SummaryData {
    totalWeight: number;
    qualifiedRate: number;
    classAWeight: number;
    classARate: number;
    classBWeight: number;
    classBRate: number;
    unqualifiedWeight: number;
    unqualifiedRate: number;
}

/** 明细行 */
export interface DetailRow {
    prodDate: string | null;
    shift: string;
    shiftNo: string;
    productSpecCode: string;
    detectionWeight: number;
    classAWeight: number;
    classARate: number;
    classBWeight: number;
    classBRate: number;
    unqualifiedWeight: number;
    qualifiedRate: number;
    unqualifiedCategories: Record<string, number>;
    isSummaryRow: boolean;
    summaryType?: string;
}

/** 班组统计行 */
export interface ShiftGroupRow {
    shift: string;
    productSpecCode: string;
    detectionWeight: number;
    classAWeight: number;
    classARate: number;
    classBWeight: number;
    classBRate: number;
    unqualifiedWeight: number;
    qualifiedRate: number;
    isSummaryRow: boolean;
    summaryType?: string;
}

/** 质量趋势数据 */
export interface QualityTrend {
    date: string;
    qualifiedRate: number;
    classARate: number;
    classBRate: number;
}

/** 不合格分类 */
export interface UnqualifiedCategory {
    categoryName: string;
    weight: number;
    rate: number;
}

/** 班次对比 */
export interface ShiftComparison {
    shift: string;
    totalWeight: number;
    qualifiedRate: number;
    classARate: number;
}

/** 不合格分类列信息 */
export interface JudgmentLevelColumn {
    id: string;
    code: string;
    name: string;
    qualityStatus: number;
    color: string;
}

/** 完整报表响应 */
export interface MonthlyReportResponse {
    summary: SummaryData;
    details: DetailRow[];
    shiftGroups: ShiftGroupRow[];
    qualityTrends: QualityTrend[];
    unqualifiedCategories: UnqualifiedCategory[];
    shiftComparisons: ShiftComparison[];
    unqualifiedColumns: JudgmentLevelColumn[];
}

// 获取完整的月度质量报表数据
export function getMonthlyReport(params: MonthlyReportQueryParams) {
    return defHttp.get<MonthlyReportResponse>({ url: Api.Prefix, params });
}

// 获取顶部汇总指标
export function getReportSummary(params: MonthlyReportQueryParams) {
    return defHttp.get<SummaryData>({ url: Api.Prefix + '/summary', params });
}

// 获取明细表格数据
export function getReportDetails(params: MonthlyReportQueryParams) {
    return defHttp.get<DetailRow[]>({ url: Api.Prefix + '/details', params });
}

// 获取班组统计数据
export function getReportShiftGroups(params: MonthlyReportQueryParams) {
    return defHttp.get<ShiftGroupRow[]>({ url: Api.Prefix + '/shift-groups', params });
}

// 获取质量趋势图表数据
export function getQualityTrend(params: MonthlyReportQueryParams) {
    return defHttp.get<QualityTrend[]>({ url: Api.Prefix + '/quality-trend', params });
}

// 获取不合格分类统计
export function getUnqualifiedCategories(params: MonthlyReportQueryParams) {
    return defHttp.get<UnqualifiedCategory[]>({ url: Api.Prefix + '/unqualified-categories', params });
}

// 获取班次对比数据
export function getShiftComparison(params: MonthlyReportQueryParams) {
    return defHttp.get<ShiftComparison[]>({ url: Api.Prefix + '/shift-comparison', params });
}
