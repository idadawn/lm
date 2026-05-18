"""逐步重建 Neo4j 知识图谱."""

import os
import sys

# 添加项目根目录到路径
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from neo4j import GraphDatabase
import pymysql

# 配置
NEO4J_URI = "bolt://localhost:7687"
NEO4J_USER = "neo4j"
NEO4J_PASSWORD = "password"

MYSQL_HOST = "47.105.59.151"
MYSQL_PORT = 8930
MYSQL_USER = "root"
MYSQL_PASSWORD = "Lm@Mysql#2025Root"
MYSQL_DB = "lumei"


def get_mysql_connection():
    return pymysql.connect(
        host=MYSQL_HOST,
        port=MYSQL_PORT,
        user=MYSQL_USER,
        password=MYSQL_PASSWORD,
        database=MYSQL_DB,
        charset="utf8mb4",
        cursorclass=pymysql.cursors.DictCursor,
    )


def get_neo4j_driver():
    return GraphDatabase.driver(NEO4J_URI, auth=(NEO4J_USER, NEO4J_PASSWORD))


def print_step(step_num, title):
    print(f"\n{'='*60}")
    print(f"Step {step_num}: {title}")
    print("=" * 60)


def count_nodes(driver):
    with driver.session() as session:
        result = session.run("MATCH (n) RETURN count(n) as total")
        return result.single()["total"]


def count_relationships(driver):
    with driver.session() as session:
        result = session.run("MATCH ()-[r]->() RETURN count(r) as total")
        return result.single()["total"]


def print_stats(driver):
    nodes = count_nodes(driver)
    rels = count_relationships(driver)
    print(f"  [当前统计] 节点: {nodes}, 关系: {rels}")


def step1_product_specs(driver, mysql_conn):
    """Step 1: 创建 ProductSpec 节点."""
    print_step(1, "创建 ProductSpec 节点")
    with mysql_conn.cursor() as cursor:
        cursor.execute("""
            SELECT F_Id as id, F_CODE as code, F_NAME as name,
                   F_DETECTION_COLUMNS as detection_columns
            FROM lab_product_spec
            WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)
              AND F_ENABLEDMARK = 1
        """)
        rows = cursor.fetchall()
    print(f"  从 MySQL 读取到 {len(rows)} 条规格记录")
    with driver.session() as session:
        for r in rows:
            session.run("""
                CREATE (s:ProductSpec {
                    id: $id,
                    code: $code,
                    name: $name,
                    detectionColumns: $detection_columns
                })
            """, {
                "id": str(r["id"]),
                "code": str(r["code"]) if r["code"] else None,
                "name": r["name"],
                "detection_columns": r["detection_columns"],
            })
    print(f"  已创建 {len(rows)} 个 ProductSpec 节点")
    print_stats(driver)


def step2_formulas(driver, mysql_conn):
    """Step 2: 创建 Formula 节点."""
    print_step(2, "创建 Formula 节点")
    with mysql_conn.cursor() as cursor:
        cursor.execute("""
            SELECT F_Id as id, F_FORMULA_NAME as name,
                   F_COLUMN_NAME as column_name, F_FORMULA as formula,
                   F_UNIT_NAME as unit, F_FORMULA_TYPE as formula_type,
                   F_REMARK as description, F_SOURCE_TYPE as source_type
            FROM lab_intermediate_data_formula
            WHERE F_DeleteMark = 0 OR F_DeleteMark IS NULL
        """)
        rows = cursor.fetchall()
    print(f"  从 MySQL 读取到 {len(rows)} 条公式记录")
    with driver.session() as session:
        for r in rows:
            session.run("""
                CREATE (f:Formula {
                    id: $id,
                    name: $name,
                    columnName: $column_name,
                    formula: $formula,
                    unit: $unit,
                    formulaType: $formula_type,
                    description: $description,
                    sourceType: $source_type
                })
            """, {
                "id": str(r["id"]),
                "name": r["name"],
                "column_name": r["column_name"],
                "formula": r["formula"],
                "unit": r["unit"],
                "formula_type": r["formula_type"],
                "description": r["description"],
                "source_type": r["source_type"],
            })
    print(f"  已创建 {len(rows)} 个 Formula 节点")
    print_stats(driver)


