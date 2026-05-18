"""知识图谱 Step 1 + Step 2 增量构建."""

import pymysql
from neo4j import GraphDatabase

# 配置
MYSQL_HOST = "47.105.59.151"
MYSQL_PORT = 8930
MYSQL_USER = "root"
MYSQL_PASSWORD = "Lm@Mysql#2025Root"
MYSQL_DB = "lumei"

NEO4J_URI = "bolt://localhost:7687"
NEO4J_USER = "neo4j"
NEO4J_PASSWORD = "password"


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


def main():
    mysql_conn = get_mysql_connection()
    neo4j = get_neo4j_driver()

    try:
        # Step 1: 清空 + 创建根节点 Ribbon + ProductSpec
        print("=" * 60)
        print("Step 1: 创建根节点 Ribbon + ProductSpec")
        print("=" * 60)

        with neo4j.session() as session:
            session.run("MATCH (n) DETACH DELETE n")
        print("  [1] Neo4j 已清空")

        with neo4j.session() as session:
            session.run(
                "CREATE (r:Ribbon {id: 'domain:ribbon', name: '带材', description: '检测中心业务根节点'})"
            )
        print("  [2] 根节点 Ribbon(带材) 已创建")

        with mysql_conn.cursor() as cursor:
            cursor.execute(
                """
                SELECT F_Id as id, F_CODE as code, F_NAME as name, F_DETECTION_COLUMNS as detection_columns
                FROM lab_product_spec
                WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0) AND F_ENABLEDMARK = 1
                """
            )
            specs = cursor.fetchall()
        print(f"  [3] 从 MySQL 读取到 {len(specs)} 条产品规格")

        with neo4j.session() as session:
            for spec in specs:
                session.run(
                    """
                    MATCH (r:Ribbon {id: 'domain:ribbon'})
                    CREATE (s:ProductSpec {id: $sid, code: $code, name: $name, detectionColumns: $detection_columns})
                    CREATE (r)-[:BELONGS_TO_SPEC]->(s)
                    """,
                    sid=str(spec["id"]),
                    code=spec["code"] or "",
                    name=spec["name"] or "",
                    detection_columns=str(spec["detection_columns"]) if spec["detection_columns"] is not None else "",
                )
        print(f"  [4] 已创建 {len(specs)} 个 ProductSpec 节点及 BELONGS_TO_SPEC 关系")

        # Step 2: 创建 SpecAttribute
        print()
        print("=" * 60)
        print("Step 2: 创建 SpecAttribute 扩展属性")
        print("=" * 60)

        with mysql_conn.cursor() as cursor:
            cursor.execute(
                """
                SELECT a.F_Id as id, a.F_PRODUCT_SPEC_ID as spec_id,
                       a.F_ATTRIBUTE_NAME as name, a.F_ATTRIBUTE_KEY as attr_key,
                       a.F_ATTRIBUTE_VALUE as value, a.F_VALUE_TYPE as value_type,
                       a.F_UNIT as unit
                FROM lab_product_spec_attribute a
                INNER JOIN lab_product_spec_version v
                    ON a.F_PRODUCT_SPEC_ID = v.F_PRODUCT_SPEC_ID AND a.F_VERSION = v.F_VERSION
                WHERE v.F_IS_CURRENT = 1
                  AND (a.F_DeleteMark IS NULL OR a.F_DeleteMark = 0)
                  AND (v.F_DELETE_MARK IS NULL OR v.F_DELETE_MARK = 0)
                """
            )
            attrs = cursor.fetchall()
        print(f"  [1] 从 MySQL 读取到 {len(attrs)} 条规格扩展属性")

        created = 0
        with neo4j.session() as session:
            for attr in attrs:
                if attr["spec_id"]:
                    result = session.run(
                        """
                        MATCH (s:ProductSpec {id: $spec_id})
                        CREATE (s)-[:HAS_ATTRIBUTE {
                            attrKey: $attr_key,
                            value: $value,
                            valueType: $value_type,
                            unit: $unit
                        }]->(a:SpecAttribute {
                            id: $aid,
                            name: $name,
                            attrKey: $attr_key,
                            valueType: $value_type,
                            unit: $unit
                        })
                        RETURN count(*) as c
                        """,
                        aid=str(attr["id"]),
                        spec_id=str(attr["spec_id"]),
                        name=attr["name"] or "",
                        attr_key=attr["attr_key"] or "",
                        value=attr["value"] or "",
                        value_type=attr["value_type"] or "",
                        unit=attr["unit"] or "",
                    )
                    created += result.single()["c"]
        print(f"  [2] 已创建 {created} 个 SpecAttribute 节点及 HAS_ATTRIBUTE 关系")

        # 统计
        print()
        print("=" * 60)
        print("当前图谱状态")
        print("=" * 60)
        with neo4j.session() as session:
            nodes = session.run("MATCH (n) RETURN count(n) as total").single()["total"]
            rels = session.run("MATCH ()-[r]->() RETURN count(r) as total").single()["total"]
            labels = session.run(
                "MATCH (n) RETURN labels(n)[0] as label, count(n) as count ORDER BY count DESC"
            ).data()
            types = session.run(
                "MATCH ()-[r]->() RETURN type(r) as type, count(r) as count ORDER BY count DESC"
            ).data()

        print(f"  节点总数: {nodes}")
        print(f"  关系总数: {rels}")
        print()
        print("  节点分布:")
        for l in labels:
            print(f"    {l['label']}: {l['count']}")
        print()
        print("  关系分布:")
        for t in types:
            print(f"    {t['type']}: {t['count']}")

    finally:
        mysql_conn.close()
        neo4j.close()


if __name__ == "__main__":
    main()
