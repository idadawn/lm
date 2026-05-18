<template>
  <view class="report-page">
    <!-- 顶部：快速日期 + 横屏切换 -->
    <view class="header">
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
          <view class="quick-chip quick-chip--rotate" @click="toggleLandscape">
            <text class="quick-chip-text">{{ isLandscape ? '↩️ 竖屏' : '🔄 横屏' }}</text>
          </view>
        </view>
      </scroll-view>
      <view class="range-summary">
        <text class="range-text">{{ startDate }} ～ {{ endDate }}</text>
      </view>
    </view>

    <!-- 视图切换 tab -->
    <view class="view-tabs">
      <view
        class="view-tab"
        :class="{ active: activeView === 'detail' }"
        @click="activeView = 'detail'"
      >
        <text class="view-tab-text">质量检测明细</text>
        <text class="view-tab-count">{{ details.length }}</text>
      </view>
      <view
        class="view-tab"
        :class="{ active: activeView === 'shift' }"
        @click="activeView = 'shift'"
      >
        <text class="view-tab-text">班组统计</text>
        <text class="view-tab-count">{{ shiftStats.length }}</text>
      </view>
    </view>

    <!-- 表格区 -->
    <view class="table-wrap" v-if="!loading">
      <!-- 明细 -->
      <scroll-view
        v-if="activeView === 'detail'"
        class="table-scroll"
        scroll-x
        scroll-y
        :show-scrollbar="true"
      >
        <view v-if="details.length === 0" class="table-empty">
          <text class="empty-icon">📋</text>
          <text class="empty-text">所选时间范围内没有明细记录</text>
        </view>
        <view v-else class="table-inner">
          <view class="tr tr-head">
            <view class="th" v-for="col in detailColumns" :key="col.key" :style="{ width: col.width + 'rpx' }">
              <text class="th-text">{{ col.title }}</text>
            </view>
          </view>
          <view
            class="tr tr-body"
            v-for="(row, ri) in details"
            :key="ri"
            :class="{ alt: ri % 2 === 1 }"
          >
            <view class="td" v-for="col in detailColumns" :key="col.key" :style="{ width: col.width + 'rpx' }">
              <text class="td-text">{{ formatCell(row[col.key], col) }}</text>
            </view>
          </view>
        </view>
      </scroll-view>

      <!-- 班组统计 -->
      <scroll-view
        v-if="activeView === 'shift'"
        class="table-scroll"
        scroll-x
        scroll-y
        :show-scrollbar="true"
      >
        <view v-if="shiftStats.length === 0" class="table-empty">
          <text class="empty-icon">👥</text>
          <text class="empty-text">所选时间范围内没有班组数据</text>
        </view>
        <view v-else class="table-inner">
          <view class="tr tr-head">
            <view class="th" v-for="col in shiftColumns" :key="col.key" :style="{ width: col.width + 'rpx' }">
              <text class="th-text">{{ col.title }}</text>
            </view>
          </view>
          <view
            class="tr tr-body"
            v-for="(row, ri) in shiftStats"
            :key="ri"
            :class="{ alt: ri % 2 === 1, summary: row.isSummary }"
          >
            <view class="td" v-for="col in shiftColumns" :key="col.key" :style="{ width: col.width + 'rpx' }">
              <text class="td-text">{{ formatCell(row[col.key], col) }}</text>
            </view>
          </view>
        </view>
      </scroll-view>
    </view>

    <!-- 加载占位 -->
    <view v-if="loading" class="table-empty">
      <text class="empty-text">加载中…</text>
    </view>

    <!-- 横屏 overlay：表格在 portrait 视口里旋转 90° 显示，可看到更多列。
         用户歪头看（向左转头），整个 app 不动。
         旋转方式：fixed 占满视口的 wrapper 内放一个 stage，stage 宽高互换后绕中心 rotate(90deg)，
         即可严格填满视口且不溢出。 -->
    <view v-if="isLandscape" class="landscape-overlay">
      <view class="landscape-stage">
        <view class="landscape-toolbar">
          <text class="landscape-title">
            {{ activeView === 'detail' ? '质量检测明细' : '班组统计' }}
            ({{ activeView === 'detail' ? details.length : shiftStats.length }})
          </text>
          <view class="landscape-exit" @click="toggleLandscape">
            <text class="landscape-exit-text">↩️ 退出横屏</text>
          </view>
        </view>
        <scroll-view class="landscape-scroll" scroll-x scroll-y :show-scrollbar="true">
          <view class="table-inner" v-if="activeView === 'detail'">
            <view class="tr tr-head">
              <view class="th" v-for="col in detailColumns" :key="col.key" :style="{ width: col.width + 'rpx' }">
                <text class="th-text">{{ col.title }}</text>
              </view>
            </view>
            <view class="tr tr-body" v-for="(row, ri) in details" :key="ri" :class="{ alt: ri % 2 === 1 }">
              <view class="td" v-for="col in detailColumns" :key="col.key" :style="{ width: col.width + 'rpx' }">
                <text class="td-text">{{ formatCell(row[col.key], col) }}</text>
              </view>
            </view>
          </view>
          <view class="table-inner" v-else>
            <view class="tr tr-head">
              <view class="th" v-for="col in shiftColumns" :key="col.key" :style="{ width: col.width + 'rpx' }">
                <text class="th-text">{{ col.title }}</text>
              </view>
            </view>
            <view class="tr tr-body" v-for="(row, ri) in shiftStats" :key="ri" :class="{ alt: ri % 2 === 1, summary: row.isSummary }">
              <view class="td" v-for="col in shiftColumns" :key="col.key" :style="{ width: col.width + 'rpx' }">
                <text class="td-text">{{ formatCell(row[col.key], col) }}</text>
              </view>
            </view>
          </view>
        </scroll-view>
      </view>
    </view>
  </view>
