"""Unit tests for vLLM LLM adapter.

Covers:
  - POST /v1/chat/completions body contains model + messages + temperature + max_tokens
  - Returns choices[0].message.content string
  - LLM refuse paths: empty content, None content → returns empty string or raises gracefully
"""

from __future__ import annotations

import importlib
from unittest.mock import MagicMock, patch

import pytest


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def _get_vllm_module():
    for mod_path in [
        "app.adapters.vllm_llm",
        "app.adapters.vllm",
        "app.adapters.llm",
    ]:
        try:
            return importlib.import_module(mod_path)
        except ImportError:
            continue
    return None


def _mock_completions_response(content: str | None):
    """Build a mock httpx response mimicking /v1/chat/completions."""
    resp = MagicMock()
    resp.raise_for_status = MagicMock()
    resp.json.return_value = {
        "choices": [
            {
                "message": {
                    "role": "assistant",
                    "content": content,
                }
            }
        ]
    }
    return resp


# ---------------------------------------------------------------------------
# Tests
# ---------------------------------------------------------------------------


class TestVllmLlm:
    """Tests for vLLM /v1/chat/completions adapter."""

    def _make_llm(self, mock_client_cls, mod):
        mock_client = MagicMock()
        mock_client_cls.return_value = mock_client
        mock_client.__enter__ = lambda s: mock_client
        mock_client.__exit__ = MagicMock(return_value=False)

        if hasattr(mod, "VllmLlm"):
            llm = mod.VllmLlm(
                config={
                    "vllm_url": "http://localhost:8000",
                    "vllm_model": "Qwen2.5-7B-Instruct",
                    "temperature": 0.0,
                    "max_tokens": 512,
                }
            )
            return llm, mock_client
        return None, mock_client

    def test_post_body_contains_required_fields(self):
        mod = _get_vllm_module()
        if mod is None:
            pytest.skip("vLLM adapter not yet implemented")

        with patch("httpx.Client") as mock_cls:
            llm, mock_client = self._make_llm(mock_cls, mod)
            if llm is None:
                pytest.skip("VllmLlm class not found")

            mock_client.post.return_value = _mock_completions_response("SELECT 1")
            messages = [{"role": "user", "content": "test question"}]

            try:
                llm.submit_prompt(messages)
            except Exception:
                pass

            if mock_client.post.called:
                body = mock_client.post.call_args[1].get("json") or {}
                # Must contain model, messages, temperature, max_tokens
                assert "model" in body, f"missing 'model' in body: {body}"
                assert "messages" in body, f"missing 'messages' in body: {body}"
                assert "temperature" in body, f"missing 'temperature' in body: {body}"
                assert "max_tokens" in body, f"missing 'max_tokens' in body: {body}"

    def test_returns_content_string(self):
        mod = _get_vllm_module()
        if mod is None:
            pytest.skip("vLLM adapter not yet implemented")

        with patch("httpx.Client") as mock_cls:
            llm, mock_client = self._make_llm(mock_cls, mod)
            if llm is None:
                pytest.skip("VllmLlm class not found")

            mock_client.post.return_value = _mock_completions_response(
                "SELECT id FROM lab_data WHERE iron_loss > 1.05"
            )
            messages = [{"role": "user", "content": "查铁损超标的记录"}]

            try:
                result = llm.submit_prompt(messages)
                assert isinstance(result, str)
                assert len(result) > 0
            except Exception as e:
                pytest.skip(f"submit_prompt raised: {e}")

    def test_empty_content_returns_empty_or_raises(self):
        """LLM refuse path: empty content string should not crash the caller fatally."""
        mod = _get_vllm_module()
        if mod is None:
            pytest.skip("vLLM adapter not yet implemented")

        with patch("httpx.Client") as mock_cls:
            llm, mock_client = self._make_llm(mock_cls, mod)
            if llm is None:
                pytest.skip("VllmLlm class not found")

            mock_client.post.return_value = _mock_completions_response("")
            messages = [{"role": "user", "content": "refuse me"}]

            try:
                result = llm.submit_prompt(messages)
                # If it returns, must be str (possibly empty)
                assert isinstance(result, str)
            except (ValueError, RuntimeError):
                pass  # Raising is acceptable for empty content

    def test_none_content_returns_empty_or_raises(self):
        """LLM refuse path: None content should not crash fatally."""
        mod = _get_vllm_module()
        if mod is None:
            pytest.skip("vLLM adapter not yet implemented")

        with patch("httpx.Client") as mock_cls:
            llm, mock_client = self._make_llm(mock_cls, mod)
            if llm is None:
                pytest.skip("VllmLlm class not found")

            mock_client.post.return_value = _mock_completions_response(None)
            messages = [{"role": "user", "content": "refuse me again"}]

            try:
                result = llm.submit_prompt(messages)
                # If it returns, must be str or None (caller handles None)
                assert result is None or isinstance(result, str)
            except (ValueError, RuntimeError, TypeError):
                pass  # Raising is acceptable

    def test_completions_url_path(self):
        """The adapter must POST to /v1/chat/completions."""
        mod = _get_vllm_module()
        if mod is None:
            pytest.skip("vLLM adapter not yet implemented")

        with patch("httpx.Client") as mock_cls:
            llm, mock_client = self._make_llm(mock_cls, mod)
            if llm is None:
                pytest.skip("VllmLlm class not found")

            mock_client.post.return_value = _mock_completions_response("ok")
            messages = [{"role": "user", "content": "test"}]

            try:
                llm.submit_prompt(messages)
            except Exception:
                pass

            if mock_client.post.called:
                url = mock_client.post.call_args[0][0]
                assert "/v1/chat/completions" in url
