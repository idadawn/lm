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
 */
export function getFieldPrecision(fieldName: string): number {
  if (!precisionCache) {
    return 2; // 默认2位小数
  }
  return precisionCache[fieldName] ?? precisionCache[fieldName.toLowerCase()] ?? 2;
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
        if (item.columnName && item.precision !== null && item.precision !== undefined) {
          precisionCache![item.columnName] = item.precision;
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