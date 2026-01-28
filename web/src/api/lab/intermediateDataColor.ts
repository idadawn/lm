import { defHttp } from '/@/utils/http/axios';
import { CellColorInfo, IntermediateDataColorDto, SaveIntermediateDataColorInput, GetIntermediateDataColorInput, DeleteIntermediateDataColorInput } from './model/intermediateDataColorModel';

enum Api {
  // 保存颜色配置
  SaveColors = '/api/lab/intermediate-data-color/save-colors',
  // 获取颜色配置
  GetColors = '/api/lab/intermediate-data-color/get-colors',
  // 删除颜色配置
  DeleteColors = '/api/lab/intermediate-data-color/delete-colors',
  // 保存单个单元格颜色（实时保存）
  SaveCellColor = '/api/lab/intermediate-data-color/save-cell-color',
}

/**
 * 保存颜色配置
 */
export function saveIntermediateDataColors(params: SaveIntermediateDataColorInput) {
  return defHttp.post<boolean>({
    url: Api.SaveColors,
    data: params,
  });
}

/**
 * 获取颜色配置
 */
export function getIntermediateDataColors(params: GetIntermediateDataColorInput) {
  return defHttp.post<IntermediateDataColorDto>({
    url: Api.GetColors,
    data: params,
  });
}

/**
 * 删除颜色配置
 */
export function deleteIntermediateDataColors(params: DeleteIntermediateDataColorInput) {
  return defHttp.post<boolean>({
    url: Api.DeleteColors,
    data: params,
  });
}

/**
 * 保存单个单元格颜色（实时保存）
 */
export function saveIntermediateDataCellColor(
  intermediateDataId: string,
  fieldName: string,
  colorValue: string,
  productSpecId: string
) {
  return defHttp.post<boolean>({
    url: Api.SaveCellColor,
    data: {
      intermediateDataId,
      fieldName,
      colorValue,
      productSpecId,
    },
  });
}