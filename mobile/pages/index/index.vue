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

    <!-- 快速日期选择 chip 行 -->
    <scroll-view class="quick-range-bar" scroll-x :show-scrollbar="false">
      <view class="quick-range-chips">
        <view
          v-for="opt in quickRangeOptions"
          :key="opt.key"
          class="quick-chip"
          :class="{ active: activeRangeKey === opt.key }"
          @click="applyQuickRange(opt.key)"
        >
          <text class="quick-chip-text">{{ opt.label }}</text>
        </view>
        <view
          class="quick-chip quick-chip--custom"
          :class="{ active: !activeRangeKey && !showCustomPicker, ring: showCustomPicker }"
          @click="showCustomPicker = !showCustomPicker"
        >
          <text class="quick-chip-text">{{ showCustomPicker ? '收起' : '自定义' }}</text>
        </view>
        <view class="quick-chip quick-chip--refresh" @click="fetchData(true)">
          <text class="quick-chip-text">🔄 刷新</text>
        </view>
      </view>
    </scroll-view>

    <!-- 当前选中区间显示 -->
    <view class="range-summary">
      <text class="range-text">{{ startDate }} ～ {{ endDate }}</text>
    </view>

    <!-- 自定义日期（折叠） -->
    <view v-if="showCustomPicker" class="custom-date-row">
      <picker mode="date" :value="startDate" @change="onStartDateChange">
        <view class="date-picker">起：{{ startDate }}</view>
      </picker>
      <text class="date-sep">~</text>
      <picker mode="date" :value="endDate" @change="onEndDateChange">
        <view class="date-picker">止：{{ endDate }}</view>
      </picker>
    </view>

    <!-- KPI 卡片：始终显示，无数据时显示占位 — — -->
    <view class="kpi-section" :class="{ 'kpi-loaded': !loading, 'kpi-empty': !loading && totalWeight === 0 }">
      <view class="kpi-card kpi-card--weight" @click="onKpiClick('weight')">
        <view class="kpi-top-icon kpi-top-icon--weight"></view>
        <text class="kpi-value">{{ totalWeight > 0 ? formatNumber(totalWeight) : '—' }}</text>
        <text class="kpi-label">检测总重量(kg)</text>
      </view>
      <view class="kpi-card kpi-card--check" @click="onKpiClick('check')">
        <view class="kpi-top-icon kpi-top-icon--check"></view>
        <text class="kpi-value" :style="{ color: totalWeight > 0 ? getRateColor(qualifiedRate) : '#cbd5e1' }">
          {{ totalWeight > 0 ? qualifiedRate.toFixed(2) + '%' : '—' }}
        </text>
        <text class="kpi-label">合格率</text>
      </view>
      <view class="kpi-card kpi-card--chart" @click="onKpiClick('chart')">
        <view class="kpi-top-icon kpi-top-icon--chart"></view>
        <text class="kpi-value">{{ laminationAvg > 0 ? laminationAvg.toFixed(4) : '—' }}</text>
        <text class="kpi-label">叠片系数均值</text>
      </view>
      <view class="kpi-card kpi-card--trend" @click="onKpiClick('trend')">
        <view class="kpi-top-icon kpi-top-icon--trend"></view>
        <text class="kpi-value">{{ todayWeight > 0 ? formatNumber(todayWeight) : '—' }}</text>
        <text class="kpi-label">今日产量(kg)</text>
        <text v-if="todayWeight > 0" class="kpi-change" :class="changeRate >= 0 ? 'up' : 'down'">
          {{ changeRate >= 0 ? '↑' : '↓' }}{{ Math.abs(changeRate).toFixed(1) }}%
        </text>
      </view>
    </view>

    <!-- 质量分布 -->
    <view class="section-card section-card--distribution" :style="cardStyle(0)">
      <view class="section-header">
        <text class="section-title">质量分布</text>
      </view>
      <view v-if="distributionList.length > 0" class="distribution-list">
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
      <view v-else class="section-empty">
        <text class="section-empty-icon">📊</text>
        <text class="section-empty-text">本期暂无质量分布数据</text>
      </view>
    </view>

    <!-- 叠片系数趋势 -->
    <view class="section-card section-card--trend" :style="cardStyle(1)">
      <view class="section-header">
        <text class="section-title">叠片系数趋势</text>
      </view>
      <UChart
        v-if="laminationTrendData.length > 0"
        type="area"
        :categories="trendCategories"
        :series="trendSeriesForChart"
        :width="canvasWidth"
        :height="220"
        :colors="trendColors"
        :y-axis-decimal="3"
      />
      <view v-else class="section-empty">
        <text class="section-empty-icon">📈</text>
        <text class="section-empty-text">本期暂无叠片系数趋势</text>
      </view>
    </view>

    <!-- 不合格 Top5 -->
    <view class="section-card section-card--top5" :style="cardStyle(2)">
      <view class="section-header">
        <text class="section-title">不合格原因 Top5</text>
      </view>
      <view v-if="unqualifiedTop5.length > 0" class="top5-list">
        <view class="top5-item" v-for="(item, index) in unqualifiedTop5" :key="index">
          <text class="top5-rank">{{ index + 1 }}</text>
          <text class="top5-name">{{ item.categoryName }}</text>
          <text class="top5-weight">{{ formatNumber(item.weight) }}kg</text>
          <view class="top5-bar-bg">
            <view class="top5-bar-fill" :style="{ width: item.maxRate > 0 ? (item.rate / item.maxRate * 100) + '%' : '0%' }"></view>
          </view>
        </view>
      </view>
      <view v-else class="section-empty">
        <text class="section-empty-icon">✅</text>
        <text class="section-empty-text">本期没有不合格记录</text>
      </view>
    </view>

    <!-- 班次产量对比（柱+线混合） -->
    <view class="section-card section-card--shift" :style="cardStyle(3)">
      <view class="section-header">
        <text class="section-title">班次产量对比</text>
      </view>
      <UChart
        v-if="shiftComparisonData.length > 0"
        type="mix"
        :categories="shiftChartCategories"
        :series="shiftMixSeries"
        :width="canvasWidth"
        :height="240"
        :colors="['#1890ff', '#722ed1']"
        :y-axis-decimal="0"
      />
      <view v-else class="section-empty">
        <text class="section-empty-icon">👥</text>
        <text class="section-empty-text">本期暂无班次对比数据</text>
      </view>
    </view>

    <!-- 质量趋势分析（按日聚合的不合格率，平滑曲线） -->
    <view class="section-card section-card--correlation" :style="cardStyle(4)">
      <view class="section-header">
        <text class="section-title">质量趋势分析</text>
      </view>
      <UChart
        v-if="qualityTrendData.length > 0"
        type="line"
        :categories="qualityTrendCategories"
        :series="qualityTrendSeries"
        :width="canvasWidth"
        :height="220"
        :colors="['#722ed1', '#52c41a']"
        :y-axis-decimal="2"
      />
      <view v-else class="section-empty">
        <text class="section-empty-icon">📈</text>
        <text class="section-empty-text">本期暂无质量趋势数据</text>
      </view>
    </view>
  </view>
