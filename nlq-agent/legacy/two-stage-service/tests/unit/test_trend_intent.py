"""
T6: Unit tests — trend intent classification via keyword routing.

Verifies:
1. Trend keywords → IntentType.TREND (keyword pre-check bypasses LLM)
2. time_window extracted correctly from "近 N 月" patterns
3. Default time_window=6 when no explicit window
4. Non-trend questions do not match
"""

from __future__ import annotations

import asyncio
from unittest.mock import AsyncMock, MagicMock

import pytest

from src.models.schemas import IntentType
from src.pipelines.stage1.semantic_kg_agent import SemanticKGAgent


def _make_agent() -> SemanticKGAgent:
    """Build a SemanticKGAgent with mocked dependencies."""
    llm = MagicMock()
    llm.chat_json = AsyncMock()
    qdrant = MagicMock()
    emitter = MagicMock()
    return SemanticKGAgent(llm=llm, qdrant=qdrant, emitter=emitter)


@pytest.mark.parametrize(
    "question, expected_window",
    [
        ("最近6个月合格率变化趋势", 6),
        ("近3月铁损走势", 3),
        ("最近12个月的产品合格率趋势", 12),
        ("近1个月的合格率变化趋势", 1),
    ],
    ids=["最近6个月", "近3月", "最近12个月", "近1个月"],
)
async def test_trend_keyword_routes_to_trend(
    question: str, expected_window: int
) -> None:
    """Trend keywords → IntentType.TREND with correct time_window."""
    agent = _make_agent()
    result = await agent._classify_intent(question)

    assert result.intent == IntentType.TREND
    assert result.confidence >= 0.9
    assert result.extracted_entities.get("time_window") == expected_window


async def test_trend_keyword_default_time_window() -> None:
    """Trend keyword without explicit N → default time_window=6."""
    agent = _make_agent()
    result = await agent._classify_intent("产品合格率走势")

    assert result.intent == IntentType.TREND
    assert result.extracted_entities.get("time_window") == 6


async def test_trend_keyword_does_not_fire_on_statistical() -> None:
    """Non-trend statistical question does NOT trigger keyword routing."""
    agent = _make_agent()
    # This question has no trend keywords — will fall through to LLM mock
    result = await agent._classify_intent("合格率不低于75%且抽样数量不少于100的产品规格")

    # Should NOT be TREND (will be whatever the LLM returns or default)
    assert result.intent != IntentType.TREND


@pytest.mark.parametrize(
    "keyword",
    ["趋势", "变化趋势", "走势", "环比", "同比"],
    ids=["趋势", "变化趋势", "走势", "环比", "同比"],
)
async def test_all_trend_keywords(keyword: str) -> None:
    """Each trend keyword triggers IntentType.TREND."""
    agent = _make_agent()
    question = f"产品{keyword}怎么样"
    result = await agent._classify_intent(question)

    assert result.intent == IntentType.TREND
