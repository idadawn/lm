-- ============================================
-- 指标定义表（lab_metric_definition）
-- 数据库类型：MySQL
-- 创建日期：2026-01-27
-- 说明：创建指标定义表，用于存储可配置的指标计算公式
-- ============================================

CREATE TABLE IF NOT EXISTS `lab_metric_definition` (
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
    `name` VARCHAR(100) NOT NULL COMMENT '指标名称',
    `code` VARCHAR(50) NOT NULL COMMENT '指标代码（唯一标识，用于公式引用）',
    `description` VARCHAR(500) DEFAULT NULL COMMENT '指标描述',
    `formula` TEXT NOT NULL COMMENT '计算公式（支持四则运算和变量引用）',
    `unit_id` VARCHAR(50) DEFAULT NULL COMMENT '单位ID，关联unit_definition表',
    `category` VARCHAR(100) DEFAULT NULL COMMENT '指标分类（用于分组管理）',
    `status` TINYINT DEFAULT 1 COMMENT '状态：1-启用，0-禁用',
    `sort_order` INT DEFAULT 0 COMMENT '排序序号（用于界面显示顺序）',
    `remark` VARCHAR(1000) DEFAULT NULL COMMENT '备注信息',
    
    PRIMARY KEY (`F_ID`),
    UNIQUE KEY `UK_CODE_TENANT` (`code`, `F_TENANTID`, `F_DeleteMark`),
    KEY `IDX_CATEGORY` (`category`, `F_TENANTID`),
    KEY `IDX_STATUS` (`status`, `F_TENANTID`),
    KEY `IDX_TENANT_ID` (`F_TENANTID`),
    KEY `IDX_DELETE_MARK` (`F_DeleteMark`),
    KEY `IDX_CREATOR_TIME` (`F_CREATORTIME`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='指标定义表';
