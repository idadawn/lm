"""
路美业务数据库 DDL 定义

本文件集中管理所有需要暴露给 LLM 的数据库表结构。
Stage 2 的 SQL Agent 会将这些 DDL 注入到 prompt 中，
帮助 LLM 理解表结构并生成正确的 SQL。

注意：
- 只包含 LLM 需要知道的核心列，省略审计列（创建人、修改人等）
- 每个列都附带中文注释，帮助 LLM 理解业务含义
- 使用 triple-quote 原始字符串，方便直接注入 prompt
"""

# ── 中间数据表（核心检测数据表）──────────────────────────────
DDL_INTERMEDIATE_DATA = """
CREATE TABLE LAB_INTERMEDIATE_DATA (
    F_ID                    VARCHAR(50)     PRIMARY KEY     COMMENT '主键ID',
    F_RAW_DATA_ID           VARCHAR(50)                     COMMENT '原始数据ID',
    F_DETECTION_DATE        DATE                            COMMENT '检测日期',
    F_PROD_DATE             DATETIME                        COMMENT '生产日期',

    -- 基础信息
    F_FURNACE_NO            VARCHAR(100)                    COMMENT '原始炉号',
    F_LINE_NO               INT                             COMMENT '产线编号（从炉号解析）',
    F_SHIFT                 VARCHAR(10)                     COMMENT '班次（甲/乙/丙）',
    F_SHIFT_NUMERIC         INT                             COMMENT '班次数字（甲=1,乙=2,丙=3）',
    F_FURNACE_BATCH_NO      INT                             COMMENT '炉次号',
    F_COIL_NO               DECIMAL(10,2)                   COMMENT '卷号',
    F_SUBCOIL_NO            DECIMAL(10,2)                   COMMENT '分卷号',
    F_SPRAY_NO              VARCHAR(50)                     COMMENT '喷次（8位日期-炉号）',
    F_LABELING              VARCHAR(50)                     COMMENT '贴标',
    F_FURNACE_NO_FORMATTED  VARCHAR(200)                    COMMENT '格式化炉号',
    F_SHIFT_NO              VARCHAR(20)                     COMMENT '班次编号',

    -- 性能数据
    F_PERF_SS_POWER         DECIMAL(18,6)                   COMMENT '1.35T 50Hz Ss激磁功率(VA/kg)',
    F_PERF_PS_LOSS          DECIMAL(18,6)                   COMMENT '1.35T 50Hz Ps铁损(W/kg) — 核心质量指标',
    F_PERF_HC               DECIMAL(18,6)                   COMMENT '1.35T 50Hz Hc(A/m)',
    F_AFTER_SS_POWER        DECIMAL(18,6)                   COMMENT '刻痕后Ss激磁功率(VA/kg)',
    F_AFTER_PS_LOSS         DECIMAL(18,6)                   COMMENT '刻痕后Ps铁损(W/kg)',
    F_AFTER_HC              DECIMAL(18,6)                   COMMENT '刻痕后Hc(A/m)',
    F_IS_SCRATCHED          INT                             COMMENT '是否刻痕（0-否,1-是）',

    -- 重量与尺寸
    F_ONE_METER_WT          DECIMAL(18,6)                   COMMENT '一米带材重量(g)',
    F_WIDTH                 DECIMAL(18,6)                   COMMENT '带宽(mm)',
    F_AVG_THICKNESS         DECIMAL(18,6)                   COMMENT '平均厚度(μm)',
    F_COIL_WEIGHT           DECIMAL(18,6)                   COMMENT '四米带材重量(g)',
    F_SINGLE_COIL_WEIGHT    DECIMAL(18,6)                   COMMENT '单卷重量(kg)',

    -- 密度与叠片
    F_PRODUCT_DENSITY       DECIMAL(18,6)                   COMMENT '产品密度',
    F_DENSITY               DECIMAL(18,6)                   COMMENT '密度(g/cm³)',
    F_LAMINATION_FACTOR     DECIMAL(18,6)                   COMMENT '叠片系数(%)',

    -- 判定结果
    F_MAGNETIC_RES          VARCHAR(50)                     COMMENT '磁性能判定结果',
    F_THICK_RES             VARCHAR(50)                     COMMENT '厚度判定结果',
    F_LAM_FACTOR_RES        VARCHAR(50)                     COMMENT '叠片系数判定结果',
    F_FIRST_INSPECTION      VARCHAR(50)                     COMMENT '一次交检结果',

    -- 产品规格关联
    F_PRODUCT_SPEC_ID       VARCHAR(50)                     COMMENT '产品规格ID',
    F_PRODUCT_SPEC_CODE     VARCHAR(50)                     COMMENT '产品规格代码（如120/142/170/213）',
    F_PRODUCT_SPEC_NAME     VARCHAR(100)                    COMMENT '产品规格名称',
    F_PRODUCT_SPEC_VERSION  VARCHAR(50)                     COMMENT '产品规格版本',

    -- 计算状态
    F_CALC_STATUS           VARCHAR(20)     DEFAULT 'PENDING'   COMMENT '公式计算状态(PENDING/PROCESSING/SUCCESS/FAILED)',
    F_JUDGE_STATUS          VARCHAR(20)     DEFAULT 'PENDING'   COMMENT '判定状态(PENDING/PROCESSING/SUCCESS/FAILED)',

    -- 带厚分布（动态列 1~22）
    F_THICK_1               DECIMAL(18,6)                   COMMENT '带厚1(μm)',
    F_THICK_2               DECIMAL(18,6)                   COMMENT '带厚2(μm)',
    -- ... F_THICK_3 ~ F_THICK_22 结构相同，省略

    -- 检测数据列（动态列 1~22）
    F_DETECTION_1           DECIMAL(18,6)                   COMMENT '检测数据列1',
    F_DETECTION_2           DECIMAL(18,6)                   COMMENT '检测数据列2',
    -- ... F_DETECTION_3 ~ F_DETECTION_22 结构相同，省略

    F_DETECTION_COLUMNS     INT                             COMMENT '有效检测列数',

    -- 审计字段（省略：F_CREATOR_USER_ID, F_CREATOR_TIME, F_LAST_MODIFY_USER_ID, F_LAST_MODIFY_TIME, F_DELETE_MARK, F_ENABLED_MARK, F_TENANT_ID）

    INDEX idx_prod_date (F_PROD_DATE),
    INDEX idx_spec_code (F_PRODUCT_SPEC_CODE),
    INDEX idx_line_shift (F_LINE_NO, F_SHIFT),
    INDEX idx_detection_date (F_DETECTION_DATE)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='中间数据表 — 检测数据核心表';
"""

