"""SQL WHERE-clause parser → condition ReasoningSteps.

Uses sqlglot to parse generated SQL and extract individual AND predicates
as condition steps for the SSE reasoning chain.

ADR-3 / A3 SHOULD: results capped at ≤ 8 steps to avoid mobile UI overflow.

Exposed helpers for unit testing:
  _flatten_and(node)        — recursively flatten AND tree to list
  _cond_to_step(node, dialect) — convert a single predicate node to ReasoningStep
"""

from __future__ import annotations

import sqlglot
from sqlglot import expressions as exp

from app.api.schemas import ReasoningStep


def extract_conditions(sql: str, dialect: str = "mysql") -> list[ReasoningStep]:
    """Parse SQL WHERE clause and return one condition step per AND predicate.

    Notes:
      - Only the top-level WHERE is parsed; sub-queries are ignored.
      - Results are capped at 8 (A3 SHOULD: avoid mobile UI visual overflow).
      - On parse failure a single fallback step is returned instead of raising.

    Args:
        sql:     The SQL string to parse (typically from vn.generate_sql).
        dialect: sqlglot dialect; defaults to "mysql" matching the target DB.

    Returns:
        A list of ReasoningStep with kind="condition", length 0–8,
        or a single-element list with kind="fallback" on parse error.
    """
    try:
        tree = sqlglot.parse_one(sql, dialect=dialect)
    except sqlglot.errors.ParseError:
        return [
            ReasoningStep(
                kind="fallback",
                label="SQL 解析失败",
                detail=sql[:200],
            )
        ]

    where = tree.args.get("where")
    if not where:
        return []

    steps: list[ReasoningStep] = []
    for cond in _flatten_and(where.this):
        steps.append(_cond_to_step(cond, dialect))

    return steps[:8]  # cap to 8 — A3 SHOULD (line 56)


# ---------------------------------------------------------------------------
# Internal helpers (exposed for unit testing)
# ---------------------------------------------------------------------------


def _flatten_and(node: exp.Expression) -> list[exp.Expression]:
    """Recursively flatten a tree of AND expressions into a flat list.

    Example::

        A AND B AND C
        └─ And(And(A, B), C)
           → [A, B, C]

    Args:
        node: A sqlglot expression node (often the root of a WHERE clause).

    Returns:
        A flat list of non-AND leaf expression nodes.
    """
    if isinstance(node, exp.And):
        return _flatten_and(node.this) + _flatten_and(node.expression)
    return [node]


def _cond_to_step(node: exp.Expression, dialect: str) -> ReasoningStep:
    """Convert a single WHERE predicate node to a condition ReasoningStep.

    Attempts to extract ``field`` (left-hand side) and ``expected``
    (right-hand side) from binary predicates.  Falls back to raw SQL
    label when the node shape is unexpected.

    Args:
        node:    A sqlglot expression representing one predicate.
        dialect: Used when rendering the node back to SQL for the label.

    Returns:
        A ReasoningStep with kind="condition".
    """
    label = node.sql(dialect=dialect)

    # Extract field name (LHS)
    field: str | None = None
    if hasattr(node, "this") and node.this is not None:
        field = node.this.sql(dialect=dialect)

    # Extract expected value (RHS)
    expected: str | None = None
    if (
        hasattr(node, "expression")
        and node.expression is not None  # type: ignore[union-attr]
    ):
        expected = node.expression.sql(dialect=dialect)  # type: ignore[union-attr]

    return ReasoningStep(
        kind="condition",
        label=label,
        field=field,
        expected=expected,
    )
