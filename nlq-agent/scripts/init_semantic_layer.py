"""
语义层初始化脚本

从 MySQL 中读取判定规则、产品规格和公式定义，
向量化后写入 Qdrant，完成语义层的初始构建。

使用方式：
    python -m scripts.init_semantic_layer

注意：需要先启动 Qdrant 和 TEI 服务。
"""

from __future__ import annotations

import asyncio
import json
import logging
import sys
from pathlib import Path

# 将项目根目录加入 sys.path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from src.core.settings import get_settings
from src.services.database import DatabaseService
from src.services.embedding_client import EmbeddingClient
from src.services.qdrant_service import QdrantService

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s | %(levelname)-7s | %(message)s",
)
logger = logging.getLogger(__name__)


async def load_judgment_rules(db: DatabaseService) -> list[dict]:
    """从数据库加载判定规则。"""
    result = await db.execute_query("""
        SELECT
            jl.F_ID AS id,
            jl.F_NAME AS name,
            jl.F_CODE AS code,
            jl.F_QUALITY_STATUS AS quality_status,
            jl.F_PRIORITY AS priority,
            jl.F_DESCRIPTION AS description,
            jl.F_CONDITION AS `condition`,
            jl.F_FORMULA_NAME AS formula_name,
            jl.F_PRODUCT_SPEC_NAME AS product_spec_name,
            jl.F_PRODUCT_SPEC_ID AS product_spec_id,
            jl.F_IS_STATISTIC AS is_statistic
        FROM LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL jl
        WHERE jl.F_DELETE_MARK IS NULL OR jl.F_DELETE_MARK = 0
        ORDER BY jl.F_PRIORITY
    """)
    return result["rows"]


async def load_product_specs(db: DatabaseService) -> list[dict]:
    """从数据库加载产品规格及其属性。"""
    specs_result = await db.execute_query("""
        SELECT
            ps.F_ID AS id,
            ps.F_CODE AS code,
            ps.F_NAME AS name,
            ps.F_DESCRIPTION AS description,
            ps.F_DETECTION_COLUMNS AS detection_columns
        FROM LAB_PRODUCT_SPEC ps
        WHERE ps.F_DELETE_MARK IS NULL OR ps.F_DELETE_MARK = 0
        ORDER BY ps.F_SORTCODE
    """)

    specs = specs_result["rows"]

    # 为每个规格加载属性
    for spec in specs:
        attrs_result = await db.execute_query(f"""
            SELECT
                F_ATTRIBUTE_NAME AS name,
                F_ATTRIBUTE_KEY AS `key`,
                F_ATTRIBUTE_VALUE AS value,
                F_UNIT AS unit,
                F_VALUE_TYPE AS value_type
            FROM LAB_PRODUCT_SPEC_ATTRIBUTE
            WHERE F_PRODUCT_SPEC_ID = '{spec["id"]}'
              AND (F_DELETE_MARK IS NULL OR F_DELETE_MARK = 0)
            ORDER BY F_SORTCODE
        """)
        spec["attributes"] = attrs_result["rows"]

    return specs


async def load_formulas(db: DatabaseService) -> list[dict]:
    """从数据库加载公式定义。"""
    result = await db.execute_query("""
        SELECT
            F_ID AS id,
            F_COLUMN_NAME AS column_name,
            F_FORMULA_NAME AS formula_name,
            F_FORMULA AS formula,
            F_FORMULA_TYPE AS formula_type,
            F_UNIT_NAME AS unit_name,
            F_REMARK AS remark,
            F_IS_ENABLED AS is_enabled
        FROM LAB_INTERMEDIATE_DATA_FORMULA
        WHERE F_IS_ENABLED = 1
          AND (F_DELETE_MARK IS NULL OR F_DELETE_MARK = 0)
        ORDER BY F_SORT_ORDER
    """)
    return result["rows"]


