"""Eval-specific pytest fixtures and CLI options.

Fixtures:
  vanna_app : real VannaApp (mockable via env; skipped if EVAL_SKIP=1)
  questions : list[dict] loaded from questions.yaml

Custom options:
  --baseline : disable few-shot Q&A injection (runs baseline mode, G2)
"""

from __future__ import annotations

import os
from pathlib import Path

import pytest
import yaml


# ---------------------------------------------------------------------------
# Custom pytest CLI option
# ---------------------------------------------------------------------------


def pytest_addoption(parser: pytest.Parser) -> None:
    """Add --baseline flag to disable few-shot injection for baseline eval (G2)."""
    parser.addoption(
        "--baseline",
        action="store_true",
        default=False,
        help=(
            "Run eval in baseline mode: skip Q&A few-shot injection. "
            "Used for G2 baseline vs. few-shot comparison."
        ),
    )


# ---------------------------------------------------------------------------
# questions fixture
# ---------------------------------------------------------------------------


@pytest.fixture(scope="session")
def questions() -> list[dict]:
    """Load all questions from questions.yaml."""
    yaml_path = Path(__file__).parent / "questions.yaml"
    if not yaml_path.exists():
        pytest.skip(f"questions.yaml not found at {yaml_path}")
    with yaml_path.open(encoding="utf-8") as f:
        data = yaml.safe_load(f)
    assert isinstance(data, list), f"questions.yaml must be a list, got {type(data)}"
    return data


# ---------------------------------------------------------------------------
# vanna_app fixture (real, but skippable via env)
# ---------------------------------------------------------------------------


@pytest.fixture(scope="session")
def vanna_app(request):
    """Provide a VannaApp instance for eval tests.

    If EVAL_SKIP=1 env var is set, skip the test (CI without live services).
    If --baseline is passed, attach_emitter is skipped and few-shot is disabled.
    """
    if os.environ.get("EVAL_SKIP", "0") == "1":
        pytest.skip("EVAL_SKIP=1: skipping eval tests (no live services)")

    baseline_mode: bool = request.config.getoption("--baseline", default=False)

    try:
        from app.config import get_settings
        from app.vanna_app import VannaApp

        settings = get_settings()
        config = {
            "qdrant_url": settings.qdrant_url,
            "embedding_dim": settings.tei_embedding_dim,
            "tei_url": settings.tei_url,
            "vllm_url": settings.vllm_url,
            "vllm_model": settings.vllm_model,
            "mysql_host": settings.mysql_host,
            "mysql_port": settings.mysql_port,
            "mysql_user": settings.mysql_user,
            "mysql_password": settings.mysql_password,
            "mysql_db": settings.mysql_database,
        }
        vn = VannaApp(config=config)

        if baseline_mode:
            # Baseline: monkey-patch get_similar_question_sql to return empty list
            vn.get_similar_question_sql = lambda q, **kw: []

        return vn
    except Exception as exc:
        pytest.skip(f"Could not instantiate VannaApp (no live services?): {exc}")
