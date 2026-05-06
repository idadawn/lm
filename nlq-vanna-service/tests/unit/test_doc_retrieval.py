"""Unit tests for QdrantStoreMixin.get_related_documentation.

Architect MUST-A + Critic MAJOR #3 强化版:
  - Positive assertion: len(docs) >= 1 when hits are returned
  - known_term substring assertion: "叠片系数" appears in at least one doc
  - Negative test: 0 hits → len(docs) == 0 (zero recall is a valid path,
    distinct from "recalled but missing keyword")
"""

from __future__ import annotations

from unittest.mock import MagicMock, patch

import pytest

from app.api.schemas import ReasoningStep


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def _make_store_with_mock_search(search_return_value):
    """Build a QdrantStoreMixin instance whose _search is fully mocked."""
    from app.adapters.qdrant_store import QdrantStoreMixin

    mock_client = MagicMock()
    coll = MagicMock()
    coll.name = "nlq_vanna_knowledge"
    coll_resp = MagicMock()
    coll_resp.collections = [coll]
    mock_client.get_collections.return_value = coll_resp

    with patch("app.adapters.qdrant_store.QdrantClient", return_value=mock_client):
        store = QdrantStoreMixin.__new__(QdrantStoreMixin)
        store._qdrant_url = "http://localhost:6333"
        store._embedding_dim = 768
        store._client = mock_client
        store._emitter = None
        store.generate_embedding = MagicMock(return_value=[0.1] * 768)

    # Override _search directly
    store._search = MagicMock(return_value=search_return_value)
    return store


def _make_hit(content: str, subtype: str = "terminology", score: float = 0.9):
    hit = MagicMock()
    hit.payload = {"type": "documentation", "subtype": subtype, "content": content}
    hit.score = score
    return hit


# ---------------------------------------------------------------------------
# Positive tests (known_term fixture)
# ---------------------------------------------------------------------------


class TestDocRetrievalPositive:
    """Tests with known_term="叠片系数" in mock search results."""

    @pytest.fixture
    def known_term(self) -> str:
        return "叠片系数"

    @pytest.fixture
    def store_with_known_term(self, known_term):
        """Store whose _search returns a hit containing the known term."""
        hits = [
            _make_hit(f"叠片系数是指叠片压实程度的系数，通常在0.95-1.0之间", subtype="terminology"),
            _make_hit("铁损判级规则：铁损 > 1.05 判为 C 级", subtype="judgment_rule"),
        ]
        return _make_store_with_mock_search(hits)

    def test_returns_list(self, store_with_known_term):
        docs = store_with_known_term.get_related_documentation("查询叠片系数")
        assert isinstance(docs, list)

    def test_all_elements_are_str(self, store_with_known_term):
        docs = store_with_known_term.get_related_documentation("查询叠片系数")
        assert all(isinstance(d, str) for d in docs)

    def test_positive_len_ge_1(self, store_with_known_term, known_term):
        """MAJOR #3: positive assertion — len >= 1 when hits are returned."""
        docs = store_with_known_term.get_related_documentation("查询叠片系数")
        assert len(docs) >= 1, (
            f"Expected at least 1 doc when search returns hits, got {len(docs)}"
        )

    def test_known_term_in_docs(self, store_with_known_term, known_term):
        """MAJOR #3: known_term substring must appear in at least one doc."""
        docs = store_with_known_term.get_related_documentation(f"查询{known_term}")
        assert any(known_term in d for d in docs), (
            f"known_term '{known_term}' not found in any doc: {docs}"
        )

    def test_exact_hit_count(self, store_with_known_term):
        docs = store_with_known_term.get_related_documentation("查询叠片系数")
        assert len(docs) == 2


# ---------------------------------------------------------------------------
# Negative test: zero recall is a valid path
# ---------------------------------------------------------------------------


class TestDocRetrievalZeroRecall:
    """Explicitly tests the zero-recall path.

    Zero recall (0 hits) is a legitimate path — distinct from 'recalled but
    missing keyword'.  The caller (VannaBase) gracefully handles empty list.
    """

    def test_zero_hits_returns_empty_list(self):
        store = _make_store_with_mock_search([])
        docs = store.get_related_documentation("无关问题")
        assert isinstance(docs, list)
        assert len(docs) == 0, (
            "Zero hits from Qdrant must produce an empty list[], "
            "not raise or return None."
        )

    def test_zero_hits_is_valid_not_an_error(self):
        """Zero-recall path must not raise any exception."""
        store = _make_store_with_mock_search([])
        try:
            docs = store.get_related_documentation("完全不相关的查询")
            assert docs == []
        except Exception as exc:
            pytest.fail(f"get_related_documentation raised on zero hits: {exc}")


# ---------------------------------------------------------------------------
# Content integrity tests
# ---------------------------------------------------------------------------


class TestDocRetrievalContentIntegrity:
    def test_content_strings_not_object_repr(self):
        """Returned strings must be raw content, not '[object Object]' noise."""
        hits = [
            _make_hit("真实业务内容：叠片系数计算方法"),
            _make_hit("判级规则内容"),
        ]
        store = _make_store_with_mock_search(hits)
        docs = store.get_related_documentation("叠片系数")
        for doc in docs:
            assert "[object" not in doc
            assert "Object" not in doc or "object" not in doc.lower().replace("object object", "")

    def test_each_doc_is_payload_content(self):
        """Each returned string should match the payload content field."""
        contents = ["术语内容A", "规则内容B", "术语内容C"]
        hits = [_make_hit(c) for c in contents]
        store = _make_store_with_mock_search(hits)
        docs = store.get_related_documentation("test")
        assert docs == contents

    def test_order_matches_search_result_order(self):
        """Documents are returned in the same order as search results (cosine rank)."""
        hits = [
            _make_hit("高分结果", score=0.95),
            _make_hit("中分结果", score=0.80),
            _make_hit("低分结果", score=0.65),
        ]
        store = _make_store_with_mock_search(hits)
        docs = store.get_related_documentation("test")
        assert docs[0] == "高分结果"
        assert docs[1] == "中分结果"
        assert docs[2] == "低分结果"
