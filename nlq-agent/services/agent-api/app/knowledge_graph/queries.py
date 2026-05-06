"""知识图谱查询模块.

提供常用的知识图谱查询功能.
"""

from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from app.knowledge_graph.neo4j_graph import Neo4jKnowledgeGraph


async def get_spec_judgment_rules(graph: "Neo4jKnowledgeGraph", spec_code: str) -> list[dict]:
    """获取产品规格的判定规则.

    Args:
        graph: 知识图谱实例
        spec_code: 产品规格代码（如 "120"）

    Returns:
        判定规则列表，包含关联的指标信息
    """
    query = """
        MATCH (s:ProductSpec {code: $spec_code})-[:HAS_RULE]->(r:JudgmentRule)
        OPTIONAL MATCH (m:Metric)
        WHERE m.columnName = r.formulaId OR m.name = r.formulaId
        RETURN r {
            .id, .formulaId, .name, .priority,
            .qualityStatus, .color, .isDefault
        } as rule,
        m {.id, .name, .columnName, .formulaType} as metric
        ORDER BY r.priority ASC
    """
    results = await graph.query_async(query, spec_code=spec_code)
    return [
        {**r["rule"], "metric": r["metric"] if r["metric"] and r["metric"].get("id") else None}
        for r in results
    ]


async def get_metric_formulas(
    graph: "Neo4jKnowledgeGraph", metric_name: str | None = None
) -> list[dict]:
    """获取指标公式定义.

    Args:
        graph: 知识图谱实例
        metric_name: 指标名称过滤，None表示所有

    Returns:
        指标公式列表
    """
    if metric_name:
        query = """
            MATCH (m:Metric)
            WHERE m.name = $metric_name OR m.columnName = $metric_name
            RETURN m {
                .id, .name, .columnName, .formula,
                .unit, .formulaType, .description
            } as metric
        """
        results = await graph.query_async(query, metric_name=metric_name)
    else:
        query = """
            MATCH (m:Metric)
            RETURN m {
                .id, .name, .columnName, .formula,
                .unit, .formulaType, .description
            } as metric
            ORDER BY m.name
        """
        results = await graph.query_async(query)

    return [r["metric"] for r in results]


async def get_related_metrics_by_spec(graph: "Neo4jKnowledgeGraph", spec_code: str) -> list[dict]:
    """获取产品规格相关的所有指标.

    通过判定规则关联查询该规格涉及的所有指标.

    Args:
        graph: 知识图谱实例
        spec_code: 产品规格代码

    Returns:
        指标列表
    """
    query = """
        MATCH (s:ProductSpec {code: $spec_code})-[:HAS_RULE]->(r:JudgmentRule)
              -[:EVALUATES]->(m:Metric)
        RETURN DISTINCT m {
            .id, .name, .columnName, .formula, .unit
        } as metric
        ORDER BY m.name
    """
    results = await graph.query_async(query, spec_code=spec_code)
    return [r["metric"] for r in results]


async def get_judgment_types_for_spec(graph: "Neo4jKnowledgeGraph", spec_code: str) -> list[dict]:
    """获取产品规格支持的判定类型.

    Args:
        graph: 知识图谱实例
        spec_code: 产品规格代码

    Returns:
        判定类型列表
    """
    query = """
        MATCH (s:ProductSpec {code: $spec_code})-[:HAS_RULE]->(r:JudgmentRule)
        OPTIONAL MATCH (m:Metric)
        WHERE m.columnName = r.formulaId OR m.name = r.formulaId
        RETURN DISTINCT r.formulaId as formula_id,
               count(r) as rule_count,
               m {.id, .name, .columnName} as metric
        ORDER BY rule_count DESC
    """
    results = await graph.query_async(query, spec_code=spec_code)

    name_mapping = {
        "Labeling": "贴标",
        "MagneticResult": "磁性能判定",
        "LaminationResult": "叠片系数判定",
        "ThicknessResult": "厚度判定",
        "FirstInspection": "一次交检",
    }

    return [
        {
            "formula_id": r["formula_id"],
            "name": r["metric"]["name"]
            if r["metric"] and r["metric"].get("name")
            else name_mapping.get(r["formula_id"], r["formula_id"]),
            "rule_count": r["rule_count"],
            "metric": r["metric"] if r["metric"] and r["metric"].get("id") else None,
        }
        for r in results
    ]


async def get_first_inspection_config(graph: "Neo4jKnowledgeGraph") -> dict:
    """获取一次交检合格率配置.

    Args:
        graph: 知识图谱实例

    Returns:
        配置信息，包含合格等级列表
    """
    query = """
        MATCH (c:ReportConfig)
        WHERE c.code = 'FirstInspectionPassGrades'
           OR c.name CONTAINS '一次交检'
        RETURN c {.id, .code, .name, .value, .description} as config
        ORDER BY c.id DESC
        LIMIT 1
    """
    results = await graph.query_async(query)

    if not results:
        return {"grades": ["A"], "description": "默认配置"}

    config = results[0]["config"]
    value = config.get("value", "")

    # 解析等级列表
    import json

    grades = []
    if value:
        try:
            parsed = json.loads(value)
            if isinstance(parsed, list):
                grades = parsed
            elif isinstance(parsed, str):
                grades = [g.strip() for g in parsed.split(",")]
        except json.JSONDecodeError:
            grades = [g.strip() for g in value.split(",")]

    return {
        "grades": grades or ["A"],
        "description": config.get("description", ""),
    }


