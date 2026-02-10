<template>
  <div class="kpi-cards-row" :style="{ gridTemplateColumns: `repeat(${1 + headerConfigs.length}, 1fr)` }">
    <!-- Card 1: Total Weight (固定) -->
    <div class="kpi-card">
      <div class="kpi-icon-wrapper" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%)">
        <DatabaseOutlined class="kpi-icon" />
      </div>
      <div class="kpi-content">
        <div class="kpi-label">检验总重</div>
        <div class="kpi-value">
          <span class="number">{{ formatNumber(summary?.totalWeight) }}</span>
          <span class="unit">kg</span>
        </div>
      </div>
    </div>

    <!-- 动态配置卡片 - 只展示 isHeader=true 的配置 -->
    <div v-for="(config, index) in headerConfigs" :key="config.id" class="kpi-card">
      <div class="kpi-icon-wrapper" :style="{ background: getConfigGradient(index) }">
        <CheckCircleOutlined v-if="config.isPercentage" class="kpi-icon" />
        <CheckSquareOutlined v-else class="kpi-icon" />
      </div>
      <div class="kpi-content">
        <div class="kpi-label">{{ config.name }}</div>
        <template v-if="config.isPercentage">
          <!-- 百分比类型：主值显示百分比 -->
          <div class="kpi-value" :class="getRateClass(getDynamicStat(config.id).rate)">
            <span class="number">{{ formatNumber(getDynamicStat(config.id).rate) }}</span>
            <span class="unit">%</span>
          </div>
        </template>
        <template v-else>
          <!-- 非百分比类型：主值显示求和(weight) -->
          <div class="kpi-value">
            <span class="number">{{ formatNumber(getDynamicStat(config.id).weight) }}</span>
            <span class="unit">kg</span>
          </div>
          <!-- isShowRatio 时下方显示占比 -->
          <div v-if="config.isShowRatio" class="kpi-sub">
            <span class="sub-label">占比:</span>
            <span class="sub-value">{{ formatNumber(getDynamicStat(config.id).rate) }}%</span>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {
  DatabaseOutlined,
  CheckCircleOutlined,
  CheckSquareOutlined,
} from '@ant-design/icons-vue';
import { computed } from 'vue';
import type { SummaryData } from '/@/api/lab/monthlyQualityReport';
import type { ReportConfig } from '/@/api/lab/reportConfig';

interface Props {
  summary?: SummaryData | null;
  loading?: boolean;
  reportConfigs?: ReportConfig[];
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  reportConfigs: () => [],
});

// 只展示 isHeader=true 的配置
const headerConfigs = computed(() => {
  return (props.reportConfigs || []).filter(c => c.isHeader);
});

// 获取动态统计数据
function getDynamicStat(configId: string) {
  return props.summary?.dynamicStats?.[configId] || { weight: 0, rate: 0 };
}

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

// 动态配置的渐变色
function getConfigGradient(index: number): string {
  const gradients = [
    'linear-gradient(135deg, #42e695 0%, #3bb2b8 100%)',
    'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
    'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
    'linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%)',
    'linear-gradient(135deg, #8fd3f4 0%, #84fab0 100%)',
    'linear-gradient(135deg, #a18cd1 0%, #fbc2eb 100%)',
  ];
  return gradients[index % gradients.length];
}
</script>

<style lang="less" scoped>
.kpi-cards-row {
  display: grid;
  gap: 20px;
  margin-bottom: 24px;
}

.kpi-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
  display: flex;
  align-items: center;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
  border: 1px solid rgba(0, 0, 0, 0.02);

  &:hover {
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
    transform: translateY(-4px);
  }

  &::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 4px;
    height: 100%;
    background: transparent;
    transition: background 0.3s;
  }
}

.kpi-icon-wrapper {
  width: 64px;
  height: 64px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 20px;
  flex-shrink: 0;
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);

  .kpi-icon {
    font-size: 32px;
    color: #fff;
  }
}

.kpi-content {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.kpi-label {
  font-size: 14px;
  color: #8c9eae;
  margin-bottom: 8px;
  font-weight: 500;
  white-space: nowrap;
}

.kpi-value {
  font-size: 24px;
  font-weight: 700;
  color: #2d3748;
  line-height: 1.2;
  display: flex;
  align-items: baseline;
  gap: 4px;

  .number {
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
  }

  .unit {
    font-size: 14px;
    font-weight: 500;
    color: #a0aec0;
  }

  &.success .number {
    color: #42e695;
  }

  &.primary .number {
    color: #4facfe;
  }

  &.error .number {
    color: #ff9a9e;
  }
}

.kpi-sub {
  font-size: 12px;
  color: #a0aec0;
  margin-top: 6px;
  display: flex;
  gap: 4px;
}

// Responsive
@media (max-width: 1400px) {
  .kpi-cards-row {
    grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)) !important;
  }
}

@media (max-width: 900px) {
  .kpi-cards-row {
    grid-template-columns: repeat(2, 1fr) !important;
  }
}

@media (max-width: 600px) {
  .kpi-cards-row {
    grid-template-columns: 1fr !important;
  }
}
</style>
