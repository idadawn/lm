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

const Bar = dynamic(() => import("@ant-design/charts").then((mod) => mod.Bar), {
  ssr: false,
  loading: () => (
    <div className="my-4 h-72 w-full">
      <Shimmer className="h-full w-full" />
    </div>
  ),
});

interface HorizontalBarProps {
  descriptor: ChartDescriptor;
}

export function HorizontalBar({ descriptor }: HorizontalBarProps) {
  const xField = descriptor.xField ?? "value";
  const yField = descriptor.yField ?? "category";

  return (
    <div className="my-4 h-72 w-full">
      <Bar
        data={descriptor.data}
        xField={xField}
        yField={yField}
        seriesField={descriptor.colorField}
        color={["#3b82f6", "#60a5fa", "#93c5fd", "#f97316", "#ef4444"]}
        legend={false}
        label={{
          position: "right",
          style: {
            fill: "#4b5563",
          },
        }}
        meta={{
          [xField]: {
            alias: descriptor.meta?.unit
              ? `${descriptor.meta.metricName} (${descriptor.meta.unit})`
              : descriptor.meta?.metricName ?? "数值",
          },
          [yField]: {
            alias: "类别",
          },
        }}
        tooltip={{
          formatter: (datum: Record<string, unknown>) => ({
            name: String(datum[yField]),
            value: `${Number(datum[xField]).toFixed(2)} ${descriptor.meta?.unit ?? ""}`,
          }),
        }}
      />
    </div>
  );
}

export default HorizontalBar;
