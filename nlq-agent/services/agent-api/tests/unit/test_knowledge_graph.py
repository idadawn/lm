"""知识图谱模块单元测试."""

from unittest.mock import AsyncMock, MagicMock, patch

import pytest

from app.knowledge_graph.manager import (
    close_knowledge_graph,
    get_knowledge_graph,
    init_knowledge_graph,
    is_knowledge_graph_ready,
    refresh_knowledge_graph,
)


class TestKnowledgeGraphManager:
    """Test knowledge graph manager functions."""

    @pytest.mark.asyncio
    async def test_init_knowledge_graph_disabled(self) -> None:
        """Test initialization when Neo4j is disabled."""
        with patch("app.knowledge_graph.manager.settings") as mock_settings:
            mock_settings.NEO4J_ENABLED = False

            result = await init_knowledge_graph()
            assert result is None
            assert not is_knowledge_graph_ready()

    @pytest.mark.asyncio
    async def test_init_knowledge_graph_enabled(self) -> None:
        """Test successful initialization."""
        with patch("app.knowledge_graph.manager.settings") as mock_settings:
            mock_settings.NEO4J_ENABLED = True
            mock_settings.NEO4J_URI = "bolt://localhost:7687"
            mock_settings.NEO4J_USER = "neo4j"
            mock_settings.NEO4J_PASSWORD = "password"

            # Mock Neo4jKnowledgeGraph
            mock_graph = MagicMock()
            mock_graph.build = AsyncMock()

            with patch(
                "app.knowledge_graph.neo4j_graph.Neo4jKnowledgeGraph",
                return_value=mock_graph,
            ):
                result = await init_knowledge_graph()
                assert result is not None
                mock_graph.build.assert_called_once()

    @pytest.mark.asyncio
    async def test_init_knowledge_graph_failure(self) -> None:
        """Test initialization failure handling."""
        with patch("app.knowledge_graph.manager.settings") as mock_settings:
            mock_settings.NEO4J_ENABLED = True

            with patch(
                "app.knowledge_graph.neo4j_graph.Neo4jKnowledgeGraph",
                side_effect=Exception("Connection failed"),
            ):
                result = await init_knowledge_graph()
                assert result is None
                assert not is_knowledge_graph_ready()

    @pytest.mark.asyncio
    async def test_close_knowledge_graph(self) -> None:
        """Test closing knowledge graph."""
        # First set up a mock graph
        mock_graph = MagicMock()
        mock_graph.close = AsyncMock()

        with patch("app.knowledge_graph.manager._knowledge_graph", mock_graph):
            await close_knowledge_graph()
            mock_graph.close.assert_called_once()

    @pytest.mark.asyncio
    async def test_refresh_knowledge_graph_not_initialized(self) -> None:
        """Test refresh when not initialized."""
        with patch("app.knowledge_graph.manager._knowledge_graph", None):
            result = await refresh_knowledge_graph()
            assert result is False

    @pytest.mark.asyncio
    async def test_refresh_knowledge_graph_success(self) -> None:
        """Test successful refresh."""
        mock_graph = MagicMock()
        mock_graph.refresh = AsyncMock()

        with patch("app.knowledge_graph.manager._knowledge_graph", mock_graph):
            result = await refresh_knowledge_graph()
            assert result is True
            mock_graph.refresh.assert_called_once()

    def test_get_knowledge_graph(self) -> None:
        """Test getting knowledge graph instance."""
        mock_graph = MagicMock()
        with patch("app.knowledge_graph.manager._knowledge_graph", mock_graph):
            result = get_knowledge_graph()
            assert result == mock_graph


