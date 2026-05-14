// upstream-sha: 49d11d2205d55a4b97adffa7d316bd81b8bec04933302965ede39fbbbbfc55f0
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
