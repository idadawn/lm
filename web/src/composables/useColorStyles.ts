import { watch, onUnmounted, type Ref, nextTick } from 'vue';

const STYLE_ID = 'intermediate-data-colors';

interface ColorStylesOptions {
  coloredCells: Ref<Record<string, string>>;
}

/**
 * 颜色样式管理器 - 性能优化版本
 * 使用批量更新和浅层监听，大幅减少响应式查找
 *
 * 对于2000行x80列的表格：
 * - 原方案：160,000+次响应式查找（deep: true）
 * - 优化后：浅层监听 + 增量CSS更新
 */
export function useColorStyles({ coloredCells }: ColorStylesOptions) {
  // 使用 Map 存储颜色到 CSS 类的映射，避免重复生成类名
  const colorClassCache = new Map<string, string>();

  // 增量更新：记录已处理过的颜色，避免重复生成CSS规则
  const processedColors = new Set<string>();

  // 批量更新调度器状态
  let updateScheduled = false;
  let isUpdating = false;

  /**
   * 生成CSS类名（带缓存）
   * 将 :: 等特殊字符替换为 -
   */
  const generateClass = (key: string): string => {
    if (colorClassCache.has(key)) {
      return colorClassCache.get(key)!;
    }
    const className = `cell-color-${key.replace(/[^a-zA-Z0-9]/g, '-')}`;
    colorClassCache.set(key, className);
    return className;
  };

  /**
   * 增量更新样式表
   * 只处理新增的颜色，避免全量重建CSS规则
   */
  const updateStyles = (colors: Record<string, string>) => {
    let styleEl = document.getElementById(STYLE_ID) as HTMLStyleElement;
    if (!styleEl) {
      styleEl = document.createElement('style');
      styleEl.id = STYLE_ID;
      document.head.appendChild(styleEl);
    }

    // 只为新增的颜色生成CSS规则
    const newRules: string[] = [];
    Object.entries(colors).forEach(([key, color]) => {
      const colorKey = `${key}::${color}`;
      if (!processedColors.has(colorKey)) {
        const className = generateClass(key);
        newRules.push(`.${className} { background-color: ${color} !important; }`);
        processedColors.add(colorKey);
      }
    });

    // 增量添加新规则
    if (newRules.length > 0) {
      styleEl.innerHTML += newRules.join('\n');
    }
  };

  /**
   * 批量更新调度器
   * 使用 nextTick 确保在同一次事件循环中多次变化只触发一次更新
   * 添加防抖标志防止递归更新
   */
  const scheduleUpdate = () => {
    if (updateScheduled || isUpdating) return;
    updateScheduled = true;

    nextTick(() => {
      isUpdating = true;
      try {
        updateStyles(coloredCells.value);
      } finally {
        updateScheduled = false;
        // 延迟重置 isUpdating，确保当前更新周期完全结束
        setTimeout(() => {
          isUpdating = false;
        }, 0);
      }
    });
  };

  // 使用浅层监听 - 只监听对象引用变化，不深度遍历
  const stopWatch = watch(
    () => coloredCells.value,
    () => {
      try {
        scheduleUpdate();
      } catch (error) {
        console.error('Error in useColorStyles watcher:', error);
      }
    },
    { immediate: false }
  );

  /**
   * 获取单元格CSS类名
   */
  const getCellClass = (rowId: string, field: string): string => {
    const key = `${rowId}::${field}`;
    const color = coloredCells.value[key];
    return color ? generateClass(key) : '';
  };

  // 清理
  onUnmounted(() => {
    stopWatch();
    const styleEl = document.getElementById(STYLE_ID);
    if (styleEl) {
      styleEl.remove();
    }
    colorClassCache.clear();
    processedColors.clear();
  });

  return { getCellClass };
}