def step3_judgment_rules(driver, mysql_conn):
    """Step 3: 创建 JudgmentRule 节点及关系."""
    print_step(3, "创建 JudgmentRule 节点及关系 (HAS_RULE, USES_FORMULA)")
    with mysql_conn.cursor() as cursor:
        cursor.execute("""
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
        """)
        rows = cursor.fetchall()
    print(f"  从 MySQL 读取到 {len(rows)} 条规则记录")
    
    with driver.session() as session:
        for r in rows:
            session.run("""
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
            """, {
                "id": str(r["id"]),
                "formula_id": r["formula_id"],
                "name": r["name"],
                "priority": r["priority"],
                "quality_status": r["quality_status"],
                "color": r["color"],
                "is_default": r["is_default"],
                "condition_json": r["condition_json"],
            })
    print(f"  已创建 {len(rows)} 个 JudgmentRule 节点")
    
    # 创建关系
    with driver.session() as session:
        # ProductSpec -[:HAS_RULE]-> JudgmentRule
        has_rule_count = 0
        for r in rows:
            if r["spec_code"]:
                result = session.run("""
                    MATCH (s:ProductSpec {code: $spec_code})
                    MATCH (r:JudgmentRule {id: $rule_id})
                    CREATE (s)-[:HAS_RULE]->(r)
                    RETURN count(*) as c
                """, {"spec_code": str(r["spec_code"]), "rule_id": str(r["id"])})
                has_rule_count += result.single()["c"]
        print(f"  已创建 {has_rule_count} 个 HAS_RULE 关系")
        
        # JudgmentRule -[:USES_FORMULA]-> Formula
        uses_formula_count = 0
        for r in rows:
            if r["formula_id"]:
                result = session.run("""
                    MATCH (f:Formula {id: $formula_id})
                    MATCH (r:JudgmentRule {id: $rule_id})
                    CREATE (r)-[:USES_FORMULA]->(f)
                    RETURN count(*) as c
                """, {"formula_id": str(r["formula_id"]), "rule_id": str(r["id"])})
                uses_formula_count += result.single()["c"]
        print(f"  已创建 {uses_formula_count} 个 USES_FORMULA 关系")
    print_stats(driver)


def step4_spec_attributes(driver, mysql_conn):
    """Step 4: 创建 SpecAttribute 节点及关系."""
    print_step(4, "创建 SpecAttribute 节点及关系 (HAS_ATTRIBUTE)")
    with mysql_conn.cursor() as cursor:
        cursor.execute("""
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
        """)
        rows = cursor.fetchall()
    print(f"  从 MySQL 读取到 {len(rows)} 条规格属性记录")
    
    with driver.session() as session:
        for r in rows:
            if not r["spec_id"]:
                continue
            session.run("""
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
            """, {
                "id": str(r["id"]),
                "spec_id": str(r["spec_id"]),
                "name": r["name"],
                "attr_key": r["attr_key"],
                "value": r["value"],
                "value_type": r["value_type"],
                "unit": r["unit"],
                "precision_val": r["precision_val"],
                "version": r["version"],
            })
    print(f"  已创建 {len(rows)} 个 SpecAttribute 节点及关系")
    print_stats(driver)


def step5_appearance_levels(driver, mysql_conn):
    """Step 5: 创建 AppearanceLevel 节点."""
    print_step(5, "创建 AppearanceLevel 节点")
    with mysql_conn.cursor() as cursor:
        cursor.execute("""
            SELECT F_Id as id, F_NAME as name, F_DESCRIPTION as description
            FROM lab_appearance_feature_level
            WHERE F_ENABLED = 1 AND (F_DeleteMark IS NULL OR F_DeleteMark = 0)
        """)
        rows = cursor.fetchall()
    print(f"  从 MySQL 读取到 {len(rows)} 条外观等级记录")
    
    with driver.session() as session:
        for r in rows:
            session.run("""
                CREATE (l:AppearanceLevel {
                    id: $id,
                    name: $name,
                    description: $description
                })
            """, {"id": str(r["id"]), "name": r["name"], "description": r["description"]})
    print(f"  已创建 {len(rows)} 个 AppearanceLevel 节点")
    print_stats(driver)


def step6_appearance_categories(driver, mysql_conn):
    """Step 6: 创建 AppearanceCategory 节点."""
    print_step(6, "创建 AppearanceCategory 节点")
    with mysql_conn.cursor() as cursor:
        cursor.execute("""
            SELECT F_Id as id, F_NAME as name, F_DESCRIPTION as description,
                   F_PARENTID as parent_id
            FROM lab_appearance_feature_category
            WHERE F_ENABLEDMARK = 1 AND (F_DeleteMark IS NULL OR F_DeleteMark = 0)
        """)
        rows = cursor.fetchall()
    print(f"  从 MySQL 读取到 {len(rows)} 条外观分类记录")
    
    with driver.session() as session:
        for r in rows:
            session.run("""
                CREATE (c:AppearanceCategory {
                    id: $id,
                    name: $name,
                    description: $description,
                    parentId: $parent_id
                })
            """, {
                "id": str(r["id"]),
                "name": r["name"],
                "description": r["description"],
                "parent_id": r["parent_id"],
            })
    print(f"  已创建 {len(rows)} 个 AppearanceCategory 节点")
    print_stats(driver)


