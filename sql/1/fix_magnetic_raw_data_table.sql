-- ============================================
-- 修复磁性原始数据表 - 添加缺失的CLDEntityBase基类字段
-- 数据库类型：MySQL
-- 创建日期：2025-01-20
-- ============================================
-- 说明：此脚本用于修复 LAB_MAGNETIC_RAW_DATA 表，确保包含所有 CLDEntityBase 基类字段
-- 如果表不存在，将创建完整的表结构
-- 如果表存在但缺少字段，将添加缺失的字段
-- 
-- 注意：如果字段已存在，ALTER TABLE 语句会报错，可以忽略该错误继续执行

-- 方法1：使用 ALTER TABLE 语句直接添加字段（推荐，MySQL 8.0.19+ 支持 IF NOT EXISTS）
-- 如果您的 MySQL 版本 >= 8.0.19，可以使用以下语句：
ALTER TABLE `LAB_MAGNETIC_RAW_DATA`
ADD COLUMN IF NOT EXISTS `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
ADD COLUMN IF NOT EXISTS `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
ADD COLUMN IF NOT EXISTS `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
ADD COLUMN IF NOT EXISTS `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
ADD COLUMN IF NOT EXISTS `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
ADD COLUMN IF NOT EXISTS `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
ADD COLUMN IF NOT EXISTS `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
ADD COLUMN IF NOT EXISTS `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',
ADD COLUMN IF NOT EXISTS `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID';

-- 方法2：如果 MySQL 版本 < 8.0.19，请使用以下方式（需要手动检查字段是否存在）
-- 或者直接执行 ALTER TABLE，如果字段已存在会报错，可以忽略该错误
/*
ALTER TABLE `LAB_MAGNETIC_RAW_DATA`
ADD COLUMN `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
ADD COLUMN `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
ADD COLUMN `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
ADD COLUMN `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
ADD COLUMN `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
ADD COLUMN `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
ADD COLUMN `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
ADD COLUMN `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',
ADD COLUMN `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID';
*/

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
    AND TABLE_NAME = 'LAB_MAGNETIC_RAW_DATA'
    AND COLUMN_NAME IN (
        'F_CREATORTIME',
        'F_CREATORUSERID',
        'F_ENABLEDMARK',
        'F_LastModifyTime',
        'F_LastModifyUserId',
        'F_DeleteMark',
        'F_DeleteTime',
        'F_DeleteUserId',
        'F_TENANTID'
    )
ORDER BY ORDINAL_POSITION;

-- ============================================
-- 如果表不存在，请先执行以下 CREATE TABLE 语句
-- ============================================
/*
CREATE TABLE IF NOT EXISTS `LAB_MAGNETIC_RAW_DATA` (
    -- 基础字段（从CLDEntityBase继承）
    `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
    `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
    `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
    `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
    `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
    `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',
    
    -- 业务字段
    `F_ORIGINAL_FURNACE_NO` VARCHAR(100) DEFAULT NULL COMMENT '原始炉号（B列，包含K标识）',
    `F_FURNACE_NO` VARCHAR(100) DEFAULT NULL COMMENT '炉号（去掉K后的炉号）',
    `F_IS_SCRATCHED` INT DEFAULT 0 COMMENT '是否刻痕（0-否，1-是，是否带K）',
    `F_PS_LOSS` DECIMAL(18, 2) DEFAULT NULL COMMENT 'Ps铁损（H列）',
    `F_SS_POWER` DECIMAL(18, 2) DEFAULT NULL COMMENT 'Ss激磁功率（I列）',
    `F_HC` DECIMAL(18, 2) DEFAULT NULL COMMENT 'Hc（F列）',
    `F_DETECTION_TIME` DATETIME DEFAULT NULL COMMENT '检测时间（P列）',
    
    -- 解析后的炉号信息（参考原始数据表）
    `F_LINE_NO` INT DEFAULT NULL COMMENT '产线（从炉号解析）',
    `F_SHIFT` VARCHAR(10) DEFAULT NULL COMMENT '班次（从炉号解析，存储原始汉字：甲、乙、丙）',
    `F_SHIFT_NUMERIC` INT DEFAULT NULL COMMENT '班次数字（用于排序：甲=1, 乙=2, 丙=3）',
    `F_FURNACE_BATCH_NO` INT DEFAULT NULL COMMENT '炉次号（从炉号解析）',
    `F_COIL_NO` DECIMAL(18, 2) DEFAULT NULL COMMENT '卷号（从炉号解析，支持小数，磁性数据炉号格式不包含卷号，通常为NULL）',
    `F_SUBCOIL_NO` DECIMAL(18, 2) DEFAULT NULL COMMENT '分卷号（从炉号解析，支持小数，磁性数据炉号格式不包含分卷号，通常为NULL）',
    `F_PROD_DATE` DATETIME DEFAULT NULL COMMENT '生产日期（从炉号解析）',
    `F_FURNACE_NO_PARSED` VARCHAR(50) DEFAULT NULL COMMENT '炉号数字部分（从炉号解析，与F_FURNACE_BATCH_NO相同）',
    
    -- 导入相关字段
    `F_IMPORT_SESSION_ID` VARCHAR(50) DEFAULT NULL COMMENT '导入会话ID（关联导入会话）',
    `F_ROW_INDEX` INT DEFAULT NULL COMMENT 'Excel行号（用于错误提示）',
    `F_IS_VALID` INT DEFAULT 0 COMMENT '是否有效数据（0-无效，1-有效）',
    `F_ERROR_MESSAGE` VARCHAR(500) DEFAULT NULL COMMENT '错误信息',
    `F_SORTCODE` BIGINT DEFAULT NULL COMMENT '排序码',
    
    PRIMARY KEY (`F_ID`),
    KEY `idx_furnace_no` (`F_FURNACE_NO`),
    KEY `idx_original_furnace_no` (`F_ORIGINAL_FURNACE_NO`),
    KEY `idx_import_session_id` (`F_IMPORT_SESSION_ID`),
    KEY `idx_is_valid` (`F_IS_VALID`),
    KEY `idx_prod_date` (`F_PROD_DATE`),
    KEY `idx_line_no` (`F_LINE_NO`),
    KEY `idx_shift` (`F_SHIFT`),
    KEY `idx_furnace_batch_no` (`F_FURNACE_BATCH_NO`),
    KEY `idx_tenant_id` (`F_TENANTID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='磁性原始数据表';
*/
