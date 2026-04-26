<template>
  <view class="dashboard-page">
    <!-- 骨架屏：加载时占满全屏 -->
    <view class="skeleton-overlay" v-if="loading">
      <view class="skeleton-header"></view>
      <view class="skeleton-kpi">
        <view class="skeleton-kpi-item" v-for="i in 4" :key="i"></view>
      </view>
      <view class="skeleton-card" v-for="i in 3" :key="'c'+i">
        <view class="skeleton-card-title"></view>
        <view class="skeleton-card-body">
          <view class="skeleton-card-line" v-for="j in 4" :key="'l'+j"></view>
        </view>
      </view>
    </view>

    <!-- 日期选择与刷新 -->
    <view class="header-bar">
      <view class="date-picker-group">
        <picker mode="date" :value="startDate" @change="onStartDateChange">
          <view class="date-picker">{{ startDate }}</view>
        </picker>
        <text class="date-sep">~</text>
        <picker mode="date" :value="endDate" @change="onEndDateChange">
          <view class="date-picker">{{ endDate }}</view>
        </picker>
      </view>
      <text class="refresh-btn" @click="fetchData(true)">刷新</text>
    </view>

    <!-- 空数据提示 -->
    <view class="empty-tip" v-if="!loading && totalWeight === 0">
      <text>当前日期范围内暂无数据</text>
    </view>

    <!-- KPI 卡片 -->
    <view class="kpi-section" v-if="totalWeight > 0 || loading">
      <view class="kpi-card">
        <text class="kpi-value">{{ formatNumber(totalWeight) }}</text>
        <text class="kpi-label">检测总重量(kg)</text>
      </view>
      <view class="kpi-card">
        <text class="kpi-value" :style="{ color: getRateColor(qualifiedRate) }">
          {{ qualifiedRate.toFixed(2) }}%
        </text>
        <text class="kpi-label">合格率</text>
      </view>
      <view class="kpi-card">
        <text class="kpi-value">{{ laminationAvg.toFixed(4) }}</text>
        <text class="kpi-label">叠片系数均值</text>
      </view>
      <view class="kpi-card">
        <text class="kpi-value">{{ formatNumber(todayWeight) }}</text>
        <text class="kpi-label">今日产量(kg)</text>
        <text class="kpi-change" :class="changeRate >= 0 ? 'up' : 'down'">
          {{ changeRate >= 0 ? '↑' : '↓' }}{{ Math.abs(changeRate).toFixed(1) }}%
        </text>
      </view>
    </view>

    <!-- 质量分布 -->
    <view class="section-card" v-if="distributionList.length > 0">
      <view class="section-header">
        <text class="section-title">质量分布</text>
      </view>
      <view class="distribution-list">
        <view class="distribution-item" v-for="(item, index) in distributionList" :key="index">
          <view class="dist-color" :style="{ backgroundColor: item.color }"></view>
          <text class="dist-name">{{ item.name }}</text>
          <text class="dist-weight">{{ formatNumber(item.weight) }}kg</text>
          <text class="dist-rate">{{ item.rate.toFixed(2) }}%</text>
          <view class="dist-bar-bg">
            <view class="dist-bar-fill" :style="{ width: item.rate + '%', backgroundColor: item.color }"></view>
          </view>
        </view>
      </view>
    </view>

    <!-- 叠片系数趋势 -->
    <view class="section-card" v-if="laminationTrendData.length > 0">
      <view class="section-header">
        <text class="section-title">叠片系数趋势</text>
      </view>
      <view class="trend-legend" v-if="trendSpecs.length > 0">
        <view class="legend-item" v-for="(spec, idx) in trendSpecs" :key="idx">
          <view class="legend-dot" :style="{ backgroundColor: trendColors[idx % trendColors.length] }"></view>
          <text class="legend-text">{{ spec }}</text>
        </view>
      </view>
      <canvas canvas-id="trendCanvas" class="trend-canvas" :style="{ width: canvasWidth + 'px', height: '200px' }"></canvas>
    </view>

    <!-- 不合格 Top5 -->
    <view class="section-card" v-if="unqualifiedTop5.length > 0">
      <view class="section-header">
        <text class="section-title">不合格原因 Top5</text>
      </view>
      <view class="top5-list">
        <view class="top5-item" v-for="(item, index) in unqualifiedTop5" :key="index">
          <text class="top5-rank">{{ index + 1 }}</text>
          <text class="top5-name">{{ item.categoryName }}</text>
          <text class="top5-weight">{{ formatNumber(item.weight) }}kg</text>
          <view class="top5-bar-bg">
            <view class="top5-bar-fill" :style="{ width: item.maxRate > 0 ? (item.rate / item.maxRate * 100) + '%' : '0%' }"></view>
          </view>
        </view>
      </view>
    </view>

    <!-- 班次对比 -->
    <view class="section-card" v-if="shiftComparisonData.length > 0">
      <view class="section-header">
        <text class="section-title">班次对比</text>
      </view>
      <view class="shift-list">
        <view class="shift-item" v-for="(item, index) in shiftComparisonData" :key="index">
          <text class="shift-name">{{ item.shift }}</text>
          <text class="shift-weight">{{ formatNumber(item.totalWeight) }}kg</text>
          <text class="shift-rate" :style="{ color: getRateColor(item.qualifiedRate) }">
            {{ item.qualifiedRate.toFixed(2) }}%
          </text>
        </view>
      </view>
    </view>

    <!-- 厚度-叠片系数关联 -->
    <view class="section-card" v-if="thicknessCorrelationData.length > 0">
      <view class="section-header">
        <text class="section-title">厚度-叠片系数关联</text>
      </view>
      <canvas canvas-id="scatterCanvas" class="scatter-canvas" :style="{ width: canvasWidth + 'px', height: '200px' }"></canvas>
    </view>
  </view>