</template>

<script setup>
import { ref, computed, nextTick, onMounted } from 'vue'
import { onShow, onReady, onPullDownRefresh } from '@dcloudio/uni-app'
import { getMonthlyReport, getLaminationTrend, getThicknessCorrelation, getDailyProduction } from '@/api/dashboard.js'
import { formatDate, getStartOfMonth, getToday, getQuickRange, detectQuickRangeKey } from '@/utils/date.js'
import UChart from '@/components/u-chart/u-chart.vue'

const loading = ref(false)
const canvasWidth = ref(350)
const startDate = ref(formatDate(getStartOfMonth()))
const endDate = ref(formatDate(getToday()))

// ── 快速日期选择 ──
const quickRangeOptions = [
  { key: 'today',          label: '今日' },
  { key: 'this_week',      label: '本周' },
  { key: 'last_week',      label: '上周' },
  { key: 'current_month',  label: '本月' },
  { key: 'last_month',     label: '上月' },
  { key: 'this_year',      label: '今年' },
]
const showCustomPicker = ref(false)
// computed：当前 startDate/endDate 对应哪个快速 key（自定义时为空串）
const activeRangeKey = computed(() => detectQuickRangeKey(startDate.value, endDate.value))

function applyQuickRange(key) {
  const [s, e] = getQuickRange(key)
  if (s === startDate.value && e === endDate.value) return  // 已选中，不重复请求
  startDate.value = s
  endDate.value = e
  showCustomPicker.value = false
  fetchData(true)
}

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

