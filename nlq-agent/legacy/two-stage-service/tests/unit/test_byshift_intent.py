"""
S6: Unit tests — by_shift intent classification via keyword routing.

Verifies:
1. Shift keywords → IntentType.BY_SHIFT (keyword pre-check bypasses LLM)
2. time_window extracted correctly from "近 N 月" patterns
3. Default time_window=1 when no explicit window
4. Non-shift questions do not match
"""

from __future__ import annotations

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
        ("白班和晚班的铁损合格率差异", 1),
        ("近3个月各班次合格率对比", 3),
        ("最近6个月不同班次的产品合格率", 6),
        ("早班中班晚班产量对比", 1),
    ],
    ids=["默认本月", "近3个月", "最近6个月", "无显式窗口"],
)
async def test_shift_keyword_routes_to_by_shift(
    question: str, expected_window: int
) -> None:
    """Shift keywords → IntentType.BY_SHIFT with correct time_window."""
    agent = _make_agent()
    result = await agent._classify_intent(question)

    assert result.intent == IntentType.BY_SHIFT
    assert result.confidence >= 0.9
    assert result.extracted_entities.get("time_window") == expected_window


async def test_shift_keyword_default_time_window() -> None:
    """Shift keyword without explicit N → default time_window=1."""
    agent = _make_agent()
    result = await agent._classify_intent("各班次合格率怎么样")

    assert result.intent == IntentType.BY_SHIFT
    assert result.extracted_entities.get("time_window") == 1


async def test_shift_keyword_does_not_fire_on_statistical() -> None:
    """Non-shift statistical question does NOT trigger keyword routing."""
    agent = _make_agent()
    result = await agent._classify_intent("合格率不低于75%且抽样数量不少于100的产品规格")

    assert result.intent != IntentType.BY_SHIFT


@pytest.mark.parametrize(
    "keyword",
    ["班次", "白班", "晚班", "早班", "中班", "夜班"],
    ids=["班次", "白班", "晚班", "早班", "中班", "夜班"],
)
async def test_all_shift_keywords(keyword: str) -> None:
    """Each shift keyword triggers IntentType.BY_SHIFT."""
    agent = _make_agent()
    question = f"产品{keyword}的合格率对比"
    result = await agent._classify_intent(question)

    assert result.intent == IntentType.BY_SHIFT