# ── 判定等级表 ────────────────────────────────────────────────
DDL_JUDGMENT_LEVEL = """
CREATE TABLE LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL (
    F_ID                    VARCHAR(50)     PRIMARY KEY     COMMENT '主键ID',
    F_FORMULA_ID            VARCHAR(50)     NOT NULL        COMMENT '判定公式ID',
    F_FORMULA_NAME          VARCHAR(100)                    COMMENT '判定公式名称',
    F_PRODUCT_SPEC_ID       VARCHAR(50)                     COMMENT '产品规格ID',
    F_PRODUCT_SPEC_NAME     VARCHAR(100)                    COMMENT '产品规格名称',
    F_CODE                  VARCHAR(50)     NOT NULL        COMMENT '等级代码（唯一）',
    F_NAME                  VARCHAR(100)    NOT NULL        COMMENT '等级名称（如：A类、B类、不合格）',
    F_QUALITY_STATUS        INT                             COMMENT '质量状态（0-合格,1-不合格,2-其他）',
    F_PRIORITY              INT                             COMMENT '判定权重/优先级',
    F_COLOR                 VARCHAR(20)                     COMMENT '展示颜色',
    F_IS_STATISTIC          TINYINT(1)      DEFAULT 1       COMMENT '是否参与统计',
    F_IS_DEFAULT            TINYINT(1)      DEFAULT 0       COMMENT '是否默认兜底判定',
    F_DESCRIPTION           VARCHAR(500)                    COMMENT '业务说明',
    F_CONDITION             VARCHAR(4000)                   COMMENT '判定条件（JSON格式）'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='判定等级表 — 定义A类/B类/不合格的判定规则';
"""

