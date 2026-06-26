<template>
  <view class="u-chart-wrap" :style="{ width: width + 'px', height: height + 'px' }">
    <!-- #ifdef APP-HARMONY -->
    <!-- 鸿蒙：必须用 type="2d" + createSelectorQuery 获取 canvas 节点才能得到正确的
         CanvasRenderingContext2D；legacy canvas 的 measureText 在鸿蒙上是异步且返回值
         不正确，会导致图例重叠和 Y 轴标签越界。
         来源：uni-app 官方 CanvasContext 文档；release 4.84.2025110307 修复记录。-->
    <canvas
      class="u-chart-canvas"
      type="2d"
      :id="canvasId"
      :canvas-id="canvasId"
      :style="{ width: width + 'px', height: height + 'px' }"
      @touchstart="onTouchStart"
      @touchmove="onTouchMove"
      @touchend="onTouchEnd"
    ></canvas>
    <!-- #endif -->

    <!-- #ifndef APP-HARMONY -->
    <!-- 非鸿蒙（Android / H5 / iOS）：沿用 legacy canvas-id 模式；
         width/height HTML 属性必须同时写，否则 H5 缓冲区默认 300x150 导致坐标错位。-->
    <canvas
      class="u-chart-canvas"
      :canvas-id="canvasId"
      :id="canvasId"
      :width="width"
      :height="height"
      :style="{ width: width + 'px', height: height + 'px' }"
      @touchstart="onTouchStart"
      @touchmove="onTouchMove"
      @touchend="onTouchEnd"
    ></canvas>
    <!-- #endif -->
  </view>
</template>

<script>
import uCharts from '@qiun/ucharts'

let __counter = 0

