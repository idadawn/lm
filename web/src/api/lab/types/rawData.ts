// 原始数据导入相关类型定义

// 导入会话
export interface ImportSession {
  id: string;
  fileName: string;
  sourceFileId?: string;
  importStrategy: 'incremental' | 'full' | 'overwrite' | 'deduplicate';
  currentStep: number; // 0-3
  totalRows: number;
  validDataRows: number;
  status: 'pending' | 'in_progress' | 'completed' | 'failed' | 'cancelled';
  createTime: string;
  updateTime: string;
  creatorUserId?: string;
}

// 导入策略
export type ImportStrategy = 'incremental' | 'full' | 'overwrite' | 'deduplicate';

// 原始数据行（支持动态检测列）
export interface RawDataRow {
  id?: string;
  prodDate: string;
  furnaceNo: string;
  lineNo?: string;
  shift?: string;
  width?: number;
  coilWeight?: number;
  detectionData?: Record<string, number | null>; // JSON格式：{"1": 1.23, "2": 2.45, ...}
  detectionColumns?: string; // 检测列范围，如 "13"
  productSpecId?: string;
  productSpecCode?: string;
  productSpecName?: string;
  featureSuffix?: string; // 特性汉字
  appearanceFeatureIds?: string[]; // 匹配后的特性ID列表
  isValidData?: boolean; // 是否为有效数据
  importStatus?: number; // 导入状态：0-成功，1-失败
  importSessionId?: string;
  sourceFileId?: string;
  importMessage?: string; // 导入失败信息
  creatorTime?: string;
  // 以下字段用于前端显示
  rowIndex?: number; // 行号（前端显示用）
  furnaceNoParsed?: string; // 解析后的炉号数字部分
  coilNo?: string; // 卷号
  subcoilNo?: string; // 分卷号
  parsedFurnaceNo?: ParsedFurnaceNo;
  productSpecMatchStatus?: 'matched' | 'unmatched' | 'partial';
  appearanceFeatureMatchStatus?: 'matched' | 'unmatched' | 'partial';
  matchConfidence?: number; // 匹配置信度
}

// 炉号解析结果
export interface ParsedFurnaceNo {
  lineNo: string; // 产线
  shift: string; // 班次
  prodDate: string; // 生产日期
  furnaceNoNum: string; // 炉号数字
  coilNo: string; // 卷号
  subcoilNo: string; // 分卷号
  featureSuffix?: string; // 特性汉字
  isValid: boolean; // 是否有效
}

// 产品规格
export interface ProductSpec {
  id: string;
  code: string;
  name: string;
  detectionColumns?: string; // 检测列配置
  enabled: boolean;
}

// 外观特性
export interface AppearanceFeature {
  id: string;
  name: string;
  categoryId: string;
  categoryName?: string;
  severityLevelId?: string;
  severityLevelName?: string;
  description?: string;
  confidence?: number;
}

// 外观特性分类
export interface AppearanceFeatureCategory {
  id: string;
  name: string;
  parentId?: string;
  description?: string;
}

// 特性匹配结果
export interface AppearanceFeatureMatch {
  feature: AppearanceFeature;
  confidence: number; // 置信度
  matchMethod: 'name' | 'keyword' | 'rule'; // 匹配方式
}

// 导入结果统计
export interface ImportStatistics {
  totalRows: number;
  successRows: number;
  failRows: number;
  validDataRows: number; // 有效数据行数
  invalidDataRows: number; // 无效数据行数
  matchedProductSpecRows: number; // 已匹配产品规格的行数
  matchedAppearanceFeatureRows: number; // 已匹配特性的行数
}

// 数据预览结果
export interface DataPreviewResult {
  headers: string[]; // Excel表头
  rows: RawDataRow[]; // 解析后的数据
  statistics: ImportStatistics;
  errors?: ValidationError[];
}

// 校验错误
export interface ValidationError {
  rowIndex: number;
  rowData: any;
  errors: string[];
}

// 第一步：文件上传与解析
export interface Step1UploadAndParseInput {
  fileData?: string;  // Base64字符串（可选，文件已在创建会话时保存到后端）
  fileName: string;
  importStrategy: ImportStrategy;
  importSessionId: string;  // 必填，使用已存在的会话
}

export interface Step1UploadAndParseOutput {
  importSessionId: string;
  preview: DataPreviewResult;
}

// 第二步：产品规格识别
export interface Step2ProductSpecInput {
  importSessionId: string;
  matches: Array<{
    rowId: string;
    productSpecId: string;
  }>;
}

// 第三步：特性匹配
export interface Step3AppearanceFeatureInput {
  importSessionId: string;
  matches: Array<{
    rowId: string;
    appearanceFeatureIds: string[];
  }>;
}

// 第四步：数据核对
export interface Step4ReviewOutput {
  session: ImportSession;
  totalRows: number;
  validDataRows: number;
  matchedSpecRows: number;
  matchedFeatureRows: number;
  matchStatus: 'ok' | 'warning' | 'error' | 'completed' | 'cancelled';
  errors: string[];
  previewIntermediateData: RawDataRow[]; // 预览即将生成的中间数据
}

// 导入日志
export interface ImportLog {
  id: string;
  fileName: string;
  sourceFileId?: string;
  importStrategy: ImportStrategy;
  totalRows: number;
  successRows: number;
  failRows: number;
  validDataRows: number;
  status: string;
  importTime: string;
  creatorUserName?: string;
  lastRowsHash?: string; // 最后N行数据标识
  lastRowsCount?: number;
}

// 错误报告
export interface ErrorReport {
  importLogId: string;
  fileName: string;
  errors: ValidationError[];
  errorCount: number;
}