</template>

<script setup>
import { ref, computed } from 'vue'
import { onShow, onPullDownRefresh } from '@dcloudio/uni-app'
import { getMonthlyReport } from '@/api/dashboard.js'
import { formatDate, getStartOfMonth, getToday, getQuickRange, detectQuickRangeKey } from '@/utils/date.js'

// ── 日期范围 ──
const startDate = ref(formatDate(getStartOfMonth()))
const endDate = ref(formatDate(getToday()))

const quickRangeOptions = [
  { key: 'today',          label: '今日' },
  { key: 'this_week',      label: '本周' },
  { key: 'last_week',      label: '上周' },
  { key: 'current_month',  label: '本月' },
  { key: 'last_month',     label: '上月' },
]
const activeRangeKey = computed(() => detectQuickRangeKey(startDate.value, endDate.value))

function applyQuickRange(key) {
  const [s, e] = getQuickRange(key)
  if (s === startDate.value && e === endDate.value) return
  startDate.value = s
  endDate.value = e
  fetchData()
}

// ── 数据 ──
const loading = ref(false)
const details = ref([])

async function fetchData() {
  loading.value = true
  try {
    const res = await getMonthlyReport({ startDate: startDate.value, endDate: endDate.value })
    const data = res && res.data
    if (data) {
      details.value = Array.isArray(data.details) ? data.details : []
    } else {
      details.value = []
    }
  } catch (err) {
    console.error('[Report] fetch fail', err)
    uni.showToast({ title: '报表加载失败', icon: 'none' })
    details.value = []
  } finally {
    loading.value = false
    uni.stopPullDownRefresh()
  }
}

onShow(() => fetchData())
onPullDownRefresh(() => fetchData())

// ── 视图切换 ──
const activeView = ref('detail')

// ── 明细表列定义 ──
// 列从 details 第一条记录的字段动态生成，固定列在前 + 检测重量 + 合格重量 + 其他权重列
const FIXED_COLS = [
  { key: 'prodDate', title: '生产日期', width: 180, type: 'date' },
  { key: 'productSpecCode', title: '带宽', width: 100 },
  { key: 'shift', title: '班次', width: 80 },
  { key: 'shiftNo', title: '炉号', width: 220 },
]

const KNOWN_NUMERIC = ['detectionWeight', 'qualifiedWeight', 'unqualifiedWeight']

