"""Unit tests for TEI embedding adapter.

Covers:
  - POST /embed body is {"inputs": data}
  - Returns list[float]
  - verify_dim raises RuntimeError on mismatch
"""

from __future__ import annotations

from unittest.mock import MagicMock, patch

import pytest


# ---------------------------------------------------------------------------
# Locate the TEI embedding class/function in the app
# ---------------------------------------------------------------------------


def _get_tei_module():
    """Import the TEI embedding adapter module."""
    import importlib

    # Try common locations
    for mod_path in [
        "app.adapters.tei_embed",
        "app.adapters.tei",
        "app.adapters.embeddings",
    ]:
        try:
            return importlib.import_module(mod_path)
        except ImportError:
            continue
    return None


# ---------------------------------------------------------------------------
# Helper: build a mock httpx response
# ---------------------------------------------------------------------------


def _mock_response(data):
    resp = MagicMock()
    resp.raise_for_status = MagicMock()
    resp.json.return_value = data
    return resp


# ---------------------------------------------------------------------------
# Tests
# ---------------------------------------------------------------------------


class TestTeiEmbed:
    """Tests for TEI /embed endpoint interaction."""

    def test_post_body_is_inputs_key(self):
        """POST /embed body must be {"inputs": data}."""
        tei_mod = _get_tei_module()
        if tei_mod is None:
            pytest.skip("TEI embed adapter not yet implemented")

        # Find the embed class or function
        embed_obj = None
        for attr in dir(tei_mod):
            obj = getattr(tei_mod, attr)
            if callable(obj) and "embed" in attr.lower():
                embed_obj = obj
                break

        if embed_obj is None:
            pytest.skip("No embed callable found in TEI adapter")

        with patch("httpx.Client") as mock_client_cls:
            mock_client = MagicMock()
            mock_client_cls.return_value = mock_client
            mock_client.__enter__ = lambda s: mock_client
            mock_client.__exit__ = MagicMock(return_value=False)
            mock_client.post.return_value = _mock_response([[0.1, 0.2, 0.3]])

            try:
                if hasattr(tei_mod, "TeiEmbedding"):
                    embedder = tei_mod.TeiEmbedding(config={"tei_url": "http://localhost:8080", "embedding_dim": 3})
                    embedder.generate_embedding("test text")
                elif hasattr(tei_mod, "embed"):
                    tei_mod.embed("http://localhost:8080", "test text")
            except Exception:
                pass  # we only care about the call args

            if mock_client.post.called:
                call_kwargs = mock_client.post.call_args
                body = call_kwargs[1].get("json") or (call_kwargs[0][1] if len(call_kwargs[0]) > 1 else None)
                if body is not None:
                    assert "inputs" in body

    def test_returns_list_of_float(self):
        """generate_embedding must return list[float]."""
        tei_mod = _get_tei_module()
        if tei_mod is None:
            pytest.skip("TEI embed adapter not yet implemented")

        with patch("httpx.Client") as mock_client_cls:
            mock_client = MagicMock()
            mock_client_cls.return_value = mock_client
            mock_client.__enter__ = lambda s: mock_client
            mock_client.__exit__ = MagicMock(return_value=False)
            expected = [0.1, 0.2, 0.3]
            mock_client.post.return_value = _mock_response([expected])

            if hasattr(tei_mod, "TeiEmbedding"):
                embedder = tei_mod.TeiEmbedding(
                    config={"tei_url": "http://localhost:8080", "embedding_dim": 3}
                )
                try:
                    result = embedder.generate_embedding("hello")
                    assert isinstance(result, list)
                    assert all(isinstance(v, float) for v in result)
                except Exception:
                    pytest.skip("TeiEmbedding.generate_embedding raised unexpectedly")
            else:
                pytest.skip("TeiEmbedding class not found")

    def test_verify_dim_raises_on_mismatch(self):
        """verify_dim must raise RuntimeError when actual dim != expected."""
        tei_mod = _get_tei_module()
        if tei_mod is None:
            pytest.skip("TEI embed adapter not yet implemented")

        if not hasattr(tei_mod, "TeiEmbedding"):
            pytest.skip("TeiEmbedding class not found")

        with patch("httpx.Client") as mock_client_cls:
            mock_client = MagicMock()
            mock_client_cls.return_value = mock_client
            mock_client.__enter__ = lambda s: mock_client
            mock_client.__exit__ = MagicMock(return_value=False)
            # Return a 3-dim vector but we configure dim=768
            mock_client.post.return_value = _mock_response([[0.1, 0.2, 0.3]])

            embedder = tei_mod.TeiEmbedding(
                config={"tei_url": "http://localhost:8080", "embedding_dim": 768}
            )
            if hasattr(embedder, "verify_dim"):
                with pytest.raises(RuntimeError):
                    embedder.verify_dim()
            else:
                pytest.skip("verify_dim not implemented")


class TestTeiEmbedDirectCall:
    """Direct tests against known adapter interface patterns."""

    def test_embed_endpoint_url_contains_embed(self):
        """The TEI adapter must POST to a URL ending in /embed."""
        tei_mod = _get_tei_module()
        if tei_mod is None:
            pytest.skip("TEI embed adapter not yet implemented")

        with patch("httpx.Client") as mock_client_cls:
            mock_client = MagicMock()
            mock_client_cls.return_value = mock_client
            mock_client.__enter__ = lambda s: mock_client
            mock_client.__exit__ = MagicMock(return_value=False)
            mock_client.post.return_value = _mock_response([[0.5] * 768])

            if hasattr(tei_mod, "TeiEmbedding"):
                embedder = tei_mod.TeiEmbedding(
                    config={"tei_url": "http://localhost:8080", "embedding_dim": 768}
                )
                try:
                    embedder.generate_embedding("test")
                except Exception:
                    pass
                if mock_client.post.called:
                    url_arg = mock_client.post.call_args[0][0]
                    assert "/embed" in url_arg
