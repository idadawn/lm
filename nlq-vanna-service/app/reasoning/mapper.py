"""Mapper utilities: Qdrant payload subtype → ReasoningStepKind, terminal step builder.

Covers ADR-3 SSE mapping table (nlq-vanna-standalone-v2.md § SSE 推理步骤映射).
"""

from __future__ import annotations

from typing import TYPE_CHECKING, Literal

from app.api.schemas import ReasoningStep, ReasoningStepKind

if TYPE_CHECKING:
    import pandas as pd

# ---------------------------------------------------------------------------
# Subtype → Kind mapping
# ---------------------------------------------------------------------------

# Qdrant payload subtype values defined in ADR-6
_SUBTYPE_MAP: dict[str, ReasoningStepKind] = {
    "ddl": "spec",
    "qa": "spec",
    "terminology": "rule",
    "judgment_rule": "rule",
    "condition": "condition",
    "grade": "grade",
    "record": "record",
    "fallback": "fallback",
}

# Grade-related Chinese keywords used by build_terminal_step
_GRADE_KEYWORDS = ("判级", "等级", "级别", "评级", "定级")


def subtype_to_kind(
    subtype: str,
) -> Literal["spec", "rule", "condition", "grade", "record", "fallback"]:
    """Map a Qdrant payload subtype string to a ReasoningStepKind.

    Mapping rules (ADR-3 / ADR-6):
      ddl / qa            → spec     (schema / historical Q&A knowledge)
      terminology         → rule     (business glossary doc)
      judgment_rule       → rule     (grade rule doc)
      condition           → condition (pass-through; from SQL WHERE parsing)
      grade               → grade    (single terminal grade result)
      record              → record   (multi-row result summary)
      fallback            → fallback (error / empty result)
      <unknown>           → fallback (safe default)

    Args:
        subtype: The ``subtype`` field value stored in Qdrant payload.

    Returns:
        A valid ReasoningStepKind literal.
    """
    return _SUBTYPE_MAP.get(subtype, "fallback")


# ---------------------------------------------------------------------------
# Terminal step builder
# ---------------------------------------------------------------------------


def build_terminal_step(
    df: "pd.DataFrame",
    question: str,
    sql: str,
) -> ReasoningStep:
    """Build the terminal ReasoningStep after SQL execution.

    Decision table (ADR-3 mapping, last two rows):
      - row_count == 0                    → fallback  (empty result)
      - row_count >= 1 and grade keyword  → grade     (判级类查询)
      - row_count >= 1 otherwise          → record    (普通查询)

    Args:
        df:       DataFrame returned by vn.run_sql().
        question: Original user question (Chinese NL).
        sql:      Generated SQL string (stored in meta for traceability).

    Returns:
        A single ReasoningStep with kind in {grade, record, fallback}.
    """
    row_count: int = len(df)

    if row_count == 0:
        return ReasoningStep(
            kind="fallback",
            label="查询无结果",
            detail="SQL 执行成功，但返回空结果集。",
            meta={"sql": sql[:300], "row_count": 0},
        )

    # Detect grade-type question by Chinese keywords
    is_grade_question = any(kw in question for kw in _GRADE_KEYWORDS)

    if is_grade_question:
        # Attempt to read grade value from first row
        grade_value: str | None = None
        if not df.empty:
            first_row = df.iloc[0]
            # Common grade column names
            for col in ("grade", "等级", "level", "GRADE", "LEVEL", "quality_status"):
                if col in first_row.index:
                    grade_value = str(first_row[col])
                    break
            if grade_value is None:
                # Fall back to first column value
                grade_value = str(first_row.iloc[0])

        label = f"判定为 {grade_value}" if grade_value else "判级完成"
        return ReasoningStep(
            kind="grade",
            label=label,
            meta={
                "row_count": row_count,
                "sql": sql[:300],
                "grade_value": grade_value,
            },
        )

    # Default: record
    if row_count == 1:
        label = "命中 1 条记录"
    else:
        label = f"命中 {row_count} 条记录"

    return ReasoningStep(
        kind="record",
        label=label,
        meta={"row_count": row_count, "sql": sql[:300]},
    )