def format_rule_document(rule: dict) -> dict:
    """将判定规则格式化为 Qdrant 文档。"""
    quality_map = {0: "合格", 1: "不合格", 2: "其他"}
    quality_text = quality_map.get(rule.get("quality_status"), "未知")

    condition_text = rule.get("condition", "")
    if condition_text:
        try:
            # 尝试解析 JSON 格式的条件
            cond = json.loads(condition_text)
            condition_text = json.dumps(cond, ensure_ascii=False, indent=2)
        except (json.JSONDecodeError, TypeError):
            pass

    text = (
        f"判定等级: {rule.get('name', '')}\n"
        f"等级代码: {rule.get('code', '')}\n"
        f"质量状态: {quality_text}\n"
        f"所属公式: {rule.get('formula_name', '')}\n"
        f"产品规格: {rule.get('product_spec_name', '通用')}\n"
        f"优先级: {rule.get('priority', 0)}\n"
        f"判定条件:\n{condition_text}\n"
        f"业务说明: {rule.get('description', '')}"
    )

    return {
        "id": rule["id"],
        "text": text,
        "metadata": {
            "type": "judgment_rule",
            "name": rule.get("name", ""),
            "code": rule.get("code", ""),
            "quality_status": quality_text,
            "spec_name": rule.get("product_spec_name", ""),
        },
    }


def format_spec_document(spec: dict) -> dict:
    """将产品规格格式化为 Qdrant 文档。"""
    attrs_text = ""
    for attr in spec.get("attributes", []):
        attrs_text += (
            f"  - {attr.get('name', '')}: "
            f"{attr.get('value', '')} {attr.get('unit', '')}\n"
        )

    text = (
        f"产品规格: {spec.get('name', '')} (代码: {spec.get('code', '')})\n"
        f"有效检测列数: {spec.get('detection_columns', '')}\n"
        f"描述: {spec.get('description', '')}\n"
        f"规格属性:\n{attrs_text}"
    )

    return {
        "id": spec["id"],
        "text": text,
        "metadata": {
            "type": "product_spec",
            "code": spec.get("code", ""),
            "name": spec.get("name", ""),
        },
    }


def format_metric_document(formula: dict) -> dict:
    """将公式定义格式化为指标文档。"""
    formula_type = "计算公式" if formula.get("formula_type") == "CALC" else "判定公式"

    text = (
        f"指标名称: {formula.get('formula_name', '')}\n"
        f"类型: {formula_type}\n"
        f"对应列: {formula.get('column_name', '')}\n"
        f"计算公式: {formula.get('formula', '')}\n"
        f"单位: {formula.get('unit_name', '')}\n"
        f"备注: {formula.get('remark', '')}"
    )

    return {
        "id": formula["id"],
        "text": text,
        "metadata": {
            "type": "metric_formula",
            "column_name": formula.get("column_name", ""),
            "formula_type": formula.get("formula_type", ""),
        },
    }


