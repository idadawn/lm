-- ============================================
-- 产品规格扩展属性完整迁移脚本
-- 说明: 
--   1. 创建扩展属性表
--   2. 创建公共属性定义表
--   3. 更新扩展属性表结构（移除MinValue/MaxValue，添加IsPublic）
--   4. 从PropertyJson迁移数据到新表
--   5. 删除PropertyJson字段（可选，建议在确认迁移成功后执行）
-- 执行顺序: 按顺序执行所有步骤
-- 注意事项: 
--   - 执行前请备份数据库
--   - 步骤5（删除PropertyJson）建议在确认所有功能正常后再执行
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- ============================================
-- 步骤1: 创建产品规格扩展属性表
-- ============================================
CREATE TABLE IF NOT EXISTS `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_PRODUCT_SPEC_ID` VARCHAR(50) NOT NULL COMMENT '产品规格ID',
    `F_ATTRIBUTE_NAME` VARCHAR(100) NOT NULL COMMENT '属性名称（如：长度、层数、密度）',
    `F_ATTRIBUTE_KEY` VARCHAR(100) NOT NULL COMMENT '属性键名（如：length、layers、density）',
    `F_VALUE_TYPE` VARCHAR(20) NOT NULL COMMENT '属性值类型（decimal、int、text）',
    `F_ATTRIBUTE_VALUE` VARCHAR(500) NULL COMMENT '属性值（存储为字符串）',
    `F_UNIT` VARCHAR(20) NULL COMMENT '属性单位（如：m、mm、kg、MPa、%）',
    `F_PRECISION` INT NULL COMMENT '精度（仅用于数字类型，小数位数）',
    `F_SORTCODE` BIGINT NULL COMMENT '排序码',
    `F_IS_CORE` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否核心属性（长度、层数、密度为核心属性，不可删除）',
    `F_IS_PUBLIC` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否公共属性（从公共属性定义创建）',
    `F_CREATORUSERID` VARCHAR(50) NULL COMMENT '创建人ID',
    `F_CREATORTIME` DATETIME NULL COMMENT '创建时间',
    `F_LastModifyUserId` VARCHAR(50) NULL COMMENT '最后修改人ID',
    `F_LastModifyTime` DATETIME NULL COMMENT '最后修改时间',
    `F_DeleteMark` INT NULL COMMENT '删除标记',
    `F_DeleteTime` DATETIME NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) NULL COMMENT '删除人ID',
    `F_TenantId` VARCHAR(50) NULL COMMENT '租户ID',
    PRIMARY KEY (`F_Id`),
    INDEX `IDX_PRODUCT_SPEC_ID` (`F_PRODUCT_SPEC_ID`),
    INDEX `IDX_ATTRIBUTE_KEY` (`F_ATTRIBUTE_KEY`),
    INDEX `IDX_DELETEMARK` (`F_DeleteMark`),
    UNIQUE KEY `UK_PRODUCT_SPEC_KEY` (`F_PRODUCT_SPEC_ID`, `F_ATTRIBUTE_KEY`, `F_DeleteMark`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='产品规格扩展属性表';

-- ============================================
-- 步骤2: 创建产品规格公共属性定义表
-- ============================================
CREATE TABLE IF NOT EXISTS `LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE` (
    `F_Id` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_ATTRIBUTE_NAME` VARCHAR(100) NOT NULL COMMENT '属性名称（如：长度、层数、密度）',
    `F_ATTRIBUTE_KEY` VARCHAR(100) NOT NULL COMMENT '属性键名（如：length、layers、density）',
    `F_VALUE_TYPE` VARCHAR(20) NOT NULL COMMENT '属性值类型（decimal、int、text）',
    `F_DEFAULT_VALUE` VARCHAR(500) NULL COMMENT '默认值（存储为字符串）',
    `F_UNIT` VARCHAR(20) NULL COMMENT '属性单位（如：m、mm、kg、MPa、%）',
    `F_PRECISION` INT NULL COMMENT '精度（仅用于数字类型，小数位数）',
    `F_SORTCODE` BIGINT NULL COMMENT '排序码',
    `F_CREATORUSERID` VARCHAR(50) NULL COMMENT '创建人ID',
    `F_CREATORTIME` DATETIME NULL COMMENT '创建时间',
    `F_LastModifyUserId` VARCHAR(50) NULL COMMENT '最后修改人ID',
    `F_LastModifyTime` DATETIME NULL COMMENT '最后修改时间',
    `F_DeleteMark` INT NULL COMMENT '删除标记',
    `F_DeleteTime` DATETIME NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) NULL COMMENT '删除人ID',
    `F_TenantId` VARCHAR(50) NULL COMMENT '租户ID',
    PRIMARY KEY (`F_Id`),
    INDEX `IDX_ATTRIBUTE_KEY` (`F_ATTRIBUTE_KEY`),
    INDEX `IDX_DELETEMARK` (`F_DeleteMark`),
    UNIQUE KEY `UK_ATTRIBUTE_KEY` (`F_ATTRIBUTE_KEY`, `F_DeleteMark`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='产品规格公共属性定义表';

-- ============================================
-- 步骤3: 如果表已存在，更新表结构
-- ============================================

-- 检查并添加 F_IS_PUBLIC 字段（如果不存在）
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'LAB_PRODUCT_SPEC_ATTRIBUTE'
      AND COLUMN_NAME = 'F_IS_PUBLIC'
);

SET @sql = IF(@column_exists = 0,
    'ALTER TABLE `LAB_PRODUCT_SPEC_ATTRIBUTE` ADD COLUMN `F_IS_PUBLIC` TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''是否公共属性（从公共属性定义创建）'' AFTER `F_IS_CORE`',
    'SELECT ''Column F_IS_PUBLIC already exists'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 检查并移除 F_MIN_VALUE 字段（如果存在）
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'LAB_PRODUCT_SPEC_ATTRIBUTE'
      AND COLUMN_NAME = 'F_MIN_VALUE'
);

SET @sql = IF(@column_exists > 0,
    'ALTER TABLE `LAB_PRODUCT_SPEC_ATTRIBUTE` DROP COLUMN `F_MIN_VALUE`',
    'SELECT ''Column F_MIN_VALUE does not exist'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 检查并移除 F_MAX_VALUE 字段（如果存在）
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'LAB_PRODUCT_SPEC_ATTRIBUTE'
      AND COLUMN_NAME = 'F_MAX_VALUE'
);

SET @sql = IF(@column_exists > 0,
    'ALTER TABLE `LAB_PRODUCT_SPEC_ATTRIBUTE` DROP COLUMN `F_MAX_VALUE`',
    'SELECT ''Column F_MAX_VALUE does not exist'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 更新 F_VALUE_TYPE 注释
ALTER TABLE `LAB_PRODUCT_SPEC_ATTRIBUTE`
MODIFY COLUMN `F_VALUE_TYPE` VARCHAR(20) NOT NULL COMMENT '属性值类型（decimal、int、text）';

-- ============================================
-- 步骤4: 从PropertyJson迁移核心属性（长度、层数、密度）
-- ============================================

-- 迁移长度属性
INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_IS_PUBLIC`,
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
    'decimal' AS `F_VALUE_TYPE`,
    IFNULL(CAST(JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.length') AS CHAR), '4') AS `F_ATTRIBUTE_VALUE`,
    'm' AS `F_UNIT`,
    2 AS `F_PRECISION`,
    1 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    0 AS `F_IS_PUBLIC`,
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

-- 迁移层数属性
INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_IS_PUBLIC`,
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
    'int' AS `F_VALUE_TYPE`,
    IFNULL(CAST(JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.layers') AS CHAR), '20') AS `F_ATTRIBUTE_VALUE`,
    NULL AS `F_UNIT`,
    0 AS `F_PRECISION`,
    2 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    0 AS `F_IS_PUBLIC`,
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

-- 迁移密度属性
INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_IS_PUBLIC`,
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
    'decimal' AS `F_VALUE_TYPE`,
    IFNULL(CAST(JSON_EXTRACT(ps.`F_PROPERTYJSON`, '$.density') AS CHAR), '7.25') AS `F_ATTRIBUTE_VALUE`,
    NULL AS `F_UNIT`,
    2 AS `F_PRECISION`,
    3 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    0 AS `F_IS_PUBLIC`,
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

-- ============================================
-- 步骤5: 为没有核心属性的产品规格创建默认值
-- ============================================

-- 为没有长度属性的产品创建默认长度
INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_IS_PUBLIC`,
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
    'decimal' AS `F_VALUE_TYPE`,
    '4' AS `F_ATTRIBUTE_VALUE`,
    'm' AS `F_UNIT`,
    2 AS `F_PRECISION`,
    1 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    0 AS `F_IS_PUBLIC`,
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

-- 为没有层数属性的产品创建默认层数
INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_IS_PUBLIC`,
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
    'int' AS `F_VALUE_TYPE`,
    '20' AS `F_ATTRIBUTE_VALUE`,
    NULL AS `F_UNIT`,
    0 AS `F_PRECISION`,
    2 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    0 AS `F_IS_PUBLIC`,
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

-- 为没有密度属性的产品创建默认密度
INSERT INTO `LAB_PRODUCT_SPEC_ATTRIBUTE` (
    `F_Id`,
    `F_PRODUCT_SPEC_ID`,
    `F_ATTRIBUTE_NAME`,
    `F_ATTRIBUTE_KEY`,
    `F_VALUE_TYPE`,
    `F_ATTRIBUTE_VALUE`,
    `F_UNIT`,
    `F_PRECISION`,
    `F_SORTCODE`,
    `F_IS_CORE`,
    `F_IS_PUBLIC`,
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
    'decimal' AS `F_VALUE_TYPE`,
    '7.25' AS `F_ATTRIBUTE_VALUE`,
    NULL AS `F_UNIT`,
    2 AS `F_PRECISION`,
    3 AS `F_SORTCODE`,
    1 AS `F_IS_CORE`,
    0 AS `F_IS_PUBLIC`,
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

-- ============================================
-- 步骤6: 删除 PropertyJson 字段（可选，建议在确认所有数据已迁移后执行）
-- ============================================

-- 注意：执行此步骤前，请确保：
-- 1. 所有扩展属性数据已成功迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表
-- 2. 应用程序已更新为使用属性表，不再依赖 PropertyJson
-- 3. 已备份数据库

-- 检查并删除 F_PROPERTYJSON 字段（如果存在）
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'LAB_PRODUCT_SPEC'
      AND COLUMN_NAME = 'F_PROPERTYJSON'
);

SET @sql = IF(@column_exists > 0,
    'ALTER TABLE `LAB_PRODUCT_SPEC` DROP COLUMN `F_PROPERTYJSON`',
    'SELECT ''Column F_PROPERTYJSON does not exist or already removed'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================
-- 完成提示
-- ============================================
SELECT 'Migration completed successfully! PropertyJson field has been removed.' AS message;
