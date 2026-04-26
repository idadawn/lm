// upstream-sha: 69b2fa54a97cb5d92afb3ae83b2350ec448da502e48f2f96a4bdc8c2e0e6bd8d
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
