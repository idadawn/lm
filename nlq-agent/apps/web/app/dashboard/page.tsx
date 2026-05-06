"use client";

import { useMemo, useState } from "react";
import {
  GradePie,
  HorizontalBar,
  RadarChart,
  ScatterPlot,
  TrendLine,
} from "@/components/charts";
import type { MonthlyDashboardPayload } from "@nlq-agent/shared-types";

const DEFAULT_START = "2026-04-01";
const DEFAULT_END = "2026-04-26";

const dashboardPayload: MonthlyDashboardPayload = {
  title: "生产驾驶舱",
  subtitle: "月度质量概览",
  dateRange: {
    start: DEFAULT_START,
    end: DEFAULT_END,
  },
  hint:
    "当前日期范围内展示的是月度样例看板结构，目的是和主项目 /lab/monthly-dashboard 保持信息布局一致。",
  kpiCards: [
    {
      title: "检验总重",
      value: "12,486.50",
      unit: "kg",
      note: "月累计",
      accent: "from-indigo-500 to-violet-500",
      icon: "⬢",
    },
    {
      title: "今日产量",
      value: "468.20",
      unit: "kg",
      note: "较昨日 +6.8%",
      accent: "from-pink-500 to-rose-500",
      icon: "↗",
    },
    {
      title: "一次交检",
      value: "93.60",
      unit: "%",
      note: "本月稳定区间",
      accent: "from-emerald-500 to-teal-500",
      icon: "◉",
    },
    {
      title: "不合格占比",
      value: "6.40",
      unit: "%",
      note: "重点关注贴标波动",
      accent: "from-amber-500 to-orange-500",
      icon: "!",
    },
  ],
  laminationTrend: {
    type: "line",
    title: "叠片系数趋势",
    data: [
      { date: "04-01", value: 95.1 },
      { date: "04-05", value: 95.6 },
      { date: "04-09", value: 94.8 },
      { date: "04-13", value: 96.3 },
      { date: "04-17", value: 95.9 },
      { date: "04-21", value: 96.8 },
      { date: "04-26", value: 96.1 },
    ],
    xField: "date",
    yField: "value",
    meta: {
      metricName: "叠片系数",
      unit: "%",
      gradeThresholds: [
        { name: "目标线", threshold: 96, color: "#22c55e" },
        { name: "警戒线", threshold: 94.5, color: "#ef4444" },
      ],
    },
  },
  qualityDistribution: {
    type: "pie",
    title: "质量等级分布",
    data: [
      { category: "特优", value: 32 },
      { category: "优", value: 41 },
      { category: "良", value: 18 },
      { category: "待复判", value: 9 },
    ],
    meta: {
      metricName: "质量等级分布",
      unit: "批",
    },
  },
  shiftComparison: {
    type: "radar",
    title: "班次对比",
    data: [
      { category: "合格率", value: 95, series: "甲班" },
      { category: "交检效率", value: 88, series: "甲班" },
      { category: "贴标稳定", value: 91, series: "甲班" },
      { category: "厚差控制", value: 86, series: "甲班" },
      { category: "叠片表现", value: 93, series: "甲班" },
      { category: "合格率", value: 92, series: "乙班" },
      { category: "交检效率", value: 84, series: "乙班" },
      { category: "贴标稳定", value: 89, series: "乙班" },
      { category: "厚差控制", value: 83, series: "乙班" },
      { category: "叠片表现", value: 90, series: "乙班" },
      { category: "合格率", value: 90, series: "丙班" },
      { category: "交检效率", value: 80, series: "丙班" },
      { category: "贴标稳定", value: 85, series: "丙班" },
      { category: "厚差控制", value: 81, series: "丙班" },
      { category: "叠片表现", value: 87, series: "丙班" },
    ],
    xField: "category",
    yField: "value",
    colorField: "series",
    meta: {
      metricName: "班次综合评分",
      unit: "%",
    },
  },
  qualityTrend: {
    type: "line",
    title: "质量趋势",
    data: [
      { date: "04-01", value: 92.1 },
      { date: "04-05", value: 93.4 },
      { date: "04-09", value: 91.8 },
      { date: "04-13", value: 94.2 },
      { date: "04-17", value: 95.1 },
      { date: "04-21", value: 94.6 },
      { date: "04-26", value: 93.8 },
    ],
    xField: "date",
    yField: "value",
    meta: {
      metricName: "一次交检合格率",
      unit: "%",
      gradeThresholds: [{ name: "目标线", threshold: 95, color: "#3b82f6" }],
    },
  },
  thicknessCorrelation: {
    type: "scatter",
    title: "厚度相关性",
    data: [
      { x: 0.23, y: 95.1, series: "甲班" },
      { x: 0.26, y: 95.8, series: "甲班" },
      { x: 0.28, y: 94.9, series: "乙班" },
      { x: 0.31, y: 93.5, series: "乙班" },
      { x: 0.33, y: 96.4, series: "丙班" },
      { x: 0.35, y: 92.8, series: "丙班" },
      { x: 0.29, y: 95.9, series: "甲班" },
      { x: 0.32, y: 94.3, series: "乙班" },
    ],
    xField: "x",
    yField: "y",
    colorField: "series",
    meta: {
      metricName: "叠片系数",
      unit: "%",
    },
  },
  unqualifiedTop5: {
    type: "bar",
    title: "不合格 Top5",
    data: [
      { category: "贴标偏差", value: 38.5 },
      { category: "厚差异常", value: 27.2 },
      { category: "毛刺超限", value: 19.6 },
      { category: "边浪", value: 12.4 },
      { category: "表面压痕", value: 9.1 },
    ],
    xField: "value",
    yField: "category",
    meta: {
      metricName: "不合格占比",
      unit: "%",
    },
  },
  shiftSummaries: [
    { label: "甲班", score: "93", note: "效率与稳定性最佳" },
    { label: "乙班", score: "88", note: "厚差波动需压缩" },
    { label: "丙班", score: "85", note: "复判与贴标需跟进" },
  ],
  detailRows: [
    {
      date: "04-22",
      shift: "甲班",
      totalWeight: "462.00",
      qualifiedRate: "95.20%",
      focus: "贴标稳定",
    },
    {
      date: "04-23",
      shift: "乙班",
      totalWeight: "438.50",
      qualifiedRate: "93.80%",
      focus: "厚差回稳",
    },
    {
      date: "04-24",
      shift: "丙班",
      totalWeight: "447.10",
      qualifiedRate: "92.60%",
      focus: "复判偏多",
    },
    {
      date: "04-25",
      shift: "甲班",
      totalWeight: "471.90",
      qualifiedRate: "96.10%",
      focus: "叠片表现优",
    },
  ],
};

