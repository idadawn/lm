import { defHttp } from '/@/utils/http/axios';

enum Api {
  Prefix = '/api/lab/magnetic-data-import-session',
}

// ========== 磁性数据导入相关API ==========

// 创建导入会话
export async function createMagneticImportSession(data: {
  fileName: string;
  fileData?: string;
}): Promise<string> {
  const res = await defHttp.post({ url: Api.Prefix + '/create', data });
  // Handle case where result is wrapped in { code, data, ... } or direct string
  if (typeof res === 'string') return res;
  return res.data || res.id || res.Id || '';
}

// 获取导入会话详情
export function getMagneticImportSession(id: string): Promise<any> {
  return defHttp.get({ url: Api.Prefix + '/' + id });
}

// 更新导入会话状态
export function updateMagneticImportSessionStatus(id: string, status: string): Promise<any> {
  return defHttp.put({ url: Api.Prefix + '/' + id + '/status', data: status });
}

// 更新导入会话步骤
export function updateMagneticImportSessionStep(id: string, step: number): Promise<any> {
  return defHttp.put({ url: Api.Prefix + '/' + id + '/step', data: step });
}

// 删除导入会话
export function deleteMagneticImportSession(id: string): Promise<void> {
  return defHttp.delete({ url: Api.Prefix + '/' + id });
}

// ========== 第一步：文件上传与解析 ==========

// 上传文件并解析（第一步保存）
export async function uploadAndParseMagneticData(
  sessionId: string,
  data: {
    fileName?: string;
    fileData?: string;
  },
): Promise<any> {
  const response = await defHttp.post({
    url: Api.Prefix + '/' + sessionId + '/step1/upload-and-parse',
    data,
  });

  // Unwrap response if wrapped in { code, data, ... }
  const actualData = response.data || response;

  return {
    importSessionId: actualData.importSessionId || actualData.ImportSessionId || sessionId,
    parsedData: actualData.parsedData || actualData.ParsedData || [],
    totalRows: actualData.totalRows || actualData.TotalRows || 0,
    validDataRows: actualData.validDataRows || actualData.ValidDataRows || 0,
    errors: actualData.errors || actualData.Errors || [],
  };
}

// ========== 第二步：数据核对与完成 ==========

// 获取数据核对结果
export async function getMagneticImportReview(sessionId: string): Promise<any> {
  const response = await defHttp.get({ url: Api.Prefix + '/' + sessionId + '/review' });

  // 处理后端返回的 PascalCase 字段名
  const data = response.data || response;
  const validData = data.validData || data.ValidData || [];
  // 转换字段名为 camelCase
  const processedValidData = validData.map((item: any) => ({
    rowIndex: item.rowIndex ?? item.RowIndex ?? 0,
    originalFurnaceNo: item.originalFurnaceNo ?? item.OriginalFurnaceNo ?? '',
    furnaceNo: item.furnaceNo ?? item.FurnaceNo ?? '',
    isScratched: item.isScratched ?? item.IsScratched ?? false,
    psLoss: item.psLoss ?? item.PsLoss ?? null,
    ssPower: item.ssPower ?? item.SsPower ?? null,
    hc: item.hc ?? item.Hc ?? null,
    detectionTime: item.detectionTime ?? item.DetectionTime ?? null,
    isValid: item.isValid ?? item.IsValid ?? true,
    errorMessage: item.errorMessage ?? item.ErrorMessage ?? '',
    isBest: item.isBest ?? item.IsBest ?? false,
  }));
  return {
    session: data.session || data.Session,
    totalRows: data.totalRows ?? data.TotalRows ?? 0,
    validDataRows: data.validDataRows ?? data.ValidDataRows ?? 0,
    updatedRows: data.updatedRows ?? data.UpdatedRows ?? 0,
    skippedRows: data.skippedRows ?? data.SkippedRows ?? 0,
    errors: data.errors || data.Errors || [],
    validData: processedValidData,
  };
}

// 完成导入，更新中间数据表
export function completeMagneticImport(sessionId: string): Promise<void> {
  if (!sessionId || sessionId.trim() === '') {
    return Promise.reject(new Error('导入会话ID不能为空'));
  }
  return defHttp.post({ url: Api.Prefix + '/' + sessionId + '/complete' });
}

// ========== 磁性原始数据查询相关API ==========

enum RawDataApi {
  Prefix = '/api/lab/magnetic-raw-data',
}

// 获取磁性原始数据列表
export async function getMagneticRawDataList(params: any): Promise<any> {
  // BasicTable 内部会从返回值的 .data 字段提取数据
  // 所以这里直接返回完整响应（包含 code, data, msg）
  return defHttp.get({ url: RawDataApi.Prefix, params });
}

// 删除磁性原始数据
export function deleteMagneticRawData(id: string): Promise<void> {
  return defHttp.delete({ url: RawDataApi.Prefix + '/' + id });
}
