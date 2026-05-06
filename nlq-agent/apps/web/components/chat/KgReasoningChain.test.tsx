import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import type { ReasoningStep } from "@nlq-agent/shared-types";
import { KgReasoningChain } from "./KgReasoningChain";

const SAMPLE: ReasoningStep[] = [
  { kind: "record", label: "命中检测记录" },
  { kind: "spec", label: "产品规格 120" },
  { kind: "rule", label: "C 级判定规则" },
  {
    kind: "condition",
    label: "Ps铁损 ≤ 1.30",
    field: "F_PERF_PS_LOSS",
    expected: "<= 1.30",
    actual: 1.46,
    satisfied: false,
  },
  { kind: "grade", label: "判定为 C 级" },
];

describe("KgReasoningChain", () => {
  it("renders nothing when steps is empty", () => {
    const { container } = render(<KgReasoningChain steps={[]} />);
    expect(container.firstChild).toBeNull();
  });

  it("renders nothing when steps is undefined-ish", () => {
    // @ts-expect-error -- intentional: hardening guard
    const { container } = render(<KgReasoningChain steps={undefined} />);
    expect(container.firstChild).toBeNull();
  });

  it("renders one step", () => {
    render(
      <KgReasoningChain
        steps={[{ kind: "record", label: "唯一一步" }]}
        defaultOpen
      />,
    );
    expect(screen.getByText("唯一一步")).toBeTruthy();
    expect(screen.getByText(/知识图谱推理过程/)).toBeTruthy();
  });

  it("renders multiple steps in order with kind badges", () => {
    render(<KgReasoningChain steps={SAMPLE} defaultOpen />);
    const node = screen.getByTestId("kg-reasoning-chain");
    const text = node.textContent ?? "";
    // ordering invariants: record before rule, grade last
    const idxRecord = text.indexOf("命中检测记录");
    const idxRule = text.indexOf("C 级判定规则");
    const idxGrade = text.indexOf("判定为 C 级");
    expect(idxRecord).toBeGreaterThan(-1);
    expect(idxRule).toBeGreaterThan(idxRecord);
    expect(idxGrade).toBeGreaterThan(idxRule);
  });

  it("shows satisfied/不满足 badge for condition steps", () => {
    render(
      <KgReasoningChain
        steps={[
          {
            kind: "condition",
            label: "C1",
            field: "x",
            expected: ">= 1",
            actual: 0.5,
            satisfied: false,
          },
          {
            kind: "condition",
            label: "C2",
            field: "y",
            expected: "<= 10",
            actual: 5,
            satisfied: true,
          },
        ]}
        defaultOpen
      />,
    );
    expect(screen.getByText("不满足")).toBeTruthy();
    expect(screen.getByText("满足")).toBeTruthy();
  });

  it("renders fallback kind with rose tag class", () => {
    render(
      <KgReasoningChain
        steps={[{ kind: "fallback", label: "知识图谱不可用" }]}
        defaultOpen
      />,
    );
    expect(screen.getByText("知识图谱不可用")).toBeTruthy();
    expect(screen.getByText("降级")).toBeTruthy();
  });
});
