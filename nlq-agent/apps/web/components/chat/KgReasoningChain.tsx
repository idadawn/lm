"use client";

/**
 * KG 推理链折叠面板。
 *
 * 消费 services/agent-api 通过 SSE `reasoning_step` 事件 + `response_metadata`
 * 双通道下发的 ReasoningStep 列表。组件本身是受控的纯渲染：所有事件累积
 * 由父组件（NlqChatPanel）负责。
 *
 * 暂用原生 `<details>` 实现折叠（避免对未安装的 ai-elements/Reasoning 强依赖）。
 * 后续如团队 `pnpm dlx ai-elements add reasoning` 引入官方组件，可一键替换包裹层。
 * TODO(ai-elements): swap `<details>` for `<Reasoning>` from ai-elements once installed.
 */

import type { ReasoningStep } from "@nlq-agent/shared-types";

interface KgReasoningChainProps {
  steps: ReasoningStep[];
  defaultOpen?: boolean;
}

const KIND_LABELS: Record<ReasoningStep["kind"], string> = {
  record: "命中记录",
  spec: "产品规格",
  rule: "判定规则",
  condition: "条件评估",
  grade: "最终结论",
  fallback: "降级",
};

const KIND_COLORS: Record<ReasoningStep["kind"], string> = {
  record: "bg-slate-100 text-slate-700 border-slate-200",
  spec: "bg-blue-50 text-blue-700 border-blue-200",
  rule: "bg-purple-50 text-purple-700 border-purple-200",
  condition: "bg-emerald-50 text-emerald-700 border-emerald-200",
  grade: "bg-amber-50 text-amber-800 border-amber-200",
  fallback: "bg-rose-50 text-rose-700 border-rose-200",
};

function StepRow({ step, index }: { step: ReasoningStep; index: number }) {
  const tagClass = KIND_COLORS[step.kind] ?? KIND_COLORS.fallback;
  const tagLabel = KIND_LABELS[step.kind] ?? step.kind;
  const isCondition = step.kind === "condition";

  return (
    <li className="flex gap-3 border-l-2 border-slate-200 py-2 pl-3">
      <span className="mt-1 inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-slate-100 text-xs font-medium text-slate-500">
        {index + 1}
      </span>
      <div className="flex-1 space-y-1">
        <div className="flex flex-wrap items-center gap-2">
          <span
            className={`inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-medium ${tagClass}`}
          >
            {tagLabel}
          </span>
          {isCondition && step.satisfied !== undefined && (
            <span
              className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                step.satisfied
                  ? "bg-emerald-100 text-emerald-700"
                  : "bg-rose-100 text-rose-700"
              }`}
            >
              {step.satisfied ? "满足" : "不满足"}
            </span>
          )}
        </div>
        <div className="text-sm text-slate-700">{step.label}</div>
        {isCondition && (
          <div className="grid grid-cols-2 gap-2 text-xs text-slate-500">
            {step.expected !== undefined && (
              <div>
                <span className="text-slate-400">期望：</span>
                <span className="font-mono text-slate-600">
                  {String(step.expected)}
                </span>
              </div>
            )}
            {step.actual !== undefined && (
              <div>
                <span className="text-slate-400">实际：</span>
                <span className="font-mono text-slate-600">
                  {String(step.actual)}
                </span>
              </div>
            )}
          </div>
        )}
        {step.detail && !isCondition && (
          <div className="text-xs text-slate-500">{step.detail}</div>
        )}
      </div>
    </li>
  );
}

export function KgReasoningChain({
  steps,
  defaultOpen = false,
}: KgReasoningChainProps) {
  if (!steps || steps.length === 0) {
    return null;
  }

  return (
    <details
      className="mt-3 rounded-xl border border-slate-200 bg-slate-50/50 px-4 py-2 text-sm"
      open={defaultOpen}
      data-testid="kg-reasoning-chain"
    >
      <summary className="cursor-pointer select-none py-1 text-sm font-medium text-slate-600 outline-none">
        知识图谱推理过程（{steps.length} 步）
      </summary>
      <ol className="mt-2 space-y-1">
        {steps.map((step, index) => (
          <StepRow key={index} step={step} index={index} />
        ))}
      </ol>
    </details>
  );
}

export default KgReasoningChain;
