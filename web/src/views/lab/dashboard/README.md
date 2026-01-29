# Lab Dashboard (生产驾驶舱)

## Overview

检测室数据分析系统的首页驾驶舱，提供生产质量与设备状态的实时监控。

## Features

### 1. KPI卡片区 (KpiCards.vue)
- **合格率**: 绿色圆形仪表盘，显示 A级品卷数 / 总卷数
- **今日产量**: 大号数字展示当日炉号数量
- **叠片系数均值**: 带迷你趋势图的平均值指标
- **预警数**: 橙色告警图标，展示当前预警事件列表

### 2. 质量判定分布 (QualityDistribution.vue)
- 环形图展示 A级/B级/性能不合/其他不合 的分布比例
- 点击图例可下钻筛选
- 中心显示总计卷数

### 3. 叠片系数趋势 (LaminationTrend.vue)
- 带置信区间的折线图
- 7天历史数据展示
- 显示目标值参考线

### 4. 缺陷 Top 5 (DefectTop5.vue)
- 水平条形图展示最频发的缺陷类型
- 支持：划痕、麻点、毛边、亮线、网眼等

### 5. 生产热力图 (ProductionHeatmap.vue)
- X轴：星期（Mon-Sun）
- Y轴：小时（0:00-23:00）
- 颜色编码：绿色(高质量) → 红色(低质量)
- 支持悬停查看具体数值

### 6. 厚度-叠片系数关联 (ThicknessCorrelation.vue)
- 散点图展示两个关键物理量的相关性
- 按质量等级着色区分
- 帮助识别最佳工艺窗口

### 7. AI助手 (AiAssistant.vue)
- 悬浮聊天窗口
- 快速问答预设
- 智能数据分析响应

## Usage

```vue
<template>
  <LabDashboard />
</template>

<script setup>
import LabDashboard from '/@/views/lab/dashboard/index.vue';
</script>
```

## Component Structure

```
dashboard/
├── index.vue                    # 主页面
├── components/
│   ├── KpiCards.vue            # KPI卡片区
│   ├── QualityDistribution.vue # 质量判定分布
│   ├── LaminationTrend.vue     # 叠片系数趋势
│   ├── DefectTop5.vue          # 缺陷Top5
│   ├── ProductionHeatmap.vue   # 生产热力图
│   ├── ThicknessCorrelation.vue # 厚度-叠片系数关联
│   └── AiAssistant.vue         # AI助手
└── README.md                    # 本文档
```

## Dependencies

- Vue 3.3+
- Ant Design Vue 3.2+
- ECharts 5.4+
- dayjs

## Route

```typescript
{
  path: '/lab/dashboard',
  name: 'LabDashboard',
  component: () => import('/@/views/lab/dashboard/index.vue'),
  meta: {
    title: '生产驾驶舱',
    affix: true,
  },
}
```

## Changelog

### 2026-01-29
- Initial release
- Implemented all 7 core modules
- Added AI assistant widget
- Responsive layout support