const detailColumns = computed(() => {
  const cols = [...FIXED_COLS]
  if (details.value.length === 0) return cols
  const sample = details.value[0]
  const fixedKeys = new Set(FIXED_COLS.map((c) => c.key))
  // 优先内置数值列
  KNOWN_NUMERIC.forEach((key) => {
    if (sample[key] !== undefined && !fixedKeys.has(key)) {
      cols.push({ key, title: autoTitle(key), width: 150, type: 'number' })
      fixedKeys.add(key)
    }
  })
  // 其他数值列（动态生成的等级重量、占比等）
  // 跳过：DTO 内部字段 + 嵌套对象/数组（QualifiedCategories、UnqualifiedCategories、DynamicStats 等 Dictionary）
  const SKIP_KEYS = new Set(['isSummaryRow', 'summaryType', 'id', 'productSpecId',
    'qualifiedCategories', 'unqualifiedCategories', 'dynamicStats'])
  Object.keys(sample).forEach((key) => {
    if (fixedKeys.has(key) || SKIP_KEYS.has(key)) return
    const v = sample[key]
    if (v == null) return
    if (typeof v === 'object') return  // 跳过 Dictionary 嵌套
    if (typeof v === 'number' || typeof v === 'string') {
      const isRate = /Rate$|Ratio$|占比/.test(key)
      cols.push({
        key,
        title: autoTitle(key),
        width: isRate ? 130 : 150,
        type: isRate ? 'rate' : (typeof v === 'number' ? 'number' : 'text'),
      })
    }
  })
  return cols
})

// ── 班组统计计算 ──
// 注意：后端 details 包含 isSummaryRow=true 的合计行（详情表会单独高亮），
// 这些行**不能参与班次聚合**，否则会出现一个 shift='合计' 的伪班次混在中间。
const shiftStats = computed(() => {
  const map = new Map()
  details.value.forEach((row) => {
    if (row.isSummaryRow) return                                  // 跳过后端的合计行
    const k = (row.shift || '').trim()
    if (!k || k === '合计' || k === '总计' || k === '小计') return // 防御：异常数据也跳过
    if (!map.has(k)) {
      map.set(k, {
        shift: k,
        detectionWeight: 0,
        qualifiedWeight: 0,
        unqualifiedWeight: 0,
        rowCount: 0,
      })
    }
    const e = map.get(k)
    e.detectionWeight += Number(row.detectionWeight || 0)
    e.qualifiedWeight += Number(row.qualifiedWeight || 0)
    e.unqualifiedWeight += Number(row.unqualifiedWeight || 0)
    e.rowCount += 1
  })

  // 行业惯例顺序：甲 → 乙 → 丙；其他班次保持原序追加
  const SHIFT_ORDER = ['甲', '乙', '丙', '丁']
  const out = Array.from(map.values()).sort((a, b) => {
    const ia = SHIFT_ORDER.indexOf(a.shift)
    const ib = SHIFT_ORDER.indexOf(b.shift)
    if (ia !== -1 && ib !== -1) return ia - ib
    if (ia !== -1) return -1
    if (ib !== -1) return 1
    return (a.shift || '').localeCompare(b.shift || '')
  }).map((e) => ({
    shift: e.shift,
    rowCount: e.rowCount,
    detectionWeight: e.detectionWeight,
    qualifiedWeight: e.qualifiedWeight,
    unqualifiedWeight: e.unqualifiedWeight,
    qualifiedRate: e.detectionWeight > 0 ? (e.qualifiedWeight / e.detectionWeight * 100) : 0,
  }))

  // 合计行追加到最后一行
  if (out.length > 0) {
    const totalDet = out.reduce((s, r) => s + r.detectionWeight, 0)
    const totalQual = out.reduce((s, r) => s + r.qualifiedWeight, 0)
    const totalUnq = out.reduce((s, r) => s + r.unqualifiedWeight, 0)
    const totalCount = out.reduce((s, r) => s + r.rowCount, 0)
    out.push({
      shift: '合计',
      rowCount: totalCount,
      detectionWeight: totalDet,
      qualifiedWeight: totalQual,
      unqualifiedWeight: totalUnq,
      qualifiedRate: totalDet > 0 ? (totalQual / totalDet * 100) : 0,
      isSummary: true,
    })
  }
  return out
})

const shiftColumns = [
  { key: 'shift', title: '班次', width: 120 },
  { key: 'rowCount', title: '记录数', width: 110, type: 'number' },
  { key: 'detectionWeight', title: '检测重量(kg)', width: 170, type: 'number' },
  { key: 'qualifiedWeight', title: '合格重量(kg)', width: 170, type: 'number' },
  { key: 'unqualifiedWeight', title: '不合格重量(kg)', width: 180, type: 'number' },
  { key: 'qualifiedRate', title: '合格率', width: 110, type: 'rate' },
]

