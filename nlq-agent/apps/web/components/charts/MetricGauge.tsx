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

// 动态导入 @ant-design/charts Gauge 组件（Canvas 图表不支持 SSR）
const Gauge = dynamic(
  () => import("@ant-design/charts").then((mod) => mod.Gauge),
  {
    ssr: false,
    loading: () => (
      <div className="w-full h-48 my-4">
        <Shimmer className="w-full h-full" />
      </div>
    ),
  },
);

interface MetricGaugeProps {
  descriptor: ChartDescriptor;
}

export function MetricGauge({ descriptor }: MetricGaugeProps) {
  const { data, meta } = descriptor;

  // 获取当前值（取第一个数据点的值）
  const currentValue = data[0]?.value ?? 0;
  const metricName = meta?.metricName ?? "指标";
  const unit = meta?.unit ?? "";

  // 获取等级阈值配置
  const gradeThresholds = meta?.gradeThresholds ?? [];

  // 构建区间颜色配置
  const getIntervalConfig = () => {
    if (gradeThresholds.length === 0) {
      return {
        ranges: [
          { ticks: [0, 0.6], color: "#EF4444" },
          { ticks: [0.6, 0.8], color: "#F59E0B" },
          { ticks: [0.8, 1], color: "#10B981" },
        ],
      };
    }

    // 按阈值排序
    const sorted = [...gradeThresholds].sort(
      (a, b) => a.threshold - b.threshold,
    );
    const maxValue = Math.max(
      ...sorted.map((t) => t.threshold),
      currentValue * 1.2,
    );

    const ranges = sorted.map((t, index) => {
      const prevThreshold = index === 0 ? 0 : sorted[index - 1].threshold;
      return {
        ticks: [prevThreshold / maxValue, t.threshold / maxValue],
        color: t.color,
      };
    });

    return { ranges, maxValue };
  };

  const { ranges, maxValue = currentValue * 1.2 } = getIntervalConfig();

  const config = {
    percent: Math.min(currentValue / maxValue, 1),
    range: {
      ticks: [0, 1],
      color: ["l(0) 0:#10B981 0.5:#F59E0B 1:#EF4444"],
    },
    indicator: {
      pointer: {
        style: {
          stroke: "#D0D0D0",
        },
      },
      pin: {
        style: {
          stroke: "#D0D0D0",
        },
      },
    },
    axis: {
      label: {
        formatter: (v: number) => `${(v * maxValue).toFixed(1)}`,
      },
      subTickLine: {
        count: 3,
      },
    },
    statistic: {
      content: {
        formatter: () => `${currentValue.toFixed(3)}`,
        style: {
          fontSize: "24px",
          fontWeight: "bold",
          color: "#262626",
        },
      },
      title: {
        formatter: () => `${metricName}${unit ? ` (${unit})` : ""}`,
        style: {
          fontSize: "14px",
          color: "#8c8c8c",
        },
      },
    },
    annotations: [
      // 添加等级阈值标注线
      ...gradeThresholds.map((t) => ({
        type: "line" as const,
        start: [t.threshold / maxValue, 0],
        end: [t.threshold / maxValue, 1],
        style: {
          stroke: t.color,
          lineWidth: 2,
          lineDash: [4, 4],
        },
      })),
    ],
  };

  return (
    <div className="w-full h-48 my-2">
      <Gauge {...config} />
    </div>
  );
}

export default MetricGauge;
