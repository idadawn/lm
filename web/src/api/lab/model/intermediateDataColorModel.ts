/**
 * 单元格颜色信息
 */
export interface CellColorInfo {
  /**
   * 中间数据ID
   */
  intermediateDataId: string;
  /**
   * 字段名（列名）
   */
  fieldName: string;
  /**
   * 颜色值（HEX格式）
   */
  colorValue: string;
}

/**
 * 中间数据颜色配置DTO
 */
export interface IntermediateDataColorDto {
  /**
   * 单元格颜色配置列表
   */
  colors: CellColorInfo[];
  /**
   * 产品规格ID
   */
  productSpecId: string;
}

/**
 * 保存颜色配置请求
 */
export interface SaveIntermediateDataColorInput {
  /**
   * 要保存的颜色配置
   */
  colors: CellColorInfo[];
  /**
   * 产品规格ID
   */
  productSpecId: string;
}

/**
 * 获取颜色配置请求
 */
export interface GetIntermediateDataColorInput {
  /**
   * 中间数据ID列表（可选，不填则获取所有）
   */
  intermediateDataIds?: string[];
  /**
   * 产品规格ID
   */
  productSpecId: string;
}

/**
 * 删除颜色配置请求
 */
export interface DeleteIntermediateDataColorInput {
  /**
   * 要删除的颜色配置ID列表
   */
  ids?: string[];
  /**
   * 中间数据ID（可选，删除指定数据的所有颜色）
   */
  intermediateDataId?: string;
  /**
   * 产品规格ID（可选，删除指定规格的所有颜色）
   */
  productSpecId?: string;
}