// ── 表头名映射（把驼峰转中文，覆盖所有 DTO 字段 + 兜底转换） ──
const KEY_TITLE = {
  prodDate: '生产日期',
  productSpecCode: '带宽',
  productSpecName: '规格名称',
  shift: '班次',
  shiftNo: '炉号',
  furnaceNoFormatted: '炉号',
  furnaceBatchNo: '批次号',

  detectionWeight: '检测重量(kg)',
  qualifiedWeight: '合格重量(kg)',
  qualifiedRate: '合格率',
  unqualifiedWeight: '不合格重量(kg)',
  unqualifiedRate: '不合格率',

  classAWeight: 'A类重量(kg)',
  classARate: 'A类占比',
  classBWeight: 'B类重量(kg)',
  classBRate: 'B类占比',
  classCWeight: 'C类重量(kg)',
  classCRate: 'C类占比',
  classDWeight: 'D类重量(kg)',
  classDRate: 'D类占比',

  isSummaryRow: '汇总行',
  summaryType: '汇总类型',
}

// 驼峰 → 中文兜底（A → A类、Rate → 占比、Weight → 重量(kg)）
function autoTitle(key) {
  if (KEY_TITLE[key]) return KEY_TITLE[key]
  // class[X]Weight / class[X]Rate
  const m = /^class([A-Z][a-z]*)(Weight|Rate)$/.exec(key)
  if (m) {
    const cls = m[1]
    return cls + '类' + (m[2] === 'Weight' ? '重量(kg)' : '占比')
  }
  // 通用驼峰转空格 + 末尾常见词替换
  let t = key.replace(/([A-Z])/g, ' $1').replace(/^./, (c) => c.toUpperCase()).trim()
  t = t.replace(/\bWeight\b/g, '重量(kg)').replace(/\bRate\b/g, '占比').replace(/\bRatio\b/g, '占比')
  return t
}

function formatCell(value, col) {
  if (value == null || value === '') return '-'
  if (col.type === 'date') {
    // 支持 ISO 字符串
    try {
      const d = new Date(value)
      if (!isNaN(d.getTime())) return formatDate(d)
    } catch (_) {}
    return String(value)
  }
  if (col.type === 'rate') {
    const n = Number(value)
    if (isNaN(n)) return String(value)
    return n.toFixed(2) + '%'
  }
  if (col.type === 'number') {
    const n = Number(value)
    if (isNaN(n)) return String(value)
    if (n === Math.floor(n)) return n.toLocaleString()
    return n.toFixed(2)
  }
  return String(value)
}

// ── 横屏切换（仅对表格 overlay 做 CSS rotate，整个 app 不旋转） ──
// 设计：用户点「🔄 横屏」→ 弹出一个固定定位的 overlay，里面只有表格 + 工具栏，
// 整体绕中心旋转 90°，用户歪头（向左转头）就能看到更多列。
// 隐藏 tabBar 让 overlay 干净；退出时恢复。
const isLandscape = ref(false)

function toggleLandscape() {
  isLandscape.value = !isLandscape.value
  if (isLandscape.value) {
    try { uni.hideTabBar && uni.hideTabBar({ animation: false }) } catch (_) {}
  } else {
    try { uni.showTabBar && uni.showTabBar({ animation: false }) } catch (_) {}
  }
}
</script>

<style lang="scss">
.report-page {
  min-height: 100vh;
  background: #f7f9fc;
  display: flex;
  flex-direction: column;
}


.header {
  padding: 16rpx 24rpx 8rpx;
  background: #f7f9fc;
}