// ── uCharts 数据结构改造 ──
// 1. 收集所有日期作为 categories（按时间排序去重）
// 2. 按 spec 分组成 series，每条曲线对齐到 categories 上（没有的位置填 null）
const trendCategories = computed(() => {
  const set = new Set()
  laminationTrendData.value.forEach((item) => { if (item.date) set.add(item.date) })
  return Array.from(set).sort().map((d) => d.slice(5))  // MM-DD 显示
})

const trendSeriesForChart = computed(() => {
  const allDates = Array.from(new Set(laminationTrendData.value.map((i) => i.date))).sort()
  const bySpec = new Map()
  laminationTrendData.value.forEach((item) => {
    const spec = item.productSpecCode || '全部'
    if (!bySpec.has(spec)) bySpec.set(spec, new Map())
    bySpec.get(spec).set(item.date, item.value)
  })
  const result = []
  bySpec.forEach((dateMap, spec) => {
    result.push({
      name: String(spec),
      data: allDates.map((d) => {
        const v = dateMap.get(d)
        return v == null ? null : Number(v)
      })
    })
  })
  return result
})

// 班次对比 柱+线混合图：
// - categories：班次名（甲/乙/丙/丁）
// - series[0]：柱状，产量(kg)，挂在左 Y 轴（index=0）
// - series[1]：折线，合格率(%)，挂在右 Y 轴（index=1）
const SHIFT_ORDER = ['甲班', '乙班', '丙班', '丁班', '甲', '乙', '丙', '丁']
const sortedShiftData = computed(() => {
  const arr = [...shiftComparisonData.value]
  arr.sort((a, b) => {
    const ia = SHIFT_ORDER.indexOf(a.shift)
    const ib = SHIFT_ORDER.indexOf(b.shift)
    if (ia !== -1 && ib !== -1) return ia - ib
    if (ia !== -1) return -1
    if (ib !== -1) return 1
    return String(a.shift).localeCompare(String(b.shift))
  })
  return arr
})

const shiftChartCategories = computed(() => sortedShiftData.value.map((s) => s.shift))

const shiftMixSeries = computed(() => [
  {
    name: '产量(kg)',
    index: 0,
    type: 'column',
    data: sortedShiftData.value.map((s) => Math.round(Number(s.totalWeight || 0)))
  },
  {
    name: '合格率(%)',
    index: 1,
    type: 'line',
    color: '#722ed1',
    data: sortedShiftData.value.map((s) => Number(Number(s.qualifiedRate || 0).toFixed(2)))
  }
])