</template>

<script setup>
import { ref, computed, nextTick } from 'vue'
import { onShow, onReady, onPullDownRefresh } from '@dcloudio/uni-app'
import { getMonthlyReport, getLaminationTrend, getThicknessCorrelation, getDailyProduction } from '@/api/dashboard.js'
import { formatDate, getStartOfMonth, getToday } from '@/utils/date.js'

const loading = ref(false)
const canvasWidth = ref(350)
const startDate = ref(formatDate(getStartOfMonth()))
const endDate = ref(formatDate(getToday()))

const totalWeight = ref(0)
const qualifiedRate = ref(0)
const qualifiedCategories = ref({})
const qualifiedWeight = ref(0)
const unqualifiedCategories = ref({})
const unqualifiedWeight = ref(0)
const unqualifiedRate = ref(0)

const todayWeight = ref(0)
const yesterdayWeight = ref(0)
const changeRate = ref(0)

const laminationTrendData = ref([])
const thicknessCorrelationData = ref([])
const details = ref([])

// 缓存标识：记录当前已缓存数据的日期范围
const cacheDateRange = ref('')

const trendColors = ['#1890ff', '#52c41a', '#faad14', '#f5222d', '#722ed1', '#13c2c2']

const distributionList = computed(() => {
  const list = []
  const total = totalWeight.value
  if (total <= 0) return list

  const qKeys = Object.keys(qualifiedCategories.value || {})
  qKeys.forEach((k, i) => {
    const obj = qualifiedCategories.value[k]
    const w = (obj && typeof obj === 'object') ? (obj.weight || 0) : (obj || 0)
    list.push({
      name: k,
      weight: w,
      rate: total > 0 ? (w / total * 100) : 0,
      color: trendColors[i % trendColors.length]
    })
  })

  const uKeys = Object.keys(unqualifiedCategories.value || {})
  uKeys.forEach((k) => {
    const w = unqualifiedCategories.value[k] || 0
    if (w > 0) {
      list.push({
        name: k,
        weight: w,
        rate: total > 0 ? (w / total * 100) : 0,
        color: '#f5222d'
      })
    }
  })

  return list
})

const unqualifiedTop5 = computed(() => {
  const list = []
  const totalUnq = unqualifiedWeight.value
  const uKeys = Object.keys(unqualifiedCategories.value || {})
  uKeys.forEach((k) => {
    const w = unqualifiedCategories.value[k] || 0
    if (w > 0) {
      list.push({
        categoryName: k,
        weight: w,
        rate: totalUnq > 0 ? (w / totalUnq * 100) : 0,
        maxRate: 100
      })
    }
  })
  list.sort((a, b) => b.weight - a.weight)
  const result = list.slice(0, 5)
  const maxR = result.reduce((max, item) => Math.max(max, item.rate), 0)
  result.forEach((item) => { item.maxRate = maxR > 0 ? maxR : 100 })
  return result
})

