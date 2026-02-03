import { defHttp } from '/@/utils/http/axios';

// 等级统计（合格等级需要 重量+占比）
export interface LevelStat {
    weight: number;
    rate: number;
}

// 等级列定义
export interface JudgmentLevelColumn {
    id: string;
    code: string;
    name: string;
    qualityStatus: 'Qualified' | 'Unqualified';
    color?: string;
    priority?: number;
}

// 汇总数据
export interface SummaryData {
    totalWeight: number;
    qualifiedRate: number;
    qualifiedCategories: Record<string, LevelStat>;  // { "A": { weight: 100, rate: 50 } }
    qualifiedWeight: number;
    unqualifiedCategories: Record<string, number>;   // { "性能不合": 50 }
    unqualifiedWeight: number;
    unqualifiedRate: number;
}

// 明细行
export interface DetailRow {
    prodDate: string | Date | null;
    shift: string;
    shiftNo: string;
    productSpecCode: string;
    detectionWeight: number;
    qualifiedCategories: Record<string, LevelStat>;  // 合格分类
    qualifiedWeight: number;
    qualifiedRate: number;
    unqualifiedCategories: Record<string, number>;   // 不合格分类
    isSummaryRow?: boolean;
    summaryType?: string;
}

// 班组统计行
export interface ShiftGroupRow {
    shift: string;
    productSpecCode: string;
    detectionWeight: number;
    qualifiedCategories: Record<string, LevelStat>;
    qualifiedWeight: number;
    qualifiedRate: number;
    isSummaryRow?: boolean;
    summaryType?: string;
}

// 质量趋势
export interface QualityTrend {
    date: string;
    qualifiedRate: number;
    classARate: number;
    classBRate: number;
}

// 不合格分类
export interface UnqualifiedCategory {
    categoryName: string;
    weight: number;
    rate: number;
}

// 班次对比
export interface ShiftComparison {
    shift: string;
    totalWeight: number;
    qualifiedRate: number;
    classARate: number;
}

// 查询参数
export interface MonthlyReportQueryParams {
    startDate: string;
    endDate: string;
    shift?: string;
    shiftNo?: string;
    productSpecCode?: string;
    lineNo?: number;
}

// 响应
export interface MonthlyReportResponse {
    summary: SummaryData;
    details: DetailRow[];
    shiftGroups: ShiftGroupRow[];
    qualityTrends: QualityTrend[];
    unqualifiedCategoryStats: UnqualifiedCategory[];
    shiftComparisons: ShiftComparison[];
    qualifiedColumns: JudgmentLevelColumn[];    // 合格等级列定义
    unqualifiedColumns: JudgmentLevelColumn[];  // 不合格等级列定义
}

// 列定义响应
export interface ColumnsResponse {
    qualifiedColumns: JudgmentLevelColumn[];
    unqualifiedColumns: JudgmentLevelColumn[];
}

// API
export function getMonthlyReport(params: MonthlyReportQueryParams) {
    return defHttp.get<MonthlyReportResponse>({
        url: '/api/lab/monthly-quality-report',
        params,
    });
}

// 获取表格列定义
export function getMonthlyReportColumns() {
    return defHttp.get<ColumnsResponse>({
        url: '/api/lab/monthly-quality-report/columns',
    });
}

// 导出报表
export function exportMonthlyReport(params: MonthlyReportQueryParams) {
    return defHttp.get({
        url: '/api/lab/monthly-quality-report/export',
        params,
        responseType: 'blob',
    }, { isReturnNativeResponse: true }).then((response) => {
        // 从响应头获取文件名
        const contentDisposition = response?.headers?.['content-disposition'];
        let filename = `月度质量报表_${params.startDate}_${params.endDate}.xlsx`;

        if (contentDisposition) {
            const match = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
            if (match && match[1]) {
                filename = match[1].replace(/['"]/g, '');
            }
        }

        // 创建下载链接
        const blob = new Blob([response.data], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
    });
}
