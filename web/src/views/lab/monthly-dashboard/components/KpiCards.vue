<template>
  <div class="kpi-cards-row" :style="{ gridTemplateColumns: `repeat(${3 + (qualifiedColumns?.length || 0)}, 1fr)` }">
    <!-- Card 1: Total Weight -->
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

    <!-- Card 2: Qualified Rate (Standard Style) -->
    <div class="kpi-card">
      <div class="kpi-icon-wrapper" style="background: linear-gradient(135deg, #42e695 0%, #3bb2b8 100%)">
        <SafetyCertificateOutlined class="kpi-icon" />
      </div>
      <div class="kpi-content">
        <div class="kpi-label">合格率</div>
        <div class="kpi-value" :class="getRateClass(summary?.qualifiedRate)">
          <span class="number">{{ formatNumber(summary?.qualifiedRate) }}</span>
          <span class="unit">%</span>
        </div>
      </div>
    </div>

    <!-- Dynamic Qualified Columns -->
    <div v-for="(col, index) in qualifiedColumns" :key="col.code" class="kpi-card">
      <div class="kpi-icon-wrapper" :style="{ background: getColumnGradient(index, col) }">
        <CheckCircleOutlined v-if="index === 0" class="kpi-icon" />
        <CheckSquareOutlined v-else class="kpi-icon" />
      </div>
      <div class="kpi-content">
        <div class="kpi-label">{{ col.name }}占比</div>
        <div class="kpi-value" :class="index === 0 ? 'success' : 'primary'">
          <span class="number">{{ getCategoryRate(summary, col) }}</span>
          <span class="unit">%</span>
        </div>
        <div class="kpi-sub">
          <span class="sub-label">重量:</span>
          <span class="sub-value">{{ getCategoryWeight(summary, col) }} kg</span>
        </div>
      </div>
    </div>

    <!-- Card Last: Unqualified Rate -->
    <div class="kpi-card">
      <div class="kpi-icon-wrapper" style="background: linear-gradient(135deg, #ff9a9e 0%, #fecfef 99%, #fecfef 100%)">
        <CloseCircleOutlined class="kpi-icon" />
      </div>
      <div class="kpi-content">
        <div class="kpi-label">不合格率</div>
        <div class="kpi-value error">
          <span class="number">{{ formatNumber(summary?.unqualifiedRate) }}</span>
          <span class="unit">%</span>
        </div>
        <div class="kpi-sub">
          <span class="sub-label">重量:</span>
          <span class="sub-value">{{ formatNumber(summary?.unqualifiedWeight) }} kg</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {
  DatabaseOutlined,
  CheckCircleOutlined,
  CheckSquareOutlined,
  CloseCircleOutlined,
  SafetyCertificateOutlined,
} from '@ant-design/icons-vue';
import type { SummaryData, JudgmentLevelColumn } from '/@/api/lab/monthlyQualityReport';

interface Props {
  summary?: SummaryData | null;
  loading?: boolean;
  qualifiedColumns?: JudgmentLevelColumn[];
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  qualifiedColumns: () => [],
});

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

// Get gradient for dynamic columns
function getColumnGradient(index: number, col?: JudgmentLevelColumn): string {
  // 优先使用列定义中的颜色
  if (col?.color) {
    return `linear-gradient(135deg, ${col.color} 0%, ${col.color}dd 100%)`;
  }
  const gradients = [
    'linear-gradient(135deg, #42e695 0%, #3bb2b8 100%)', // Green-ish for first
    'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)', // Blue-ish for second
    'linear-gradient(135deg, #fa709a 0%, #fee140 100%)', // Orange-ish for third
    'linear-gradient(135deg, #8fd3f4 0%, #84fab0 100%)', // Light green
  ];
  return gradients[index % gradients.length];
}

// 获取某等级的占比（兼容多种数据格式）
function getCategoryRate(summary: SummaryData | null | undefined, col: JudgmentLevelColumn): string {
  if (!summary) return '-';

  // 优先从 qualifiedCategories 获取（使用 name 作为 key，因为后端用 name 作为 key）
  if (summary.qualifiedCategories?.[col.name]?.rate !== undefined) {
    return formatNumber(summary.qualifiedCategories[col.name].rate);
  }

  // 兼容旧字段（使用 code 或 name 匹配）
  if ((col.code === 'A' || col.name === 'A') && summary.classARate !== undefined) {
    return formatNumber(summary.classARate);
  }
  if ((col.code === 'B' || col.name === 'B') && summary.classBRate !== undefined) {
    return formatNumber(summary.classBRate);
  }

  return '-';
}

// 获取某等级的重量（兼容多种数据格式）
function getCategoryWeight(summary: SummaryData | null | undefined, col: JudgmentLevelColumn): string {
  if (!summary) return '-';

  // 优先从 qualifiedCategories 获取（使用 name 作为 key）
  if (summary.qualifiedCategories?.[col.name]?.weight !== undefined) {
    return formatNumber(summary.qualifiedCategories[col.name].weight);
  }

  // 兼容旧字段（使用 code 或 name 匹配）
  if ((col.code === 'A' || col.name === 'A') && summary.classAWeight !== undefined) {
    return formatNumber(summary.classAWeight);
  }
  if ((col.code === 'B' || col.name === 'B') && summary.classBWeight !== undefined) {
    return formatNumber(summary.classBWeight);
  }

  return '-';
}
</script>

<style lang="less" scoped>
.kpi-cards-row {
  display: grid;
  // grid-template-columns set via inline style for dynamic count
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

  &.center {
    text-align: center;
    margin-top: -10px;
  }
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
@media (max-width: 1600px) {
  .kpi-cards-row {
    // grid-template-columns: repeat(5, 1fr); // Use inline style instead
  }
}

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
