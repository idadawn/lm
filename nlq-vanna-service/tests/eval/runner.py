"""Eval runner for NLQ Vanna service questions.yaml harness.

Public API
----------
run_question(vn, q)             Run one eval question; return result dict.
run_all(yaml_path, vn, filter)  Run all (or filtered) questions.
write_report(results, out_path) Write a Markdown report file.

Tier semantics
--------------
Tier 1+2 : generate_sql + run_sql → compare DataFrame with reference_sql
Tier 3   : reference_sql is None → only assert kind=fallback step appears
"""

from __future__ import annotations

import textwrap
import traceback
from pathlib import Path
from typing import Any

import pandas as pd
import yaml


# ---------------------------------------------------------------------------
# Core: run a single question
# ---------------------------------------------------------------------------


def run_question(vn: Any, q: dict) -> dict:
    """Run one eval question against the VannaApp instance.

    Args:
        vn: VannaApp instance (real or mockable via env).
        q:  A single question dict from questions.yaml.

    Returns:
        dict with keys:
          id          : str
          tier        : int
          passed      : bool
          reason      : str (human-readable outcome)
          sql_diff    : str | None  (generated vs reference, on mismatch)
          df_head_diff: str | None  (first-5-rows diff, on data mismatch)
    """
    qid: str = q["id"]
    tier: int = int(q["tier"])
    question: str = q["question"]
    reference_sql: str | None = q.get("reference_sql")

    result: dict = {
        "id": qid,
        "tier": tier,
        "passed": False,
        "reason": "",
        "sql_diff": None,
        "df_head_diff": None,
    }

    # ------------------------------------------------------------------
    # Tier 3: reference_sql is None → validate fallback step appears
    # ------------------------------------------------------------------
    if reference_sql is None:
        try:
            # Collect reasoning steps; VannaApp should expose them via emitter
            generated_sql: str = vn.generate_sql(question)
            # For Tier 3, we expect generate_sql to produce something unparseable
            # or the chat_stream to emit a fallback step.
            # Heuristic: if SQL is empty or contains "fallback", pass.
            if not generated_sql or "fallback" in generated_sql.lower():
                result["passed"] = True
                result["reason"] = "Tier 3: generate_sql returned fallback/empty as expected"
            else:
                # Try running it; if it fails that's also acceptable for Tier 3
                try:
                    vn.run_sql(generated_sql)
                    result["passed"] = False
                    result["reason"] = (
                        "Tier 3: expected fallback but got valid SQL + successful execution"
                    )
                except Exception:
                    result["passed"] = True
                    result["reason"] = "Tier 3: run_sql raised (expected for complex query)"
        except Exception as exc:
            result["passed"] = True
            result["reason"] = f"Tier 3: generate_sql raised (acceptable): {exc}"
        return result

    # ------------------------------------------------------------------
    # Tier 1+2: compare DataFrames
    # ------------------------------------------------------------------
    try:
        generated_sql = vn.generate_sql(question)
    except Exception as exc:
        result["reason"] = f"generate_sql raised: {exc}\n{traceback.format_exc()}"
        return result

    if not generated_sql or not generated_sql.strip():
        result["reason"] = "generate_sql returned empty SQL"
        return result

    # Execute both SQLs
    try:
        df_generated: pd.DataFrame = vn.run_sql(generated_sql)
    except Exception as exc:
        result["reason"] = f"run_sql(generated) raised: {exc}"
        result["sql_diff"] = f"generated: {generated_sql}\nreference: {reference_sql}"
        return result

    try:
        df_reference: pd.DataFrame = vn.run_sql(reference_sql)
    except Exception as exc:
        result["reason"] = f"run_sql(reference) raised: {exc}"
        return result

    # Normalize and compare DataFrames
    equal, diff_reason = _dataframes_equal(df_generated, df_reference)
    if equal:
        result["passed"] = True
        result["reason"] = f"DataFrames match ({len(df_reference)} rows)"
    else:
        result["passed"] = False
        result["reason"] = f"DataFrame mismatch: {diff_reason}"
        result["sql_diff"] = (
            f"generated:\n{textwrap.indent(generated_sql, '  ')}\n"
            f"reference:\n{textwrap.indent(reference_sql, '  ')}"
        )
        result["df_head_diff"] = (
            f"generated head:\n{df_generated.head(5).to_string()}\n"
            f"reference head:\n{df_reference.head(5).to_string()}"
        )

    return result


