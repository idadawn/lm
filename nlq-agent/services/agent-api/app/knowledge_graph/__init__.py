"""知识图谱模块.

基于 lab_ 表构建元数据知识图谱，支持：
- 产品规格及其扩展属性
- 指标公式定义
- 判定规则及其与规格的关联
- 报表配置

使用 Neo4j 作为后端存储。
"""

from app.knowledge_graph.base import BaseKnowledgeGraph
from app.knowledge_graph.manager import (
    close_knowledge_graph,
    get_knowledge_graph,
    init_knowledge_graph,
    is_knowledge_graph_ready,
    refresh_knowledge_graph,
)
from app.knowledge_graph.queries import (
    find_rules_by_condition,
    get_all_specs_with_attributes,
    get_first_inspection_config,
    get_judgment_types_for_spec,
    get_metric_formulas,
    get_related_metrics_by_spec,
    get_spec_attributes,
    get_spec_judgment_rules,
)

__all__ = [
    "BaseKnowledgeGraph",
    "get_knowledge_graph",
    "init_knowledge_graph",
    "close_knowledge_graph",
    "refresh_knowledge_graph",
    "is_knowledge_graph_ready",
    "get_spec_judgment_rules",
    "get_metric_formulas",
    "get_related_metrics_by_spec",
    "get_judgment_types_for_spec",
    "get_first_inspection_config",
    "get_spec_attributes",
    "get_all_specs_with_attributes",
    "find_rules_by_condition",
]
