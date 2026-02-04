<template>
  <div class="kpi-cards-row">
    <!-- Card 1: Total Weight -->
    <div class="kpi-card">
      <div class="kpi-icon" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%)">
        <DatabaseOutlined />
      </div>
      <div class="kpi-content">
        <div class="kpi-label">检验总重</div>
        <div class="kpi-value">{{ formatNumber(summary?.totalWeight) }}</div>
        <div class="kpi-unit">kg</div>
      </div>
    </div>

    <!-- Card 2: Qualified Rate with Gauge -->
    <div class="kpi-card">
      <div class="kpi-gauge-container">
        <div ref="gaugeRef" class="kpi-gauge"></div>
        <div class="kpi-gauge-value">
          <span :class="getRateClass(summary?.qualifiedRate)">
            {{ formatNumber(summary?.qualifiedRate) }}
          </span>
          <span class="percent">%</span>
        </div>
      </div>
      <div class="kpi-label">合格率</div>
    </div>

    <!-- Card 3: Class A Rate -->
    <div class="kpi-card">
      <div class="kpi-icon" style="background: linear-gradient(135deg, #52c41a 0%, #389e0d 100%)">
        <CheckCircleOutlined />
      </div>
      <div class="kpi-content">
        <div class="kpi-label">A类占比</div>
        <div class="kpi-value success">{{ formatNumber(summary?.classARate) }}%</div>
        <div class="kpi-sub">{{ formatNumber(summary?.classAWeight) }} kg</div>
      </div>
    </div>

    <!-- Card 4: Class B Rate -->
    <div class="kpi-card">
      <div class="kpi-icon" style="background: linear-gradient(135deg, #1890ff 0%, #096dd9 100%)">
        <CheckSquareOutlined />
      </div>
      <div class="kpi-content">
        <div class="kpi-label">B类占比</div>
        <div class="kpi-value primary">{{ formatNumber(summary?.classBRate) }}%</div>
        <div class="kpi-sub">{{ formatNumber(summary?.classBWeight) }} kg</div>
      </div>
    </div>

    <!-- Card 5: Unqualified Rate -->
    <div class="kpi-card">
      <div class="kpi-icon" style="background: linear-gradient(135deg, #ff4d4f 0%, #cf1322 100%)">
        <CloseCircleOutlined />
      </div>
      <div class="kpi-content">
        <div class="kpi-label">不合格率</div>
        <div class="kpi-value error">{{ formatNumber(summary?.unqualifiedRate) }}%</div>
        <div class="kpi-sub">{{ formatNumber(summary?.unqualifiedWeight) }} kg</div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, onMounted, onUnmounted, nextTick } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import {
  DatabaseOutlined,
  CheckCircleOutlined,
  CheckSquareOutlined,
  CloseCircleOutlined,
} from '@ant-design/icons-vue';
import type { SummaryData } from '/@/api/lab/monthlyQualityReport';

interface Props {
  summary?: SummaryData | null;
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
});

const gaugeRef = ref<HTMLDivElement | null>(null);
let setGaugeOptions: ((option: any) => void) | null = null;
let gaugeEchartsInst: any = null;

// Format number with thousand separators
function formatNumber(value?: number): string {
  if (value === undefined || value === null) return '-';
  return value.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

// Get color class based on rate
function getRateClass(rate?: number): string {
  if (rate === undefined || rate === null) return '';
  if (rate >= 95) return 'success';
  if (rate >= 90) return 'primary';
  return 'error';
}

// Initialize gauge chart
function initGauge() {
  if (!gaugeRef.value) return;

  const { setOptions, echarts } = useECharts(gaugeRef.value);
  setGaugeOptions = setOptions;
  gaugeEchartsInst = echarts;

  updateGauge(props.summary?.qualifiedRate);
}

// Update gauge chart
function updateGauge(rate?: number) {
  if (!setGaugeOptions) return;

  const value = rate ?? 0;
  const color = value >= 95 ? '#52c41a' : value >= 90 ? '#1890ff' : '#ff4d4f';

  const option = {
    series: [
      {
        type: 'gauge',
        startAngle: 180,
        endAngle: 0,
        min: 0,
        max: 100,
        radius: '80%',
        center: ['50%', '70%'],
        splitNumber: 10,
        itemStyle: { color },
        progress: { show: true, width: 8 },
        pointer: { show: false },
        axisLine: { lineStyle: { width: 8, color: [[1, '#e8e8e8']] } },
        axisTick: { show: false },
        splitLine: { show: false },
        axisLabel: { show: false },
        detail: { show: false },
        data: [{ value }],
      },
    ],
  };

  setGaugeOptions(option);
}

// Watch for summary changes
watch(
  () => props.summary,
  (newSummary) => {
    if (newSummary) {
      nextTick(() => {
        updateGauge(newSummary.qualifiedRate);
      });
    }
  },
  { immediate: true }
);

onMounted(() => {
  initGauge();
});

onUnmounted(() => {
  if (gaugeEchartsInst) {
    gaugeEchartsInst.dispose();
  }
});
</script>

<style lang="less" scoped>
.kpi-cards-row {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 16px;
  margin-bottom: 16px;
}

.kpi-card {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  display: flex;
  align-items: center;
  transition: all 0.3s;

  &:hover {
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
    transform: translateY(-2px);
  }
}

.kpi-icon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 16px;
  flex-shrink: 0;

  .anticon {
    font-size: 28px;
    color: #fff;
  }
}

.kpi-content {
  flex: 1;
  min-width: 0;
}

.kpi-label {
  font-size: 14px;
  color: #8c8c8c;
  margin-bottom: 8px;
}

.kpi-value {
  font-size: 28px;
  font-weight: 700;
  color: #262626;
  line-height: 1;

  &.success {
    color: #52c41a;
  }

  &.primary {
    color: #1890ff;
  }

  &.error {
    color: #ff4d4f;
  }
}

.kpi-unit {
  font-size: 14px;
  color: #8c8c8c;
  margin-top: 4px;
}

.kpi-sub {
  font-size: 13px;
  color: #8c8c8c;
  margin-top: 4px;
}

// Gauge card special styling
.kpi-card:nth-child(2) {
  flex-direction: column;
  text-align: center;
  padding: 20px 16px 16px;
}

.kpi-gauge-container {
  position: relative;
  width: 100%;
  height: 110px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 4px;
}

.kpi-gauge {
  width: 100%;
  height: 100%;
}

.kpi-gauge-value {
  position: absolute;
  bottom: 5px;
  left: 50%;
  transform: translateX(-50%);
  text-align: center;
  z-index: 1;

  > span:first-child {
    font-size: 28px;
    font-weight: 700;
    line-height: 1;

    &.success {
      color: #52c41a;
    }

    &.primary {
      color: #1890ff;
    }

    &.error {
      color: #ff4d4f;
    }
  }

  .percent {
    font-size: 16px;
    color: #8c8c8c;
    margin-left: 2px;
  }
}

.kpi-card:nth-child(2) .kpi-label {
  margin-top: 8px;
  margin-bottom: 0;
}

// Responsive
@media (max-width: 1600px) {
  .kpi-cards-row {
    grid-template-columns: repeat(5, 1fr);
  }
}

@media (max-width: 1200px) {
  .kpi-cards-row {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (max-width: 768px) {
  .kpi-cards-row {
    grid-template-columns: 1fr;
  }

  .kpi-card {
    padding: 16px;
  }

  .kpi-icon {
    width: 48px;
    height: 48px;

    .anticon {
      font-size: 24px;
    }
  }

  .kpi-value {
    font-size: 24px;
  }
}
</style>
