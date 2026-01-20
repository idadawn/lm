-- ============================================
-- 数据迁移脚本：将 Indicator 数据迁移到 Metric
-- 数据库类型：MySQL
-- 创建日期：2026-01-27
-- 说明：整合 Indicator 和 Metric 系统，统一使用 Metric
-- ============================================

-- 注意：执行此脚本前，请先备份数据库！

-- ============================================
-- 步骤1：迁移指标定义数据（如果 LAB_INDICATOR_DEFINITION 表存在且有数据）
-- ============================================
-- 检查是否存在 LAB_INDICATOR_DEFINITION 表
-- 如果存在且有数据，则迁移到 lab_metric_definition 表

INSERT INTO `lab_metric_definition` (
    `F_ID`,
    `F_TENANTID`,
    `F_CREATORTIME`,
    `F_CREATORUSERID`,
    `F_ENABLEDMARK`,
    `F_LastModifyTime`,
    `F_LastModifyUserId`,
    `F_DeleteMark`,
    `F_DeleteTime`,
    `F_DeleteUserId`,
    `name`,
    `code`,
    `description`,
    `formula`,
    `formula_language`,
    `metric_type`,
    `return_type`,
    `unit_id`,
    `unit_name`,
    `category`,
    `is_enabled`,
    `is_system`,
    `sort_order`,
    `version`,
    `is_versioned`,
    `has_variables`,
    `store_results`,
    `precision`,
    `calculation_order`,
    `remark`
)
SELECT 
    `F_ID`,
    `F_TENANTID`,
    `F_CREATORTIME`,
    `F_CREATORUSERID`,
    `F_ENABLEDMARK`,
    `F_LastModifyTime`,
    `F_LastModifyUserId`,
    `F_DeleteMark`,
    `F_DeleteTime`,
    `F_DeleteUserId`,
    `F_INDICATOR_NAME` AS `name`,
    `F_INDICATOR_CODE` AS `code`,
    `F_DESCRIPTION` AS `description`,
    `F_FORMULA_EXPRESSION` AS `formula`,
    COALESCE(`F_FORMULA_LANGUAGE`, 'EXCEL') AS `formula_language`,
    CASE 
        WHEN `F_INDICATOR_TYPE` = 1 THEN 'COMPOSITE'
        WHEN `F_INDICATOR_TYPE` = 2 THEN 'ATOMIC'
        ELSE 'STANDARD'
    END AS `metric_type`,
    CASE 
        WHEN `F_RETURN_TYPE` = 'String' THEN 'STRING'
        WHEN `F_RETURN_TYPE` = 'Number' THEN 'NUMBER'
        WHEN `F_RETURN_TYPE` = 'Boolean' THEN 'BOOLEAN'
        WHEN `F_RETURN_TYPE` = 'DateTime' THEN 'DATETIME'
        ELSE 'NUMBER'
    END AS `return_type`,
    `F_UNIT_ID` AS `unit_id`,
    `F_UNIT_NAME` AS `unit_name`,
    NULL AS `category`,  -- Indicator 没有分类字段
    CASE WHEN `F_IS_ENABLED` = 1 THEN 1 ELSE 0 END AS `is_enabled`,
    0 AS `is_system`,  -- 默认非系统指标
    COALESCE(`F_SORTCODE`, 0) AS `sort_order`,
    COALESCE(`F_VERSION`, 1) AS `version`,
    0 AS `is_versioned`,  -- 默认不启用版本管理
    0 AS `has_variables`,  -- 需要检查是否有变量绑定
    0 AS `store_results`,  -- 默认不存储结果
    `F_PRECISION` AS `precision`,
    `F_CALCULATION_ORDER` AS `calculation_order`,
    NULL AS `remark`
FROM `LAB_INDICATOR_DEFINITION`
WHERE `F_DeleteMark` != 1
  AND NOT EXISTS (
      SELECT 1 FROM `lab_metric_definition` m 
      WHERE m.`code` = `LAB_INDICATOR_DEFINITION`.`F_INDICATOR_CODE`
        AND m.`F_TENANTID` = `LAB_INDICATOR_DEFINITION`.`F_TENANTID`
        AND m.`F_DeleteMark` != 1
  );

-- ============================================
-- 步骤2：迁移变量绑定数据（如果 LAB_INDICATOR_VARIABLE 表存在）
-- ============================================
-- 注意：Metric 系统使用 lab_metric_variable 表
-- 如果表结构相同，可以直接迁移

