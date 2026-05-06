"use client";

import type { CalculationExplanation } from "@nlq-agent/shared-types";

interface CalculationExplanationCardProps {
  explanation: CalculationExplanation;
}

/**
 * 计算方式说明卡片组件
 *
 * 显示指标计算的公式来源、数据字段和自然语言描述
 */
export function CalculationExplanationCard({
  explanation,
}: CalculationExplanationCardProps) {
  const { formula_source, data_fields, natural_language } = explanation;

  return (
    <div className="mt-4 bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-blue-950/30 dark:to-indigo-950/30 border border-blue-200 dark:border-blue-800 rounded-lg overflow-hidden">
      {/* 头部 */}
      <div className="px-4 py-3 bg-blue-100/50 dark:bg-blue-900/20 border-b border-blue-200 dark:border-blue-800">
        <div className="flex items-center gap-2">
          <svg
            className="w-4 h-4 text-blue-600 dark:text-blue-400"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z"
            />
          </svg>
          <h4 className="text-sm font-semibold text-blue-900 dark:text-blue-100">
            计算方式说明
          </h4>
        </div>
      </div>

      {/* 内容 */}
      <div className="p-4 space-y-3">
        {/* 公式来源 */}
        <div className="flex items-start gap-3">
          <span className="text-xs font-medium text-blue-600 dark:text-blue-400 uppercase tracking-wider min-w-[4rem]">
            公式来源
          </span>
          <code className="text-sm text-gray-800 dark:text-gray-200 bg-white/60 dark:bg-gray-800/60 px-2 py-0.5 rounded font-mono">
            {formula_source}
          </code>
        </div>

        {/* 数据字段 */}
        <div className="flex items-start gap-3">
          <span className="text-xs font-medium text-blue-600 dark:text-blue-400 uppercase tracking-wider min-w-[4rem]">
            数据字段
          </span>
          <div className="flex flex-wrap gap-1">
            {data_fields.map((field, index) => (
              <span
                key={index}
                className="text-xs px-2 py-0.5 bg-blue-100 dark:bg-blue-900/40 text-blue-800 dark:text-blue-200 rounded-full"
              >
                {field}
              </span>
            ))}
          </div>
        </div>

        {/* 自然语言描述 */}
        <div className="pt-2 border-t border-blue-200/50 dark:border-blue-800/50">
          <p className="text-sm text-gray-700 dark:text-gray-300 leading-relaxed">
            {natural_language}
          </p>
        </div>
      </div>
    </div>
  );
}
