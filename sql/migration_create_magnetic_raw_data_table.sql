-- ============================================
-- 磁性原始数据表
-- 数据库类型：MySQL
-- 创建日期：2025-01-XX
-- ============================================

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
