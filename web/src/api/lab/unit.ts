import { defHttp } from '/@/utils/http/axios';

enum Api {
  Prefix = '/api/lab/unit',
}

// ========== 单位换算相关API ==========

/**
 * 单位换算
 * @param data 换算请求参数
 * @returns 换算结果
 */
export function convertUnit(data: {
  value: number;
  fromUnitId: string;
  toUnitId: string;
}) {
  return defHttp.post({ url: Api.Prefix + '/convert', data });
}

// ========== 单位维度相关API ==========

/**
 * 获取所有单位维度列表
 * @returns 单位维度列表
 */
export function getUnitCategories() {
  return defHttp.get({ url: Api.Prefix + '/categories' });
}

// ========== 单位定义相关API ==========

/**
 * 根据维度ID获取单位列表
 * @param categoryId 维度ID
 * @returns 单位列表
 */
export function getUnitsByCategory(categoryId: string) {
  return defHttp.get({ url: Api.Prefix + '/units/' + categoryId });
}

/**
 * 获取所有单位列表（按维度分组）
 * @returns 按维度分组的单位列表
 */
export function getAllUnitsGroupedByCategory() {
  return defHttp.get({ url: Api.Prefix + '/units/all' });
}

// ========== 单位维度管理API ==========

/**
 * 获取单位维度列表
 */
export function getUnitCategoryList() {
  return defHttp.get({ url: '/api/lab/unit-category' });
}

/**
 * 获取单位维度详情
 */
export function getUnitCategoryInfo(id: string) {
  return defHttp.get({ url: '/api/lab/unit-category/' + id });
}

/**
 * 创建单位维度
 */
export function createUnitCategory(data: any) {
  return defHttp.post({ url: '/api/lab/unit-category', data });
}

/**
 * 更新单位维度
 */
export function updateUnitCategory(id: string, data: any) {
  return defHttp.put({ url: '/api/lab/unit-category/' + id, data });
}

/**
 * 删除单位维度
 */
export function deleteUnitCategory(id: string) {
  return defHttp.delete({ url: '/api/lab/unit-category/' + id });
}

// ========== 单位定义管理API ==========

/**
 * 获取单位定义列表
 */
export function getUnitDefinitionList(categoryId?: string) {
  return defHttp.get({ url: '/api/lab/unit-definition', params: { categoryId } });
}

/**
 * 获取单位定义详情
 */
export function getUnitDefinitionInfo(id: string) {
  return defHttp.get({ url: '/api/lab/unit-definition/' + id });
}

/**
 * 创建单位定义
 */
export function createUnitDefinition(data: any) {
  return defHttp.post({ url: '/api/lab/unit-definition', data });
}

/**
 * 更新单位定义
 */
export function updateUnitDefinition(id: string, data: any) {
  return defHttp.put({ url: '/api/lab/unit-definition/' + id, data });
}

/**
 * 删除单位定义
 */
export function deleteUnitDefinition(id: string) {
  return defHttp.delete({ url: '/api/lab/unit-definition/' + id });
}
