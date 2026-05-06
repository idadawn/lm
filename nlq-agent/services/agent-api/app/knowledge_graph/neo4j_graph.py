"""Neo4j 知识图谱实现.

使用 Neo4j 图数据库存储和查询元数据知识图谱。
"""

from typing import Any

from neo4j import AsyncGraphDatabase

from app.core.config import settings
from app.knowledge_graph.base import BaseKnowledgeGraph
from app.tools.query_tools import execute_safe_sql


class Neo4jKnowledgeGraph(BaseKnowledgeGraph):
    """Neo4j 知识图谱实现."""

    def __init__(self) -> None:
        """初始化 Neo4j 连接."""
        uri = getattr(settings, "NEO4J_URI", "bolt://localhost:7687")
        user = getattr(settings, "NEO4J_USER", "neo4j")
        password = getattr(settings, "NEO4J_PASSWORD", "password")

        self.driver = AsyncGraphDatabase.driver(
            uri,
            auth=(user, password),
            connection_timeout=5,
        )
        self._built = False

    async def close(self) -> None:
        """关闭连接."""
        await self.driver.close()

    async def build(self) -> None:
        """从数据库构建知识图谱.

        清空现有数据，从 MySQL 重新构建。
        """
        async with self.driver.session() as session:
            # 清空现有数据
            await session.run("MATCH (n) DETACH DELETE n")

            # 1. 创建产品规格节点
            await self._build_product_specs(session)

            # 2. 创建指标节点
            await self._build_metrics(session)

            # 3. 创建判定规则节点及关系
            await self._build_judgment_rules(session)

            # 4. 创建报表配置节点（暂时禁用，表结构不匹配）
            # await self._build_report_configs(session)

            # 5. 创建规格扩展属性
            await self._build_spec_attributes(session)

            # 6. 创建外观特性节点
            await self._build_appearance_features(session)

            # 7. 创建报表配置节点
            await self._build_report_configs(session)

        self._built = True

    async def _build_product_specs(self, session: Any) -> None:
        """构建产品规格节点."""
        sql = """
            SELECT F_Id as id, F_CODE as code, F_NAME as name,
                   F_DETECTION_COLUMNS as detection_columns
            FROM lab_product_spec
            WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)
              AND F_ENABLEDMARK = 1
        """
        results = await execute_safe_sql(sql, {})

        for r in results:
            await session.run(
                """
                CREATE (s:ProductSpec {
                    id: $id,
                    code: $code,
                    name: $name,
                    detectionColumns: $detection_columns
                })
            """,
                {
                    "id": str(r["id"]),
                    "code": str(r["code"]),
                    "name": r["name"],
                    "detection_columns": r["detection_columns"],
                },
            )

    async def _build_metrics(self, session: Any) -> None:
        """构建指标节点."""
        sql = """
            SELECT F_Id as id, F_FORMULA_NAME as name,
                   F_COLUMN_NAME as column_name, F_FORMULA as formula,
                   F_UNIT_NAME as unit, F_FORMULA_TYPE as formula_type,
                   F_REMARK as description, F_SOURCE_TYPE as source_type
            FROM lab_intermediate_data_formula
            WHERE F_DeleteMark = 0 OR F_DeleteMark IS NULL
        """
        results = await execute_safe_sql(sql, {})

        for r in results:
            await session.run(
                """
                CREATE (m:Metric {
                    id: $id,
                    name: $name,
                    columnName: $column_name,
                    formula: $formula,
                    unit: $unit,
                    formulaType: $formula_type,
                    description: $description,
                    sourceType: $source_type
                })
            """,
                {
                    "id": str(r["id"]),
                    "name": r["name"],
                    "column_name": r["column_name"],
                    "formula": r["formula"],
                    "unit": r["unit"],
                    "formula_type": r["formula_type"],
                    "description": r["description"],
                    "source_type": r["source_type"],
                },
            )

    async def _build_judgment_rules(self, session: Any) -> None:
        """构建判定规则节点及关系."""
        sql = """
            SELECT j.F_Id as id, j.F_FORMULA_ID as formula_id,
                   j.F_NAME as name, j.F_PRIORITY as priority,
                   j.F_QUALITY_STATUS as quality_status,
                   j.F_COLOR as color, j.F_IS_DEFAULT as is_default,
                   j.F_CONDITION as condition_json,
                   p.F_CODE as spec_code, p.F_Id as spec_id
            FROM lab_intermediate_data_judgment_level j
            LEFT JOIN lab_product_spec p
              ON j.F_PRODUCT_SPEC_ID COLLATE utf8mb4_unicode_ci = p.F_Id COLLATE utf8mb4_unicode_ci
            WHERE j.F_DeleteMark = 0 OR j.F_DeleteMark IS NULL
        """
        results = await execute_safe_sql(sql, {})

        for r in results:
            # 创建判定规则节点
            await session.run(
                """
                CREATE (r:JudgmentRule {
                    id: $id,
                    formulaId: $formula_id,
                    name: $name,
                    priority: $priority,
                    qualityStatus: $quality_status,
                    color: $color,
                    isDefault: $is_default,
                    conditionJson: $condition_json
                })
            """,
                {
                    "id": str(r["id"]),
                    "formula_id": r["formula_id"],
                    "name": r["name"],
                    "priority": r["priority"],
                    "quality_status": r["quality_status"],
                    "color": r["color"],
                    "is_default": r["is_default"],
                    "condition_json": r["condition_json"],
                },
            )

            # 关联到产品规格
            if r["spec_code"]:
                await session.run(
                    """
                    MATCH (s:ProductSpec {code: $spec_code})
                    MATCH (r:JudgmentRule {id: $rule_id})
                    CREATE (s)-[:HAS_RULE]->(r)
                """,
                    {"spec_code": str(r["spec_code"]), "rule_id": str(r["id"])},
                )

            # 关联到指标（通过 formulaId 匹配 Metric.name）
            await session.run(
                """
                MATCH (m:Metric {name: $formula_id})
                MATCH (r:JudgmentRule {id: $rule_id})
                CREATE (r)-[:EVALUATES]->(m)
            """,
                {"formula_id": r["formula_id"], "rule_id": str(r["id"])},
            )

    async def _build_report_configs(self, session: Any) -> None:
        """构建报表配置节点."""
        sql = """
            SELECT F_Id as id, F_CODE as code, F_NAME as name,
                   F_VALUE as value, F_DESCRIPTION as description
            FROM lab_report_config
            WHERE F_DeleteMark = 0 OR F_DeleteMark IS NULL
        """
        results = await execute_safe_sql(sql, {})

        for r in results:
            await session.run(
                """
                CREATE (c:ReportConfig {
                    id: $id,
                    code: $code,
                    name: $name,
                    value: $value,
                    description: $description
                })
            """,
                {
                    "id": str(r["id"]),
                    "code": r["code"],
                    "name": r["name"],
                    "value": r["value"],
                    "description": r["description"],
                },
            )

    async def _build_spec_attributes(self, session: Any) -> None:
        """构建规格扩展属性.

        从 lab_product_spec_attribute 表获取扩展属性，通过 F_PRODUCT_SPEC_ID 关联到产品规格。
        只获取当前版本（F_IS_CURRENT = 1）的数据。
        """
        # 使用 JOIN 直接获取当前版本的属性
        sql = """
            SELECT a.F_Id as id, a.F_PRODUCT_SPEC_ID as spec_id,
                   a.F_ATTRIBUTE_NAME as name, a.F_ATTRIBUTE_KEY as attr_key,
                   a.F_ATTRIBUTE_VALUE as value, a.F_VALUE_TYPE as value_type,
                   a.F_UNIT as unit, a.F_PRECISION as precision_val,
                   a.F_VERSION as version
            FROM lab_product_spec_attribute a
            INNER JOIN lab_product_spec_version v 
                ON a.F_PRODUCT_SPEC_ID = v.F_PRODUCT_SPEC_ID 
                AND a.F_VERSION = v.F_VERSION
            WHERE v.F_IS_CURRENT = 1 
              AND (a.F_DeleteMark IS NULL OR a.F_DeleteMark = 0)
              AND (v.F_DELETE_MARK IS NULL OR v.F_DELETE_MARK = 0)
        """
        results = await execute_safe_sql(sql, {})

        for r in results:
            if not r["spec_id"]:
                continue
            await session.run(
                """
                MATCH (s:ProductSpec {id: $spec_id})
                CREATE (s)-[:HAS_ATTRIBUTE {value: $value}]->(a:SpecAttribute {
                    id: $id,
                    name: $name,
                    attrKey: $attr_key,
                    valueType: $value_type,
                    unit: $unit,
                    precision: $precision_val,
                    version: $version
                })
            """,
                {
                    "id": str(r["id"]),
                    "spec_id": str(r["spec_id"]),
                    "name": r["name"],
                    "attr_key": r["attr_key"],
                    "value": r["value"],
                    "value_type": r["value_type"],
                    "unit": r["unit"],
                    "precision_val": r["precision_val"],
                    "version": r["version"],
                },
            )

    async def _build_appearance_features(self, session: Any) -> None:
        """构建外观特性节点.

        从 lab_appearance_feature_category、lab_appearance_feature_level、lab_appearance_feature 表获取外观特性数据。
        """
        # 1. 创建外观特性等级节点
        sql_level = """
            SELECT F_Id as id, F_NAME as name, F_DESCRIPTION as description
            FROM lab_appearance_feature_level
            WHERE F_ENABLED = 1 AND (F_DeleteMark IS NULL OR F_DeleteMark = 0)
        """
        level_results = await execute_safe_sql(sql_level, {})

        for r in level_results:
            await session.run(
                """
                CREATE (l:AppearanceFeatureLevel {
                    id: $id,
                    name: $name,
                    description: $description
                })
            """,
                {"id": str(r["id"]), "name": r["name"], "description": r["description"]},
            )

        # 2. 创建外观特性大类节点
        sql_category = """
            SELECT F_Id as id, F_NAME as name, F_DESCRIPTION as description,
                   F_PARENTID as parent_id
            FROM lab_appearance_feature_category
            WHERE F_ENABLEDMARK = 1 AND (F_DeleteMark IS NULL OR F_DeleteMark = 0)
        """
        category_results = await execute_safe_sql(sql_category, {})

        for r in category_results:
            await session.run(
                """
                CREATE (c:AppearanceFeatureCategory {
                    id: $id,
                    name: $name,
                    description: $description,
                    parentId: $parent_id
                })
            """,
                {
                    "id": str(r["id"]),
                    "name": r["name"],
                    "description": r["description"],
                    "parent_id": r["parent_id"],
                },
            )

        # 3. 创建外观特性节点并关联到类别和等级
        sql_feature = """
            SELECT f.F_Id as id, f.F_NAME as name, f.F_KEYWORDS as keywords,
                   f.F_DESCRIPTION as description,
                   f.F_CATEGORY_ID as category_id,
                   f.F_SEVERITY_LEVEL_ID as level_id
            FROM lab_appearance_feature f
            WHERE f.F_ENABLEDMARK = 1 AND (f.F_DeleteMark IS NULL OR f.F_DeleteMark = 0)
        """
        feature_results = await execute_safe_sql(sql_feature, {})

        for r in feature_results:
            await session.run(
                """
                MATCH (c:AppearanceFeatureCategory {id: $category_id})
                MATCH (l:AppearanceFeatureLevel {id: $level_id})
                CREATE (f:AppearanceFeature {
                    id: $id,
                    name: $name,
                    keywords: $keywords,
                    description: $description
                })
                CREATE (f)-[:BELONGS_TO]->(c)
                CREATE (f)-[:HAS_LEVEL]->(l)
            """,
                {
                    "id": str(r["id"]),
                    "name": r["name"],
                    "keywords": r["keywords"],
                    "description": r["description"],
                    "category_id": str(r["category_id"]),
                    "level_id": str(r["level_id"]),
                },
            )

    async def _build_report_configs(self, session: Any) -> None:
        """构建报表配置节点.

        从 lab_report_config 表获取报表统计配置，关联判定公式。
        """
        sql = """
            SELECT F_Id as id, F_NAME as name, F_DESCRIPTION as description,
                   F_FORMULA_ID as formula_id, F_LEVEL_NAMES as level_names,
                   F_IS_SYSTEM as is_system, F_SORT_ORDER as sort_order,
                   F_IS_HEADER as is_header, F_IS_PERCENTAGE as is_percentage,
                   F_IS_SHOW_IN_REPORT as is_show_in_report, F_IS_SHOW_RATIO as is_show_ratio
            FROM lab_report_config
            WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)
              AND F_ENABLEDMARK = 1
        """
        results = await execute_safe_sql(sql, {})

        for r in results:
            # 解析等级名称列表
            import json

            level_names = []
            if r["level_names"]:
                try:
                    # 尝试解析JSON数组
                    level_names = json.loads(r["level_names"])
                except json.JSONDecodeError:
                    # 如果是逗号分隔的字符串
                    level_names = [x.strip() for x in r["level_names"].split(",") if x.strip()]

            # 创建报表配置节点
            await session.run(
                """
                CREATE (c:ReportConfig {
                    id: $id,
                    name: $name,
                    description: $description,
                    formulaId: $formula_id,
                    levelNames: $level_names,
                    isSystem: $is_system,
                    sortOrder: $sort_order,
                    isHeader: $is_header,
                    isPercentage: $is_percentage,
                    isShowInReport: $is_show_in_report,
                    isShowRatio: $is_show_ratio
                })
            """,
                {
                    "id": str(r["id"]),
                    "name": r["name"],
                    "description": r["description"],
                    "formula_id": r["formula_id"],
                    "level_names": level_names,
                    "is_system": r["is_system"] == 1,
                    "sort_order": r["sort_order"],
                    "is_header": r["is_header"] == 1,
                    "is_percentage": r["is_percentage"] == 1,
                    "is_show_in_report": r["is_show_in_report"] == 1,
                    "is_show_ratio": r["is_show_ratio"] == 1,
                },
            )

            # 关联到判定公式（Metric）
            if r["formula_id"]:
                await session.run(
                    """
                    MATCH (c:ReportConfig {id: $config_id})
                    MATCH (m:Metric)
                    WHERE m.columnName = $formula_id OR m.name = $formula_id
                    CREATE (c)-[:USES_FORMULA]->(m)
                """,
                    {"config_id": str(r["id"]), "formula_id": r["formula_id"]},
                )

    async def refresh(self) -> None:
        """刷新知识图谱."""
        await self.build()

    def get_node(self, node_id: str) -> dict[str, Any] | None:
        """获取节点信息."""
        # 同步方法，需要通过异步方式调用
        raise NotImplementedError("Use query() for Neo4j implementation")

    def get_neighbors(
        self, node_id: str, relation_type: str | None = None, direction: str = "both"
    ) -> list[dict[str, Any]]:
        """获取相邻节点."""
        raise NotImplementedError("Use query() for Neo4j implementation")

    def query(self, query: str, **params: Any) -> list[dict[str, Any]]:
        """执行 Cypher 查询."""
        # 返回协程，需要 await
        raise NotImplementedError("Use query_async() for Neo4j implementation")

    async def query_async(self, query: str, **params: Any) -> list[dict[str, Any]]:
        """异步执行 Cypher 查询."""
        async with self.driver.session() as session:
            result = await session.run(query, **params)
            records = []
            async for record in result:
                records.append(dict(record))
            return records

    def get_specs(self) -> list[dict[str, Any]]:
        """获取所有产品规格."""
        raise NotImplementedError("Use query_async()")

    def get_metrics(self) -> list[dict[str, Any]]:
        """获取所有指标."""
        raise NotImplementedError("Use query_async()")

    def get_judgment_rules(self, spec_id: str | None = None) -> list[dict[str, Any]]:
        """获取判定规则."""
        raise NotImplementedError("Use query_async()")
