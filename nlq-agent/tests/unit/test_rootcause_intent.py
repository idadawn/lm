"""
R6: Unit tests — root_cause intent classification via keyword routing.

Verifies:
1. Root cause keywords → IntentType.ROOT_CAUSE (keyword pre-check bypasses LLM)
2. Default dimension_keys and time_window populated
3. Non-root-cause questions do not match
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
    "question",
    [
        "为什么50W470上月铁损合格率下降",
        "合格率下降的原因是什么",
        "为何昨天合格率这么低",
        "找出合格率不合格的原因",
        "铁损异常偏低的原因",
    ],
    ids=["为什么下降", "原因", "为何", "找出原因", "异常偏低"],
)
async def test_rootcause_keyword_routes_to_root_cause(question: str) -> None:
    """Root cause keywords → IntentType.ROOT_CAUSE with default entities."""
    agent = _make_agent()
    result = await agent._classify_intent(question)

    assert result.intent == IntentType.ROOT_CAUSE
    assert result.confidence >= 0.9
    assert result.extracted_entities.get("metric") == "合格率"
    dimension_keys = result.extracted_entities.get("dimension_keys", [])
    assert "F_PRODUCT_SPEC_CODE" in dimension_keys
    assert "F_CREATORUSERID" in dimension_keys


async def test_rootcause_keyword_default_time_window() -> None:
    """Root cause keyword without explicit period → default time_window=1."""
    agent = _make_agent()
    result = await agent._classify_intent("为什么合格率下降")

    assert result.intent == IntentType.ROOT_CAUSE
    assert result.extracted_entities.get("time_window") == 1


async def test_rootcause_keyword_does_not_fire_on_statistical() -> None:
    """Non-root-cause statistical question does NOT trigger keyword routing."""
    agent = _make_agent()
    result = await agent._classify_intent("合格率不低于75%且抽样数量不少于100的产品规格")

    assert result.intent != IntentType.ROOT_CAUSE


@pytest.mark.parametrize(
    "keyword",
    ["为什么", "原因", "下降", "为何", "找出原因", "不合格", "异常", "偏低"],
    ids=["为什么", "原因", "下降", "为何", "找出原因", "不合格", "异常", "偏低"],
)
async def test_all_rootcause_keywords(keyword: str) -> None:
    """Each root cause keyword triggers IntentType.ROOT_CAUSE."""
    agent = _make_agent()
    question = f"产品{keyword}怎么样"
    result = await agent._classify_intent(question)

    assert result.intent == IntentType.ROOT_CAUSE
