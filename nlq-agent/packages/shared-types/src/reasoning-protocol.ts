/**
 * Reasoning Protocol — 推理链协议类型定义（上游源文件）
 *
 * 本文件是 reasoning-protocol 的唯一真相源（Single Source of Truth）。
 * web/src/types/reasoning-protocol.d.ts 和 mobile 端类型均由此文件同步生成。
 * 修改后请运行 `pwsh scripts/check-reasoning-protocol-sync.ps1` 验证一致性。
 */

export type ReasoningStepKind =
  | "record"
  | "spec"
  | "rule"
  | "condition"
  | "grade"
  | "fallback";

export interface ReasoningStep {
  kind: ReasoningStepKind;
  label: string;
  detail?: string;
  satisfied?: boolean;
  field?: string;
  expected?: string;
  actual?: string | number;
  meta?: Record<string, unknown>;
}

export interface ReasoningChainEvent {
  steps: ReasoningStep[];
}
