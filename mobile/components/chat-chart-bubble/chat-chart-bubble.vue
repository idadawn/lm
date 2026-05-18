<template>
  <view v-if="chartConfig && (hasData || (isGrouped && hasGroupedData) || (isMultiLine && hasMultiLineData))" class="chat-chart-bubble">
    <view class="chart-header">
      <text class="chart-title">{{ chartConfig.title || '数据可视化' }}</text>
      <text v-if="chartConfig.center_label" class="chart-subtitle">
        {{ chartConfig.center_label }}：<text class="chart-total">{{ chartConfig.center_value }}</text>
      </text>
      <text v-else-if="isGrouped" class="chart-subtitle">
        总计：<text class="chart-total">{{ formatTon(groupedGrandTotal) }}</text>
      </text>
    </view>

    <!-- ── multi_line：1 cat + 1 time + 1 num，每个 cat 一条迷你趋势 ── -->
    <view v-if="isMultiLine" class="chart-multiline">
      <view class="grouped-legend">
        <view class="legend-item" v-for="(s, i) in chartConfig.series" :key="s.name">
          <view class="legend-dot" :style="{ background: palette(i) }"></view>
          <text class="legend-name">{{ s.name }}</text>
        </view>
      </view>
      <!-- 移动端不嵌完整折线图，转成"每条 series 一行"摘要：min / avg / max + sparkline 简化 -->
      <view class="multiline-row" v-for="(s, si) in chartConfig.series" :key="s.name">
        <view class="multiline-head">
          <view class="multiline-dot" :style="{ background: palette(si) }"></view>
          <text class="multiline-name">{{ s.name }}</text>
          <text class="multiline-stats">
            min {{ formatNumber(lineMin(si)) }} ·
            avg {{ formatNumber(lineAvg(si)) }} ·
            max {{ formatNumber(lineMax(si)) }}
          </text>
        </view>
        <!-- sparkline：每个点一个竖条，高度按值占该 series 最大值的比例 -->
        <view class="multiline-spark">
          <view
            v-for="(v, vi) in s.data"
            :key="vi"
            class="multiline-spark-bar"
            :style="{
              height: sparkHeight(si, vi) + '%',
              background: palette(si)
            }"
          ></view>
        </view>
      </view>
      <!-- x 轴时间标签横滚 -->
      <scroll-view class="multiline-xaxis" scroll-x :show-scrollbar="false">
        <view class="multiline-xaxis-row">
          <text class="multiline-xaxis-tick" v-for="(t, i) in chartConfig.xCategories" :key="i">{{ t }}</text>
        </view>
      </scroll-view>
    </view>

    <!-- ── grouped_bar：2 维数据（如 班组 × 等级），堆叠条 + 图例 ── -->
    <view v-if="isGrouped" class="chart-grouped">
      <!-- 图例 -->
      <view class="grouped-legend">
        <view class="legend-item" v-for="(s, i) in chartConfig.series" :key="s.name">
          <view class="legend-dot" :style="{ background: palette(i) }"></view>
          <text class="legend-name">{{ s.name }}</text>
        </view>
      </view>
      <!-- 每个 group 一行：堆叠段 + 合计 -->
      <view class="grouped-row" v-for="(g, gi) in chartConfig.groups" :key="g">
        <view class="grouped-row-head">
          <text class="grouped-group-name">{{ g }}</text>
          <text class="grouped-group-total">{{ formatTon(groupTotal(gi)) }}</text>
        </view>
        <view class="grouped-bar-track">
          <view
            v-for="(s, si) in chartConfig.series"
            :key="s.name"
            class="grouped-bar-seg"
            :style="{
              width: segPercent(gi, si) + '%',
              background: palette(si)
            }"
          ></view>
        </view>
        <!-- 段值详情：水平滚动避免挤压 -->
        <scroll-view class="grouped-detail" scroll-x :show-scrollbar="false">
          <view class="grouped-detail-row">
            <view class="grouped-detail-chip" v-for="(s, si) in chartConfig.series" :key="s.name">
              <view class="grouped-detail-dot" :style="{ background: palette(si) }"></view>
              <text class="grouped-detail-name">{{ s.name }}</text>
              <text class="grouped-detail-value">{{ formatTon(s.data[gi] || 0) }}</text>
            </view>
          </view>
        </scroll-view>
      </view>
    </view>

    <!-- ── 1 维横向 bar 列表 ── -->
    <view v-else-if="isCategorical" class="chart-bars">
      <view
        class="chart-bar-row"
        v-for="(item, idx) in normalizedData"
        :key="idx"
      >
        <view class="chart-bar-label">
          <view class="chart-color-dot" :style="{ background: palette(idx) }"></view>
          <text class="chart-name">{{ item.name }}</text>
        </view>
        <view class="chart-bar-track">
          <view
            class="chart-bar-fill"
            :style="{
              width: item.percent + '%',
              background: palette(idx)
            }"
          ></view>
        </view>
        <view class="chart-bar-meta">
          <text class="chart-value">{{ formatTon(item.value) }}</text>
          <text class="chart-percent">{{ item.percent }}%</text>
        </view>
      </view>
    </view>

    <!-- 线性数据（趋势）→ 简化为分组列表 -->
    <view v-else class="chart-bars">
      <view
        class="chart-bar-row"
        v-for="(item, idx) in normalizedData"
        :key="idx"
      >
        <view class="chart-bar-label">
          <text class="chart-name">{{ item.name }}</text>
        </view>
        <view class="chart-bar-meta">
          <text class="chart-value">{{ formatNumber(item.value) }}</text>
        </view>
      </view>
    </view>
  </view>