class TestKnowledgeGraphQueries:
    """Test knowledge graph query functions."""

    @pytest.mark.asyncio
    async def test_get_spec_judgment_rules(self) -> None:
        """Test getting judgment rules for a spec."""
        from app.knowledge_graph.queries import get_spec_judgment_rules

        mock_graph = MagicMock()
        mock_graph.query_async = AsyncMock(
            return_value=[
                {"rule": {"id": "1", "name": "一级品", "formulaId": "Labeling"}},
                {"rule": {"id": "2", "name": "二级品", "formulaId": "Labeling"}},
            ]
        )

        result = await get_spec_judgment_rules(mock_graph, "120")

        assert len(result) == 2
        assert result[0]["name"] == "一级品"
        mock_graph.query_async.assert_called_once()

    @pytest.mark.asyncio
    async def test_get_metric_formulas(self) -> None:
        """Test getting metric formulas."""
        from app.knowledge_graph.queries import get_metric_formulas

        mock_graph = MagicMock()
        mock_graph.query_async = AsyncMock(
            return_value=[
                {"metric": {"id": "1", "name": "PsIronLoss", "unit": "W/kg"}},
            ]
        )

        result = await get_metric_formulas(mock_graph, "PsIronLoss")

        assert len(result) == 1
        assert result[0]["name"] == "PsIronLoss"

    @pytest.mark.asyncio
    async def test_get_first_inspection_config(self) -> None:
        """Test getting first inspection config."""
        from app.knowledge_graph.queries import get_first_inspection_config

        mock_graph = MagicMock()
        mock_graph.query_async = AsyncMock(
            return_value=[
                {"config": {"value": '["A", "B"]', "description": "Test config"}},
            ]
        )

        result = await get_first_inspection_config(mock_graph)

        assert result["grades"] == ["A", "B"]
        assert result["description"] == "Test config"

    @pytest.mark.asyncio
    async def test_get_first_inspection_config_default(self) -> None:
        """Test getting default config when not found."""
        from app.knowledge_graph.queries import get_first_inspection_config

        mock_graph = MagicMock()
        mock_graph.query_async = AsyncMock(return_value=[])

        result = await get_first_inspection_config(mock_graph)

        assert result["grades"] == ["A"]
        assert "默认" in result["description"]

    @pytest.mark.asyncio
    async def test_get_all_specs_with_attributes(self) -> None:
        """Test getting all specs with attributes."""
        from app.knowledge_graph.queries import get_all_specs_with_attributes

        mock_graph = MagicMock()
        mock_graph.query_async = AsyncMock(
            return_value=[
                {
                    "spec": {"id": "1", "code": "120", "name": "120规格"},
                    "attrs": [{"name": "长度", "value": "1000", "dataType": "float"}],
                },
            ]
        )

        result = await get_all_specs_with_attributes(mock_graph)

        assert len(result) == 1
        assert result[0]["code"] == "120"
        assert len(result[0]["attributes"]) == 1


