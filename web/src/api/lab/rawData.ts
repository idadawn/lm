import { defHttp } from '/@/utils/http/axios';
import { downloadByData } from '/@/utils/file/download';
import type {
  ImportSession,
  Step1UploadAndParseInput,
  Step1UploadAndParseOutput,
  Step2ProductSpecInput,
  Step3AppearanceFeatureInput,
  Step4ReviewOutput,
  DataPreviewResult,
  ImportLog,
} from './types/rawData';

enum Api {
  Prefix = '/api/lab/raw-data',
  ImportSessionPrefix = '/api/lab/raw-data-import-session',
}

// ========== 分步导入向导相关API ==========

// 创建导入会话（后端返回的是字符串 sessionId，不是对象）
export async function createImportSession(data): Promise<string> {
  const res = await defHttp.post({ url: Api.ImportSessionPrefix + '/create', data });
  // Handle case where result is wrapped in { code, data, ... } or direct string
  if (typeof res === 'string') return res;
  return res.data || res.id || res.Id || '';
}

// 获取导入会话详情
export function getImportSession(id: string): Promise<ImportSession> {
  return defHttp.get({ url: Api.ImportSessionPrefix + '/' + id });
}

// 更新导入会话状态
export function updateImportSessionStatus(id: string, status: string): Promise<ImportSession> {
  return defHttp.put({ url: Api.ImportSessionPrefix + '/' + id + '/status', data: status });
}

// 更新导入会话步骤
export function updateImportSessionStep(id: string, step: number): Promise<ImportSession> {
  return defHttp.put({ url: Api.ImportSessionPrefix + '/' + id + '/step', data: step });
}

// 更新导入会话元数据（第一步专用）
export function updateImportSessionMetadata(
  id: string,
  data: {
    fileName?: string;
    totalRows?: number;
    validDataRows?: number;
    importStrategy?: string;
  },
): Promise<ImportSession> {
  return defHttp.put({ url: Api.ImportSessionPrefix + '/' + id + '/metadata', data });
}

// 更新导入会话（包含步骤和元数据）
export async function updateImportSession(id: string, data: any): Promise<ImportSession> {
  // 解构数据，分离不同类型的更新
  const { currentStep, status, fileName, totalRows, validDataRows, importStrategy } = data;

  // 如果有元数据更新（第一步专用）
  const hasMetadata =
    fileName !== undefined || totalRows !== undefined || validDataRows !== undefined || importStrategy !== undefined;
  if (hasMetadata) {
    const metadata: any = {};
    if (fileName !== undefined) metadata.fileName = fileName;
    if (totalRows !== undefined) metadata.totalRows = totalRows;
    if (validDataRows !== undefined) metadata.validDataRows = validDataRows;
    if (importStrategy !== undefined) metadata.importStrategy = importStrategy;

    // 注意：这里假设后端有/metadata接口，如果没有需要创建
    // 临时解决方案：直接返回当前会话，不更新元数据
  }

  // 如果有步骤信息，更新步骤
  if (currentStep !== undefined) {
    await updateImportSessionStep(id, currentStep);
  }

  // 如果有状态信息，更新状态
  if (status !== undefined) {
    await updateImportSessionStatus(id, status);
  }

  // 返回最新的会话信息
  return getImportSession(id);
}

// 删除导入会话
export function deleteImportSession(id: string): Promise<void> {
  return defHttp.delete({ url: Api.ImportSessionPrefix + '/' + id });
}

// 获取未完成的导入会话列表
export function getPendingImportSessions(): Promise<ImportSession[]> {
  return defHttp.get({ url: Api.ImportSessionPrefix + '/pending' });
}

// ========== 第一步：文件上传与解析 ==========

// 预览/解析Excel数据（用于数据核对）
export function previewRawData(data): Promise<DataPreviewResult> {
  return defHttp.post({ url: Api.Prefix + '/preview', data });
}