const shiftComparisonData = computed(() => {
  const shiftMap = new Map()
  const d = details.value
  d.forEach((row) => {
    const shiftKey = row.shift || '未知'
    if (!shiftMap.has(shiftKey)) {
      shiftMap.set(shiftKey, { totalWeight: 0, qualifiedWeight: 0 })
    }
    const entry = shiftMap.get(shiftKey)
    entry.totalWeight += row.detectionWeight || 0
    entry.qualifiedWeight += row.qualifiedWeight || 0
  })
  const result = []
  shiftMap.forEach((value, key) => {
    result.push({
      shift: key,
      totalWeight: value.totalWeight,
      qualifiedRate: value.totalWeight > 0 ? (value.qualifiedWeight / value.totalWeight * 100) : 0
    })
  })
  return result
})

const laminationAvg = computed(() => {
  const data = laminationTrendData.value
  if (data.length === 0) return 0
  const sum = data.reduce((s, item) => s + item.value, 0)
  return sum / data.length
})

const trendSpecs = computed(() => {
  const set = new Set()
  laminationTrendData.value.forEach((item) => {
    set.add(item.productSpecCode || '全部')
  })
  return Array.from(set)
})

function formatNumber(n) {
  if (isNaN(n)) return '0'
  if (n >= 10000) return (n / 10000).toFixed(2) + 'w'
  return Number(n).toFixed(n % 1 === 0 ? 0 : 2)
}

function getRateColor(rate) {
  if (rate >= 95) return '#52c41a'
  if (rate >= 85) return '#faad14'
  return '#f5222d'
}

function onStartDateChange(e) {
  const val = e.detail.value
  if (val && val !== startDate.value) {
    startDate.value = val
    fetchData(true)
  }
}

function onEndDateChange(e) {
  const val = e.detail.value
  if (val && val !== endDate.value) {
    endDate.value = val
    fetchData(true)
  }
}

async function fetchData(force = false) {
  const rangeKey = startDate.value + '~' + endDate.value

  // 非强制刷新时，若日期范围未变且已有数据，直接跳过
  if (!force && cacheDateRange.value === rangeKey && totalWeight.value > 0) {
    console.log('[Dashboard] 命中缓存，跳过请求:', rangeKey)
    // 确保 Canvas 图表已绘制（页面返回时可能需要重绘）
    nextTick(() => {
      drawTrendChart()
      drawScatterChart()
    })
    return
  }

  loading.value = true
  try {
    const [reportRes, laminationRes, thicknessRes] = await Promise.all([
      getMonthlyReport({ startDate: startDate.value, endDate: endDate.value }).catch((err) => {
        console.error('[Dashboard] getMonthlyReport fail:', err)
        uni.showToast({ title: '月度报表加载失败', icon: 'none' })
        return null
      }),
      getLaminationTrend({ startDate: startDate.value, endDate: endDate.value }).catch((err) => {
        console.error('[Dashboard] getLaminationTrend fail:', err)
        return null
      }),
      getThicknessCorrelation({ startDate: startDate.value, endDate: endDate.value }).catch((err) => {
        console.error('[Dashboard] getThicknessCorrelation fail:', err)
        return null
      })
    ])

    console.log('[Dashboard] reportRes:', JSON.stringify(reportRes))
    console.log('[Dashboard] laminationRes:', JSON.stringify(laminationRes ? { length: laminationRes.data?.length } : null))
    console.log('[Dashboard] thicknessRes:', JSON.stringify(thicknessRes ? { length: thicknessRes.data?.length } : null))

    if (reportRes) {
      const data = reportRes.data
      console.log('[Dashboard] monthly data:', JSON.stringify(data))
      if (data) {
        const s = data.summary
        if (s) {
          totalWeight.value = s.totalWeight || 0
          qualifiedRate.value = s.qualifiedRate || 0
          qualifiedWeight.value = s.qualifiedWeight || 0
          unqualifiedWeight.value = s.unqualifiedWeight || 0
          unqualifiedRate.value = s.unqualifiedRate || 0
          qualifiedCategories.value = s.qualifiedCategories || {}
          unqualifiedCategories.value = s.unqualifiedCategories || {}
        }
        details.value = data.details || []
      }
    }

    console.log('[Dashboard] totalWeight:', totalWeight.value)
    console.log('[Dashboard] distributionList:', JSON.stringify(distributionList.value))
    console.log('[Dashboard] unqualifiedTop5:', JSON.stringify(unqualifiedTop5.value))
    console.log('[Dashboard] shiftComparisonData:', JSON.stringify(shiftComparisonData.value))

    if (laminationRes) {
      const data = laminationRes.data
      if (Array.isArray(data)) {
        laminationTrendData.value = data.map((item) => ({
          date: item.date || '',
          productSpecCode: item.productSpecCode || null,
          productSpecName: item.productSpecName || null,
          value: item.value || 0
        }))
      }
    }

    if (thicknessRes) {
      const data = thicknessRes.data
      if (Array.isArray(data)) thicknessCorrelationData.value = data
    }

    getDailyProduction().then((res) => {
      const data = res.data
      if (data) {
        todayWeight.value = data.todayWeight || 0
        yesterdayWeight.value = data.yesterdayWeight || 0
        changeRate.value = data.changeRate || 0
      }
    }).catch(() => {})

    // 请求成功后标记缓存
    cacheDateRange.value = rangeKey

    nextTick(() => {
      drawTrendChart()
      drawScatterChart()
    })
  } catch (e) {
    console.error('[Dashboard] 获取数据失败:', e)
    uni.showToast({ title: '获取数据失败', icon: 'none' })
  } finally {
    loading.value = false
    uni.stopPullDownRefresh()
  }
}