class TestKgApi:
    """Test knowledge graph API endpoints."""

    @pytest.mark.asyncio
    async def test_health_endpoint(self) -> None:
        """Test health check endpoint."""
        from app.api.kg import knowledge_graph_health

        with patch("app.api.kg.is_knowledge_graph_ready", return_value=True):
            result = await knowledge_graph_health()
            assert result["ready"] is True
            assert result["backend"] == "neo4j"

    @pytest.mark.asyncio
    async def test_refresh_endpoint_success(self) -> None:
        """Test refresh endpoint success."""
        from app.api.kg import refresh_graph

        with patch("app.api.kg.refresh_knowledge_graph", return_value=True):
            result = await refresh_graph()
            assert "message" in result

    @pytest.mark.asyncio
    async def test_refresh_endpoint_failure(self) -> None:
        """Test refresh endpoint failure."""
        from app.api.kg import refresh_graph

        with patch("app.api.kg.refresh_knowledge_graph", return_value=False):
            with pytest.raises(Exception) as exc_info:
                await refresh_graph()
            assert "503" in str(exc_info.value)

    @pytest.mark.asyncio
    async def test_get_all_specs(self) -> None:
        """Test get all specs endpoint."""
        from app.api.kg import get_all_specs

        mock_specs = [{"code": "120", "name": "120规格"}]

        with patch("app.api.kg.get_knowledge_graph") as mock_get:
            mock_graph = MagicMock()
            with patch(
                "app.api.kg.kg_queries.get_all_specs_with_attributes",
                return_value=mock_specs,
            ):
                mock_get.return_value = mock_graph
                result = await get_all_specs()
                assert len(result) == 1

    @pytest.mark.asyncio
    async def test_get_all_specs_not_available(self) -> None:
        """Test get all specs when graph not available."""
        from app.api.kg import get_all_specs

        with patch("app.api.kg.get_knowledge_graph", return_value=None):
            with pytest.raises(Exception) as exc_info:
                await get_all_specs()
            assert "503" in str(exc_info.value)

    @pytest.mark.asyncio
    async def test_get_spec_detail_success(self) -> None:
        """Test get spec detail endpoint."""
        from app.api.kg import get_spec_detail

        mock_graph = MagicMock()
        attributes = [{"name": "长度", "value": "1000", "data_type": "float"}]
        judgment_types = [{"formula_id": "Labeling", "name": "贴标", "rule_count": 2}]

        with patch("app.api.kg.get_knowledge_graph", return_value=mock_graph):
            with patch(
                "app.api.kg.kg_queries.get_spec_attributes",
                return_value=attributes,
            ):
                with patch(
                    "app.api.kg.kg_queries.get_judgment_types_for_spec",
                    return_value=judgment_types,
                ):
                    result = await get_spec_detail("120")

        assert result["code"] == "120"
        assert result["attributes"] == attributes
        assert result["judgment_types"] == judgment_types

    @pytest.mark.asyncio
    async def test_get_spec_detail_not_available(self) -> None:
        """Test get spec detail when graph is not available."""
        from app.api.kg import get_spec_detail

        with patch("app.api.kg.get_knowledge_graph", return_value=None):
            with pytest.raises(Exception) as exc_info:
                await get_spec_detail("120")

        assert "503" in str(exc_info.value)

    @pytest.mark.asyncio
    async def test_get_spec_detail_not_found(self) -> None:
        """Test get spec detail returns 404 when spec does not exist."""
        from fastapi import HTTPException

        from app.api.kg import get_spec_detail

        mock_graph = MagicMock()

        with patch("app.api.kg.get_knowledge_graph", return_value=mock_graph):
            with patch(
                "app.api.kg.kg_queries.get_spec_attributes",
                return_value=[],
            ):
                with patch(
                    "app.api.kg.kg_queries.get_judgment_types_for_spec",
                    return_value=[],
                ):
                    with pytest.raises(HTTPException) as exc_info:
                        await get_spec_detail("999")

        assert exc_info.value.status_code == 404

    @pytest.mark.asyncio
    async def test_get_spec_rules_success(self) -> None:
        """Test get spec rules endpoint."""
        from app.api.kg import get_spec_rules

        mock_graph = MagicMock()
        rules = [{"id": "1", "name": "一级品"}]

        with patch("app.api.kg.get_knowledge_graph", return_value=mock_graph):
            with patch(
                "app.api.kg.kg_queries.get_spec_judgment_rules",
                return_value=rules,
            ):
                result = await get_spec_rules("120")

        assert result == rules

    @pytest.mark.asyncio
    async def test_get_spec_rules_not_available(self) -> None:
        """Test get spec rules when graph is not available."""
        from app.api.kg import get_spec_rules

        with patch("app.api.kg.get_knowledge_graph", return_value=None):
            with pytest.raises(Exception) as exc_info:
                await get_spec_rules("120")

        assert "503" in str(exc_info.value)

    @pytest.mark.asyncio
    async def test_get_all_metrics_success(self) -> None:
        """Test get all metrics endpoint."""
        from app.api.kg import get_all_metrics

        mock_graph = MagicMock()
        metrics = [{"id": "1", "name": "PsIronLoss"}]

        with patch("app.api.kg.get_knowledge_graph", return_value=mock_graph):
            with patch(
                "app.api.kg.kg_queries.get_metric_formulas",
                return_value=metrics,
            ):
                result = await get_all_metrics()

        assert result == metrics

    @pytest.mark.asyncio
    async def test_get_all_metrics_not_available(self) -> None:
        """Test get all metrics when graph is not available."""
        from app.api.kg import get_all_metrics

        with patch("app.api.kg.get_knowledge_graph", return_value=None):
            with pytest.raises(Exception) as exc_info:
                await get_all_metrics()

        assert "503" in str(exc_info.value)

    @pytest.mark.asyncio
    async def test_get_metric_detail_success(self) -> None:
        """Test get metric detail endpoint."""
        from app.api.kg import get_metric_detail

        mock_graph = MagicMock()
        metrics = [{"id": "1", "name": "PsIronLoss", "unit": "W/kg"}]

        with patch("app.api.kg.get_knowledge_graph", return_value=mock_graph):
            with patch(
                "app.api.kg.kg_queries.get_metric_formulas",
                return_value=metrics,
            ):
                result = await get_metric_detail("PsIronLoss")

        assert result == metrics[0]

    @pytest.mark.asyncio
    async def test_get_metric_detail_not_available(self) -> None:
        """Test get metric detail when graph is not available."""
        from app.api.kg import get_metric_detail

        with patch("app.api.kg.get_knowledge_graph", return_value=None):
            with pytest.raises(Exception) as exc_info:
                await get_metric_detail("PsIronLoss")

        assert "503" in str(exc_info.value)

    @pytest.mark.asyncio
    async def test_get_metric_detail_not_found(self) -> None:
        """Test get metric detail returns 404 when metric does not exist."""
        from fastapi import HTTPException

        from app.api.kg import get_metric_detail

        mock_graph = MagicMock()

        with patch("app.api.kg.get_knowledge_graph", return_value=mock_graph):
            with patch(
                "app.api.kg.kg_queries.get_metric_formulas",
                return_value=[],
            ):
                with pytest.raises(HTTPException) as exc_info:
                    await get_metric_detail("UnknownMetric")

        assert exc_info.value.status_code == 404

    @pytest.mark.asyncio
    async def test_get_first_inspection_config_success(self) -> None:
        """Test get first inspection config endpoint."""
        from app.api.kg import get_first_inspection_config

        mock_graph = MagicMock()
        config = {"grades": ["A", "B"], "description": "Test config"}

        with patch("app.api.kg.get_knowledge_graph", return_value=mock_graph):
            with patch(
                "app.api.kg.kg_queries.get_first_inspection_config",
                return_value=config,
            ):
                result = await get_first_inspection_config()

        assert result == config

    @pytest.mark.asyncio
    async def test_get_first_inspection_config_not_available(self) -> None:
        """Test get first inspection config when graph is not available."""
        from app.api.kg import get_first_inspection_config

        with patch("app.api.kg.get_knowledge_graph", return_value=None):
            with pytest.raises(Exception) as exc_info:
                await get_first_inspection_config()

        assert "503" in str(exc_info.value)

    @pytest.mark.asyncio
    async def test_search_rules_success(self) -> None:
        """Test search rules endpoint."""
        from app.api.kg import search_rules

        mock_graph = MagicMock()
        rules = [{"id": "1", "name": "带厚一级品"}]

        with patch("app.api.kg.get_knowledge_graph", return_value=mock_graph):
            with patch(
                "app.api.kg.kg_queries.find_rules_by_condition",
                return_value=rules,
            ):
                result = await search_rules("带厚")

        assert result == rules

    @pytest.mark.asyncio
    async def test_search_rules_not_available(self) -> None:
        """Test search rules when graph is not available."""
        from app.api.kg import search_rules

        with patch("app.api.kg.get_knowledge_graph", return_value=None):
            with pytest.raises(Exception) as exc_info:
                await search_rules("带厚")

        assert "503" in str(exc_info.value)