function cn(...values: Array<string | false | null | undefined>) {
  return values.filter(Boolean).join(" ");
}

function ChartShell({
  title,
  caption,
  children,
}: {
  title: string;
  caption?: string;
  children: React.ReactNode;
}) {
  return (
    <section className="rounded-3xl border border-slate-200 bg-white p-5 shadow-[0_12px_40px_rgba(15,23,42,0.06)]">
      <div className="mb-3 flex items-start justify-between gap-4">
        <div>
          <h3 className="text-base font-semibold text-slate-900">{title}</h3>
          {caption ? (
            <p className="mt-1 text-xs text-slate-500">{caption}</p>
          ) : null}
        </div>
      </div>
      {children}
    </section>
  );
}

export default function DashboardPage() {
  const [dateStart, setDateStart] = useState(DEFAULT_START);
  const [dateEnd, setDateEnd] = useState(DEFAULT_END);
  const [isRefreshing, setIsRefreshing] = useState(false);

  const summaryText = useMemo(
    () => `${dateStart} 至 ${dateEnd} · 月度质量概览`,
    [dateEnd, dateStart],
  );

  const handleRefresh = async () => {
    setIsRefreshing(true);
    await new Promise((resolve) => setTimeout(resolve, 600));
    setIsRefreshing(false);
  };

  return (
    <div className="min-h-screen bg-[#f7f9fc] text-slate-900">
      <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
        <header className="mb-6 rounded-3xl border border-white/70 bg-white px-6 py-5 shadow-[0_10px_40px_rgba(15,23,42,0.05)]">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <p className="text-sm font-medium tracking-[0.24em] text-sky-600">
                MONTHLY DASHBOARD
              </p>
              <h1 className="mt-1 text-2xl font-bold text-slate-900">
                {dashboardPayload.title}
              </h1>
              <p className="mt-1 text-sm text-slate-500">
                {summaryText} · {dashboardPayload.subtitle}
              </p>
            </div>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <input
                type="date"
                value={dateStart}
                onChange={(e) => setDateStart(e.target.value)}
                className="rounded-xl border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-700 outline-none focus:border-sky-400"
              />
              <input
                type="date"
                value={dateEnd}
                onChange={(e) => setDateEnd(e.target.value)}
                className="rounded-xl border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-700 outline-none focus:border-sky-400"
              />
              <button
                onClick={() => void handleRefresh()}
                disabled={isRefreshing}
                className="inline-flex items-center justify-center rounded-xl bg-sky-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-sky-700 disabled:cursor-not-allowed disabled:bg-sky-400"
              >
                {isRefreshing ? "刷新中..." : "刷新"}
              </button>
            </div>
          </div>
        </header>

        <section className="mb-6">
          <div className="flex flex-wrap gap-4">
            {dashboardPayload.kpiCards.map((card) => (
              <article
                key={card.title}
                className="min-w-[210px] flex-1 rounded-3xl border border-slate-200 bg-white p-5 shadow-[0_10px_30px_rgba(15,23,42,0.05)] transition hover:-translate-y-0.5 hover:shadow-[0_16px_36px_rgba(15,23,42,0.08)]"
              >
                <div className="flex items-center gap-4">
                  <div
                    className={cn(
                      "flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-br text-xl font-bold text-white shadow-lg",
                      card.accent,
                    )}
                  >
                    {card.icon}
                  </div>
                  <div className="min-w-0">
                    <p className="text-sm text-slate-500">{card.title}</p>
                    <div className="mt-1 flex items-baseline gap-1">
                      <span className="text-2xl font-bold text-slate-900">
                        {card.value}
                      </span>
                      <span className="text-sm text-slate-400">{card.unit}</span>
                    </div>
                    {card.note ? (
                      <p className="mt-1 text-xs text-slate-500">{card.note}</p>
                    ) : null}
                  </div>
                </div>
              </article>
            ))}
          </div>
        </section>

        <div className="mb-6 rounded-2xl border border-sky-100 bg-sky-50/80 px-4 py-3 text-sm text-sky-800">
          {dashboardPayload.hint}
        </div>

        <section className="mb-6 grid gap-6 lg:grid-cols-2">
          <ChartShell
            title="叠片系数趋势"
            caption="对应主项目左侧主趋势图区域"
          >
            <TrendLine descriptor={dashboardPayload.laminationTrend} />
          </ChartShell>
          <ChartShell
            title="质量等级分布"
            caption="对应主项目右侧分布饼图区域"
          >
            <GradePie descriptor={dashboardPayload.qualityDistribution} />
          </ChartShell>
        </section>

        <section className="mb-6 grid gap-6 lg:grid-cols-[1fr_2fr]">
          <ChartShell title="班次对比" caption="甲乙丙班综合表现">
            <RadarChart descriptor={dashboardPayload.shiftComparison} />
          </ChartShell>
          <ChartShell title="质量趋势" caption="按日期观察一次交检变化">
            <TrendLine descriptor={dashboardPayload.qualityTrend} />
          </ChartShell>
        </section>

        <section className="mb-6 grid gap-6 lg:grid-cols-[2fr_1fr_1fr]">
          <ChartShell title="厚度相关性散点" caption="厚差与叠片系数关系">
            <ScatterPlot descriptor={dashboardPayload.thicknessCorrelation} />
          </ChartShell>
          <ChartShell title="不合格 Top5" caption="重点关注缺陷分布">
            <HorizontalBar descriptor={dashboardPayload.unqualifiedTop5} />
          </ChartShell>
          <ChartShell title="班次摘要" caption="对应主项目右侧分析位">
            <div className="space-y-4 py-3">
              {dashboardPayload.shiftSummaries.map((item) => (
                <div key={item.label} className="rounded-2xl bg-slate-50 p-4">
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-slate-700">
                      {item.label}
                    </span>
                    <span className="text-lg font-bold text-slate-900">
                      {item.score}
                    </span>
                  </div>
                  <p className="mt-2 text-xs leading-5 text-slate-500">
                    {item.note}
                  </p>
                </div>
              ))}
            </div>
          </ChartShell>
        </section>

        <section className="rounded-3xl border border-slate-200 bg-white p-5 shadow-[0_12px_40px_rgba(15,23,42,0.06)]">
          <div className="mb-4 flex items-center justify-between">
            <div>
              <h3 className="text-base font-semibold text-slate-900">
                月度明细摘要
              </h3>
              <p className="mt-1 text-xs text-slate-500">
                对齐主项目底层分析节奏，便于后续接真实报表数据。
              </p>
            </div>
            <a
              href="/"
              className="rounded-xl bg-slate-900 px-4 py-2 text-sm font-medium text-white transition hover:bg-slate-800"
            >
              返回对话分析
            </a>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full border-separate border-spacing-y-2 text-sm">
              <thead>
                <tr className="text-left text-slate-500">
                  <th className="px-3 py-2 font-medium">日期</th>
                  <th className="px-3 py-2 font-medium">班次</th>
                  <th className="px-3 py-2 font-medium">检验总重</th>
                  <th className="px-3 py-2 font-medium">合格率</th>
                  <th className="px-3 py-2 font-medium">关注点</th>
                </tr>
              </thead>
              <tbody>
                {dashboardPayload.detailRows.map((row) => (
                  <tr key={`${row.date}-${row.shift}`}>
                    <td className="rounded-l-2xl bg-slate-50 px-3 py-3 text-slate-700">
                      {row.date}
                    </td>
                    <td className="bg-slate-50 px-3 py-3 text-slate-700">
                      {row.shift}
                    </td>
                    <td className="bg-slate-50 px-3 py-3 text-slate-700">
                      {row.totalWeight} kg
                    </td>
                    <td className="bg-slate-50 px-3 py-3 font-semibold text-emerald-600">
                      {row.qualifiedRate}
                    </td>
                    <td className="rounded-r-2xl bg-slate-50 px-3 py-3 text-slate-500">
                      {row.focus}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      </div>
    </div>
  );
}
