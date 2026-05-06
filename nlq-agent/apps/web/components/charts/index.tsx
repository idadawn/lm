"use client";

import dynamic from "next/dynamic";
import { Suspense } from "react";

// 加载占位组件
function Shimmer({ className }: { className?: string }) {
  return (
    <div
      className={`animate-pulse bg-gray-200 dark:bg-gray-700 rounded ${className || ""}`}
      style={{ minHeight: "200px" }}
    />
  );
}

// 动态导入趋势折线图（禁用 SSR）
export const TrendLine = dynamic(
  () => import("./TrendLine").then((mod) => mod.TrendLine),
  {
    ssr: false,
    loading: () => <Shimmer className="h-64 w-full" />,
  },
);

// 动态导入仪表盘图（禁用 SSR）
export const MetricGauge = dynamic(
  () => import("./MetricGauge").then((mod) => mod.MetricGauge),
  {
    ssr: false,
    loading: () => <Shimmer className="h-48 w-full" />,
  },
);

// 动态导入饼图（禁用 SSR）
export const GradePie = dynamic(
  () => import("./GradePie").then((mod) => mod.GradePie),
  {
    ssr: false,
    loading: () => <Shimmer className="h-64 w-full" />,
  },
);

export const HorizontalBar = dynamic(
  () => import("./HorizontalBar").then((mod) => mod.HorizontalBar),
  {
    ssr: false,
    loading: () => <Shimmer className="h-72 w-full" />,
  },
);

export const ScatterPlot = dynamic(
  () => import("./ScatterPlot").then((mod) => mod.ScatterPlot),
  {
    ssr: false,
    loading: () => <Shimmer className="h-80 w-full" />,
  },
);

export const RadarChart = dynamic(
  () => import("./RadarChart").then((mod) => mod.RadarChart),
  {
    ssr: false,
    loading: () => <Shimmer className="h-80 w-full" />,
  },
);

// 图表渲染器包装组件
interface ChartRendererProps {
  chartSpec: import("@nlq-agent/shared-types").ChartDescriptor;
}

export function ChartRenderer({ chartSpec }: ChartRendererProps) {
  if (!chartSpec) return null;

  switch (chartSpec.type) {
    case "line":
      return (
        <Suspense fallback={<Shimmer className="h-64 w-full" />}>
          <TrendLine descriptor={chartSpec} />
        </Suspense>
      );
    case "gauge":
      return (
        <Suspense fallback={<Shimmer className="h-48 w-full" />}>
          <MetricGauge descriptor={chartSpec} />
        </Suspense>
      );
    case "pie":
      return (
        <Suspense fallback={<Shimmer className="h-64 w-full" />}>
          <GradePie descriptor={chartSpec} />
        </Suspense>
      );
    default:
      return (
        <div className="p-4 bg-yellow-50 text-yellow-800 rounded">
          暂不支持图表类型: {chartSpec.type}
        </div>
      );
  }
}
