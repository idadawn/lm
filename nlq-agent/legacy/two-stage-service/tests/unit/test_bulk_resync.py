"""
Unit tests for bulk_resync_all service function.

Covers:
- Returns correct dict schema with mocked services
- Zero-row tables still return valid schema
"""

from __future__ import annotations

from unittest.mock import AsyncMock, MagicMock, patch

import pytest

from src.services.resync_service import bulk_resync_all


def _make_settings():
    s = MagicMock()
    s.collection_rules = "test_rules"
    s.collection_specs = "test_specs"
    s.collection_metrics = "test_metrics"
    return s


@pytest.mark.asyncio
async def test_bulk_resync_returns_correct_schema() -> None:
    """Normal case: returns dict with rules, specs, duration_ms keys."""
    mock_db = AsyncMock()
    mock_db.execute_query.side_effect = [
        # load_judgment_rules
        {"rows": [
            {"id": "r1", "name": "合格", "code": "A", "quality_status": 0,
             "priority": 1, "description": "desc", "condition": "",
             "formula_name": "", "product_spec_name": "", "product_spec_id": "",
             "is_statistic": 0},
        ]},
        # load_product_specs (specs)
        {"rows": [
            {"id": "s1", "code": "50W470", "name": "spec1", "description": "",
             "detection_columns": "8"},
        ]},
        # load_product_specs (attrs for s1)
        {"rows": []},
        # load_formulas
        {"rows": [
            {"id": "f1", "column_name": "col", "formula_name": "fm1",
             "formula": "x+1", "formula_type": "CALC", "unit_name": "kg",
             "remark": "", "is_enabled": 1},
        ]},
    ]
    mock_db.close = AsyncMock()

    mock_qdrant = AsyncMock()
    mock_qdrant.upsert_documents.side_effect = [1, 1, 6]  # rules, specs, metrics(1+5 predefined)
    mock_qdrant.ensure_collections = AsyncMock()
    mock_qdrant.close = AsyncMock()

    mock_embedding = MagicMock()
    mock_embedding.close = AsyncMock()

    with (
        patch("src.services.resync_service.get_settings", return_value=_make_settings()),
        patch("src.services.resync_service.EmbeddingClient", return_value=mock_embedding),
        patch("src.services.resync_service.QdrantService", return_value=mock_qdrant),
        patch("src.services.resync_service.DatabaseService", return_value=mock_db),
    ):
        result = await bulk_resync_all()

    assert "rules" in result
    assert "specs" in result
    assert "duration_ms" in result
    assert result["rules"] == 1
    assert result["specs"] == 1
    assert isinstance(result["duration_ms"], int)
    assert result["duration_ms"] >= 0


@pytest.mark.asyncio
async def test_bulk_resync_empty_tables_returns_zero_counts() -> None:
    """All tables empty → returns zeroes with valid schema."""
    mock_db = AsyncMock()
    mock_db.execute_query.side_effect = [
        {"rows": []},  # rules
        {"rows": []},  # specs
        {"rows": []},  # formulas
    ]
    mock_db.close = AsyncMock()

    mock_qdrant = AsyncMock()
    mock_qdrant.upsert_documents.side_effect = [0, 0, 5]  # 5 predefined metrics always present
    mock_qdrant.ensure_collections = AsyncMock()
    mock_qdrant.close = AsyncMock()

    mock_embedding = MagicMock()
    mock_embedding.close = AsyncMock()

    with (
        patch("src.services.resync_service.get_settings", return_value=_make_settings()),
        patch("src.services.resync_service.EmbeddingClient", return_value=mock_embedding),
        patch("src.services.resync_service.QdrantService", return_value=mock_qdrant),
        patch("src.services.resync_service.DatabaseService", return_value=mock_db),
    ):
        result = await bulk_resync_all()

    assert result["rules"] == 0
    assert result["specs"] == 0
    assert isinstance(result["duration_ms"], int)
