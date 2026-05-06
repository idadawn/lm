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

const Scatter = dynamic(
  () => import("@ant-design/charts").then((mod) => mod.Scatter),
  {
    ssr: false,
    loading: () => (
      <div className="my-4 h-80 w-full">
        <Shimmer className="h-full w-full" />
      </div>
    ),
  },
);

interface ScatterPlotProps {
  descriptor: ChartDescriptor;
}

export function ScatterPlot({ descriptor }: ScatterPlotProps) {
  const xField = descriptor.xField ?? "x";
  const yField = descriptor.yField ?? "y";
  const colorField = descriptor.colorField ?? "series";

  return (
    <div className="my-4 h-80 w-full">
      <Scatter
        data={descriptor.data}
        xField={xField}
        yField={yField}
        colorField={colorField}
        size={4}
        shape="circle"
        legend={false}
        meta={{
          [xField]: { alias: "X" },
          [yField]: { alias: descriptor.meta?.metricName ?? "Y" },
        }}
        tooltip={{
          formatter: (datum: Record<string, unknown>) => ({
            name: String(datum[colorField] ?? descriptor.meta?.metricName ?? "数据点"),
            value: `${Number(datum[xField]).toFixed(2)}, ${Number(datum[yField]).toFixed(2)}`,
          }),
        }}
      />
    </div>
  );
}

export default ScatterPlot;