</template>

<script>
const PALETTE = [
  '#16a34a', '#3b82f6', '#14b8a6', '#a855f7', '#f59e0b',
  '#ef4444', '#8b5cf6', '#06b6d4', '#84cc16', '#ec4899',
  '#f97316', '#10b981', '#6366f1', '#f43f5e', '#0ea5e9',
];

export default {
  name: 'ChatChartBubble',
  props: {
    chartConfig: { type: Object, default: null }
  },
  computed: {
    hasData() {
      const data = this.chartConfig && this.chartConfig.data;
      return Array.isArray(data) && data.length > 0;
    },
    // donut/pie/bar 算"分类型"；line 算时序型
    isCategorical() {
      const t = String(this.chartConfig?.type || '').toLowerCase();
      return t !== 'line';
    },
    // 2 维：grouped_bar（按 cat1 分组的堆叠条形）
    isGrouped() {
      const t = String(this.chartConfig?.type || '').toLowerCase();
      return t === 'grouped_bar' || t === 'stacked_bar';
    },
    hasGroupedData() {
      const cfg = this.chartConfig;
      return Array.isArray(cfg?.groups) && cfg.groups.length > 0 && Array.isArray(cfg?.series) && cfg.series.length > 0;
    },
    // 多折线：每个 series 一条线，x 是时间
    isMultiLine() {
      const t = String(this.chartConfig?.type || '').toLowerCase();
      return t === 'multi_line';
    },
    hasMultiLineData() {
      const cfg = this.chartConfig;
      return Array.isArray(cfg?.xCategories) && cfg.xCategories.length > 0
        && Array.isArray(cfg?.series) && cfg.series.length > 0;
    },
    // 整体总计（所有 group × series 之和）
    groupedGrandTotal() {
      const cfg = this.chartConfig;
      if (!cfg?.series) return 0;
      let total = 0;
      cfg.series.forEach((s) => {
        (s.data || []).forEach((v) => { total += Number(v || 0); });
      });
      return total;
    },
    // 每个 group 的最大合计值，用于按比例画条
    groupedMaxTotal() {
      const cfg = this.chartConfig;
      if (!cfg?.groups || !cfg?.series) return 0;
      let max = 0;
      cfg.groups.forEach((_g, gi) => {
        let sum = 0;
        cfg.series.forEach((s) => { sum += Number((s.data || [])[gi] || 0); });
        if (sum > max) max = sum;
      });
      return max;
    },
    // 把数据归一化成 { name, value, percent }
    normalizedData() {
      const raw = this.chartConfig?.data || [];
      const totalFromCenter = Number(this.chartConfig?.center_value);
      // 优先用 item.percent，缺失就按值占总比算
      const totalValue = raw.reduce((s, d) => s + Number(d.value || 0), 0);
      return raw.map((d) => {
        const v = Number(d.value || 0);
        let p = Number(d.percent);
        if (!isFinite(p) || p < 0) {
          p = totalValue > 0 ? Math.round((v / totalValue) * 10000) / 100 : 0;
        }
        return {
          name: d.name || d.category || d.date || '—',
          value: v,
          percent: p
        };
      });
    }
  },
  methods: {
    palette(idx) { return PALETTE[idx % PALETTE.length]; },
    // 某 group 的合计（所有 series 在该 group index 的值之和）
    groupTotal(gi) {
      const cfg = this.chartConfig;
      if (!cfg?.series) return 0;
      let sum = 0;
      cfg.series.forEach((s) => { sum += Number((s.data || [])[gi] || 0); });
      return sum;
    },
    // 第 si 个 series 在第 gi 个 group 的占该 group 总合的百分比（用于堆叠段宽度）
    // 注意：宽度按"该 group 自己的合计"算，避免某个班组特别大时其他班组的条全变短
    // 但如果要对比横向规模，可改为按 groupedMaxTotal 算
    segPercent(gi, si) {
      const cfg = this.chartConfig;
      if (!cfg?.series) return 0;
      const seg = Number((cfg.series[si]?.data || [])[gi] || 0);
      const total = this.groupTotal(gi);
      if (total <= 0) return 0;
      // 按本行合计占整体最大合计的比例 × 该段占本行的比例
      const rowRatio = this.groupedMaxTotal > 0 ? (total / this.groupedMaxTotal) : 1;
      return Math.round((seg / total) * rowRatio * 10000) / 100;
    },
    // ── multi_line helpers ──
    lineSeries(si) {
      return (this.chartConfig?.series?.[si]?.data) || [];
    },
    lineMax(si) {
      const d = this.lineSeries(si);
      return d.length ? Math.max(...d.map(Number)) : 0;
    },
    lineMin(si) {
      const d = this.lineSeries(si);
      return d.length ? Math.min(...d.map(Number)) : 0;
    },
    lineAvg(si) {
      const d = this.lineSeries(si);
      if (!d.length) return 0;
      return d.reduce((s, v) => s + Number(v || 0), 0) / d.length;
    },
    // sparkline 高度：该点值 / 该 series 最大值 × 100%
    sparkHeight(si, vi) {
      const max = this.lineMax(si);
      if (max <= 0) return 4;  // 全 0 时给个最小高度便于可见
      const v = Number(this.lineSeries(si)[vi] || 0);
      return Math.max(4, Math.round((v / max) * 100));
    },
    formatTon(kg) {
      if (!kg || isNaN(kg)) return '0';
      if (kg >= 1000) return (kg / 1000).toFixed(2) + ' 吨';
      return kg.toFixed(0) + ' kg';
    },
    formatNumber(v) {
      if (v == null || isNaN(v)) return '—';
      const n = Number(v);
      if (n === Math.floor(n)) return n.toLocaleString();
      return n.toFixed(2);
    }
  }
};
</script>

