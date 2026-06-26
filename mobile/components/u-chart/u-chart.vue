<template>
  <view class="u-chart-wrap" :style="{ width: width + 'px', height: height + 'px' }">
    <!-- 三端通用：uni-app 的 <canvas> + canvas-id；H5 上 uni 内部会创建 <canvas id="xxx"> -->
    <!-- 关键：width / height 属性必须同时设为 HTML 属性（不仅 style），否则 H5 会用默认 300x150 像素缓冲区，导致 uCharts 坐标错位 -->
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
  </view>
</template>

<script>
import uCharts from '@qiun/ucharts'

let __counter = 0

export default {
  name: 'UChart',
  props: {
    type: { type: String, default: 'line' },        // line / area / scatter / column / pie / ring
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
      dpr: (typeof uni !== 'undefined' && uni.getSystemInfoSync) ? (uni.getSystemInfoSync().pixelRatio || 2) : 2
    }
  },
  watch: {
    categories() { this.$nextTick(() => this.render()) },
    series:     { handler() { this.$nextTick(() => this.render()) }, deep: true },
    width()     { this.$nextTick(() => this.render()) },
    height()    { this.$nextTick(() => this.render()) },
    type()      { this.$nextTick(() => this.render()) }
  },
  mounted() {
    // 等 canvas DOM 完整插入 + 尺寸就绪。在 H5/小程序上即使 nextTick 也可能拿不到 ctx，
    // setTimeout 50ms 是 uni-app/qiun-data-charts 官方示例的标准做法。
    setTimeout(() => this.render(), 50)
  },
  beforeUnmount() { this.chart = null },
  methods: {
    getContext() {
      // 全平台统一用 uni.createCanvasContext —— H5 上 uni 内部已经 polyfill 了 .draw()
      // 方法，避免 uCharts 在 H5 上调用 ctx.draw() 时报错 / 静默不画。
      return uni.createCanvasContext(this.canvasId, this)
    },
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
        enableScroll: false,    // 数据稀疏时滚动会把所有点挤到一起，关掉避免坐标错位
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
          // 类目多时旋转 45°避免重叠
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
            // 透明度走 bgOpacity，#0f172a == rgb(15,23,42)，最终等价于 rgba(15,23,42,0.92)。
            bgColor: '#0f172a',
            bgOpacity: 0.92,
            fontColor: '#f1f5f9'
          }
        }
      }

      // ── 鸿蒙专项纯配置兜底（不改 canvas 模式）─────────────────────────────
      // 鸿蒙 legacy canvas 的 measureText 在 HBuilderX <4.84 上不准 → 左轴标签裁切、图例重叠。
      // 加大左 padding + 图例间距补偿；HBuilderX ≥4.84 已在运行时修 measureText，留着也无副作用。
      // 注：曾试 canvas2d/type="2d" 路径，但该机型 createSelectorQuery 取不到 canvas 节点 →
      //     图表整块空白，已回退（见提交历史）。鸿蒙继续用 legacy createCanvasContext。
      // #ifdef APP-HARMONY
      base.padding = [16, 16, 8, 60]
      base.legend.itemGap = 24
      // #endif

      if (this.type === 'line' || this.type === 'area') {
        // 类目过多时，把中间的 label 置空（保留首尾 + 等间距），uCharts 仍按全量索引定位坐标，
        // 只是不渲染空 label。配合 rotateLabel 45°，30 天范围也能看清。
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
          data: s.data,
          // 点用大一点的圆，线用平滑曲线
          // 不指定 type，让 area 系列继承顶层 type
        }))
        base.dataLabel = false
        base.dataPointShape = true
        base.dataPointShapeType = 'solid'
        base.extra.line = {
          type: 'curve',         // 平滑曲线
          width: 2.5,
          activeType: 'hollow'   // 选中时空心圆
        }
        if (this.type === 'area') {
          base.extra.area = {
            type: 'curve',
            opacity: 0.18,
            addLine: true,
            width: 2,
            gradient: true       // 渐变填充
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
          linearType: 'custom',     // 每根柱子从底部浅色渐变到顶部主色
          linearOpacity: 0.55,
          barBorderCircle: true,
          // 注意：uCharts 内部 hexToRgb 只接受 hex 字符串，不能用 rgba()
          customColor: ['#bae7ff', '#d9f7be', '#ffe7ba', '#ffccc7', '#efdbff']
        }
        base.dataLabel = true
      }

      // mix：柱+线混合（默认柱:产量、线:合格率）。series 中每项需带 type: 'column' | 'line'，并可带 index 指定 y 轴
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
            // 必须是 hex，uCharts 内部 hexToRgb 不接受 rgba()
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
      const ctx = this.getContext()
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
    },
    // 触摸交互转发给 uCharts。
    // 关键：双指/多指事件**不能**传给 uCharts —— 它内部的 scroll 处理会陷入大量
    // 重绘并 block 主线程，整个页面会卡得切不动 tab。直接 bail 让浏览器
    // 处理默认手势（缩放等），uCharts 只处理单指。
    onTouchStart(e) {
      if (!this.chart) return
      if (e.touches && e.touches.length > 1) return
      if (this.chart.scrollStart) this.chart.scrollStart(e)
    },
    onTouchMove(e) {
      if (!this.chart) return
      if (e.touches && e.touches.length > 1) return
      if (this.chart.scroll) this.chart.scroll(e)
      if (this.chart.showToolTip) this.chart.showToolTip(e, { format: this.tooltipFormat })
    },
    onTouchEnd(e) {
      if (!this.chart) return
      // touchend 时 e.touches 已经空了，用 changedTouches 数量也只是一根抬起，
      // 直接调用 scrollEnd 让 uCharts 收尾即可
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