function drawTrendChart() {
  const data = laminationTrendData.value
  if (data.length === 0) return

  const ctx = uni.createCanvasContext('trendCanvas')
  if (!ctx) return

  const width = canvasWidth.value
  const height = 200
  const padding = { top: 20, right: 20, bottom: 30, left: 40 }
  const chartWidth = width - padding.left - padding.right
  const chartHeight = height - padding.top - padding.bottom

  ctx.clearRect(0, 0, width, height)

  const specMap = new Map()
  data.forEach((item) => {
    const key = item.productSpecCode || '默认'
    if (!specMap.has(key)) specMap.set(key, { dates: [], values: [] })
    const entry = specMap.get(key)
    entry.dates.push(item.date)
    entry.values.push(item.value)
  })

  let minVal = Infinity, maxVal = -Infinity
  data.forEach((item) => {
    if (item.value < minVal) minVal = item.value
    if (item.value > maxVal) maxVal = item.value
  })
  if (minVal === Infinity) { minVal = 0; maxVal = 1 }
  const yMin = minVal - (maxVal - minVal) * 0.1
  const yMax = maxVal + (maxVal - minVal) * 0.1
  const yRange = yMax - yMin

  ctx.strokeStyle = '#f0f0f0'
  ctx.lineWidth = 1
  for (let i = 0; i <= 4; i++) {
    const y = padding.top + chartHeight * i / 4
    ctx.beginPath()
    ctx.moveTo(padding.left, y)
    ctx.lineTo(width - padding.right, y)
    ctx.stroke()

    const label = (yMax - yRange * i / 4).toFixed(3)
    ctx.fillStyle = '#8c8c8c'
    ctx.font = '10px sans-serif'
    ctx.textAlign = 'right'
    ctx.fillText(label, padding.left - 4, y + 3)
  }

  let colorIdx = 0
  specMap.forEach((entry) => {
    const color = trendColors[colorIdx % trendColors.length]
    colorIdx++
    const { dates, values } = entry
    if (dates.length === 0) return

    ctx.strokeStyle = color
    ctx.lineWidth = 2
    ctx.beginPath()
    values.forEach((v, i) => {
      const x = padding.left + chartWidth * i / (dates.length > 1 ? dates.length - 1 : 1)
      const y = padding.top + chartHeight * (1 - (v - yMin) / yRange)
      if (i === 0) ctx.moveTo(x, y)
      else ctx.lineTo(x, y)
    })
    ctx.stroke()
  })

  let firstEntry = null
  specMap.forEach((value) => { if (!firstEntry) firstEntry = value })
  if (firstEntry) {
    const dates = firstEntry.dates
    const step = Math.ceil(dates.length / 5)
    ctx.fillStyle = '#8c8c8c'
    ctx.font = '10px sans-serif'
    ctx.textAlign = 'center'
    for (let i = 0; i < dates.length; i += step) {
      const x = padding.left + chartWidth * i / (dates.length > 1 ? dates.length - 1 : 1)
      ctx.fillText(dates[i].substring(5), x, height - 8)
    }
  }

  ctx.draw()
}

