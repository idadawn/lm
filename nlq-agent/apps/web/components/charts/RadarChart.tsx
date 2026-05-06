"use client";

import dynamic from "next/dynamic";
import type { ChartDescriptor } from "@nlq-agent/shared-types";

function Shimmer({ className }: { className?: string }) {
  return (
    <div
      className={`animate-pulse rounded bg-gray-200 dark:bg-gray-700 ${className || ""}`}
      style={{ minHeight: "200px" }}
    />
  );
}

const Radar = dynamic(
  () => import("@ant-design/charts").then((mod) => mod.Radar),
  {
    ssr: false,
    loading: () => (
      <div className="my-4 h-80 w-full">
        <Shimmer className="h-full w-full" />
      </div>
    ),
  },
);

interface RadarChartProps {
  descriptor: ChartDescriptor;
}

export function RadarChart({ descriptor }: RadarChartProps) {
  const xField = descriptor.xField ?? "category";
  const yField = descriptor.yField ?? "value";
  const colorField = descriptor.colorField ?? "series";

  return (
    <div className="my-4 h-80 w-full">
      <Radar
        data={descriptor.data}
        xField={xField}
        yField={yField}
        seriesField={colorField}
        area={{
          style: {
            fillOpacity: 0.14,
          },
        }}
        point={{
          size: 3,
        }}
        meta={{
          [xField]: { alias: "维度" },
          [yField]: {
            alias: descriptor.meta?.metricName ?? "数值",
          },
        }}
      />
    </div>
  );
}

export default RadarChart;