<style lang="scss" scoped>
.chat-chart-bubble {
  margin-top: 20rpx;
  padding: 24rpx;
  background: #fafbfc;
  border: 1rpx solid #e2e8f0;
  border-radius: 16rpx;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  margin-bottom: 20rpx;
  flex-wrap: wrap;
  gap: 12rpx;
}

.chart-title {
  font-size: 26rpx;
  color: #1e293b;
  font-weight: 600;
}

.chart-subtitle {
  font-size: 22rpx;
  color: #64748b;
}

.chart-total {
  color: #0f172a;
  font-weight: 700;
}

.chart-bars {
  display: flex;
  flex-direction: column;
  gap: 14rpx;
}

.chart-bar-row {
  display: flex;
  align-items: center;
  gap: 16rpx;
}

.chart-bar-label {
  flex-shrink: 0;
  width: 140rpx;
  display: flex;
  align-items: center;
  gap: 8rpx;
}

.chart-color-dot {
  width: 14rpx;
  height: 14rpx;
  border-radius: 50%;
  flex-shrink: 0;
}

.chart-name {
  font-size: 22rpx;
  color: #334155;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.chart-bar-track {
  flex: 1;
  height: 14rpx;
  background: #e2e8f0;
  border-radius: 10rpx;
  overflow: hidden;
  min-width: 60rpx;
}

