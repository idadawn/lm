import { getUnitsByCategory, getAllUnitsGroupedByCategory } from '/@/api/lab/unit';

/**
 * 单位定义接口
 */
export interface UnitDefinition {
  id: string;
  categoryId: string;
  name: string;
  symbol: string;
  isBase: boolean;
  scaleToBase: number;
  offset: number;
  precision: number;
  displayName: string;
}

// 单位缓存
let unitCache: Map<string, UnitDefinition> | null = null;
let allUnitsCache: UnitDefinition[] | null = null;

/**
 * 加载所有单位到缓存
 */
export async function loadAllUnits(): Promise<UnitDefinition[]> {
  if (allUnitsCache) {
    return allUnitsCache;
  }

  try {
    const response = await getAllUnitsGroupedByCategory();
    const grouped = response.data || response || {};
    allUnitsCache = Object.values(grouped).flat() as UnitDefinition[];
    
    // 构建单位ID到单位的映射
    unitCache = new Map();
    allUnitsCache.forEach(unit => {
      unitCache!.set(unit.id, unit);
    });
    
    return allUnitsCache;
  } catch (error) {
    console.error('加载单位列表失败:', error);
    return [];
  }
}

/**
 * 根据单位ID获取单位信息
 */
export async function getUnitById(unitId: string): Promise<UnitDefinition | null> {
  if (!unitId) return null;
  
  // 如果缓存为空，先加载
  if (!unitCache) {
    await loadAllUnits();
  }
  
  return unitCache?.get(unitId) || null;
}

/**
 * 根据单位ID获取单位符号
 */
export async function getUnitSymbol(unitId: string): Promise<string | null> {
  const unit = await getUnitById(unitId);
  return unit?.symbol || null;
}

/**
 * 根据单位ID获取显示精度
 */
export async function getUnitPrecision(unitId: string): Promise<number> {
  const unit = await getUnitById(unitId);
  return unit?.precision ?? 2;
}

/**
 * 格式化数值显示（根据单位精度）
 * @param value 数值
 * @param unitId 单位ID（可选）
 * @param defaultPrecision 默认精度（当单位ID不存在时使用）
 */
export async function formatValueWithUnit(
  value: number | null | undefined,
  unitId?: string,
  defaultPrecision: number = 2
): Promise<string> {
  if (value === null || value === undefined) {
    return '-';
  }

  const precision = unitId ? await getUnitPrecision(unitId) : defaultPrecision;
  return value.toFixed(precision);
}

/**
 * 格式化数值显示（同步版本，使用缓存的单位信息）
 * @param value 数值
 * @param unit 单位对象（可选）
 * @param defaultPrecision 默认精度
 */
export function formatValueWithUnitSync(
  value: number | null | undefined,
  unit?: UnitDefinition | null,
  defaultPrecision: number = 2
): string {
  if (value === null || value === undefined) {
    return '-';
  }

  const precision = unit?.precision ?? defaultPrecision;
  return value.toFixed(precision);
}

/**
 * 清除单位缓存（用于刷新单位数据）
 */
export function clearUnitCache() {
  unitCache = null;
  allUnitsCache = null;
}
