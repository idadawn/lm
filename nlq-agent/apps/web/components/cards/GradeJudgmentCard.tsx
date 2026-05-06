"use client";

import type {
  GradeJudgment,
  GradeJudgmentDetail,
} from "@nlq-agent/shared-types";

interface GradeJudgmentCardProps {
  judgment: GradeJudgment;
}

/**
 * 等级判定明细卡片组件
 *
 * 显示指标值的等级判定结果，包括等级、品质状态、判定说明等
 */
export function GradeJudgmentCard({ judgment }: GradeJudgmentCardProps) {
  const {
    available,
    grade,
    quality_status,
    color,
    metric_value,
    matched_rule,
    all_rules,
    summary,
  } = judgment;

  // 如果没有等级判定数据，不显示卡片
  if (!available) {
    return null;
  }

  // 根据颜色生成对应的样式
  const getColorClasses = (colorCode?: string | null) => {
    const colorMap: Record<string, string> = {
      "#10B981":
        "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/40 dark:text-emerald-200 border-emerald-200 dark:border-emerald-800",
      "#22C55E":
        "bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-200 border-green-200 dark:border-green-800",
      "#3B82F6":
        "bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-200 border-blue-200 dark:border-blue-800",
      "#F59E0B":
        "bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-200 border-amber-200 dark:border-amber-800",
      "#EF4444":
        "bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-200 border-red-200 dark:border-red-800",
      "#6B7280":
        "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200 border-gray-200 dark:border-gray-700",
    };
    return colorMap[colorCode || ""] || colorMap["#6B7280"];
  };

  // 获取品质状态的中文显示
  const getQualityStatusLabel = (status?: string | null) => {
    const statusMap: Record<string, string> = {
      excellent: "优秀",
      good: "良好",
      qualified: "合格",
      unqualified: "不合格",
      normal: "正常",
      abnormal: "异常",
    };
    return statusMap[status || ""] || status || "未知";
  };

  return (
    <div className="mt-4 bg-gradient-to-br from-purple-50 to-pink-50 dark:from-purple-950/30 dark:to-pink-950/30 border border-purple-200 dark:border-purple-800 rounded-lg overflow-hidden">
      {/* 头部 */}
      <div className="px-4 py-3 bg-purple-100/50 dark:bg-purple-900/20 border-b border-purple-200 dark:border-purple-800">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <svg
              className="w-4 h-4 text-purple-600 dark:text-purple-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z"
              />
            </svg>
            <h4 className="text-sm font-semibold text-purple-900 dark:text-purple-100">
              等级判定明细
            </h4>
          </div>
          {grade && (
            <span
              className={`px-2 py-0.5 text-xs font-bold rounded-full border ${getColorClasses(color)}`}
            >
              {grade}
            </span>
          )}
        </div>
      </div>

      {/* 内容 */}
      <div className="p-4 space-y-4">
        {/* 主要判定结果 */}
        <div className="flex items-center gap-4">
          {metric_value !== undefined && metric_value !== null && (
            <div className="flex-1">
              <span className="text-xs text-gray-500 dark:text-gray-400 block mb-1">
                指标值
              </span>
              <span className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {metric_value}
              </span>
            </div>
          )}
          {quality_status && (
            <div className="flex-1">
              <span className="text-xs text-gray-500 dark:text-gray-400 block mb-1">
                品质状态
              </span>
              <span
                className={`inline-block px-3 py-1 text-sm font-medium rounded-full border ${getColorClasses(color)}`}
              >
                {getQualityStatusLabel(quality_status)}
              </span>
            </div>
          )}
        </div>

        {/* 判定说明 */}
        {summary && (
          <div className="pt-3 border-t border-purple-200/50 dark:border-purple-800/50">
            <p className="text-sm text-gray-700 dark:text-gray-300">
              {summary}
            </p>
          </div>
        )}

        {/* 匹配的判定规则详情 */}
        {matched_rule && (
          <div className="pt-3 border-t border-purple-200/50 dark:border-purple-800/50">
            <span className="text-xs font-medium text-purple-600 dark:text-purple-400 uppercase tracking-wider block mb-2">
              匹配规则
            </span>
            <div className="bg-white/60 dark:bg-gray-800/60 rounded-lg p-3 space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600 dark:text-gray-400">
                  规则名称
                </span>
                <span className="text-sm font-medium text-gray-900 dark:text-gray-100">
                  {matched_rule.name || "-"}
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600 dark:text-gray-400">
                  优先级
                </span>
                <span className="text-sm font-medium text-gray-900 dark:text-gray-100">
                  {matched_rule.priority ?? "-"}
                </span>
              </div>
              {matched_rule.conditions &&
                matched_rule.conditions.length > 0 && (
                  <div className="pt-2 border-t border-gray-200 dark:border-gray-700">
                    <span className="text-xs text-gray-500 dark:text-gray-400 block mb-1">
                      判定条件
                    </span>
                    <div className="space-y-1">
                      {matched_rule.conditions.map((condition, idx) => (
                        <div
                          key={idx}
                          className="text-xs text-gray-600 dark:text-gray-400 font-mono bg-gray-100 dark:bg-gray-700/50 px-2 py-1 rounded"
                        >
                          {JSON.stringify(condition)}
                        </div>
                      ))}
                    </div>
                  </div>
                )}
            </div>
          </div>
        )}

        {/* 所有可用规则（折叠显示） */}
        {all_rules && all_rules.length > 0 && (
          <details className="group">
            <summary className="flex items-center gap-2 cursor-pointer text-xs text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 transition-colors">
              <svg
                className="w-3 h-3 transition-transform group-open:rotate-90"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9 5l7 7-7 7"
                />
              </svg>
              查看所有判定规则 ({all_rules.length}条)
            </summary>
            <div className="mt-2 space-y-1 max-h-32 overflow-y-auto">
              {all_rules.map((rule, idx) => (
                <div
                  key={rule.id || idx}
                  className="flex items-center justify-between py-1 px-2 text-xs bg-white/40 dark:bg-gray-800/40 rounded"
                >
                  <span className="text-gray-700 dark:text-gray-300">
                    {rule.name || `规则 ${idx + 1}`}
                  </span>
                  <span
                    className={`px-1.5 py-0.5 rounded text-[10px] ${getColorClasses(rule.color)}`}
                  >
                    {rule.quality_status
                      ? getQualityStatusLabel(rule.quality_status)
                      : "-"}
                  </span>
                </div>
              ))}
            </div>
          </details>
        )}
      </div>
    </div>
  );
}