/* 快速日期 chip 行（同首页样式，统一视觉） */
.quick-range-bar {
  white-space: nowrap;
}
.quick-range-chips {
  display: inline-flex;
  align-items: center;
  gap: 12rpx;
  padding: 4rpx 0;
}
.quick-chip {
  flex-shrink: 0;
  padding: 12rpx 24rpx;
  background: #ffffff;
  border-radius: 32rpx;
  border: 1rpx solid #e2e8f0;
  box-shadow: 0 2rpx 8rpx rgba(15, 23, 42, 0.04);
}
.quick-chip-text {
  font-size: 24rpx;
  color: #475569;
}
.quick-chip.active {
  background: linear-gradient(135deg, #1890ff, #40a9ff);
  border-color: transparent;
  box-shadow: 0 4rpx 12rpx rgba(24, 144, 255, 0.28);
}
.quick-chip.active .quick-chip-text {
  color: #ffffff;
  font-weight: 600;
}
.quick-chip--rotate {
  background: #fef3c7;
  border-color: #fde68a;
}
.quick-chip--rotate .quick-chip-text { color: #92400e; }

.range-summary {
  margin-top: 12rpx;
  text-align: right;
}
.range-text {
  font-size: 22rpx;
  color: #94a3b8;
}

/* 视图切换 tab */
.view-tabs {
  display: flex;
  gap: 0;
  padding: 16rpx 24rpx 8rpx;
  background: #f7f9fc;
}
.view-tab {
  flex: 1;
  padding: 18rpx 0;
  text-align: center;
  background: #ffffff;
  border: 1rpx solid #e2e8f0;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10rpx;
  transition: all 0.2s;
}
.view-tab:first-child { border-radius: 12rpx 0 0 12rpx; border-right: none; }
.view-tab:last-child  { border-radius: 0 12rpx 12rpx 0; }
.view-tab.active {
  background: linear-gradient(135deg, #1890ff, #40a9ff);
  border-color: transparent;
}
.view-tab.active .view-tab-text,
.view-tab.active .view-tab-count { color: #ffffff; }
.view-tab-text {
  font-size: 26rpx;
  color: #475569;
  font-weight: 500;
}
.view-tab-count {
  font-size: 22rpx;
  color: #94a3b8;
  background: rgba(15, 23, 42, 0.06);
  padding: 2rpx 12rpx;
  border-radius: 16rpx;
}
.view-tab.active .view-tab-count { background: rgba(255, 255, 255, 0.25); }

/* 表格区 */
.table-wrap {
  flex: 1;
  padding: 12rpx 24rpx 24rpx;
  overflow: hidden;
}
.table-scroll {
  height: calc(100vh - 320rpx);
  background: #ffffff;
  border: 1rpx solid #e2e8f0;
  border-radius: 12rpx;
  overflow: hidden;
}

.table-inner {
  display: inline-block;
  min-width: 100%;
}

.tr {
  display: flex;
  align-items: stretch;
}
.tr-head {
  position: sticky;
  top: 0;
  background: #f1f5f9;
  z-index: 1;
  border-bottom: 1rpx solid #e2e8f0;
}
.tr-body {
  border-bottom: 1rpx solid #f1f5f9;
}
.tr-body.alt { background: #fafbfc; }
.tr-body.summary {
  background: #eff6ff;
  font-weight: 600;
}

.th, .td {
  flex-shrink: 0;
  padding: 16rpx 12rpx;
  border-right: 1rpx solid #e2e8f0;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  box-sizing: border-box;
}
.th:last-child, .td:last-child { border-right: none; }

.th-text {
  font-size: 22rpx;
  color: #334155;
  font-weight: 600;
  white-space: nowrap;
}
.td-text {
  font-size: 22rpx;
  color: #1e293b;
  white-space: nowrap;
}
.tr-body.summary .td-text { color: #1e3a8a; font-weight: 700; }

/* 空态 */
.table-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 120rpx 40rpx;
  gap: 16rpx;
}
.empty-icon { font-size: 64rpx; }
.empty-text {
  font-size: 26rpx;
  color: #94a3b8;
}

/* ── 横屏 overlay：只旋转表格，整个 app 不动 ── */
.landscape-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: #ffffff;
  z-index: 9999;
  overflow: hidden;
}

/* stage：宽高互换（width = 100vh, height = 100vw），居中后旋转 90°
   → 旋转后正好填满 portrait 视口（100vw × 100vh） */
.landscape-stage {
  position: absolute;
  top: 50%;
  left: 50%;
  width: 100vh;
  height: 100vw;
  margin-top: -50vw;
  margin-left: -50vh;
  transform: rotate(90deg);
  transform-origin: center center;
  background: #ffffff;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
}

.landscape-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12rpx 24rpx;
  background: #f1f5f9;
  border-bottom: 1rpx solid #e2e8f0;
  flex-shrink: 0;
}

.landscape-title {
  font-size: 28rpx;
  font-weight: 600;
  color: #1e293b;
}

.landscape-exit {
  background: linear-gradient(135deg, #1890ff, #40a9ff);
  border-radius: 24rpx;
  padding: 10rpx 24rpx;
  box-shadow: 0 4rpx 12rpx rgba(24, 144, 255, 0.28);
}
.landscape-exit-text {
  font-size: 24rpx;
  color: #ffffff;
  font-weight: 600;
}

.landscape-scroll {
  flex: 1;
  overflow: hidden;
}
</style>
