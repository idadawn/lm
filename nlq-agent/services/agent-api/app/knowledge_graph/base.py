"""知识图谱基础接口.

定义知识图谱的抽象接口，支持多种后端实现（NetworkX、Neo4j）。
"""

from abc import ABC, abstractmethod
from typing import Any


class BaseKnowledgeGraph(ABC):
    """知识图谱抽象基类."""

    @abstractmethod
    async def build(self) -> None:
        """从数据库构建知识图谱."""
        pass

    @abstractmethod
    async def refresh(self) -> None:
        """刷新知识图谱数据."""
        pass

    @abstractmethod
    def get_node(self, node_id: str) -> dict[str, Any] | None:
        """获取节点信息.

        Args:
            node_id: 节点ID

        Returns:
            节点属性字典，不存在则返回None
        """
        pass

    @abstractmethod
    def get_neighbors(
        self, node_id: str, relation_type: str | None = None, direction: str = "both"
    ) -> list[dict[str, Any]]:
        """获取相邻节点.

        Args:
            node_id: 节点ID
            relation_type: 关系类型过滤，None表示不过滤
            direction: 方向 "out"|"in"|"both"

        Returns:
            相邻节点列表
        """
        pass

    @abstractmethod
    def query(self, query: str, **params: Any) -> list[dict[str, Any]]:
        """执行查询.

        Args:
            query: 查询语句（Cypher或自定义）
            **params: 查询参数

        Returns:
            查询结果列表
        """
        pass

    @abstractmethod
    def get_specs(self) -> list[dict[str, Any]]:
        """获取所有产品规格."""
        pass

    @abstractmethod
    def get_metrics(self) -> list[dict[str, Any]]:
        """获取所有指标."""
        pass

    @abstractmethod
    def get_judgment_rules(self, spec_id: str | None = None) -> list[dict[str, Any]]:
        """获取判定规则.

        Args:
            spec_id: 规格ID过滤，None表示所有
        """
        pass