// 上传文件并解析（第一步保存）
export async function uploadAndParse(data: Step1UploadAndParseInput): Promise<Step1UploadAndParseOutput> {
  const response = await defHttp.post({ url: Api.ImportSessionPrefix + '/step1/upload-and-parse', data });

  // Unwrap response if wrapped in { code, data, ... }
  const actualData = response.data || response;

  // 转换后端返回的数据格式为前端期望的格式
  // 后端返回的字段可能是PascalCase或camelCase，需要兼容处理
  const previewDataRaw = actualData.previewData || actualData.PreviewData || response.previewData || response.PreviewData;
  if (previewDataRaw) {
    const parsedData = previewDataRaw.parsedData || previewDataRaw.ParsedData || [];
    const headerOrder = previewDataRaw.headerOrder || previewDataRaw.HeaderOrder || [];

    // 转换为前端期望的格式
    const convertedPreview: DataPreviewResult = {
      headers: headerOrder,
      rows: parsedData.map((item: any) => {
        // 处理检测数据：优先使用detection1-detection22字段，如果没有则从detectionData JSON解析（向后兼容）
        let detectionDataObj = {};
        // 检查是否有detection1字段（新格式）
        if (item.detection1 !== undefined) {
          // 从detection1-detection22字段构建detectionData对象
          for (let i = 1; i <= 22; i++) {
            const key = `detection${i}`;
            if (item[key] !== null && item[key] !== undefined) {
              detectionDataObj[i] = item[key];
            }
          }
        } else if (item.detectionData) {
          // 旧格式：从JSON解析（向后兼容）
          if (typeof item.detectionData === 'string') {
            try {
              detectionDataObj = JSON.parse(item.detectionData);
            } catch {
              detectionDataObj = {};
            }
          } else {
            detectionDataObj = item.detectionData;
          }
        }

        return {
          id: item.id,
          prodDate: item.prodDate || item.ProdDate,
          furnaceNo: item.furnaceNo || item.FurnaceNo,
          lineNo: item.lineNo || item.LineNo,
          shift: item.shift || item.Shift,
          width: item.width || item.Width,
          coilWeight: item.coilWeight || item.CoilWeight,
          detectionData: detectionDataObj,
          // 同时保留detection1-detection22字段（如果存在）
          ...(item.detection1 !== undefined ? {
            detection1: item.detection1, detection2: item.detection2, detection3: item.detection3,
            detection4: item.detection4, detection5: item.detection5, detection6: item.detection6,
            detection7: item.detection7, detection8: item.detection8, detection9: item.detection9,
            detection10: item.detection10, detection11: item.detection11, detection12: item.detection12,
            detection13: item.detection13, detection14: item.detection14, detection15: item.detection15,
            detection16: item.detection16, detection17: item.detection17, detection18: item.detection18,
            detection19: item.detection19, detection20: item.detection20, detection21: item.detection21,
            detection22: item.detection22
          } : {}),
          isValidData: (item.isValidData === 1 || item.IsValidData === 1),
          furnaceNoParsed: item.furnaceNoParsed || item.FurnaceNoParsed || item.furnaceNoParsed,
          coilNo: item.coilNo || item.CoilNo,
          subcoilNo: item.subcoilNo || item.SubcoilNo,
          featureSuffix: item.featureSuffix || item.FeatureSuffix,
          productSpecId: item.productSpecId || item.ProductSpecId,
          productSpecName: item.productSpecName || item.ProductSpecName,
          rowIndex: 0, // 将在组件中设置
          // 炉号重复相关字段
          status: item.status || item.Status || (item.isValidData ? 'success' : 'failed'),
          errorMessage: item.errorMessage || item.ErrorMessage,
          isDuplicateInFile: item.isDuplicateInFile || item.IsDuplicateInFile || false,
          existsInDatabase: item.existsInDatabase || item.ExistsInDatabase || false,
          standardFurnaceNo: item.standardFurnaceNo || item.StandardFurnaceNo,
          selectedForImport: item.selectedForImport || item.SelectedForImport,
        };
      }),
      statistics: {
        totalRows: actualData.totalRows || actualData.TotalRows || 0,
        successRows: actualData.validDataRows || actualData.ValidDataRows || 0,
        failRows: (actualData.totalRows || actualData.TotalRows || 0) - (actualData.validDataRows || actualData.ValidDataRows || 0),
        validDataRows: actualData.validDataRows || actualData.ValidDataRows || 0,
        invalidDataRows: (actualData.totalRows || actualData.TotalRows || 0) - (actualData.validDataRows || actualData.ValidDataRows || 0),
        matchedProductSpecRows: 0,
        matchedAppearanceFeatureRows: 0,
      },
      errors: parsedData
        .filter((item: any) => {
          const status = item.status || item.Status;
          const isValid = item.isValidData === 1 || item.IsValidData === 1;
          return status === 'failed' || !isValid;
        })
        .map((item: any, index: number) => ({
          rowIndex: index,
          rowData: item,
          errors: (item.errorMessage || item.ErrorMessage) ? [item.errorMessage || item.ErrorMessage] : ['数据验证失败'],
        })),
    };

    return {
      importSessionId: response.importSessionId || response.ImportSessionId,
      preview: convertedPreview,
    };
  }

  // 如果没有预览数据，返回原始响应
  return response;
}

// ========== 第二步：数据预览与重复数据处理 ==========

// 更新重复数据的选择结果（将未选择的数据标记为无效）
export function updateDuplicateSelections(importSessionId: string, data: { rawDataId: string; isValidData: boolean }[]): Promise<void> {
  const url = Api.ImportSessionPrefix + '/' + importSessionId + '/duplicate-selections';
  
  return defHttp.put({ 
    url: url, 
    data: { items: data } 
  }).then((response) => {
    return response;
  }).catch((error) => {
    throw error;
  });
}