// 日期解析：兼容三种后端返回格式：
// - ISO 字符串 "2026-04-15T16:00:00Z" 或 "2026-04-15"
// - Date 对象
// - Unix 时间戳数字（ms 或 s），如 1745609123456
// 全部归一化成 "YYYY-MM-DD"。
function toIsoDate(raw) {
  if (!raw) return ''
  if (raw instanceof Date) return isNaN(raw.getTime()) ? '' : formatDate(raw)
  const s = String(raw)
  // 已经是 YYYY-MM-DD 开头：直接取前 10
  if (/^\d{4}-\d{2}-\d{2}/.test(s)) return s.slice(0, 10)
  // 纯数字 → 时间戳。13 位是 ms，10 位是 s（×1000）
  if (/^\d+$/.test(s)) {
    const n = Number(s)
    const ms = s.length === 10 ? n * 1000 : n
    const d = new Date(ms)
    return isNaN(d.getTime()) ? '' : formatDate(d)
  }
  // 其他字符串交给 Date 兜底（".NET" 风格 / RFC2822 等）
  const d = new Date(s)
  return isNaN(d.getTime()) ? '' : formatDate(d)
}

// 质量趋势分析：从 details 按日聚合 不合格率/合格率
// - 后端的 isSummaryRow 合计行必须跳过，否则会把"合计"也当成一天
// - 日期格式不固定，统一走 toIsoDate 归一化成 "YYYY-MM-DD"
const qualityTrendData = computed(() => {
  const byDate = new Map()
  details.value.forEach((row) => {
    if (row.isSummaryRow) return
    const date = toIsoDate(row.prodDate)
    if (!date) return
    if (!byDate.has(date)) byDate.set(date, { det: 0, qual: 0, unq: 0 })
    const e = byDate.get(date)
    e.det += Number(row.detectionWeight || 0)
    e.qual += Number(row.qualifiedWeight || 0)
    e.unq += Number(row.unqualifiedWeight || 0)
  })
  return Array.from(byDate.entries())
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([date, v]) => ({
      date,
      unqualifiedRate: v.det > 0 ? (v.unq / v.det * 100) : 0,
      qualifiedRate:   v.det > 0 ? (v.qual / v.det * 100) : 0
    }))
})

const qualityTrendCategories = computed(() =>
  qualityTrendData.value.map((d) => d.date.slice(5))   // MM-DD
)

const qualityTrendSeries = computed(() => [
  {
    name: '不合格率(%)',
    data: qualityTrendData.value.map((d) => Number(d.unqualifiedRate.toFixed(2)))
  }
])

