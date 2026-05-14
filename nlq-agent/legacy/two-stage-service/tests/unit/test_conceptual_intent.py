"""
C5: Unit tests — conceptual intent classification via keyword routing.

Verifies:
1. Conceptual keywords → IntentType.CONCEPTUAL (keyword pre-check bypasses LLM)
2. Filter is empty for conceptual queries (no SQL needed)
3. Non-conceptual questions do not match
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
    "question",
    [
        "什么是铁损 P17/50",
        "合格率是怎么计算的",
        "A类产品的定义是什么",
        "叠片系数的含义",
        "铁损怎么计算",
        "P17/50 是什么意思",
        "如何理解合格率",
    ],
    ids=["什么是", "怎么计算", "定义", "含义", "怎么", "什么意思", "如何"],
)
async def test_conceptual_keyword_routes_to_conceptual(question: str) -> None:
    """Conceptual keywords → IntentType.CONCEPTUAL with default entities."""
    agent = _make_agent()
    result = await agent._classify_intent(question)

    assert result.intent == IntentType.CONCEPTUAL
    assert result.confidence >= 0.9


async def test_conceptual_keyword_empty_entities() -> None:
    """Conceptual keyword query has empty extracted_entities."""
    agent = _make_agent()
    result = await agent._classify_intent("什么是铁损")

    assert result.intent == IntentType.CONCEPTUAL
    assert result.extracted_entities == {}


async def test_conceptual_keyword_does_not_fire_on_statistical() -> None:
    """Non-conceptual statistical question does NOT trigger keyword routing."""
    agent = _make_agent()
    result = await agent._classify_intent("合格率不低于75%且抽样数量不少于100的产品规格")

    assert result.intent != IntentType.CONCEPTUAL


@pytest.mark.parametrize(
    "keyword",
    ["什么是", "如何", "定义", "含义", "怎么计算", "是什么", "什么意思", "怎么"],
    ids=["什么是", "如何", "定义", "含义", "怎么计算", "是什么", "什么意思", "怎么"],
)
async def test_all_conceptual_keywords(keyword: str) -> None:
    """Each conceptual keyword triggers IntentType.CONCEPTUAL."""
    agent = _make_agent()
    question = f"{keyword}合格率"
    result = await agent._classify_intent(question)

    assert result.intent == IntentType.CONCEPTUAL