async def get_spec_attributes(graph: "Neo4jKnowledgeGraph", spec_code: str) -> list[dict]:
    """获取产品规格的扩展属性.

    Args:
        graph: 知识图谱实例
        spec_code: 产品规格代码

    Returns:
        属性列表，包含名称和值
    """
    query = """
        MATCH (s:ProductSpec {code: $spec_code})-[h:HAS_ATTRIBUTE]->(a:SpecAttribute)
        RETURN a.name as name, h.value as value, a.valueType as data_type, a.unit as unit, a.version as version
    """
    results = await graph.query_async(query, spec_code=spec_code)
    return [
        {
            "name": r["name"],
            "value": r["value"],
            "data_type": r["data_type"],
            "unit": r.get("unit"),
            "version": r.get("version"),
        }
        for r in results
    ]


async def get_all_specs_with_attributes(graph: "Neo4jKnowledgeGraph") -> list[dict]:
    """获取所有产品规格及其扩展属性.

    Args:
        graph: 知识图谱实例

    Returns:
        规格列表，每个规格包含属性
    """
    # 简化查询，暂时不包含属性（因为属性构建被禁用）
    query = """
        MATCH (s:ProductSpec)
        RETURN s {.id, .code, .name} as spec
        ORDER BY s.code
    """
    results = await graph.query_async(query)

    return [
        {
            **r["spec"],
            "attributes": r.get("attrs", []),
        }
        for r in results
    ]


async def find_rules_by_condition(
    graph: "Neo4jKnowledgeGraph", condition_keyword: str
) -> list[dict]:
    """根据条件关键词查找判定规则.

    例如查找所有涉及"带厚"条件的规则.

    Args:
        graph: 知识图谱实例
        condition_keyword: 条件关键词

    Returns:
        规则列表
    """
    query = """
        MATCH (r:JudgmentRule)
        WHERE r.conditionJson CONTAINS $keyword
        OPTIONAL MATCH (s:ProductSpec)-[:HAS_RULE]->(r)
        RETURN r {
            .id, .formulaId, .name, .priority, .conditionJson
        } as rule,
        s {.code, .name} as spec
    """
    results = await graph.query_async(query, keyword=condition_keyword)

    return [
        {
            **r["rule"],
            "spec_code": r["spec"]["code"] if r["spec"] else None,
            "spec_name": r["spec"]["name"] if r["spec"] else None,
        }
        for r in results
    ]


async def get_appearance_feature_levels(graph: "Neo4jKnowledgeGraph") -> list[dict]:
    """获取所有外观特性等级.

    Args:
        graph: 知识图谱实例

    Returns:
        等级列表
    """
    query = """
        MATCH (l:AppearanceFeatureLevel)
        RETURN l {.id, .name, .description} as level
        ORDER BY l.name
    """
    results = await graph.query_async(query)
    return [r["level"] for r in results]


async def get_appearance_feature_categories(graph: "Neo4jKnowledgeGraph") -> list[dict]:
    """获取所有外观特性大类.

    Args:
        graph: 知识图谱实例

    Returns:
        大类列表
    """
    query = """
        MATCH (c:AppearanceFeatureCategory)
        RETURN c {.id, .name, .description, parentId: c.parentId} as category
        ORDER BY c.name
    """
    results = await graph.query_async(query)
    return [r["category"] for r in results]


async def get_appearance_features(graph: "Neo4jKnowledgeGraph") -> list[dict]:
    """获取所有外观特性.

    包含特性名称、关键字、描述、所属大类及等级.

    Args:
        graph: 知识图谱实例

    Returns:
        特性列表
    """
    query = """
        MATCH (f:AppearanceFeature)-[:BELONGS_TO]->(c:AppearanceFeatureCategory)
        MATCH (f:AppearanceFeature)-[:HAS_LEVEL]->(l:AppearanceFeatureLevel)
        RETURN f {
            .id, .name, .keywords, .description,
            category: c {.id, .name},
            level: l {.id, .name}
        } as feature
        ORDER BY c.name, l.name
    """
    results = await graph.query_async(query)
    return [r["feature"] for r in results]


async def get_report_configs(graph: "Neo4jKnowledgeGraph") -> list[dict]:
    """获取所有报表配置.

    Args:
        graph: 知识图谱实例

    Returns:
        报表配置列表，包含关联的判定公式信息
    """
    query = """
        MATCH (c:ReportConfig)
        OPTIONAL MATCH (c)-[:USES_FORMULA]->(m:Metric)
        RETURN c {
            .id, .name, .description, .formulaId, .levelNames,
            .isSystem, .sortOrder, .isHeader, .isPercentage,
            .isShowInReport, .isShowRatio
        } as config,
        m {.id, .name, .columnName} as metric
        ORDER BY c.sortOrder
    """
    results = await graph.query_async(query)
    return [
        {**r["config"], "metric": r["metric"] if r["metric"] and r["metric"].get("id") else None}
        for r in results
    ]


async def get_report_config_by_formula(graph: "Neo4jKnowledgeGraph", formula_id: str) -> list[dict]:
    """根据判定公式ID获取报表配置.

    Args:
        graph: 知识图谱实例
        formula_id: 判定公式ID

    Returns:
        报表配置列表
    """
    query = """
        MATCH (c:ReportConfig)-[:USES_FORMULA]->(m:Metric)
        WHERE m.columnName = $formula_id OR m.name = $formula_id
        RETURN c {
            .id, .name, .description, .formulaId, .levelNames,
            .isSystem, .sortOrder
        } as config
        ORDER BY c.sortOrder
    """
    results = await graph.query_async(query, formula_id=formula_id)
    return [r["config"] for r in results]
