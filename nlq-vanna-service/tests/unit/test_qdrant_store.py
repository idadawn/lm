"""Unit tests for QdrantStoreMixin.

Covers:
  - Collection creation parameters (_ensure_collection)
  - add_ddl / add_terminology / add_judgment_rule / add_question_sql payload subtype (ADR-6)
  - add_documentation raises NotImplementedError
  - get_related_documentation returns list[str] with controllable length
  - ReasoningStep emit kind/subtype mapping correctness
"""

from __future__ import annotations

from unittest.mock import MagicMock, call, patch

import pytest

from app.api.schemas import ReasoningStep


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def _make_store(mock_qdrant_instance, embedding_dim: int = 768):
    """Construct a QdrantStoreMixin with the provided mock Qdrant instance."""
    from app.adapters.qdrant_store import QdrantStoreMixin

    with patch("app.adapters.qdrant_store.QdrantClient", return_value=mock_qdrant_instance):
        # collection already exists so _ensure_collection is a no-op
        coll_resp = MagicMock()
        coll_resp.collections = [MagicMock(name="nlq_vanna_knowledge")]
        coll_resp.collections[0].name = "nlq_vanna_knowledge"
        mock_qdrant_instance.get_collections.return_value = coll_resp

        store = QdrantStoreMixin.__new__(QdrantStoreMixin)
        store._qdrant_url = "http://localhost:6333"
        store._embedding_dim = embedding_dim
        store._client = mock_qdrant_instance
        store._emitter = None

        # Provide a fake generate_embedding so _upsert / _search work
        store.generate_embedding = MagicMock(return_value=[0.1] * embedding_dim)
        return store


# ---------------------------------------------------------------------------
# Collection creation
# ---------------------------------------------------------------------------


class TestEnsureCollection:
    def test_creates_collection_when_missing(self):
        mock_client = MagicMock()
        coll_resp = MagicMock()
        coll_resp.collections = []  # empty → collection missing
        mock_client.get_collections.return_value = coll_resp

        from app.adapters.qdrant_store import QdrantStoreMixin

        with patch("app.adapters.qdrant_store.QdrantClient", return_value=mock_client):
            store = QdrantStoreMixin.__new__(QdrantStoreMixin)
            store._qdrant_url = "http://localhost:6333"
            store._embedding_dim = 768
            store._client = mock_client
            store._emitter = None
            store._ensure_collection()

        mock_client.create_collection.assert_called_once()
        kwargs = mock_client.create_collection.call_args
        assert kwargs[1]["collection_name"] == "nlq_vanna_knowledge" or (
            kwargs[0] and kwargs[0][0] == "nlq_vanna_knowledge"
        )

    def test_skips_creation_when_exists(self):
        mock_client = MagicMock()
        coll = MagicMock()
        coll.name = "nlq_vanna_knowledge"
        coll_resp = MagicMock()
        coll_resp.collections = [coll]
        mock_client.get_collections.return_value = coll_resp

        from app.adapters.qdrant_store import QdrantStoreMixin

        store = QdrantStoreMixin.__new__(QdrantStoreMixin)
        store._qdrant_url = "http://localhost:6333"
        store._embedding_dim = 768
        store._client = mock_client
        store._emitter = None
        store._ensure_collection()

        mock_client.create_collection.assert_not_called()


# ---------------------------------------------------------------------------
# add_* payload subtype tests (ADR-6)
# ---------------------------------------------------------------------------


class TestAddMethodPayloads:
    def setup_method(self):
        self.mock_client = MagicMock()
        self.store = _make_store(self.mock_client)

    def _get_upserted_payload(self):
        """Extract the payload from the last upsert call."""
        call_kwargs = self.mock_client.upsert.call_args
        points = call_kwargs[1].get("points") or call_kwargs[0][1]
        return points[0].payload

    def test_add_ddl_subtype(self):
        self.store.add_ddl("CREATE TABLE foo (id INT)")
        payload = self._get_upserted_payload()
        assert payload["type"] == "documentation"
        assert payload["subtype"] == "ddl"
        assert "CREATE TABLE foo" in payload["content"]

    def test_add_terminology_subtype(self):
        self.store.add_terminology("叠片系数是指…")
        payload = self._get_upserted_payload()
        assert payload["type"] == "documentation"
        assert payload["subtype"] == "terminology"
        assert "叠片系数" in payload["content"]

    def test_add_judgment_rule_subtype(self):
        self.store.add_judgment_rule("铁损 > 1.05 时判为 C 级")
        payload = self._get_upserted_payload()
        assert payload["type"] == "documentation"
        assert payload["subtype"] == "judgment_rule"

    def test_add_question_sql_type(self):
        self.store.add_question_sql(
            question="查询铁损", sql="SELECT * FROM lab WHERE iron_loss > 1.0"
        )
        payload = self._get_upserted_payload()
        assert payload["type"] == "qa"
        assert payload["question"] == "查询铁损"
        assert "SELECT" in payload["sql"]

    def test_add_documentation_raises(self):
        with pytest.raises(NotImplementedError):
            self.store.add_documentation("some doc")


# ---------------------------------------------------------------------------
# get_related_documentation returns list[str]
# ---------------------------------------------------------------------------


class TestGetRelatedDocumentation:
    def setup_method(self):
        self.mock_client = MagicMock()
        self.store = _make_store(self.mock_client)

    def _make_hit(self, content: str, subtype: str = "terminology", score: float = 0.9):
        hit = MagicMock()
        hit.payload = {"type": "documentation", "subtype": subtype, "content": content}
        hit.score = score
        return hit

    def test_returns_list_of_str(self):
        self.mock_client.search.return_value = [
            self._make_hit("叠片系数定义"),
            self._make_hit("判级规则示例", subtype="judgment_rule"),
        ]
        result = self.store.get_related_documentation("叠片系数")
        assert isinstance(result, list)
        assert all(isinstance(d, str) for d in result)

    def test_length_matches_hits(self):
        hits = [self._make_hit(f"doc_{i}") for i in range(4)]
        self.mock_client.search.return_value = hits
        result = self.store.get_related_documentation("test")
        assert len(result) == 4

    def test_empty_hits_returns_empty_list(self):
        self.mock_client.search.return_value = []
        result = self.store.get_related_documentation("test")
        assert result == []

    def test_emits_reasoning_step_per_hit(self):
        mock_emitter = MagicMock()
        self.store._emitter = mock_emitter
        hits = [
            self._make_hit("术语A", subtype="terminology"),
            self._make_hit("规则B", subtype="judgment_rule"),
        ]
        self.mock_client.search.return_value = hits
        self.store.get_related_documentation("test question")
        # Each hit should emit one step
        assert mock_emitter.put_step.call_count == 2
        # Check kind=rule for terminology emit
        first_step = mock_emitter.put_step.call_args_list[0][0][0]
        assert first_step.kind == "rule"

    def test_emit_kind_rule_for_both_subtypes(self):
        mock_emitter = MagicMock()
        self.store._emitter = mock_emitter
        hits = [
            self._make_hit("术语A", subtype="terminology"),
            self._make_hit("规则B", subtype="judgment_rule"),
        ]
        self.mock_client.search.return_value = hits
        self.store.get_related_documentation("test")
        for c in mock_emitter.put_step.call_args_list:
            step = c[0][0]
            assert step.kind == "rule"