function drawScatterChart() {
  const data = thicknessCorrelationData.value
  if (data.length === 0) return

  const ctx = uni.createCanvasContext('scatterCanvas')
  if (!ctx) return

  const width = canvasWidth.value
  const height = 200
  const padding = { top: 20, right: 20, bottom: 30, left: 40 }
  const chartWidth = width - padding.left - padding.right
  const chartHeight = height - padding.top - padding.bottom

  ctx.clearRect(0, 0, width, height)

  let minX = Infinity, maxX = -Infinity
  let minY = Infinity, maxY = -Infinity
  data.forEach((item) => {
    const x = item.thickness || 0
    const y = item.laminationFactor || 0
    if (x < minX) minX = x
    if (x > maxX) maxX = x
    if (y < minY) minY = y
    if (y > maxY) maxY = y
  })
  const xRange = maxX - minX
  const yRange = maxY - minY

  ctx.strokeStyle = '#f0f0f0'
  ctx.lineWidth = 1
  for (let i = 0; i <= 4; i++) {
    const y = padding.top + chartHeight * i / 4
    ctx.beginPath()
    ctx.moveTo(padding.left, y)
    ctx.lineTo(width - padding.right, y)
    ctx.stroke()
  }

  data.forEach((item) => {
    const xVal = item.thickness || 0
    const yVal = item.laminationFactor || 0
    const x = padding.left + chartWidth * (xVal - minX) / (xRange > 0 ? xRange : 1)
    const y = padding.top + chartHeight * (1 - (yVal - minY) / (yRange > 0 ? yRange : 1))

    ctx.fillStyle = '#1890ff'
    ctx.beginPath()
    ctx.arc(x, y, 3, 0, Math.PI * 2)
    ctx.fill()
  })

  ctx.fillStyle = '#8c8c8c'
  ctx.font = '10px sans-serif'
  ctx.textAlign = 'right'
  ctx.fillText(maxY.toFixed(3), padding.left - 4, padding.top + 3)
  ctx.fillText(minY.toFixed(3), padding.left - 4, padding.top + chartHeight + 3)
  ctx.textAlign = 'center'
  ctx.fillText(minX.toFixed(2), padding.left, height - 8)
  ctx.fillText(maxX.toFixed(2), width - padding.right, height - 8)
  ctx.fillText('厚度', width / 2, height - 8)

  ctx.draw()
}

onShow(() => {
  fetchData(false)
})

onReady(() => {
  const sysInfo = uni.getSystemInfoSync()
  canvasWidth.value = sysInfo.windowWidth - 32
  if (laminationTrendData.value.length > 0) drawTrendChart()
  if (thicknessCorrelationData.value.length > 0) drawScatterChart()
})

onPullDownRefresh(() => {
  fetchData(true)
})
</script>

<style lang="scss">
.dashboard-page {
  min-height: 100vh;
  background: #f7f9fc;
  padding: 12px;
  padding-bottom: constant(safe-area-inset-bottom);
  padding-bottom: env(safe-area-inset-bottom);
  box-sizing: border-box;
}

.header-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #ffffff;
  border-radius: 12px;
  padding: 12px 16px;
  margin-bottom: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
}

.date-picker-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.date-picker {
  font-size: 14px;
  color: #262626;
  background: #f5f7fa;
  padding: 6px 12px;
  border-radius: 6px;
}

.date-sep {
  font-size: 14px;
  color: #8c8c8c;
}

.refresh-btn {
  font-size: 14px;
  color: #1890ff;
  font-weight: 600;
}

.empty-tip {
  text-align: center;
  padding: 40px 0;
  color: #8c8c8c;
  font-size: 14px;
}