# ── 预定义的业务指标（不在数据库中，手动维护）─────────────────
PREDEFINED_METRICS = [
    {
        "id": "metric_qualified_rate",
        "text": (
            "指标名称: 合格率\n"
            "定义: 同时满足磁性能合格、厚度合格、叠片系数合格的产品数量占总数量的百分比\n"
            "计算方式: COUNT(磁性能合格 AND 厚度合格 AND 叠片系数合格) / COUNT(*) * 100\n"
            "相关列: F_MAGNETIC_RES, F_THICK_RES, F_LAM_FACTOR_RES\n"
            "合格判定: 三项均为'合格'时视为合格产品\n"
            "常用维度: 按日期(F_PROD_DATE)、班次(F_SHIFT)、产线(F_LINE_NO)、产品规格(F_PRODUCT_SPEC_CODE)"
        ),
        "metadata": {"type": "predefined_metric", "name": "合格率"},
    },
    {
        "id": "metric_production_volume",
        "text": (
            "指标名称: 产量\n"
            "定义: 单卷重量之和，反映总生产量\n"
            "计算方式: SUM(F_SINGLE_COIL_WEIGHT)\n"
            "单位: kg\n"
            "常用维度: 按日期、班次、产线"
        ),
        "metadata": {"type": "predefined_metric", "name": "产量"},
    },
    {
        "id": "metric_iron_loss",
        "text": (
            "指标名称: 铁损 / Ps铁损\n"
            "定义: 1.35T 50Hz 条件下的 Ps 铁损值，是衡量硅钢片磁性能的核心指标\n"
            "列名: F_PERF_PS_LOSS\n"
            "单位: W/kg\n"
            "越低越好，A类产品要求铁损低于特定阈值\n"
            "刻痕后铁损: F_AFTER_PS_LOSS"
        ),
        "metadata": {"type": "predefined_metric", "name": "铁损"},
    },
    {
        "id": "metric_lamination_factor",
        "text": (
            "指标名称: 叠片系数\n"
            "定义: 反映硅钢片叠装后的空间利用率\n"
            "列名: F_LAMINATION_FACTOR\n"
            "单位: %\n"
            "判定列: F_LAM_FACTOR_RES（合格/不合格）\n"
            "一般要求 ≥ 0.97 (97%)"
        ),
        "metadata": {"type": "predefined_metric", "name": "叠片系数"},
    },
    {
        "id": "metric_thickness",
        "text": (
            "指标名称: 平均厚度\n"
            "定义: 带材多点测量的平均厚度值\n"
            "列名: F_AVG_THICKNESS\n"
            "单位: μm\n"
            "判定列: F_THICK_RES（合格/不合格）\n"
            "厚度分布列: F_THICK_1 ~ F_THICK_22"
        ),
        "metadata": {"type": "predefined_metric", "name": "平均厚度"},
    },
]


async def main() -> None:
    """主函数：执行语义层初始化。"""
    settings = get_settings()

    logger.info("=" * 60)
    logger.info("语义层初始化开始")
    logger.info("=" * 60)

    # 初始化服务
    embedding = EmbeddingClient()
    qdrant = QdrantService(embedding)
    db = DatabaseService()

    try:
        await db.init_pool()
        await qdrant.ensure_collections()

        # ── 1. 加载并写入判定规则 ────────────────────────────
        logger.info("正在加载判定规则...")
        rules = await load_judgment_rules(db)
        logger.info("从数据库加载了 %d 条判定规则", len(rules))

        rule_docs = [format_rule_document(r) for r in rules]
        count = await qdrant.upsert_documents(
            settings.collection_rules, rule_docs
        )
        logger.info("已写入 %d 条规则到 %s", count, settings.collection_rules)

        # ── 2. 加载并写入产品规格 ────────────────────────────
        logger.info("正在加载产品规格...")
        specs = await load_product_specs(db)
        logger.info("从数据库加载了 %d 个产品规格", len(specs))

        spec_docs = [format_spec_document(s) for s in specs]
        count = await qdrant.upsert_documents(
            settings.collection_specs, spec_docs
        )
        logger.info("已写入 %d 条规格到 %s", count, settings.collection_specs)

        # ── 3. 加载并写入公式/指标定义 ───────────────────────
        logger.info("正在加载公式定义...")
        formulas = await load_formulas(db)
        logger.info("从数据库加载了 %d 条公式", len(formulas))

        metric_docs = [format_metric_document(f) for f in formulas]
        # 追加预定义指标
        metric_docs.extend(PREDEFINED_METRICS)

        count = await qdrant.upsert_documents(
            settings.collection_metrics, metric_docs
        )
        logger.info("已写入 %d 条指标到 %s", count, settings.collection_metrics)

        # ── 完成 ─────────────────────────────────────────────
        logger.info("=" * 60)
        logger.info("语义层初始化完成！")
        logger.info("  规则: %d 条", len(rule_docs))
        logger.info("  规格: %d 条", len(spec_docs))
        logger.info("  指标: %d 条", len(metric_docs))
        logger.info("=" * 60)

    finally:
        await embedding.close()
        await qdrant.close()
        await db.close()


if __name__ == "__main__":
    asyncio.run(main())
