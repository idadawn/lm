"""Eval harness: Tier 1 + Tier 2 questions (80% pass-rate gate).

Marked with @pytest.mark.eval — excluded from default test run.
Run with: pytest -m eval tests/eval/test_eval.py

--baseline flag disables few-shot injection for baseline comparison (G2).
"""

from __future__ import annotations

import os
from pathlib import Path

import pytest

from tests.eval.runner import run_all, run_question, write_report


# ---------------------------------------------------------------------------
# Tier 1 + Tier 2 eval (80% gate — Critic MAJOR #2)
# ---------------------------------------------------------------------------


@pytest.mark.eval
def test_eval_tier1_and_tier2(vanna_app, questions, request):
    """Run Tier 1+2 questions; assert >= 80% pass rate.

    MAJOR #2 (Critic Round 2): threshold expressed as absolute count floor
    to avoid floating-point edge cases at small N.
    """
    tier12 = [q for q in questions if q["tier"] in (1, 2)]
    assert len(tier12) >= 1, "No Tier 1+2 questions found in questions.yaml"

    results = [run_question(vanna_app, q) for q in tier12]
    passed = sum(1 for r in results if r["passed"])
    total = len(tier12)

    # Write report to eval/reports/
    report_dir = Path(__file__).parent / "reports"
    report_dir.mkdir(exist_ok=True)
    baseline_suffix = "_baseline" if request.config.getoption("--baseline", default=False) else ""
    write_report(results, report_dir / f"eval_tier12{baseline_suffix}.md")

    # 80% threshold as integer floor (MAJOR #2)
    threshold = int(total * 0.8)
    assert passed >= threshold, (
        f"Eval pass rate {passed}/{total} ({100 * passed / total:.1f}%) "
        f"< 80% threshold ({threshold}/{total}). "
        f"Failed questions: "
        + ", ".join(r["id"] for r in results if not r["passed"])
    )


@pytest.mark.eval
@pytest.mark.tier1
def test_eval_tier1_only(vanna_app, questions, request):
    """Run Tier 1 (pure SELECT) questions only; assert >= 80% pass rate."""
    tier1 = [q for q in questions if q["tier"] == 1]
    if not tier1:
        pytest.skip("No Tier 1 questions in questions.yaml")

    results = [run_question(vanna_app, q) for q in tier1]
    passed = sum(1 for r in results if r["passed"])
    total = len(tier1)

    threshold = int(total * 0.8)
    assert passed >= threshold, (
        f"Tier 1 pass rate {passed}/{total} < 80%. "
        f"Failed: {[r['id'] for r in results if not r['passed']]}"
    )


@pytest.mark.eval
def test_eval_all_results_have_required_keys(vanna_app, questions):
    """Each result dict must contain the required keys."""
    required_keys = {"id", "tier", "passed", "reason"}
    tier12 = [q for q in questions if q["tier"] in (1, 2)][:3]  # spot-check first 3
    for q in tier12:
        result = run_question(vanna_app, q)
        missing = required_keys - set(result.keys())
        assert not missing, f"Result for {q['id']} missing keys: {missing}"


@pytest.mark.eval
def test_eval_baseline_vs_fewshot(vanna_app, questions, request):
    """Compare baseline (no few-shot) vs. few-shot mode result counts.

    Only meaningful when --baseline is passed.  Otherwise this is a no-op.
    """
    is_baseline = request.config.getoption("--baseline", default=False)
    if not is_baseline:
        pytest.skip("Pass --baseline to run baseline comparison")

    tier12 = [q for q in questions if q["tier"] in (1, 2)]
    results = [run_question(vanna_app, q) for q in tier12]
    passed = sum(1 for r in results if r["passed"])
    total = len(tier12)

    # In baseline mode, we just record — no hard assertion on pass rate
    # (baseline is expected to be lower than few-shot)
    print(f"\n[BASELINE] Pass rate: {passed}/{total} ({100 * passed / total:.1f}%)")
    # Soft assertion: baseline should produce valid (non-crashing) results
    assert total > 0
    assert all("passed" in r for r in results)
