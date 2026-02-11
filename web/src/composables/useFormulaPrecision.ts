import { ref, onMounted } from 'vue';
import { defHttp } from '/@/utils/http/axios';

// 公式精度配置
interface FormulaPrecision {
  [key: string]: number; // fieldName -> precision
}

let precisionCache: FormulaPrecision | null = null;
let loadingPrecision = false;

/**
 * 获取字段的精度配置
 * 支持：1. 精确匹配 2. 不区分大小写匹配 3. 前缀匹配（如 detection1 匹配 Detection）
 */
export function getFieldPrecision(fieldName: string): number {
  if (!precisionCache) {
    return 4; // 默认4位小数
  }

  // 1. 精确匹配
  if (precisionCache[fieldName] !== undefined) {
    return precisionCache[fieldName];
  }

  // 2. 不区分大小写匹配
  const lowerField = fieldName.toLowerCase();
  if (precisionCache[lowerField] !== undefined) {
    return precisionCache[lowerField];
  }

  // 3. 前缀匹配 - 对于动态字段如 detection1, thickness1 等
  // 提取前缀（去掉末尾的数字）
  const prefixMatch = fieldName.match(/^([a-zA-Z]+)\d*$/);
  if (prefixMatch) {
    const prefix = prefixMatch[1];
    // 尝试查找前缀的精度配置
    if (precisionCache[prefix] !== undefined) {
      return precisionCache[prefix];
    }
    // 尝试首字母大写的前缀（如 Detection, Thickness）
    const capitalizedPrefix = prefix.charAt(0).toUpperCase() + prefix.slice(1).toLowerCase();
    if (precisionCache[capitalizedPrefix] !== undefined) {
      return precisionCache[capitalizedPrefix];
    }
  }

  return 4; // 默认4位小数
}

/**
 * 加载公式精度配置
 */
export async function loadFormulaPrecision(): Promise<FormulaPrecision> {
  if (precisionCache) {
    return Promise.resolve(precisionCache);
  }

  if (loadingPrecision) {
    return Promise.resolve({}); // 正在加载中返回空对象
  }

  loadingPrecision = true;

  try {
    // 调用公式配置接口
    const result = await defHttp.get({
      url: '/api/lab/intermediate-data-formula',
    });

    precisionCache = {};

    // 处理返回数据
    if (result?.records) {
      // 根据公式配置中的precision字段建立映射
      result.records.forEach((item: any) => {
        if (item.precision !== null && item.precision !== undefined) {
          // 存储 columnName 映射
          if (item.columnName) {
            precisionCache![item.columnName] = item.precision;
            // 同时存储小写版本
            precisionCache![item.columnName.toLowerCase()] = item.precision;
          }
          // 同时存储 displayName 映射（用于可能的字段名称匹配）
          if (item.displayName) {
            precisionCache![item.displayName] = item.precision;
          }
        }
      });
    }

    return precisionCache;
  } catch (error) {
    console.error('加载公式精度配置失败:', error);
    precisionCache = {};
    return Promise.resolve({});
  } finally {
    loadingPrecision = false;
  }
}

/**
 * 精度配置钩子函数
 */
export function useFormulaPrecision() {
  const loading = ref(false);

  onMounted(async () => {
    if (!precisionCache && !loadingPrecision) {
      loading.value = true;
      await loadFormulaPrecision();
      loading.value = false;
    }
  });

  return {
    getFieldPrecision,
    loading,
    reloadFormulaPrecision: loadFormulaPrecision,
  };
}