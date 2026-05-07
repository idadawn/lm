// upstream-sha: 50a305743d29f7d774f9b4c0a3b3faf91396bf25debbf28890adf5df2d9c7913
//
// 自动同步自 nlq-agent/packages/shared-types/src/reasoning-protocol.ts。
// 修改请同步上游并通过 `pwsh scripts/check-reasoning-protocol-sync.ps1` 验证。

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