// 厚度-叠片系数散点图：uCharts scatter 系列要求每个点是 [x, y] 二元数组
const scatterSeriesForChart = computed(() => {
  const pts = thicknessCorrelationData.value
    .map((item) => [Number(item.thickness || 0), Number(item.laminationFactor || 0)])
    .filter(([x, y]) => x > 0 && y > 0)
  return [{
    name: '厚度-叠片系数',
    data: pts,
  }]
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

const animTick = ref(0)

function cardStyle(index) {
  const delay = index * 80
  return {
    animation: `card-enter 0.6s ease ${delay}ms both`,
    '--anim-tick': animTick.value
  }
}

function onKpiClick(type) {
  // KPI 卡片点击微交互：轻微震动反馈
  // #ifdef APP-PLUS
  if (uni.vibrateShort) uni.vibrateShort({ type: 'light' })
  // #endif
  console.log('[Dashboard] KPI clicked:', type)
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
      // UChart 组件自动响应数据变化，无需手动重绘
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
        const dateSet = new Set(laminationTrendData.value.map((i) => i.date).filter(Boolean))
        const specSet = new Set(laminationTrendData.value.map((i) => i.productSpecCode || '全部'))
        console.log('[Dashboard] 叠片趋势 原始行数=', data.length,
          '不重复日期数=', dateSet.size,
          '不重复规格数=', specSet.size,
          '前3行=', JSON.stringify(data.slice(0, 3)))
      }
    }

    if (thicknessRes) {
      const data = thicknessRes.data
      if (Array.isArray(data)) {
        thicknessCorrelationData.value = data
        const xs = data.map((d) => Number(d.thickness || 0)).filter((v) => v > 0)
        const ys = data.map((d) => Number(d.laminationFactor || 0)).filter((v) => v > 0)
        console.log('[Dashboard] 厚度散点 原始行数=', data.length,
          'x范围=', xs.length ? (Math.min(...xs) + '~' + Math.max(...xs)) : '空',
          'y范围=', ys.length ? (Math.min(...ys) + '~' + Math.max(...ys)) : '空',
          '前3行=', JSON.stringify(data.slice(0, 3)))
      }
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
    animTick.value++

    nextTick(() => {
      // UChart 组件自动响应数据变化，无需手动重绘
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

// 测量画布宽度：优先 getWindowInfo（鸿蒙 4.23+ 专用、更可靠），回退 getSystemInfoSync。
// 不用 createSelectorQuery().boundingClientRect()——鸿蒙兼容表标注其不支持。
function measureCanvasWidth() {
  try {
    const info = uni.getWindowInfo ? uni.getWindowInfo() : uni.getSystemInfoSync()
    if (info && info.windowWidth) canvasWidth.value = info.windowWidth - 32
  } catch (_) {}
}

// onReady 在鸿蒙「持续渲染」场景下有不触发的已知回归，单靠它会让 canvasWidth 停在 350 默认值、
// 图表渲染不满宽。补一个 onMounted（Vue 钩子，鸿蒙可靠触发）兜底；两处都调，幂等。
onMounted(() => {
  nextTick(() => measureCanvasWidth())
})

onReady(() => {
  measureCanvasWidth()
  // UChart 组件根据 props 自动渲染，不需要手动触发
})

onPullDownRefresh(() => {
  fetchData(true)
})
</script>

<style lang="scss">
.dashboard-page {
  min-height: 100vh;
  background: linear-gradient(180deg, #e6f7ff 0%, #f7f9fc 240px);
  padding: 16px 12px 12px;
  padding-bottom: calc(12px + constant(safe-area-inset-bottom));
  padding-bottom: calc(12px + env(safe-area-inset-bottom));
  box-sizing: border-box;
}

/* ── 快速日期选择 chip 行 ── */
.quick-range-bar {
  white-space: nowrap;
  margin-bottom: 8px;
}

.quick-range-chips {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 4px 4px 4px 0;
}

.quick-chip {
  flex-shrink: 0;
  padding: 8px 16px;
  background: #ffffff;
  border-radius: 18px;
  border: 1px solid #e2e8f0;
  font-size: 13px;
  color: #475569;
  transition: all 0.2s ease;
  box-shadow: 0 2px 6px rgba(15, 23, 42, 0.04);
}

.quick-chip-text {
  font-size: 13px;
  color: inherit;
}

.quick-chip:active,
.quick-chip.ring {
  transform: scale(0.96);
}

.quick-chip.active {
  background: linear-gradient(135deg, #1890ff, #40a9ff);
  border-color: transparent;
  color: #ffffff;
  box-shadow: 0 4px 12px rgba(24, 144, 255, 0.28);
}

.quick-chip.active .quick-chip-text {
  color: #ffffff;
  font-weight: 600;
}

.quick-chip--custom {
  background: #f8fafc;
}

.quick-chip--refresh {
  background: #f0fdf4;
  border-color: #bbf7d0;
  color: #15803d;
}

/* ── 当前选中区间显示 + 自定义日期 ── */
.range-summary {
  text-align: right;
  padding: 0 4px;
  margin-bottom: 12px;
}

.range-text {
  font-size: 12px;
  color: #94a3b8;
}

.custom-date-row {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #ffffff;
  border-radius: 12px;
  padding: 12px 14px;
  margin-bottom: 12px;
  box-shadow: 0 4px 12px rgba(24, 144, 255, 0.08);
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

.empty-tip {
  text-align: center;
  padding: 40px 0;
  color: #8c8c8c;
  font-size: 14px;
}

/* 单个 section 卡片内的空状态占位 */
.section-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 32px 12px;
  gap: 8px;
  color: #94a3b8;
}

.section-empty-icon {
  font-size: 36px;
  line-height: 1;
}

.section-empty-text {
  font-size: 13px;
  color: #94a3b8;
}

/* KPI 空态：值变浅灰但保持位置 */
.kpi-section.kpi-empty .kpi-value {
  color: #cbd5e1;
}

.kpi-section {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  margin-bottom: 16px;
}

.kpi-card {
  position: relative;
  background: #ffffff;
  border-radius: 12px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  align-items: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

.kpi-card::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
}

.kpi-card--weight::before {
  background: #1890ff;
}

.kpi-card--check::before {
  background: #52c41a;
}

.kpi-card--chart::before {
  background: #fa8c16;
}

.kpi-card--trend::before {
  background: #722ed1;
}

.kpi-top-icon {
  position: absolute;
  top: 10px;
  right: 10px;
  width: 20px;
  height: 20px;
  opacity: 0.12;
}

.kpi-top-icon--weight {
  border: 2px solid #1890ff;
  border-radius: 2px;
}

.kpi-top-icon--check {
  border: 2px solid #52c41a;
  border-radius: 50%;
}

.kpi-top-icon--chart {
  background: #fa8c16;
  border-radius: 2px;
}

.kpi-top-icon--trend {
  width: 0;
  height: 0;
  border-left: 8px solid transparent;
  border-right: 8px solid transparent;
  border-bottom: 14px solid #722ed1;
  opacity: 0.12;
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
  margin-bottom: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.04);
}

.section-header {
  display: flex;
  align-items: center;
  margin-bottom: 14px;
}

.section-title {
  font-size: 15px;
  font-weight: 600;
  color: #262626;
  padding-left: 10px;
  position: relative;
}

.section-title::before {
  content: '';
  position: absolute;
  left: 0;
  top: 2px;
  bottom: 2px;
  width: 3px;
  border-radius: 2px;
  background: #1890ff;
}

.section-card--distribution .section-title::before {
  background: #1890ff;
}

.section-card--trend .section-title::before {
  background: #fa8c16;
}

.section-card--top5 .section-title::before {
  background: #f5222d;
}

.section-card--shift .section-title::before {
  background: #52c41a;
}

.section-card--correlation .section-title::before {
  background: #722ed1;
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

/* 紧凑版：跟在混合图下方，三列网格，去掉 border */
.shift-list--compact {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(96px, 1fr));
  gap: 8px;
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px dashed #e2e8f0;
}

.shift-list--compact .shift-item {
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 8px 6px;
  border-bottom: none;
  background: #f8fafc;
  border-radius: 8px;
  gap: 4px;
}

.shift-list--compact .shift-name {
  width: auto;
  font-size: 12px;
  color: #94a3b8;
}

.shift-list--compact .shift-weight {
  font-size: 14px;
  font-weight: 600;
  color: #1e293b;
}

.shift-list--compact .shift-rate {
  font-size: 12px;
  font-weight: 600;
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

/* 卡片入场动画 */
@keyframes card-enter {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* KPI 卡片点击反馈 */
.kpi-card {
  transition: transform 0.15s ease, box-shadow 0.2s ease;
}

.kpi-card:active {
  transform: scale(0.97);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

/* 进度条过渡动画 */
.dist-bar-fill {
  transition: width 0.8s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.top5-bar-fill {
  transition: width 0.8s cubic-bezier(0.34, 1.56, 0.64, 1);
}

/* 刷新按钮旋转 */
.refresh-btn {
  transition: opacity 0.2s;
}

.refresh-btn:active {
  opacity: 0.6;
}

.refresh-rotate {
  display: inline-block;
  animation: refresh-spin 0.6s linear;
}

@keyframes refresh-spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Section 卡片悬停/按压 */
.section-card {
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.section-card:active {
  transform: scale(0.985);
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.06);
}

/* 日期选择器按压 */
.date-picker {
  transition: background 0.2s;
}

.date-picker:active {
  background: #e4e7ed;
}
</style>