export default {
  name: 'UChart',
  props: {
    type: { type: String, default: 'line' },        // line / area / scatter / column / pie / ring / mix
    categories: { type: Array, default: () => [] }, // x 轴
    series: { type: Array, default: () => [] },     // 多系列：[{ name, data }]
    width: { type: Number, default: 350 },
    height: { type: Number, default: 200 },
    colors: { type: Array, default: () => ['#1890ff', '#52c41a', '#fa8c16', '#f5222d', '#722ed1', '#13c2c2'] },
    title: { type: String, default: '' },
    yAxisDecimal: { type: Number, default: 2 }
  },
  data() {
    __counter += 1
    return {
      canvasId: 'uchart_' + __counter + '_' + Date.now(),
      chart: null,
      // dpr 用于 canvas2d 路径（鸿蒙）的背景缓冲区缩放和触摸坐标换算
      dpr: (typeof uni !== 'undefined' && uni.getSystemInfoSync) ? (uni.getSystemInfoSync().pixelRatio || 2) : 2,
      _harmony2dCtx: null,
      _harmony2dCanvas: null
    }
  },
  watch: {
    categories() { this.$nextTick(() => this.render()) },
    series:     { handler() { this.$nextTick(() => this.render()) }, deep: true },
    width()     { this.$nextTick(() => this._resetAndRender()) },
    height()    { this.$nextTick(() => this._resetAndRender()) },
    type()      { this.$nextTick(() => this.render()) }
  },
  mounted() {
    // 鸿蒙路径需要先通过 createSelectorQuery 拿到节点再渲染；
    // 其他平台沿用 50ms 延迟（uni-app/qiun-data-charts 官方示例标准做法）。
    // #ifdef APP-HARMONY
    this._initHarmonyCanvas()
    // #endif
    // #ifndef APP-HARMONY
    setTimeout(() => this.render(), 50)
    // #endif
  },
  beforeUnmount() {
    this.chart = null
    this._harmony2dCtx = null
    this._harmony2dCanvas = null
  },
  methods: {
    // ── 鸿蒙 canvas2d 初始化 ────────────────────────────────────────────────
    // 通过 createSelectorQuery 获取真实 canvas 节点，设置背景缓冲区尺寸，
    // 保存 ctx/canvas 供后续 render() 直接复用（无需每次重查）。
    _initHarmonyCanvas(callback) {
      // #ifdef APP-HARMONY
      const cid = this.canvasId
      const dpr = this.dpr
      uni.createSelectorQuery()
        .in(this)
        .select('#' + cid)
        .fields({ node: true, size: true })
        .exec((res) => {
          if (!res || !res[0] || !res[0].node) {
            console.warn('[u-chart] 鸿蒙 canvas 节点未就绪，100ms 后重试', cid)
            setTimeout(() => this._initHarmonyCanvas(callback), 100)
            return
          }
          const canvas = res[0].node
          const ctx = canvas.getContext('2d')
          // 背景缓冲区放大到物理像素，保证高清屏渲染清晰
          canvas.width  = res[0].width  * dpr
          canvas.height = res[0].height * dpr
          this._harmony2dCanvas = canvas
          this._harmony2dCtx    = ctx
          console.log('[u-chart] 鸿蒙 canvas2d 节点就绪', cid, 'dpr=', dpr,
            'css=', res[0].width + 'x' + res[0].height,
            'buf=', canvas.width + 'x' + canvas.height)
          if (callback) callback()
          else this.render()
        })
      // #endif
    },

    // width/height 变化时需要重置 canvas2d 节点（缓冲区尺寸已变），再重绘
    _resetAndRender() {
      // #ifdef APP-HARMONY
      this._harmony2dCtx    = null
      this._harmony2dCanvas = null
      this._initHarmonyCanvas()
      // #endif
      // #ifndef APP-HARMONY
      this.render()
      // #endif
    },

    // ── 非鸿蒙 legacy context 获取 ──────────────────────────────────────────
    _getLegacyContext() {
      // 全平台统一用 uni.createCanvasContext —— H5 上 uni 内部已经 polyfill 了 .draw()
      // 方法，避免 uCharts 在 H5 上调用 ctx.draw() 时报错 / 静默不画。
      return uni.createCanvasContext(this.canvasId, this)
    },

    // ── 配置构建 ─────────────────────────────────────────────────────────────
    buildOpts() {
      const base = {
        type: this.type,
        canvasId: this.canvasId,
        canvas2d: false,
        width: this.width,
        height: this.height,
        background: '#ffffff',
        color: this.colors,
        padding: [16, 16, 8, 16],
        enableScroll: false,
        animation: true,
        legend: {
          show: this.series.length > 1,
          position: 'top',
          fontSize: 11,
          padding: 6,
          itemGap: 12
        },
        xAxis: {
          disableGrid: true,
          fontSize: 10,
          fontColor: '#94a3b8',
          rotateLabel: (this.categories && this.categories.length > 8),
          rotateAngle: 45
        },
        yAxis: {
          gridType: 'dash',
          dashLength: 4,
          gridColor: '#e2e8f0',
          fontSize: 10,
          fontColor: '#94a3b8',
          format: (v) => Number(v).toFixed(this.yAxisDecimal)
        },
        extra: {
          tooltip: {
            showBox: true,
            // bgColor 必须是 hex —— uCharts 画 tooltip 时会把它喂给内部 hexToRgb()，
            // 传 rgba() 会让其正则匹配返回 null 进而 `[1] of null` 崩溃（drawToolTip）。
            bgColor: '#0f172a',
            bgOpacity: 0.92,
            fontColor: '#f1f5f9'
          }
        }
      }

      // ── 鸿蒙专项修正 ──────────────────────────────────────────────────────
      // 即使在 HBuilderX 4.84+ measureText 修复后，左侧留白仍需稍大以防边缘裁切；
      // 图例 itemGap 加大补偿宽度计算误差；Y 轴数字缩写减少标签宽度需求。
      // #ifdef APP-HARMONY
      base.canvas2d   = true
      base.pixelRatio = this.dpr
      base.padding    = [16, 16, 8, 60]
      base.legend.itemGap = 24
      if (this.type !== 'mix') {
        base.yAxis.format = (v) => {
          const n = Number(v)
          if (n >= 10000)  return (n / 10000).toFixed(1) + 'w'
          if (n >= 1000)   return (n / 1000).toFixed(1) + 'k'
          return n.toFixed(this.yAxisDecimal)
        }
      }
      // #endif

      // ── 各图表类型专项配置 ────────────────────────────────────────────────
      if (this.type === 'line' || this.type === 'area') {
        const cats = Array.isArray(this.categories) ? this.categories : []
        const maxLabels = 10
        base.categories = (cats.length > maxLabels)
          ? cats.map((c, i) => {
              const step = Math.ceil(cats.length / maxLabels)
              return (i === 0 || i === cats.length - 1 || i % step === 0) ? c : ''
            })
          : cats
        base.series = this.series.map((s) => ({
          name: s.name,
          data: s.data
        }))
        base.dataLabel = false
        base.dataPointShape = true
        base.dataPointShapeType = 'solid'
        base.extra.line = {
          type: 'curve',
          width: 2.5,
          activeType: 'hollow'
        }
        if (this.type === 'area') {
          base.extra.area = {
            type: 'curve',
            opacity: 0.18,
            addLine: true,
            width: 2,
            gradient: true
          }
        }
      }

      if (this.type === 'scatter') {
        base.series = this.series
        base.xAxis.disabled = false
        base.xAxis.disableGrid = false
        base.xAxis.gridType = 'dash'
        base.xAxis.gridColor = '#e2e8f0'
        base.extra.scatter = {}
      }

      if (this.type === 'column') {
        base.categories = this.categories
        base.series = this.series
        base.extra.column = {
          type: 'group',
          width: 18,
          activeBgColor: '#000000',
          activeBgOpacity: 0.06,
          linearType: 'custom',
          linearOpacity: 0.55,
          barBorderCircle: true,
          // uCharts 内部 hexToRgb 只接受 hex 字符串，不能用 rgba()
          customColor: ['#bae7ff', '#d9f7be', '#ffe7ba', '#ffccc7', '#efdbff']
        }
        base.dataLabel = true
      }

      if (this.type === 'mix') {
        base.categories = this.categories
        base.series = this.series
        base.extra.mix = {
          column: {
            width: 18,
            linearType: 'custom',
            linearOpacity: 0.55,
            barBorderCircle: true,
            activeBgColor: '#000000',
            activeBgOpacity: 0.06,
            customColor: ['#bae7ff', '#d9f7be', '#ffe7ba', '#ffccc7']
          },
          line: {
            type: 'curve',
            width: 2.5,
            activeType: 'hollow'
          }
        }
        base.dataLabel = true
        base.dataPointShape = true
        base.dataPointShapeType = 'solid'
        // 双 Y 轴：左产量、右合格率(%)
        base.yAxis = {
          disabled: false,
          disableGrid: false,
          data: [
            {
              min: 0,
              position: 'left',
              fontSize: 10,
              fontColor: '#94a3b8',
              gridType: 'dash',
              dashLength: 4,
              gridColor: '#e2e8f0',
              format: (v) => Number(v) >= 10000 ? (Number(v) / 1000).toFixed(0) + 'k' : Number(v).toFixed(0)
            },
            {
              min: 0,
              max: 100,
              position: 'right',
              fontSize: 10,
              fontColor: '#94a3b8',
              disabled: false,
              calibration: true,
              format: (v) => Number(v).toFixed(0) + '%'
            }
          ]
        }
      }

      if (this.type === 'pie' || this.type === 'ring') {
        base.series = this.series
        base.extra[this.type] = {
          activeOpacity: 0.5,
          activeRadius: 6,
          offsetAngle: -90,
          labelWidth: 12,
          border: true,
          borderWidth: 2,
          borderColor: '#ffffff',
          ...(this.type === 'ring' ? { ringWidth: 24, customRadius: 0 } : {})
        }
      }

      return base
    },

    // ── 渲染入口 ─────────────────────────────────────────────────────────────
    render() {
      if (!this.width || !this.height) {
        console.warn('[u-chart] width/height invalid, skip render', this.width, this.height)
        return
      }
      const hasData = (this.series && this.series.length > 0) &&
        this.series.some((s) => Array.isArray(s.data) && s.data.length > 0)
      if (!hasData) {
        console.log('[u-chart] empty series, skip render')
        return
      }

      // #ifdef APP-HARMONY
      if (!this._harmony2dCtx || !this._harmony2dCanvas) {
        console.log('[u-chart] 鸿蒙 canvas2d 未就绪，等待节点初始化后渲染', this.canvasId)
        this._initHarmonyCanvas()
        return
      }
      try {
        const opts = this.buildOpts()
        opts.context = this._harmony2dCtx
        opts.canvas  = this._harmony2dCanvas
        this.chart = new uCharts(opts)
        console.log('[u-chart] 鸿蒙 canvas2d 渲染完成', this.canvasId,
          'type=', this.type, 'w/h=', this.width + 'x' + this.height, 'dpr=', this.dpr)
      } catch (e) {
        console.error('[u-chart] 鸿蒙渲染失败:', e)
      }
      return
      // #endif

      // #ifndef APP-HARMONY
      const ctx = this._getLegacyContext()
      if (!ctx) {
        console.warn('[u-chart] context not ready, retry in 100ms', this.canvasId)
        setTimeout(() => this.render(), 100)
        return
      }
      try {
        const opts = this.buildOpts()
        opts.context = ctx
        this.chart = new uCharts(opts)
        const seriesLens = (this.series || []).map((s) => Array.isArray(s.data) ? s.data.length : 0)
        console.log('[u-chart] rendered', this.canvasId,
          'type=', this.type,
          'w/h=', this.width + 'x' + this.height,
          'categories=', (this.categories || []).length,
          'seriesCount=', this.series.length,
          'seriesDataLens=', JSON.stringify(seriesLens))
      } catch (e) {
        console.error('[u-chart] render failed:', e)
      }
      // #endif
    },

    // ── 触摸事件 ─────────────────────────────────────────────────────────────
    // 鸿蒙 canvas2d 路径：uCharts 内部把触摸坐标乘以 pixelRatio，
    // 所以这里必须先除以 dpr，否则 tooltip 坐标会偏移 dpr 倍。
    // 非鸿蒙（legacy）：坐标直接透传。
    // 双指/多指事件直接 bail：uCharts scroll 会大量重绘并 block 主线程。
    _scaleTouchEvent(e) {
      // #ifdef APP-HARMONY
      if (!e || !e.touches || this.dpr === 1) return e
      const dpr = this.dpr
      return Object.assign({}, e, {
        touches: Array.from(e.touches).map((t) => Object.assign({}, t, {
          x: t.x / dpr,
          y: t.y / dpr,
          clientX: (t.clientX || t.x) / dpr,
          clientY: (t.clientY || t.y) / dpr
        })),
        changedTouches: Array.from(e.changedTouches || []).map((t) => Object.assign({}, t, {
          x: t.x / dpr,
          y: t.y / dpr,
          clientX: (t.clientX || t.x) / dpr,
          clientY: (t.clientY || t.y) / dpr
        }))
      })
      // #endif
      // #ifndef APP-HARMONY
      return e
      // #endif
    },
    onTouchStart(e) {
      if (!this.chart) return
      if (e.touches && e.touches.length > 1) return
      const ev = this._scaleTouchEvent(e)
      if (this.chart.scrollStart) this.chart.scrollStart(ev)
    },
    onTouchMove(e) {
      if (!this.chart) return
      if (e.touches && e.touches.length > 1) return
      const ev = this._scaleTouchEvent(e)
      if (this.chart.scroll) this.chart.scroll(ev)
      if (this.chart.showToolTip) this.chart.showToolTip(ev, { format: this.tooltipFormat })
    },
    onTouchEnd(e) {
      if (!this.chart) return
      if (this.chart.scrollEnd) this.chart.scrollEnd(e)
    },
    tooltipFormat(item, category) {
      return item.name + '：' + (item.data == null ? '-' : Number(item.data).toFixed(this.yAxisDecimal))
    }
  }
}
</script>

<style lang="scss" scoped>
.u-chart-wrap {
  position: relative;
  display: block;
}
.u-chart-canvas {
  width: 100%;
  height: 100%;
}
</style>