// ========== 第二步：产品规格识别 ==========

// 获取产品规格匹配结果
export function getProductSpecMatches(importSessionId: string): Promise<any> {
  return defHttp.get({ url: Api.ImportSessionPrefix + '/' + importSessionId + '/product-specs' });
}

// 批量更新产品规格
export function updateProductSpecMatches(importSessionId: string, data: Step2ProductSpecInput): Promise<void> {
  // 转换数据格式以匹配后端API
  const requestData = {
    sessionId: importSessionId,
    items: data.matches.map(match => ({
      rawDataId: match.rowId,
      productSpecId: match.productSpecId,
    })),
  };
  return defHttp.put({ url: Api.ImportSessionPrefix + '/' + importSessionId + '/product-specs', data: requestData });
}

// ========== 第三步：特性匹配 ==========

// 获取特性匹配结果
export function getAppearanceFeatureMatches(importSessionId: string): Promise<any> {
  return defHttp.get({ url: Api.ImportSessionPrefix + '/' + importSessionId + '/features' });
}

// 批量更新特性匹配
export function updateAppearanceFeatureMatches(
  importSessionId: string,
  data: Step3AppearanceFeatureInput,
): Promise<void> {
  // 转换为后端期望的格式
  const backendData = {
    sessionId: importSessionId,
    items: data.matches.map(match => ({
      rawDataId: match.rowId,
      appearanceFeatureIds: match.appearanceFeatureIds || [],
    })),
  };
  return defHttp.put({ url: Api.ImportSessionPrefix + '/' + importSessionId + '/features', data: backendData });
}

// ========== 第四步：数据核对与完成 ==========

// 获取数据核对结果
export async function getImportReview(importSessionId: string): Promise<Step4ReviewOutput> {
  const response = await defHttp.get({ url: Api.ImportSessionPrefix + '/' + importSessionId + '/review' });
  
  // 处理后端返回的 PascalCase 字段名
  const data = response.data || response;
  return {
    session: data.session || data.Session,
    totalRows: data.totalRows ?? data.TotalRows ?? 0,
    validDataRows: data.validDataRows ?? data.ValidDataRows ?? 0,
    matchedSpecRows: data.matchedSpecRows ?? data.MatchedSpecRows ?? 0,
    matchedFeatureRows: data.matchedFeatureRows ?? data.MatchedFeatureRows ?? 0,
    matchStatus: data.matchStatus || data.MatchStatus || 'error',
    errors: data.errors || data.Errors || [],
    previewIntermediateData: data.previewIntermediateData || data.PreviewIntermediateData || [],
  };
}

// 完成导入，生成中间数据
export function completeImport(importSessionId: string): Promise<void> {
  if (!importSessionId || importSessionId.trim() === '') {
    return Promise.reject(new Error('导入会话ID不能为空'));
  }
  return defHttp.post({ url: Api.ImportSessionPrefix + '/' + importSessionId + '/complete' });
}

// ========== 原有API（保持兼容） ==========

// 导入Excel（旧版接口，保持兼容）
export function importRawData(data) {
  return defHttp.post({ url: Api.Prefix + '/import', data });
}

// 简化导入Excel（直接上传、解析、保存，支持炉号去重）
export function simpleImportRawData(data: SimpleImportInput) {
  return defHttp.post<SimpleImportOutput>({ url: Api.ImportSessionPrefix + '/simple-import', data });
}

// 获取列表
export function getRawDataList(params) {
  return defHttp.get({ url: Api.Prefix, params });
}

// 获取详情
export function getRawDataInfo(id) {
  return defHttp.get({ url: Api.Prefix + '/' + id });
}

// 删除
export function delRawData(id) {
  return defHttp.delete({ url: Api.Prefix + '/' + id });
}

// 获取导入日志列表
export function getImportLogList(params?: any): Promise<ImportLog[]> {
  return defHttp.get({ url: Api.Prefix + '/import-log', params });
}

// 删除导入日志
export function deleteImportLog(id: string): Promise<void> {
  return defHttp.delete({ url: Api.Prefix + '/import-log/' + id });
}

// 下载源文件
export async function downloadSourceFile(fileId: string, fileName: string): Promise<void> {
  const response = await defHttp.get<Blob>(
    { url: Api.Prefix + '/import-log/' + fileId + '/source-file', responseType: 'blob' },
    { isReturnNativeResponse: true },
  );
  downloadByData((response as any).data || response, fileName);
}

// 下载错误报告
export async function downloadErrorReport(importLogId: string): Promise<void> {
  const response = await defHttp.get<Blob>(
    { url: Api.Prefix + '/import-log/' + importLogId + '/error-report', responseType: 'blob' },
    { isReturnNativeResponse: true },
  );
  const fileName = `错误报告_${importLogId}.xlsx`;
  downloadByData((response as any).data || response, fileName);
}