.chart-bar-fill {
  height: 100%;
  border-radius: 10rpx;
  transition: width 0.4s ease;
  min-width: 4rpx;
}

.chart-bar-meta {
  flex-shrink: 0;
  width: 160rpx;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 2rpx;
}

.chart-value {
  font-size: 22rpx;
  color: #1e293b;
  font-weight: 500;
}

.chart-percent {
  font-size: 20rpx;
  color: #64748b;
}

/* ── grouped_bar：2 维分组堆叠条形图 ── */
.chart-grouped {
  display: flex;
  flex-direction: column;
  gap: 22rpx;
}

.grouped-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 10rpx 18rpx;
  margin-bottom: 4rpx;
  padding: 10rpx 14rpx;
  background: #ffffff;
  border-radius: 10rpx;
  border: 1rpx solid #e2e8f0;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 8rpx;
}

.legend-dot {
  width: 14rpx;
  height: 14rpx;
  border-radius: 4rpx;
  flex-shrink: 0;
}

.legend-name {
  font-size: 22rpx;
  color: #475569;
}

.grouped-row {
  display: flex;
  flex-direction: column;
  gap: 8rpx;
}

.grouped-row-head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
}

.grouped-group-name {
  font-size: 26rpx;
  color: #1e293b;
  font-weight: 600;
}

.grouped-group-total {
  font-size: 22rpx;
  color: #475569;
  font-weight: 500;
}

/* 堆叠条 */
.grouped-bar-track {
  display: flex;
  height: 22rpx;
  background: #f1f5f9;
  border-radius: 12rpx;
  overflow: hidden;
  min-width: 0;
}

.grouped-bar-seg {
  height: 100%;
  transition: width 0.5s ease;
  /* 段之间留 1px 白色细缝增加可读性 */
  box-shadow: inset -1rpx 0 0 0 #ffffff;
}

.grouped-bar-seg:last-child {
  box-shadow: none;
}

/* 段值详情：水平横滚 */
.grouped-detail {
  white-space: nowrap;
}

.grouped-detail-row {
  display: inline-flex;
  gap: 10rpx;
  padding-top: 4rpx;
}

.grouped-detail-chip {
  display: inline-flex;
  align-items: center;
  gap: 6rpx;
  flex-shrink: 0;
  padding: 4rpx 12rpx;
  background: #f8fafc;
  border-radius: 12rpx;
  border: 1rpx solid #e2e8f0;
}

.grouped-detail-dot {
  width: 10rpx;
  height: 10rpx;
  border-radius: 50%;
  flex-shrink: 0;
}

.grouped-detail-name {
  font-size: 20rpx;
  color: #64748b;
}

.grouped-detail-value {
  font-size: 20rpx;
  color: #1e293b;
  font-weight: 600;
}

/* ── multi_line：每个 series 一行 sparkline + 统计摘要 ── */
.chart-multiline {
  display: flex;
  flex-direction: column;
  gap: 18rpx;
}

.multiline-row {
  display: flex;
  flex-direction: column;
  gap: 6rpx;
}

.multiline-head {
  display: flex;
  align-items: center;
  gap: 10rpx;
  flex-wrap: wrap;
}

.multiline-dot {
  width: 14rpx;
  height: 14rpx;
  border-radius: 4rpx;
  flex-shrink: 0;
}

.multiline-name {
  font-size: 24rpx;
  color: #1e293b;
  font-weight: 600;
}

.multiline-stats {
  flex: 1;
  text-align: right;
  font-size: 20rpx;
  color: #64748b;
}

/* sparkline：等宽竖条 */
.multiline-spark {
  display: flex;
  align-items: flex-end;
  gap: 2rpx;
  height: 50rpx;
  padding: 4rpx 0;
  background: #f8fafc;
  border-radius: 6rpx;
}

.multiline-spark-bar {
  flex: 1;
  min-width: 2rpx;
  border-radius: 2rpx;
  transition: height 0.4s ease;
}

.multiline-xaxis {
  white-space: nowrap;
  margin-top: 4rpx;
}

.multiline-xaxis-row {
  display: inline-flex;
  gap: 14rpx;
  padding: 0 4rpx;
}

.multiline-xaxis-tick {
  font-size: 18rpx;
  color: #94a3b8;
  flex-shrink: 0;
}
</style>