# ── 产品规格表 ────────────────────────────────────────────────
DDL_PRODUCT_SPEC = """
CREATE TABLE LAB_PRODUCT_SPEC (
    F_ID                    VARCHAR(50)     PRIMARY KEY     COMMENT '主键ID',
    F_CODE                  VARCHAR(50)                     COMMENT '规格代码（如120/142/170/213）',
    F_NAME                  VARCHAR(100)                    COMMENT '规格名称',
    F_VERSION               VARCHAR(50)                     COMMENT '版本',
    F_DETECTION_COLUMNS     INT                             COMMENT '对应原始数据有效列数',
    F_DESCRIPTION           TEXT                            COMMENT '描述',
    F_SORTCODE              BIGINT                          COMMENT '排序码'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='产品规格表';
"""

# ── 产品规格属性表 ────────────────────────────────────────────
DDL_PRODUCT_SPEC_ATTRIBUTE = """
CREATE TABLE LAB_PRODUCT_SPEC_ATTRIBUTE (
    F_ID                    VARCHAR(50)     PRIMARY KEY     COMMENT '主键ID',
    F_PRODUCT_SPEC_ID       VARCHAR(50)     NOT NULL        COMMENT '产品规格ID',
    F_ATTRIBUTE_NAME        VARCHAR(100)    NOT NULL        COMMENT '属性名称（如：标准厚度、理论密度）',
    F_ATTRIBUTE_KEY         VARCHAR(100)    NOT NULL        COMMENT '属性键（如：std_thickness）',
    F_VALUE_TYPE            VARCHAR(20)     NOT NULL        COMMENT '值类型（decimal/int/text）',
    F_ATTRIBUTE_VALUE       VARCHAR(500)                    COMMENT '属性值',
    F_UNIT                  VARCHAR(20)                     COMMENT '单位（如：mm、g/cm³）',
    F_VERSION               INT             DEFAULT 1       COMMENT '版本号'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='产品规格属性表 — 存储每个规格的具体参数';
"""

# ── 公式定义表 ────────────────────────────────────────────────
DDL_FORMULA = """
CREATE TABLE LAB_INTERMEDIATE_DATA_FORMULA (
    F_ID                    VARCHAR(50)     PRIMARY KEY     COMMENT '主键ID',
    F_TABLE_NAME            VARCHAR(50)     DEFAULT 'INTERMEDIATE_DATA' COMMENT '目标表名',
    F_COLUMN_NAME           VARCHAR(100)    NOT NULL        COMMENT '目标列名（如：OneMeterWeight）',
    F_FORMULA_NAME          VARCHAR(100)    NOT NULL        COMMENT '公式名称',
    F_FORMULA               TEXT            NOT NULL        COMMENT '计算公式（EXCEL风格）',
    F_FORMULA_LANGUAGE      VARCHAR(20)     DEFAULT 'EXCEL' COMMENT '公式语言（EXCEL/MATH）',
    F_FORMULA_TYPE          VARCHAR(20)     DEFAULT 'CALC'  COMMENT '公式类型（CALC-计算/JUDGE-判定）',
    F_UNIT_NAME             VARCHAR(50)                     COMMENT '单位名称',
    F_PRECISION             INT                             COMMENT '小数位数',
    F_IS_ENABLED            TINYINT(1)      DEFAULT 1       COMMENT '是否启用',
    F_SORT_ORDER            INT             DEFAULT 0       COMMENT '排序序号',
    F_DEFAULT_VALUE         VARCHAR(100)                    COMMENT '默认值',
    F_REMARK                VARCHAR(1000)                   COMMENT '备注',
    F_SOURCE_TYPE           VARCHAR(20)     DEFAULT 'SYSTEM' COMMENT '来源类型（SYSTEM/CUSTOM）'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='公式定义表 — 计算公式与判定公式';
"""


