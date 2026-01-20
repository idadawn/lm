-- ============================================
-- 中间数据表公式维护表（LAB_INTERMEDIATE_DATA_FORMULA）- 完整创建脚本
-- 数据库类型：MySQL
-- 创建日期：2025-01-XX
-- 说明：创建中间数据表公式维护表，用于存储中间数据表的计算公式
-- 注意：本脚本字段名与 IntermediateDataFormulaEntity 实体类映射一致
-- ============================================

-- 如果表已存在，先删除（仅用于开发环境，生产环境请谨慎使用）
DROP TABLE IF EXISTS `LAB_INTERMEDIATE_DATA_FORMULA`;

CREATE TABLE `LAB_INTERMEDIATE_DATA_FORMULA` (
    -- ============================================
    -- 来自 OEntityBase 的字段
    -- 注意：使用基类默认字段名 F_Id（I 是小写），不需要在实体类中重写
    -- ============================================
    `F_Id` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TenantId` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
    `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
    `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
    `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
    `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
    `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',
    
    -- ============================================
    -- 业务字段
    -- ============================================
    `F_TABLE_NAME` VARCHAR(50) NOT NULL DEFAULT 'INTERMEDIATE_DATA' COMMENT '表名（枚举：INTERMEDIATE_DATA，未来可扩展其他表）',
    `F_COLUMN_NAME` VARCHAR(100) NOT NULL COMMENT '中间数据表列名（对应 IntermediateDataEntity 的属性名）',
    `F_FORMULA_NAME` VARCHAR(100) NOT NULL COMMENT '公式名称（默认使用列名，可自定义）',
    `F_FORMULA` TEXT NOT NULL COMMENT '计算公式表达式（支持EXCEL风格公式）',
    `F_FORMULA_LANGUAGE` VARCHAR(20) NOT NULL DEFAULT 'EXCEL' COMMENT '公式语言：EXCEL（默认）、MATH',
    `F_UNIT_ID` VARCHAR(50) DEFAULT NULL COMMENT '单位ID（关联单位定义表）',
    `F_UNIT_NAME` VARCHAR(50) DEFAULT NULL COMMENT '单位名称（用于显示，冗余字段）',
    `F_PRECISION` INT DEFAULT NULL COMMENT '小数点保留位数（精度）',
    `F_IS_ENABLED` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用（1-启用，0-禁用）',
    `F_SORT_ORDER` INT NOT NULL DEFAULT 0 COMMENT '排序序号',
    `F_REMARK` VARCHAR(1000) DEFAULT NULL COMMENT '备注',
    
    -- ============================================
    -- 主键和索引
    -- ============================================
    PRIMARY KEY (`F_Id`),
    
    -- 唯一索引：表名+列名在同一租户和删除状态下唯一
    UNIQUE KEY `UK_TABLE_COLUMN` (`F_TABLE_NAME`, `F_COLUMN_NAME`, `F_TenantId`, `F_DeleteMark`),
    
    -- 普通索引
    KEY `IDX_TABLE_NAME` (`F_TABLE_NAME`),
    KEY `IDX_COLUMN_NAME` (`F_COLUMN_NAME`),
    KEY `IDX_UNIT_ID` (`F_UNIT_ID`),
    KEY `IDX_TENANT_ID` (`F_TenantId`),
    KEY `IDX_DELETE_MARK` (`F_DeleteMark`),
    KEY `IDX_SORT_ORDER` (`F_SORT_ORDER`),
    KEY `IDX_IS_ENABLED` (`F_IS_ENABLED`, `F_TenantId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='中间数据表公式维护表';

-- ============================================
-- 验证表结构
-- ============================================
-- 执行以下查询可以验证表结构是否正确创建：
-- SELECT
--     COLUMN_NAME AS '字段名',
--     DATA_TYPE AS '数据类型',
--     CHARACTER_MAXIMUM_LENGTH AS '长度',
--     IS_NULLABLE AS '可空',
--     COLUMN_DEFAULT AS '默认值',
--     COLUMN_COMMENT AS '注释'
-- FROM information_schema.COLUMNS
-- WHERE TABLE_SCHEMA = DATABASE()
--     AND TABLE_NAME = 'LAB_INTERMEDIATE_DATA_FORMULA'
-- ORDER BY ORDINAL_POSITION;

-- ============================================
-- SQL Server版本
-- ============================================
/*
CREATE TABLE [dbo].[LAB_INTERMEDIATE_DATA_FORMULA] (
    -- 来自 OEntityBase 的字段
    [F_Id] VARCHAR(50) NOT NULL PRIMARY KEY,
    [F_TenantId] VARCHAR(50) NULL,
    
    -- 来自 CLDEntityBase 的字段
    -- 创建相关字段（全大写，无下划线）
    [F_CREATORTIME] DATETIME NULL,
    [F_CREATORUSERID] VARCHAR(50) NULL,
    [F_ENABLEDMARK] INT DEFAULT 1,
    
    -- 修改和删除相关字段（F_ 后首字母大写）
    [F_LastModifyTime] DATETIME NULL,
    [F_LastModifyUserId] VARCHAR(50) NULL,
    [F_DeleteMark] INT DEFAULT 0,
    [F_DeleteTime] DATETIME NULL,
    [F_DeleteUserId] VARCHAR(50) NULL,
    
    -- 业务字段
    [F_TABLE_NAME] VARCHAR(50) NOT NULL DEFAULT 'INTERMEDIATE_DATA',
    [F_COLUMN_NAME] VARCHAR(100) NOT NULL,
    [F_FORMULA_NAME] VARCHAR(100) NOT NULL,
    [F_FORMULA] NVARCHAR(MAX) NOT NULL,
    [F_FORMULA_LANGUAGE] VARCHAR(20) NOT NULL DEFAULT 'EXCEL',
    [F_UNIT_ID] VARCHAR(50) NULL,
    [F_UNIT_NAME] VARCHAR(50) NULL,
    [F_PRECISION] INT NULL,
    [F_IS_ENABLED] BIT NOT NULL DEFAULT 1,
    [F_SORT_ORDER] INT NOT NULL DEFAULT 0,
    [F_REMARK] NVARCHAR(1000) NULL,
    
    CONSTRAINT [UK_TABLE_COLUMN] UNIQUE ([F_TABLE_NAME], [F_COLUMN_NAME], [F_TenantId], [F_DeleteMark])
);

CREATE INDEX [IDX_TABLE_NAME] ON [dbo].[LAB_INTERMEDIATE_DATA_FORMULA] ([F_TABLE_NAME]);
CREATE INDEX [IDX_COLUMN_NAME] ON [dbo].[LAB_INTERMEDIATE_DATA_FORMULA] ([F_COLUMN_NAME]);
CREATE INDEX [IDX_UNIT_ID] ON [dbo].[LAB_INTERMEDIATE_DATA_FORMULA] ([F_UNIT_ID]);
CREATE INDEX [IDX_TENANT_ID] ON [dbo].[LAB_INTERMEDIATE_DATA_FORMULA] ([F_TenantId]);
CREATE INDEX [IDX_DELETE_MARK] ON [dbo].[LAB_INTERMEDIATE_DATA_FORMULA] ([F_DeleteMark]);
CREATE INDEX [IDX_SORT_ORDER] ON [dbo].[LAB_INTERMEDIATE_DATA_FORMULA] ([F_SORT_ORDER]);
CREATE INDEX [IDX_IS_ENABLED] ON [dbo].[LAB_INTERMEDIATE_DATA_FORMULA] ([F_IS_ENABLED], [F_TenantId]);
*/
