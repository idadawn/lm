-- ============================================
-- 产品规格扩展属性 - 从 PropertyJson 迁移到独立表
-- 说明: 
--   1. 将现有 PropertyJson 中的数据迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表
--   2. 核心属性（length、layers、density）标记为 IsCore = 1
--   3. 迁移完成后，PropertyJson 字段可以保留用于向后兼容或删除
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 步骤1: 迁移核心属性（长度、层数、密度）
INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_MIN_VALUE`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_CREATORUSERID`,
    `F_CREATORTIME`,
    `F_LastModifyUserId`,
    `F_LastModifyTime`,
    `F_TenantId`
)
SELECT
    CONCAT(ps.`F_Id`, '_length') AS `F_Id`,
    ps.`F_Id` AS `F_PRODUCT_SPEC_ID`,
    '长度' AS `F_ATTRIBUTE_NAME`,
    'length' AS `F_ATTRIBUTE_KEY`,
    'number' AS `F_VALUE_TYPE`,
    IFNULL(CAST(JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.length') AS CHAR), '4') AS `F_ATTRIBUTE_VALUE`,
    'm' AS `F_UNIT`,
    0 AS `F_MIN_VALUE`,
    2 AS `F_PRECISION`,
    1 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    ps.`F_CREATORUSERID`,
    ps.`F_CREATORTIME`,
    ps.`F_LastModifyUserId`,
    ps.`F_LastModifyTime`,
    ps.`F_TenantId`
FROM `LAB_PRODUCT_SPEC` ps
WHERE ps.`F_DeleteMark` IS NULL
  AND (ps.`F_PROPERTYJSON` IS NOT NULL AND ps.`F_PROPERTYJSON` != '' AND JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.length') IS NOT NULL)
ON DUPLICATE KEY UPDATE
    `F_ATTRIBUTE_VALUE` = IFNULL(CAST(JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.length') AS CHAR), '4'),
    `F_LastModifyTime` = ps.`F_LastModifyTime`;

INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_MIN_VALUE`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_CREATORUSERID`,
    `F_CREATORTIME`,
    `F_LastModifyUserId`,
    `F_LastModifyTime`,
    `F_TenantId`
)
SELECT
    CONCAT(ps.`F_Id`, '_layers') AS `F_Id`,
    ps.`F_Id` AS `F_PRODUCT_SPEC_ID`,
    '层数' AS `F_ATTRIBUTE_NAME`,
    'layers' AS `F_ATTRIBUTE_KEY`,
    'number' AS `F_VALUE_TYPE`,
    IFNULL(CAST(JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.layers') AS CHAR), '20') AS `F_ATTRIBUTE_VALUE`,
    NULL AS `F_UNIT`,
    1 AS `F_MIN_VALUE`,
    0 AS `F_PRECISION`,
    2 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    ps.`F_CREATORUSERID`,
    ps.`F_CREATORTIME`,
    ps.`F_LastModifyUserId`,
    ps.`F_LastModifyTime`,
    ps.`F_TenantId`
FROM `LAB_PRODUCT_SPEC` ps
WHERE ps.`F_DeleteMark` IS NULL
  AND (ps.`F_PROPERTYJSON` IS NOT NULL AND ps.`F_PROPERTYJSON` != '' AND JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.layers') IS NOT NULL)
ON DUPLICATE KEY UPDATE
    `F_ATTRIBUTE_VALUE` = IFNULL(CAST(JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.layers') AS CHAR), '20'),
    `F_LastModifyTime` = ps.`F_LastModifyTime`;

INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_MIN_VALUE`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_CREATORUSERID`,
    `F_CREATORTIME`,
    `F_LastModifyUserId`,
    `F_LastModifyTime`,
    `F_TenantId`
)
SELECT
    CONCAT(ps.`F_Id`, '_density') AS `F_Id`,
    ps.`F_Id` AS `F_PRODUCT_SPEC_ID`,
    '密度' AS `F_ATTRIBUTE_NAME`,
    'density' AS `F_ATTRIBUTE_KEY`,
    'number' AS `F_VALUE_TYPE`,
    IFNULL(CAST(JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.density') AS CHAR), '7.25') AS `F_ATTRIBUTE_VALUE`,
    NULL AS `F_UNIT`,
    0 AS `F_MIN_VALUE`,
    2 AS `F_PRECISION`,
    3 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    ps.`F_CREATORUSERID`,
    ps.`F_CREATORTIME`,
    ps.`F_LastModifyUserId`,
    ps.`F_LastModifyTime`,
    ps.`F_TenantId`
FROM `LAB_PRODUCT_SPEC` ps
WHERE ps.`F_DeleteMark` IS NULL
  AND (ps.`F_PROPERTYJSON` IS NOT NULL AND ps.`F_PROPERTYJSON` != '' AND JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.density') IS NOT NULL)
ON DUPLICATE KEY UPDATE
    `F_ATTRIBUTE_VALUE` = IFNULL(CAST(JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.density') AS CHAR), '7.25'),
    `F_LastModifyTime` = ps.`F_LastModifyTime`;

-- 步骤2: 迁移其他扩展属性（从 PropertyJson 中解析）
-- 注意：这里需要根据实际的 PropertyJson 结构来解析，以下是一个示例
-- 实际使用时可能需要根据 PropertyJson 的具体格式调整

-- 为没有核心属性的产品规格创建默认值
INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_MIN_VALUE`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_CREATORUSERID`,
    `F_CREATORTIME`,
    `F_LastModifyUserId`,
    `F_LastModifyTime`,
    `F_TenantId`
)
SELECT
    CONCAT(ps.`F_Id`, '_length') AS `F_Id`,
    ps.`F_Id` AS `F_PRODUCT_SPEC_ID`,
    '长度' AS `F_ATTRIBUTE_NAME`,
    'length' AS `F_ATTRIBUTE_KEY`,
    'number' AS `F_VALUE_TYPE`,
    '4' AS `F_ATTRIBUTE_VALUE`,
    'm' AS `F_UNIT`,
    0 AS `F_MIN_VALUE`,
    2 AS `F_PRECISION`,
    1 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    ps.`F_CREATORUSERID`,
    ps.`F_CREATORTIME`,
    ps.`F_LastModifyUserId`,
    ps.`F_LastModifyTime`,
    ps.`F_TenantId`
FROM `LAB_PRODUCT_SPEC` ps
WHERE ps.`F_DeleteMark` IS NULL
  AND NOT EXISTS (
    SELECT 1 FROM `LAB_PRODUCT_SPEC_ATTRIBUTE` attr 
    WHERE attr.`F_PRODUCT_SPEC_ID` = ps.`F_Id` 
      AND attr.`F_ATTRIBUTE_KEY` = 'length'
      AND attr.`F_DeleteMark` IS NULL
  );

INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_MIN_VALUE`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_CREATORUSERID`,
    `F_CREATORTIME`,
    `F_LastModifyUserId`,
    `F_LastModifyTime`,
    `F_TenantId`
)
SELECT
    CONCAT(ps.`F_Id`, '_layers') AS `F_Id`,
    ps.`F_Id` AS `F_PRODUCT_SPEC_ID`,
    '层数' AS `F_ATTRIBUTE_NAME`,
    'layers' AS `F_ATTRIBUTE_KEY`,
    'number' AS `F_VALUE_TYPE`,
    '20' AS `F_ATTRIBUTE_VALUE`,
    NULL AS `F_UNIT`,
    1 AS `F_MIN_VALUE`,
    0 AS `F_PRECISION`,
    2 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    ps.`F_CREATORUSERID`,
    ps.`F_CREATORTIME`,
    ps.`F_LastModifyUserId`,
    ps.`F_LastModifyTime`,
    ps.`F_TenantId`
FROM `LAB_PRODUCT_SPEC` ps
WHERE ps.`F_DeleteMark` IS NULL
  AND NOT EXISTS (
    SELECT 1 FROM `LAB_PRODUCT_SPEC_ATTRIBUTE` attr 
    WHERE attr.`F_PRODUCT_SPEC_ID` = ps.`F_Id` 
      AND attr.`F_ATTRIBUTE_KEY` = 'layers'
      AND attr.`F_DeleteMark` IS NULL
  );

INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_MIN_VALUE`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_CREATORUSERID`,
    `F_CREATORTIME`,
    `F_LastModifyUserId`,
    `F_LastModifyTime`,
    `F_TenantId`
)
SELECT
    CONCAT(ps.`F_Id`, '_density') AS `F_Id`,
    ps.`F_Id` AS `F_PRODUCT_SPEC_ID`,
    '密度' AS `F_ATTRIBUTE_NAME`,
    'density' AS `F_ATTRIBUTE_KEY`,
    'number' AS `F_VALUE_TYPE`,
    '7.25' AS `F_ATTRIBUTE_VALUE`,
    NULL AS `F_UNIT`,
    0 AS `F_MIN_VALUE`,
    2 AS `F_PRECISION`,
    3 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    ps.`F_CREATORUSERID`,
    ps.`F_CREATORTIME`,
    ps.`F_LastModifyUserId`,
    ps.`F_LastModifyTime`,
    ps.`F_TenantId`
FROM `LAB_PRODUCT_SPEC` ps
WHERE ps.`F_DeleteMark` IS NULL
  AND NOT EXISTS (
    SELECT 1 FROM `LAB_PRODUCT_SPEC_ATTRIBUTE` attr 
    WHERE attr.`F_PRODUCT_SPEC_ID` = ps.`F_Id` 
      AND attr.`F_ATTRIBUTE_KEY` = 'density'
      AND attr.`F_DeleteMark` IS NULL
  );