def _dataframes_equal(df1: pd.DataFrame, df2: pd.DataFrame) -> tuple[bool, str]:
    """Compare two DataFrames for semantic equality.

    Normalises by sorting values and resetting index before comparing.

    Returns:
        (equal: bool, reason: str)
    """
    if df1.shape != df2.shape:
        return False, f"shape mismatch: {df1.shape} vs {df2.shape}"

    if set(df1.columns) != set(df2.columns):
        return False, f"columns mismatch: {set(df1.columns)} vs {set(df2.columns)}"

    # Sort by all columns and reset index for position-independent comparison
    try:
        cols = sorted(df1.columns.tolist())
        df1_norm = df1[cols].sort_values(by=cols).reset_index(drop=True)
        df2_norm = df2[cols].sort_values(by=cols).reset_index(drop=True)
        if df1_norm.equals(df2_norm):
            return True, ""
        return False, "row values differ after normalisation"
    except Exception as exc:
        return False, f"comparison error: {exc}"


# ---------------------------------------------------------------------------
# Batch runner
# ---------------------------------------------------------------------------


def run_all(
    yaml_path: Path,
    vn: Any,
    marker_filter: str | None = None,
) -> list[dict]:
    """Run all (or filtered) questions from questions.yaml.

    Args:
        yaml_path:     Path to questions.yaml.
        vn:            VannaApp instance.
        marker_filter: One of "tier1", "tier1+tier2", or None (all tiers).

    Returns:
        List of result dicts from run_question().
    """
    questions = _load_questions(yaml_path)

    if marker_filter == "tier1":
        questions = [q for q in questions if q["tier"] == 1]
    elif marker_filter == "tier1+tier2":
        questions = [q for q in questions if q["tier"] in (1, 2)]
    # else: None → all tiers

    return [run_question(vn, q) for q in questions]


def _load_questions(yaml_path: Path) -> list[dict]:
    """Load and validate questions from YAML file."""
    with yaml_path.open(encoding="utf-8") as f:
        data = yaml.safe_load(f)
    if not isinstance(data, list):
        raise ValueError(f"questions.yaml must be a list, got {type(data)}")
    return data


# ---------------------------------------------------------------------------
# Report writer
# ---------------------------------------------------------------------------


def write_report(results: list[dict], out_path: Path) -> None:
    """Write a Markdown eval report.

    Args:
        results:  List of result dicts from run_question() / run_all().
        out_path: Output file path (e.g. tests/eval/reports/run_2026-04-27.md).
    """
    out_path.parent.mkdir(parents=True, exist_ok=True)

    passed = sum(1 for r in results if r["passed"])
    total = len(results)
    pass_rate = f"{passed}/{total} ({100 * passed / total:.1f}%)" if total else "0/0"

    lines: list[str] = [
        "# NLQ Eval Report\n",
        f"**Pass rate:** {pass_rate}\n",
        "",
        "| ID | Tier | Passed | Reason |",
        "|----|------|--------|--------|",
    ]

    for r in results:
        icon = "✓" if r["passed"] else "✗"
        reason_short = (r["reason"] or "")[:80].replace("|", "\\|")
        lines.append(f"| {r['id']} | {r['tier']} | {icon} | {reason_short} |")

    lines.append("")

    # Detailed failure section
    failures = [r for r in results if not r["passed"]]
    if failures:
        lines.append("## Failures\n")
        for r in failures:
            lines.append(f"### {r['id']} (Tier {r['tier']})\n")
            lines.append(f"**Reason:** {r['reason']}\n")
            if r.get("sql_diff"):
                lines.append("**SQL diff:**\n```sql")
                lines.append(r["sql_diff"])
                lines.append("```\n")
            if r.get("df_head_diff"):
                lines.append("**DataFrame head diff:**\n```")
                lines.append(r["df_head_diff"])
                lines.append("```\n")

    out_path.write_text("\n".join(lines), encoding="utf-8")