.kpi-section {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  margin-bottom: 12px;
}

.kpi-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  align-items: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
}

.kpi-value {
  font-size: 20px;
  font-weight: 700;
  color: #262626;
  margin-bottom: 4px;
}

.kpi-label {
  font-size: 12px;
  color: #8c8c8c;
}

.kpi-change {
  font-size: 11px;
  margin-top: 2px;
}

.kpi-change.up {
  color: #52c41a;
}

.kpi-change.down {
  color: #f5222d;
}

.section-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
}

.section-header {
  margin-bottom: 12px;
}

.section-title {
  font-size: 15px;
  font-weight: 600;
  color: #262626;
}

.distribution-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.distribution-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.dist-color {
  width: 10px;
  height: 10px;
  border-radius: 2px;
  flex-shrink: 0;
}

.dist-name {
  font-size: 13px;
  color: #595959;
  width: 70px;
  flex-shrink: 0;
}

.dist-weight {
  font-size: 12px;
  color: #8c8c8c;
  width: 70px;
  text-align: right;
  flex-shrink: 0;
}

.dist-rate {
  font-size: 12px;
  color: #262626;
  width: 50px;
  text-align: right;
  font-weight: 600;
  flex-shrink: 0;
}

.dist-bar-bg {
  flex: 1;
  height: 6px;
  background: #f0f0f0;
  border-radius: 3px;
  overflow: hidden;
}

.dist-bar-fill {
  height: 100%;
  border-radius: 3px;
}

.trend-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 8px;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.legend-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.legend-text {
  font-size: 11px;
  color: #8c8c8c;
}

.trend-canvas,
.scatter-canvas {
  width: 100%;
  height: 200px;
}

.top5-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.top5-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.top5-rank {
  width: 18px;
  height: 18px;
  line-height: 18px;
  text-align: center;
  background: #f0f0f0;
  border-radius: 50%;
  font-size: 11px;
  color: #595959;
  flex-shrink: 0;
}

.top5-name {
  font-size: 13px;
  color: #595959;
  width: 80px;
  flex-shrink: 0;
}

.top5-weight {
  font-size: 12px;
  color: #8c8c8c;
  width: 70px;
  text-align: right;
  flex-shrink: 0;
}

.top5-bar-bg {
  flex: 1;
  height: 8px;
  background: #f0f0f0;
  border-radius: 4px;
  overflow: hidden;
}

.top5-bar-fill {
  height: 100%;
  background: linear-gradient(90deg, #ff4d4f, #ff7875);
  border-radius: 4px;
}

.shift-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.shift-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
}

.shift-item:last-child {
  border-bottom: none;
}

.shift-name {
  font-size: 14px;
  color: #595959;
  width: 80px;
}

.shift-weight {
  font-size: 13px;
  color: #262626;
  font-weight: 500;
}

.shift-rate {
  font-size: 14px;
  font-weight: 600;
}

/* 骨架屏 */
.skeleton-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: #f7f9fc;
  z-index: 999;
  padding: 12px;
  box-sizing: border-box;
  overflow-y: auto;
}

.skeleton-header {
  height: 48px;
  background: #e8e8e8;
  border-radius: 12px;
  margin-bottom: 12px;
  animation: skeleton-pulse 1.5s ease-in-out infinite;
}

.skeleton-kpi {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  margin-bottom: 12px;
}

.skeleton-kpi-item {
  height: 90px;
  background: #e8e8e8;
  border-radius: 12px;
  animation: skeleton-pulse 1.5s ease-in-out infinite;
}

.skeleton-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
}

.skeleton-card-title {
  width: 40%;
  height: 16px;
  background: #e8e8e8;
  border-radius: 4px;
  margin-bottom: 12px;
  animation: skeleton-pulse 1.5s ease-in-out infinite;
}

.skeleton-card-body {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.skeleton-card-line {
  height: 14px;
  background: #e8e8e8;
  border-radius: 4px;
  animation: skeleton-pulse 1.5s ease-in-out infinite;
}

.skeleton-card-line:nth-child(2) {
  width: 80%;
}

.skeleton-card-line:nth-child(3) {
  width: 60%;
}

.skeleton-card-line:nth-child(4) {
  width: 90%;
}

@keyframes skeleton-pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.4;
  }
}
</style>
