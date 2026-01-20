-- ============================================
-- 修复 lab_metric_definition 表结构
-- 数据库类型：MySQL
-- 创建日期：2026-01-27
-- 说明：添加缺失的 CLDEntityBase 基类字段
-- ============================================

-- 方法1：使用存储过程安全地添加字段（推荐）
DROP PROCEDURE IF EXISTS `fix_lab_metric_definition_columns`;
DELIMITER $$
CREATE PROCEDURE `fix_lab_metric_definition_columns`()
BEGIN
    -- F_ID (主键字段，如果表不存在则创建表，如果存在但缺少字段则添加)
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS 
                  WHERE TABLE_SCHEMA=DATABASE() 
                  AND TABLE_NAME='lab_metric_definition' 
                  AND COLUMN_NAME='F_ID') THEN
        -- 如果表不存在，需要先创建表
        IF NOT EXISTS(SELECT * FROM information_schema.TABLES 
                      WHERE TABLE_SCHEMA=DATABASE() 
                      AND TABLE_NAME='lab_metric_definition') THEN
            -- 表不存在，需要执行完整的创建脚本
            SELECT '表 lab_metric_definition 不存在，请先执行 migration_create_lab_metric_definition_table.sql' AS Result;
        ELSE
            -- 表存在但缺少 F_ID 字段，添加字段
            ALTER TABLE `lab_metric_definition` 
            ADD COLUMN `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID' FIRST;
            -- 设置为主键
            ALTER TABLE `lab_metric_definition` 
            ADD PRIMARY KEY (`F_ID`);
        END IF;
    END IF;

    -- F_DeleteMark
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS 
                  WHERE TABLE_SCHEMA=DATABASE() 
                  AND TABLE_NAME='lab_metric_definition' 
                  AND COLUMN_NAME='F_DeleteMark') THEN
        ALTER TABLE `lab_metric_definition` 
        ADD COLUMN `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）';
    END IF;

    -- F_DeleteTime
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS 
                  WHERE TABLE_SCHEMA=DATABASE() 
                  AND TABLE_NAME='lab_metric_definition' 
                  AND COLUMN_NAME='F_DeleteTime') THEN
        ALTER TABLE `lab_metric_definition` 
        ADD COLUMN `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间';
    END IF;

    -- F_DeleteUserId
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS 
                  WHERE TABLE_SCHEMA=DATABASE() 
                  AND TABLE_NAME='lab_metric_definition' 
                  AND COLUMN_NAME='F_DeleteUserId') THEN
        ALTER TABLE `lab_metric_definition` 
        ADD COLUMN `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID';
    END IF;

    -- F_LastModifyTime
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS 
                  WHERE TABLE_SCHEMA=DATABASE() 
                  AND TABLE_NAME='lab_metric_definition' 
                  AND COLUMN_NAME='F_LastModifyTime') THEN
        ALTER TABLE `lab_metric_definition` 
        ADD COLUMN `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间';
    END IF;

    -- F_LastModifyUserId
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS 
                  WHERE TABLE_SCHEMA=DATABASE() 
                  AND TABLE_NAME='lab_metric_definition' 
                  AND COLUMN_NAME='F_LastModifyUserId') THEN
        ALTER TABLE `lab_metric_definition` 
        ADD COLUMN `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID';
    END IF;

    -- F_CREATORTIME
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS 
                  WHERE TABLE_SCHEMA=DATABASE() 
                  AND TABLE_NAME='lab_metric_definition' 
                  AND COLUMN_NAME='F_CREATORTIME') THEN
        ALTER TABLE `lab_metric_definition` 
        ADD COLUMN `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间';
    END IF;

    -- F_CREATORUSERID
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS 
                  WHERE TABLE_SCHEMA=DATABASE() 
                  AND TABLE_NAME='lab_metric_definition' 
                  AND COLUMN_NAME='F_CREATORUSERID') THEN
        ALTER TABLE `lab_metric_definition` 
        ADD COLUMN `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID';
    END IF;

    -- F_ENABLEDMARK
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS 
                  WHERE TABLE_SCHEMA=DATABASE() 
                  AND TABLE_NAME='lab_metric_definition' 
                  AND COLUMN_NAME='F_ENABLEDMARK') THEN
        ALTER TABLE `lab_metric_definition` 
        ADD COLUMN `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）';
    END IF;

    -- F_TENANTID
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS 
                  WHERE TABLE_SCHEMA=DATABASE() 
                  AND TABLE_NAME='lab_metric_definition' 
                  AND COLUMN_NAME='F_TENANTID') THEN
        ALTER TABLE `lab_metric_definition` 
        ADD COLUMN `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID';
    END IF;

    SELECT 'lab_metric_definition 表字段修复完成' AS Result;
END$$
DELIMITER ;

-- 执行存储过程
CALL `fix_lab_metric_definition_columns`();

-- 删除存储过程（可选）
DROP PROCEDURE IF EXISTS `fix_lab_metric_definition_columns`;

-- 验证修改结果
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    COLUMN_COMMENT
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'lab_metric_definition'
ORDER BY ORDINAL_POSITION;

-- ============================================
-- 方法2：直接使用 ALTER TABLE（如果 MySQL 版本不支持存储过程）
-- ============================================
/*
-- 注意：MySQL 5.7 及以下版本不支持 ADD COLUMN IF NOT EXISTS
-- 如果使用 MySQL 8.0+，可以直接使用以下语句：

ALTER TABLE `lab_metric_definition`
ADD COLUMN IF NOT EXISTS `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
ADD COLUMN IF NOT EXISTS `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
ADD COLUMN IF NOT EXISTS `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',
ADD COLUMN IF NOT EXISTS `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
ADD COLUMN IF NOT EXISTS `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
ADD COLUMN IF NOT EXISTS `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
ADD COLUMN IF NOT EXISTS `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
ADD COLUMN IF NOT EXISTS `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
ADD COLUMN IF NOT EXISTS `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID';
*/
