"use client";

import dynamic from "next/dynamic";
import type { ChartDescriptor } from "@nlq-agent/shared-types";

// 本地 Shimmer 组件
function Shimmer({ className }: { className?: string }) {
  return (
    <div
      className={`animate-pulse bg-gray-200 dark:bg-gray-700 rounded ${className || ""}`}
      style={{ minHeight: "200px" }}
    />
  );
}

// 动态导入 @ant-design/charts Line 组件（Canvas 图表不支持 SSR）
const Line = dynamic(
  () => import("@ant-design/charts").then((mod) => mod.Line),
  {
    ssr: false,
    loading: () => (
      <div className="w-full h-64 my-4">
        <Shimmer className="w-full h-full" />
      </div>
    ),
  },
);

interface TrendLineProps {
  descriptor: ChartDescriptor;
}

export function TrendLine({ descriptor }: TrendLineProps) {
  const config = {
    data: descriptor.data,
    xField: descriptor.xField ?? "date",
    yField: descriptor.yField ?? "value",
    smooth: true,
    point: {
      size: 3,
      shape: "circle",
    },
    lineStyle: {
      lineWidth: 2,
    },
    meta: {
      [descriptor.xField ?? "date"]: {
        alias: "日期",
      },
      [descriptor.yField ?? "value"]: {
        alias: descriptor.meta?.metricName ?? "数值",
      },
    },
    // 等级阈值参考线
    annotations:
      descriptor.meta?.gradeThresholds?.map((t) => ({
        type: "line" as const,
        start: ["min", t.threshold],
        end: ["max", t.threshold],
        style: {
          stroke: t.color,
          lineDash: [4, 4],
          lineWidth: 1,
        },
        text: {
          content: t.name,
          position: "right",
          style: {
            fill: t.color,
            fontSize: 12,
          },
        },
      })) ?? [],
    tooltip: {
      title: descriptor.meta?.metricName ?? "指标值",
      formatter: (datum: Record<string, unknown>) => ({
        name: String(datum[descriptor.xField ?? "date"]),
        value: `${Number(datum[descriptor.yField ?? "value"]).toFixed(3)} ${descriptor.meta?.unit ?? ""}`,
      }),
    },
  };

  return (
    <div className="w-full h-64 my-4">
      <Line {...config} />
    </div>
  );
}

export default TrendLine;
