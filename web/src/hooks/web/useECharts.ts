import type { EChartsOption } from 'echarts';
import type { Ref } from 'vue';
import { useTimeoutFn } from '/@/hooks/core/useTimeout';
import { tryOnUnmounted } from '@vueuse/core';
import { unref, nextTick, watch, computed, ref } from 'vue';
import { useDebounceFn } from '@vueuse/core';
import { useEventListener } from '/@/hooks/event/useEventListener';
import { useBreakpoint } from '/@/hooks/event/useBreakpoint';
import * as echarts from 'echarts';
import { useRootSetting } from '/@/hooks/setting/useRootSetting';
import { useMenuSetting } from '/@/hooks/setting/useMenuSetting';

/**
 * 关闭「多值轴 + 固定 min/max」图表的 alignTicks 兜底处理。
 * ECharts 5 中值轴 alignTicks 默认开启；当同一 grid 上存在 2 个以上值轴、且其中至少一个
 * 固定了 min/max 时，会强制对齐刻度并打印告警：
 *   “[ECharts] The ticks may be not readable when set min: 0, max: 100 and alignTicks: true”。
 * 这里在 setOption 前统一兜底：仅当图表未显式设置过 alignTicks 时才关闭，保留各轴各自的可读刻度，
 * 不影响显式选择对齐刻度的图表。
 */
function disableRiskyAlignTicks(option: any): void {
  if (!option || typeof option !== 'object') return;
  for (const dim of ['xAxis', 'yAxis'] as const) {
    const axes = option[dim];
    if (!Array.isArray(axes) || axes.length < 2) continue;
    const valueAxes = axes.filter((ax: any) => ax && (ax.type === 'value' || ax.type === 'log'));
    if (valueAxes.length < 2) continue;
    const hasFixedExtent = valueAxes.some((ax: any) => ax.min != null || ax.max != null);
    const hasExplicitAlignTicks = valueAxes.some((ax: any) => ax.alignTicks != null);
    if (hasFixedExtent && !hasExplicitAlignTicks) {
      valueAxes.forEach((ax: any) => {
        ax.alignTicks = false;
      });
    }
  }
}

export function useECharts(elRef: Ref<HTMLDivElement>, theme: 'light' | 'dark' | 'default' = 'default') {
  const { getDarkMode: getSysDarkMode } = useRootSetting();
  const { getCollapsed } = useMenuSetting();

  const getDarkMode = computed(() => {
    return theme === 'default' ? getSysDarkMode.value : theme;
  });
  let chartInstance: echarts.ECharts | null = null;
  let resizeFn: Fn = resize;
  const cacheOptions = ref({}) as Ref<EChartsOption>;
  let removeResizeFn: Fn = () => {};

  resizeFn = useDebounceFn(resize, 200);

  const getOptions = computed(() => {
    if (getDarkMode.value !== 'dark') {
      return cacheOptions.value as EChartsOption;
    }
    return {
      backgroundColor: 'transparent',
      ...cacheOptions.value,
    } as EChartsOption;
  });

  function initCharts(t = theme) {
    const el = unref(elRef);
    if (!el || !unref(el)) {
      return;
    }

    chartInstance = echarts.init(el, t);
    const { removeEvent } = useEventListener({
      el: window,
      name: 'resize',
      listener: resizeFn,
    });
    removeResizeFn = removeEvent;
    const { widthRef, screenEnum } = useBreakpoint();
    if (unref(widthRef) <= screenEnum.MD || el.offsetHeight === 0) {
      useTimeoutFn(() => {
        resizeFn();
      }, 30);
    }
  }

  function setOptions(options: EChartsOption, clear = true) {
    disableRiskyAlignTicks(options);
    cacheOptions.value = options;
    return new Promise(resolve => {
      if (unref(elRef)?.offsetHeight === 0) {
        useTimeoutFn(() => {
          setOptions(unref(getOptions));
          resolve(null);
        }, 30);
      }
      nextTick(() => {
        useTimeoutFn(() => {
          if (!chartInstance) {
            initCharts(getDarkMode.value as 'default');

            if (!chartInstance) return;
          }
          clear && chartInstance?.clear();

          chartInstance?.setOption(unref(getOptions));
          resolve(null);
        }, 30);
      });
    });
  }

  function resize() {
    chartInstance?.resize({
      animation: {
        duration: 300,
        easing: 'quadraticIn',
      },
    });
  }

  watch(
    () => getDarkMode.value,
    theme => {
      if (chartInstance) {
        chartInstance.dispose();
        initCharts(theme as 'default');
        setOptions(cacheOptions.value);
      }
    },
  );

  watch(getCollapsed, _ => {
    useTimeoutFn(() => {
      resizeFn();
    }, 300);
  });

  tryOnUnmounted(() => {
    if (!chartInstance) return;
    removeResizeFn();
    chartInstance.dispose();
    chartInstance = null;
  });

  function getInstance(): echarts.ECharts | null {
    if (!chartInstance) {
      initCharts(getDarkMode.value as 'default');
    }
    return chartInstance;
  }

  return {
    setOptions,
    resize,
    echarts,
    getInstance,
    resizeFn,
  };
}
