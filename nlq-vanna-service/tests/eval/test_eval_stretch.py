"""Eval harness: Tier 3 stretch tests (fallback-only, not counted toward 80% gate).

Marked with @pytest.mark.eval_stretch — excluded from default and @pytest.mark.eval runs.
Run with: pytest -m eval_stretch tests/eval/test_eval_stretch.py

Tier 3 acceptance criterion: at least one kind=fallback step must appear.
These questions are beyond the current NLQ capability boundary — the correct
behaviour is a graceful fallback, not a SQL result.
"""

from __future__ import annotations

from pathlib import Path
from unittest.mock import MagicMock

import pytest

from tests.eval.runner import run_question, write_report


# ---------------------------------------------------------------------------
# Tier 3 stretch: each question must produce at least one fallback step
# ---------------------------------------------------------------------------


@pytest.mark.eval_stretch
def test_eval_tier3_fallback_appears(vanna_app, questions):
    """Every Tier 3 question must trigger at least one kind=fallback step.

    Tier 3 questions have reference_sql=None.  The runner.run_question()
    validates this path by checking generate_sql raises or returns empty/fallback.
    """
    tier3 = [q for q in questions if q["tier"] == 3]
    if not tier3:
        pytest.skip("No Tier 3 questions in questions.yaml")

    results = [run_question(vanna_app, q) for q in tier3]

    report_dir = Path(__file__).parent / "reports"
    report_dir.mkdir(exist_ok=True)
    write_report(results, report_dir / "eval_tier3_stretch.md")

    for result in results:
        assert result["tier"] == 3
        # Tier 3 "passed" means a fallback was correctly triggered
        assert result["passed"], (
            f"Tier 3 question {result['id']} did not trigger expected fallback. "
            f"Reason: {result['reason']}"
        )


@pytest.mark.eval_stretch
def test_eval_tier3_not_counted_in_80pct_gate(questions):
    """Verify Tier 3 questions are excluded from the 80% gate calculation.

    This is a structural assertion — Tier 3 must have reference_sql=None
    so the standard DataFrame-comparison path is bypassed.
    """
    tier3 = [q for q in questions if q["tier"] == 3]
    if not tier3:
        pytest.skip("No Tier 3 questions in questions.yaml")

    for q in tier3:
        assert q.get("reference_sql") is None, (
            f"Tier 3 question {q['id']} has reference_sql={q.get('reference_sql')!r}. "
            "Tier 3 questions must have reference_sql: null to indicate fallback-only."
        )


@pytest.mark.eval_stretch
def test_eval_tier3_graceful_no_crash(vanna_app, questions):
    """Tier 3 questions must never crash the service — graceful degradation only."""
    tier3 = [q for q in questions if q["tier"] == 3]
    if not tier3:
        pytest.skip("No Tier 3 questions in questions.yaml")

    for q in tier3:
        try:
            result = run_question(vanna_app, q)
            # run_question should return a valid dict, not raise
            assert isinstance(result, dict)
            assert "passed" in result
            assert "reason" in result
        except Exception as exc:
            pytest.fail(
                f"run_question raised for Tier 3 question {q['id']}: {exc}. "
                "Tier 3 must degrade gracefully, never raise."
            )