def get_all_ddl() -> str:
    """获取所有 DDL 拼接后的完整文本，用于注入 LLM prompt。"""
    return "\n\n".join([
        DDL_INTERMEDIATE_DATA,
        DDL_JUDGMENT_LEVEL,
        DDL_PRODUCT_SPEC,
        DDL_PRODUCT_SPEC_ATTRIBUTE,
        DDL_FORMULA,
    ])


# ── 常用统计指标的 SQL 模板（语义层预定义）────────────────────
METRIC_SQL_TEMPLATES = {
    "合格率": {
        "name": "合格率",
        "description": "合格产品重量占总重量的百分比",
        "sql_template": """
            SELECT
                {group_by_clause},
                COUNT(*) AS total_count,
                SUM(CASE WHEN F_MAGNETIC_RES = '合格'
                         AND F_THICK_RES = '合格'
                         AND F_LAM_FACTOR_RES = '合格'
                    THEN 1 ELSE 0 END) AS qualified_count,
                ROUND(
                    SUM(CASE WHEN F_MAGNETIC_RES = '合格'
                             AND F_THICK_RES = '合格'
                             AND F_LAM_FACTOR_RES = '合格'
                        THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 2
                ) AS qualified_rate
            FROM LAB_INTERMEDIATE_DATA
            WHERE F_PROD_DATE BETWEEN '{start_date}' AND '{end_date}'
                  {extra_where}
            GROUP BY {group_by_clause}
        """,
    },
    "产量统计": {
        "name": "产量统计",
        "description": "按维度统计总重量（单卷重量之和）",
        "sql_template": """
            SELECT
                {group_by_clause},
                COUNT(*) AS coil_count,
                ROUND(SUM(F_SINGLE_COIL_WEIGHT), 2) AS total_weight_kg
            FROM LAB_INTERMEDIATE_DATA
            WHERE F_PROD_DATE BETWEEN '{start_date}' AND '{end_date}'
                  {extra_where}
            GROUP BY {group_by_clause}
        """,
    },
    "铁损均值": {
        "name": "铁损均值",
        "description": "Ps铁损的平均值统计",
        "sql_template": """
            SELECT
                {group_by_clause},
                COUNT(*) AS sample_count,
                ROUND(AVG(F_PERF_PS_LOSS), 4) AS avg_ps_loss,
                ROUND(MIN(F_PERF_PS_LOSS), 4) AS min_ps_loss,
                ROUND(MAX(F_PERF_PS_LOSS), 4) AS max_ps_loss
            FROM LAB_INTERMEDIATE_DATA
            WHERE F_PROD_DATE BETWEEN '{start_date}' AND '{end_date}'
                  AND F_PERF_PS_LOSS IS NOT NULL
                  {extra_where}
            GROUP BY {group_by_clause}
        """,
    },
    "叠片系数统计": {
        "name": "叠片系数统计",
        "description": "叠片系数的平均值和达标率",
        "sql_template": """
            SELECT
                {group_by_clause},
                COUNT(*) AS sample_count,
                ROUND(AVG(F_LAMINATION_FACTOR), 4) AS avg_lamination,
                SUM(CASE WHEN F_LAM_FACTOR_RES = '合格' THEN 1 ELSE 0 END) AS qualified_count,
                ROUND(
                    SUM(CASE WHEN F_LAM_FACTOR_RES = '合格' THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 2
                ) AS lamination_qualified_rate
            FROM LAB_INTERMEDIATE_DATA
            WHERE F_PROD_DATE BETWEEN '{start_date}' AND '{end_date}'
                  AND F_LAMINATION_FACTOR IS NOT NULL
                  {extra_where}
            GROUP BY {group_by_clause}
        """,
    },
}
