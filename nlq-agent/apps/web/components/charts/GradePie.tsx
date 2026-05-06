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

// 动态导入 @ant-design/charts Pie 组件（Canvas 图表不支持 SSR）
const Pie = dynamic(() => import("@ant-design/charts").then((mod) => mod.Pie), {
  ssr: false,
  loading: () => (
    <div className="w-full h-64 my-4">
      <Shimmer className="w-full h-full" />
    </div>
  ),
});

interface GradePieProps {
  descriptor: ChartDescriptor;
}

export function GradePie({ descriptor }: GradePieProps) {
  const { data, meta } = descriptor;
  const metricName = meta?.metricName ?? "等级分布";

  // 等级颜色映射
  const gradeColorMap: Record<string, string> = {
    A: "#10B981", // 绿色
    B: "#3B82F6", // 蓝色
    C: "#F59E0B", // 橙色
    D: "#EF4444", // 红色
    合格: "#10B981",
    不合格: "#EF4444",
    优秀: "#22C55E",
    良好: "#3B82F6",
    正常: "#6B7280",
    异常: "#DC2626",
  };

  // 处理数据，确保有颜色字段
  const processedData = data.map((item) => ({
    ...item,
    color: gradeColorMap[String(item.category)] || item.color || "#6B7280",
  }));

  const config = {
    data: processedData,
    angleField: "value",
    colorField: "category",
    radius: 0.8,
    innerRadius: 0.5, // 环形图
    label: {
      type: "outer",
      content: "{name}: {percentage}",
    },
    color: ({ category }: { category: string }) => {
      return gradeColorMap[category] || "#6B7280";
    },
    legend: {
      position: "bottom" as const,
    },
    tooltip: {
      title: metricName,
      formatter: (datum: Record<string, unknown>) => ({
        name: String(datum.category),
        value: `${Number(datum.value).toFixed(0)} (${((Number(datum.value) / processedData.reduce((sum, d) => sum + Number(d.value), 0)) * 100).toFixed(1)}%)`,
      }),
    },
    statistic: {
      title: {
        formatter: () => metricName,
        style: {
          fontSize: "14px",
          color: "#8c8c8c",
        },
      },
      content: {
        formatter: (
          _: unknown,
          data: Array<{ value?: number }> | undefined,
        ) => {
          if (!data) return "0";
          const total = data.reduce(
            (sum: number, d: { value?: number }) => sum + (d.value || 0),
            0,
          );
          return `${total.toFixed(0)}`;
        },
        style: {
          fontSize: "20px",
          fontWeight: "bold",
          color: "#262626",
        },
      },
    },
  };

  return (
    <div className="w-full h-64 my-4">
      <Pie {...config} />
    </div>
  );
}

export default GradePie;
