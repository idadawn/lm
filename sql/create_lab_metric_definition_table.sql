-- ============================================
-- 指标定义表（lab_metric_definition）- 完整创建脚本
-- 数据库类型：MySQL
-- 创建日期：2026-01-27
-- 说明：创建指标定义表，用于存储可配置的指标计算公式
-- 注意：本脚本字段名与 MetricDefinitionEntity 实体类映射一致
-- ============================================

-- 如果表已存在，先删除（仅用于开发环境，生产环境请谨慎使用）
DROP TABLE IF EXISTS `lab_metric_definition`;

CREATE TABLE `lab_metric_definition` (
    -- ============================================
    -- 来自 OEntityBase 的字段
    -- 注意：实体类重写了 Id 属性，使用 F_ID（全大写）
    -- ============================================
    `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TenantId` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    
    -- ============================================
    -- 来自 CLDEntityBase 的字段
    -- ============================================
    -- 创建相关字段（全大写，无下划线）
    `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
    `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
    
    -- 修改和删除相关字段（F_ 后首字母大写）
    `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
    `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
    `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
    `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',
    
    -- ============================================
    -- 业务字段
    -- ============================================
    `name` VARCHAR(100) NOT NULL COMMENT '指标名称',
    `code` VARCHAR(50) NOT NULL COMMENT '指标代码（唯一标识，用于公式引用）',
    `description` VARCHAR(500) DEFAULT NULL COMMENT '指标描述',
    `formula` TEXT NOT NULL COMMENT '计算公式（支持四则运算和变量引用）',
    
    -- 公式相关字段
    `formula_language` VARCHAR(20) DEFAULT 'EXCEL' COMMENT '公式语言：EXCEL（默认）、MATH、SCRIPT',
    `metric_type` VARCHAR(20) DEFAULT 'STANDARD' COMMENT '指标类型：STANDARD（标准）、COMPOSITE（复合）、ATOMIC（原子）',
    `return_type` VARCHAR(20) DEFAULT 'NUMBER' COMMENT '返回值类型：NUMBER（默认）、STRING、BOOLEAN、DATETIME',
    
    -- 单位相关字段
    `unit_id` VARCHAR(50) DEFAULT NULL COMMENT '单位ID，关联unit_definition表',
    `unit_name` VARCHAR(50) DEFAULT NULL COMMENT '单位名称（用于显示）',
    
    -- 分类和状态字段
    `category` VARCHAR(100) DEFAULT NULL COMMENT '指标分类（用于分组管理）',
    `is_enabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用（1-启用，0-禁用）',
    `status` TINYINT DEFAULT 1 COMMENT '状态：1-启用，0-禁用（用于数据库存储，兼容字段）',
    `is_system` TINYINT(1) DEFAULT 0 COMMENT '是否为系统内置指标（0-否，1-是）',
    
    -- 版本管理字段
    `version` INT DEFAULT 1 COMMENT '当前版本号',
    `is_versioned` TINYINT(1) DEFAULT 0 COMMENT '是否启用版本管理（0-否，1-是）',
    
    -- 变量绑定字段
    `has_variables` TINYINT(1) DEFAULT 0 COMMENT '是否有变量绑定（0-否，1-是）',
    
    -- 计算相关字段
    `store_results` TINYINT(1) DEFAULT 0 COMMENT '是否存储计算结果（0-否，1-是）',
    `precision` INT DEFAULT NULL COMMENT '小数位数（精度）',
    `calculation_order` INT DEFAULT 0 COMMENT '计算顺序（用于解决依赖关系）',
    
    -- 其他字段
    `sort_order` INT DEFAULT 0 COMMENT '排序序号（用于界面显示顺序）',
    `remark` VARCHAR(1000) DEFAULT NULL COMMENT '备注信息',
    
    -- ============================================
    -- 主键和索引
    -- ============================================
    PRIMARY KEY (`F_ID`),
    
    -- 唯一索引：指标代码在同一租户和删除状态下唯一
    UNIQUE KEY `UK_CODE_TENANT` (`code`, `F_TenantId`, `F_DeleteMark`),
    
    -- 普通索引
    KEY `IDX_TENANT_ID` (`F_TenantId`),
    KEY `IDX_DELETE_MARK` (`F_DeleteMark`),
    KEY `IDX_CREATOR_TIME` (`F_CREATORTIME`),
    KEY `IDX_CATEGORY` (`category`, `F_TenantId`),
    KEY `IDX_STATUS` (`status`, `F_TenantId`),
    KEY `IDX_IS_ENABLED` (`is_enabled`, `F_TenantId`),
    KEY `IDX_METRIC_TYPE` (`metric_type`, `F_TenantId`),
    KEY `IDX_RETURN_TYPE` (`return_type`, `F_TenantId`),
    KEY `IDX_IS_VERSIONED` (`is_versioned`, `F_TenantId`),
    KEY `IDX_HAS_VARIABLES` (`has_variables`, `F_TenantId`),
    KEY `IDX_STORE_RESULTS` (`store_results`, `F_TenantId`),
    KEY `IDX_IS_SYSTEM` (`is_system`, `F_TenantId`),
    KEY `IDX_SORT_ORDER` (`sort_order`, `F_TenantId`),
    KEY `IDX_CALCULATION_ORDER` (`calculation_order`, `F_TenantId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='指标定义表';

-- ============================================
-- 验证表结构
-- ============================================
-- 执行以下查询可以验证表结构是否正确创建：
SELECT
    COLUMN_NAME AS '字段名',
    DATA_TYPE AS '数据类型',
    CHARACTER_MAXIMUM_LENGTH AS '长度',
    IS_NULLABLE AS '可空',
    COLUMN_DEFAULT AS '默认值',
    COLUMN_COMMENT AS '注释'
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'lab_metric_definition'
ORDER BY ORDINAL_POSITION;