def step7_appearance_features(driver, mysql_conn):
    """Step 7: 创建 AppearanceFeature 节点及关系."""
    print_step(7, "创建 AppearanceFeature 节点及关系 (BELONGS_TO_CATEGORY, HAS_LEVEL)")
    with mysql_conn.cursor() as cursor:
        cursor.execute("""
            SELECT f.F_Id as id, f.F_NAME as name, f.F_KEYWORDS as keywords,
                   f.F_DESCRIPTION as description,
                   f.F_CATEGORY_ID as category_id,
                   f.F_SEVERITY_LEVEL_ID as level_id
            FROM lab_appearance_feature f
            WHERE f.F_ENABLEDMARK = 1 AND (f.F_DeleteMark IS NULL OR f.F_DeleteMark = 0)
        """)
        rows = cursor.fetchall()
    print(f"  从 MySQL 读取到 {len(rows)} 条外观特性记录")
    
    with driver.session() as session:
        for r in rows:
            session.run("""
                MATCH (c:AppearanceCategory {id: $category_id})
                MATCH (l:AppearanceLevel {id: $level_id})
                CREATE (f:AppearanceFeature {
                    id: $id,
                    name: $name,
                    keywords: $keywords,
                    description: $description
                })
                CREATE (f)-[:BELONGS_TO_CATEGORY]->(c)
                CREATE (f)-[:HAS_LEVEL]->(l)
            """, {
                "id": str(r["id"]),
                "name": r["name"],
                "keywords": r["keywords"],
                "description": r["description"],
                "category_id": str(r["category_id"]),
                "level_id": str(r["level_id"]),
            })
    print(f"  已创建 {len(rows)} 个 AppearanceFeature 节点及关系")
    print_stats(driver)


def step8_report_configs(driver, mysql_conn):
    """Step 8: 创建 ReportConfig 节点及关系."""
    print_step(8, "创建 ReportConfig 节点及关系 (USES_FORMULA_CONFIG)")
    with mysql_conn.cursor() as cursor:
        cursor.execute("""
            SELECT F_Id as id, F_NAME as name, F_DESCRIPTION as description,
                   F_FORMULA_ID as formula_id, F_LEVEL_NAMES as level_names,
                   F_IS_SYSTEM as is_system, F_SORT_ORDER as sort_order,
                   F_IS_HEADER as is_header, F_IS_PERCENTAGE as is_percentage,
                   F_IS_SHOW_IN_REPORT as is_show_in_report, F_IS_SHOW_RATIO as is_show_ratio
            FROM lab_report_config
            WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)
              AND F_ENABLEDMARK = 1
        """)
        rows = cursor.fetchall()
    print(f"  从 MySQL 读取到 {len(rows)} 条报表配置记录")
    
    with driver.session() as session:
        for r in rows:
            session.run("""
                CREATE (c:ReportConfig {
                    id: $id,
                    code: $code,
                    name: $name,
                    value: $value,
                    description: $description
                })
            """, {
                "id": str(r["id"]),
                "code": None,
                "name": r["name"],
                "value": None,
                "description": r["description"],
            })
    print(f"  已创建 {len(rows)} 个 ReportConfig 节点")
    print_stats(driver)


def main():
    print("=" * 60)
    print("Neo4j 知识图谱逐步重建")
    print("=" * 60)
    
    driver = get_neo4j_driver()
    mysql_conn = get_mysql_connection()
    
    try:
        # 先清空
        print("\n[准备] 清空现有数据...")
        with driver.session() as session:
            session.run("MATCH (n) DETACH DELETE n")
        print("  已清空")
        print_stats(driver)
        
        # 逐步构建
        step1_product_specs(driver, mysql_conn)
        step2_formulas(driver, mysql_conn)
        step3_judgment_rules(driver, mysql_conn)
        step4_spec_attributes(driver, mysql_conn)
        step5_appearance_levels(driver, mysql_conn)
        step6_appearance_categories(driver, mysql_conn)
        step7_appearance_features(driver, mysql_conn)
        step8_report_configs(driver, mysql_conn)
        
        # 最终统计
        print("\n" + "=" * 60)
        print("重建完成!")
        print("=" * 60)
        print_stats(driver)
        
        # 检查旧类型残留
        print("\n[检查旧类型残留]")
        old_labels = ["Metric", "JudgementRule", "AppearanceFeatureCategory", 
                      "AppearanceFeatureLevel", "RuleCondition", "InspectionRecord"]
        old_rels = ["EVALUATES", "USES_RULE", "HAS_CONDITION", "USES_SPEC"]
        
        with driver.session() as session:
            for label in old_labels:
                try:
                    result = session.run(f"MATCH (n:{label}) RETURN count(n) as count")
                    count = result.single()["count"]
                    if count > 0:
                        print(f"  WARNING: {label}: {count}")
                    else:
                        print(f"  OK: {label}: 0")
                except Exception:
                    print(f"  OK: {label}: 0 (label not found)")
            
            for rel in old_rels:
                try:
                    result = session.run(f"MATCH ()-[r:{rel}]->() RETURN count(r) as count")
                    count = result.single()["count"]
                    if count > 0:
                        print(f"  WARNING: {rel}: {count}")
                    else:
                        print(f"  OK: {rel}: 0")
                except Exception:
                    print(f"  OK: {rel}: 0 (relation not found)")
    finally:
        driver.close()
        mysql_conn.close()


if __name__ == "__main__":
    main()
