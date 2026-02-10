<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">班次对比</h3>
    </div>
    <div class="chart-body">
      <div ref="chartRef" class="chart-container"></div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, onMounted, type Ref } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import type { ShiftComparison, JudgmentLevelColumn } from '/@/api/lab/monthlyQualityReport';
import type { ReportConfig } from '/@/api/lab/reportConfig';

interface Props {
  data?: ShiftComparison[] | null;
  loading?: boolean;
  qualifiedColumns?: JudgmentLevelColumn[];
  reportConfigs?: ReportConfig[];
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  qualifiedColumns: () => [],
  reportConfigs: () => [],
});

const chartRef = ref<HTMLDivElement | null>(null);
let setChartOptions: ((option: any) => void) | null = null;

// 颜色池
const colors = ['#42e695', '#4facfe', '#ff9a9e', '#fa709a', '#8ec5fc'];

// 获取某个等级的占比数据
function getCategoryRate(shift: ShiftComparison, statKey: string): number {
  if (shift.dynamicStats) {
    const dynamicVal = shift.dynamicStats[statKey];
    if (dynamicVal !== undefined && dynamicVal !== null) {
      return Number(dynamicVal) || 0;
    }
  }

  const dynamicField = `class${statKey}Rate`;
  const val = (shift as any)[dynamicField];
  if (val !== undefined && val !== null) {
    return Number(val) || 0;
  }
  if (statKey === 'A') {
    const legacyVal = (shift as any).classARate;
    if (legacyVal !== undefined && legacyVal !== null) {
      return Number(legacyVal) || 0;
    }
  }
  if (statKey === 'B') {
    const legacyVal = (shift as any).classBRate;
    if (legacyVal !== undefined && legacyVal !== null) {
      return Number(legacyVal) || 0;
    }
  }
  return 0;
}
function initChart() {
  if (!chartRef.value) return;

  const { setOptions } = useECharts(chartRef as Ref<HTMLDivElement>);
  setChartOptions = setOptions;

  updateChart();
}

function updateChart() {
  if (!setChartOptions || !props.data) return;

  const shifts = props.data;
  console.log('ShiftComparisonRadar data:', shifts); // Debug logging
  const visibleConfigs = (props.reportConfigs || []).filter((c) =>
    String(c?.formulaId || '').toLowerCase() === 'firstinspection' && c.isShowInReport,
  );
  const indicatorConfigs = visibleConfigs.length > 0
    ? visibleConfigs.map((c) => ({ name: c.name, key: c.id }))
    : props.qualifiedColumns.map((c) => ({ name: c.name, key: c.code }));

  // 动态构建指标：总产量占比（归一化）、合格率，以及所有合格等级
  const indicators = [
    { name: '产量占比', max: 100 },
    { name: '合格率', max: 100 },
  ];

  // 根据合格等级列动态添加指标
  for (const cfg of indicatorConfigs) {
    indicators.push({ name: `${cfg.name}占比`, max: 100 });
  }

  // 计算总产量的最大值用于归一化
  const weights = shifts.map((s) => s.totalWeight || 0);
  const maxVal = weights.length > 0 ? Math.max(...weights) : 0;
  const maxWeight = maxVal > 0 ? maxVal * 1.2 : 100;

  // 构建系列数据（总产量归一化为百分比）
  const series = shifts.map((shift) => {
    const weightPercent = maxVal > 0 ? ((shift.totalWeight || 0) / maxWeight) * 100 : 0;
    const value = [
      weightPercent,
      shift.qualifiedRate || 0,
    ];

    // 动态添加各等级占比数据
    for (const cfg of indicatorConfigs) {
      value.push(getCategoryRate(shift, cfg.key));
    }

    return {
      value,
      name: shift.shift,
    };
  });

  const option = {
    tooltip: {
      trigger: 'item',
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      borderColor: '#f0f2f5',
      textStyle: { color: '#262626' },
      formatter: (params: any) => {
        const shiftName = params.name;
        const shiftData = shifts.find((s) => s.shift === shiftName);
        if (!shiftData) return params.name;
        
        let html = `<div style="font-weight:600;margin-bottom:8px;">${shiftName}</div>`;
        
        // 产量占比 - 显示实际重量
        const weightPercent = maxVal > 0 ? ((shiftData.totalWeight || 0) / maxWeight) * 100 : 0;
        html += `<div style="display:flex;align-items:center;margin:4px 0;">
          <span style="display:inline-block;width:10px;height:10px;border-radius:50%;background:${params.color};margin-right:8px;"></span>
          <span>产量占比: ${weightPercent.toFixed(1)}% (${shiftData.totalWeight?.toFixed(2) || 0}kg)</span>
        </div>`;
        
        // 合格率
        html += `<div style="display:flex;align-items:center;margin:4px 0;">
          <span style="display:inline-block;width:10px;height:10px;border-radius:50%;background:${params.color};margin-right:8px;"></span>
          <span>合格率: ${(shiftData.qualifiedRate || 0).toFixed(2)}%</span>
        </div>`;
        
        // 各等级占比
        for (const cfg of indicatorConfigs) {
          const rate = getCategoryRate(shiftData, cfg.key);
          html += `<div style="display:flex;align-items:center;margin:4px 0;">
            <span style="display:inline-block;width:10px;height:10px;border-radius:50%;background:${params.color};margin-right:8px;"></span>
            <span>${cfg.name}占比: ${rate.toFixed(2)}%</span>
          </div>`;
        }
        
        return html;
      },
    },
    legend: {
      data: shifts.map((s) => s.shift),
      bottom: 0,
      icon: 'circle',
      textStyle: { color: '#666' }
    },
    radar: {
      indicator: indicators,
      radius: '65%',
      center: ['50%', '50%'],
      splitNumber: 4,
      scale: false,
      axisName: {
        color: '#666',
        fontSize: 12,
        fontWeight: 500
      },
      splitLine: {
        lineStyle: { color: '#e8e8e8' },
      },
      splitArea: {
        show: true,
        areaStyle: {
          color: ['rgba(245, 247, 250, 1)', 'rgba(255, 255, 255, 1)']
        },
      },
      axisLine: {
        lineStyle: { color: '#e8e8e8' },
      },
    },
    series: [
      {
        type: 'radar',
        data: series.map((item, index) => ({
          ...item,
          itemStyle: { color: colors[index % colors.length] },
          areaStyle: { opacity: 0.3 }
        })),
        symbol: 'circle',
        symbolSize: 6,
      },
    ],
  };

  setChartOptions(option);
}

watch(
  () => props.data,
  () => {
    if (setChartOptions) {
      updateChart();
    }
  },
  { deep: true }
);

onMounted(() => {
  initChart();
});


</script>

<style lang="less" scoped>
.chart-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
  height: 100%;
  display: flex;
  flex-direction: column;
  transition: all 0.3s;
  border: 1px solid rgba(0, 0, 0, 0.02);

  &:hover {
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.08);
  }
}

.chart-header {
  margin-bottom: 20px;
}

.chart-title {
  font-size: 16px;
  font-weight: 600;
  color: #2d3748;
  margin: 0;
  display: flex;
  align-items: center;

  &::before {
    content: '';
    width: 4px;
    height: 16px;
    background: #4facfe;
    border-radius: 2px;
    margin-right: 8px;
  }
}

.chart-body {
  flex: 1;
  min-height: 320px;
}

.chart-container {
  width: 100%;
  height: 100%;
  min-height: 320px;
}
</style>