-- 检查是否存在 lab_metric_variable 表，如果不存在则创建
CREATE TABLE IF NOT EXISTS `lab_metric_variable` (
    `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
    `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识',
    `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
    `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
    `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志',
    `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',
    `metric_id` VARCHAR(50) NOT NULL COMMENT '指标ID',
    `variable_name` VARCHAR(100) NOT NULL COMMENT '变量名称',
    `source_type` VARCHAR(20) NOT NULL COMMENT '来源类型：COLUMN/DIMENSION/CONSTANT',
    `source_id` VARCHAR(100) DEFAULT NULL COMMENT '来源ID',
    `data_type` VARCHAR(20) NOT NULL COMMENT '数据类型',
    `is_required` TINYINT DEFAULT 1 COMMENT '是否必需',
    `default_value` VARCHAR(200) DEFAULT NULL COMMENT '默认值',
    `sortcode` BIGINT DEFAULT NULL COMMENT '排序码',
    PRIMARY KEY (`F_ID`),
    KEY `IDX_VARIABLE_METRIC` (`metric_id`, `F_TENANTID`),
    KEY `IDX_TENANT_ID` (`F_TENANTID`),
    KEY `IDX_DELETE_MARK` (`F_DeleteMark`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='指标变量绑定表';

-- 迁移变量数据（需要将 F_INDICATOR_ID 映射到新的 metric_id）
INSERT INTO `lab_metric_variable` (
    `F_ID`,
    `F_TENANTID`,
    `F_CREATORTIME`,
    `F_CREATORUSERID`,
    `F_ENABLEDMARK`,
    `F_LastModifyTime`,
    `F_LastModifyUserId`,
    `F_DeleteMark`,
    `F_DeleteTime`,
    `F_DeleteUserId`,
    `metric_id`,
    `variable_name`,
    `source_type`,
    `source_id`,
    `data_type`,
    `is_required`,
    `default_value`,
    `sortcode`
)
SELECT 
    v.`F_ID`,
    v.`F_TENANTID`,
    v.`F_CREATORTIME`,
    v.`F_CREATORUSERID`,
    v.`F_ENABLEDMARK`,
    v.`F_LastModifyTime`,
    v.`F_LastModifyUserId`,
    v.`F_DeleteMark`,
    v.`F_DeleteTime`,
    v.`F_DeleteUserId`,
    v.`F_INDICATOR_ID` AS `metric_id`,  -- 使用相同的ID
    v.`F_VARIABLE_NAME` AS `variable_name`,
    v.`F_SOURCE_TYPE` AS `source_type`,
    v.`F_SOURCE_ID` AS `source_id`,
    v.`F_DATA_TYPE` AS `data_type`,
    CASE WHEN v.`F_IS_REQUIRED` = 1 THEN 1 ELSE 0 END AS `is_required`,
    v.`F_DEFAULT_VALUE` AS `default_value`,
    v.`F_SORTCODE` AS `sortcode`
FROM `LAB_INDICATOR_VARIABLE` v
WHERE v.`F_DeleteMark` != 1
  AND EXISTS (
      SELECT 1 FROM `lab_metric_definition` m 
      WHERE m.`F_ID` = v.`F_INDICATOR_ID`
        AND m.`F_DeleteMark` != 1
  )
  AND NOT EXISTS (
      SELECT 1 FROM `lab_metric_variable` mv 
      WHERE mv.`F_ID` = v.`F_ID`
        AND mv.`F_DeleteMark` != 1
  );

-- ============================================
-- 步骤3：更新已迁移指标的 has_variables 标志
-- ============================================
UPDATE `lab_metric_definition` m
SET `has_variables` = 1
WHERE EXISTS (
    SELECT 1 FROM `lab_metric_variable` v
    WHERE v.`metric_id` = m.`F_ID`
      AND v.`F_DeleteMark` != 1
);

-- ============================================
-- 步骤4：迁移完成后的清理（可选，建议先保留原表作为备份）
-- ============================================
-- 注意：以下操作会删除原表，请确保数据迁移成功后再执行！

-- 删除 Indicator 相关表（谨慎操作！）
-- DROP TABLE IF EXISTS `LAB_INDICATOR_RESULT`;
-- DROP TABLE IF EXISTS `LAB_INDICATOR_FORMULA_VERSION`;
-- DROP TABLE IF EXISTS `LAB_INDICATOR_VARIABLE`;
-- DROP TABLE IF EXISTS `LAB_INDICATOR_DEFINITION`;

-- ============================================
-- 迁移完成
-- ============================================
-- 迁移后请验证：
-- 1. 检查迁移的数据数量是否正确
-- 2. 验证指标代码的唯一性
-- 3. 测试指标计算功能是否正常
-- 4. 确认前端页面可以正常访问
