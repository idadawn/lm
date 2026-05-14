"""
Lock _doc_type_to_step_kind 6-kind enum against reasoning-protocol.ts SoT.

SoT: nlq-agent/packages/shared-types/src/reasoning-protocol.ts
  ReasoningStepKind = "record" | "spec" | "rule" | "condition" | "grade" | "fallback"

This test locks the mapping and ensures no kind values diverge from the .ts source.
"""

from __future__ import annotations

from unittest.mock import patch

import pytest

from src.models.schemas import ReasoningStepKind
from src.pipelines.stage1.semantic_kg_agent import SemanticKGAgent


# ── 6-kind enum lock ──────────────────────────────────────


class TestReasoningStepKindEnum:
    """验证 Python 枚举与 TypeScript SoT 一致。"""

    EXPECTED_KINDS = {
        "record": "record",
        "spec": "spec",
        "rule": "rule",
        "condition": "condition",
        "grade": "grade",
        "fallback": "fallback",
    }

    def test_exactly_six_kinds(self) -> None:
        """枚举恰好包含 6 个值。"""
        assert len(ReasoningStepKind) == 6

    @pytest.mark.parametrize(
        "enum_member, expected_value",
        [
            (ReasoningStepKind.RECORD, "record"),
            (ReasoningStepKind.SPEC, "spec"),
            (ReasoningStepKind.RULE, "rule"),
            (ReasoningStepKind.CONDITION, "condition"),
            (ReasoningStepKind.GRADE, "grade"),
            (ReasoningStepKind.FALLBACK, "fallback"),
        ],
    )
    def test_kind_value_matches_ts(
        self, enum_member: ReasoningStepKind, expected_value: str
    ) -> None:
        """每个枚举值与 TS 端 ReasoningStepKind 一致。"""
        assert enum_member.value == expected_value


class TestDocTypeToStepKindMapping:
    """验证 _doc_type_to_step_kind 返回正确的 kind。"""

    @pytest.fixture
    def agent(self) -> SemanticKGAgent:
        from unittest.mock import MagicMock
        from src.services.llm_client import LLMClient
        from src.services.qdrant_service import QdrantService

        llm = MagicMock(spec=LLMClient)
        qdrant = MagicMock(spec=QdrantService)
        emitter = __import__("src.services.sse_emitter", fromlist=["SSEEmitter"]).SSEEmitter()
        return SemanticKGAgent(llm=llm, qdrant=qdrant, emitter=emitter)

    def test_rules_collection_maps_to_rule(self, agent: SemanticKGAgent) -> None:
        with patch("src.pipelines.stage1.semantic_kg_agent.get_settings") as mock_settings:
            s = mock_settings.return_value
            s.collection_rules = "luma_rules"
            s.collection_specs = "luma_specs"
            s.collection_metrics = "luma_metrics"
            assert agent._doc_type_to_step_kind("luma_rules") == ReasoningStepKind.RULE

    def test_specs_collection_maps_to_spec(self, agent: SemanticKGAgent) -> None:
        with patch("src.pipelines.stage1.semantic_kg_agent.get_settings") as mock_settings:
            s = mock_settings.return_value
            s.collection_rules = "luma_rules"
            s.collection_specs = "luma_specs"
            s.collection_metrics = "luma_metrics"
            assert agent._doc_type_to_step_kind("luma_specs") == ReasoningStepKind.SPEC

    def test_metrics_collection_maps_to_spec(self, agent: SemanticKGAgent) -> None:
        """collection_metrics → "spec" (lowercase)，符合 plan F4。"""
        with patch("src.pipelines.stage1.semantic_kg_agent.get_settings") as mock_settings:
            s = mock_settings.return_value
            s.collection_rules = "luma_rules"
            s.collection_specs = "luma_specs"
            s.collection_metrics = "luma_metrics"
            result = agent._doc_type_to_step_kind("luma_metrics")
            assert result == ReasoningStepKind.SPEC
            assert result.value == "spec"

    def test_unknown_collection_maps_to_record(self, agent: SemanticKGAgent) -> None:
        """未识别的 collection 降级为 record。"""
        with patch("src.pipelines.stage1.semantic_kg_agent.get_settings") as mock_settings:
            s = mock_settings.return_value
            s.collection_rules = "luma_rules"
            s.collection_specs = "luma_specs"
            s.collection_metrics = "luma_metrics"
            assert agent._doc_type_to_step_kind("unknown") == ReasoningStepKind.RECORD
