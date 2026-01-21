-- =============================================
-- 中间数据单元格颜色配置表
-- 注意：本脚本符合 .cursorrules 规范，使用基类标准字段名
-- =============================================

-- SQL Server 版本
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LAB_INTERMEDIATE_DATA_COLOR' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LAB_INTERMEDIATE_DATA_COLOR]
    (
        -- ============================================
        -- 来自 OEntityBase 的字段
        -- ============================================
        [F_Id] NVARCHAR(50) NOT NULL PRIMARY KEY,
        [F_TenantId] NVARCHAR(50) NULL,
        
        -- ============================================
        -- 来自 CLDEntityBase 的字段
        -- ============================================
        -- 创建相关字段（全大写，无下划线）
        [F_CREATORTIME] DATETIME NULL,
        [F_CREATORUSERID] NVARCHAR(50) NULL,
        [F_ENABLEDMARK] INT NULL DEFAULT 1,
        
        -- 修改和删除相关字段（F_ 后首字母大写）
        [F_LastModifyTime] DATETIME NULL,
        [F_LastModifyUserId] NVARCHAR(50) NULL,
        [F_DeleteMark] INT NULL DEFAULT 0,
        [F_DeleteTime] DATETIME NULL,
        [F_DeleteUserId] NVARCHAR(50) NULL,
        
        -- ============================================
        -- 业务字段
        -- ============================================
        [IntermediateDataId] NVARCHAR(50) NOT NULL,
        [FieldName] NVARCHAR(100) NOT NULL,
        [ColorValue] NVARCHAR(7) NOT NULL,
        [ProductSpecId] NVARCHAR(50) NOT NULL,
        [UpdateUserId] NVARCHAR(50) NULL,
        [UpdateTime] DATETIME NULL
    );

    -- 创建索引
    CREATE NONCLUSTERED INDEX [IX_IntermediateDataColor_DataId] ON [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ([IntermediateDataId]);
    CREATE NONCLUSTERED INDEX [IX_IntermediateDataColor_ProductSpecId] ON [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ([ProductSpecId]);
    CREATE NONCLUSTERED INDEX [IX_IntermediateDataColor_FieldName] ON [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ([FieldName]);
    CREATE NONCLUSTERED INDEX [IX_IntermediateDataColor_TenantId] ON [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ([F_TenantId]);
    CREATE NONCLUSTERED INDEX [IX_IntermediateDataColor_DeleteMark] ON [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ([F_DeleteMark]);
    CREATE UNIQUE NONCLUSTERED INDEX [IX_IntermediateDataColor_DataField] ON [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ([IntermediateDataId], [FieldName]) WHERE [F_DeleteMark] = 0;
END

-- MySQL 版本
CREATE TABLE IF NOT EXISTS `LAB_INTERMEDIATE_DATA_COLOR` (
    -- ============================================
    -- 来自 OEntityBase 的字段
    -- ============================================
    `F_Id` VARCHAR(50) NOT NULL COMMENT '主键ID',
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
    `IntermediateDataId` VARCHAR(50) NOT NULL COMMENT '中间数据ID',
    `FieldName` VARCHAR(100) NOT NULL COMMENT '字段名（列名）',
    `ColorValue` VARCHAR(7) NOT NULL COMMENT '颜色值（HEX格式，如#FF0000）',
    `ProductSpecId` VARCHAR(50) NOT NULL COMMENT '产品规格ID',
    `UpdateUserId` VARCHAR(50) DEFAULT NULL COMMENT '更新用户ID（业务字段）',
    `UpdateTime` DATETIME DEFAULT NULL COMMENT '更新时间（业务字段）',
    
    -- ============================================
    -- 主键和索引
    -- ============================================
    PRIMARY KEY (`F_Id`),
    KEY `IX_IntermediateDataColor_DataId` (`IntermediateDataId`),
    KEY `IX_IntermediateDataColor_ProductSpecId` (`ProductSpecId`),
    KEY `IX_IntermediateDataColor_FieldName` (`FieldName`),
    KEY `IX_IntermediateDataColor_TenantId` (`F_TenantId`),
    KEY `IX_IntermediateDataColor_DeleteMark` (`F_DeleteMark`),
    UNIQUE KEY `IX_IntermediateDataColor_DataField` (`IntermediateDataId`, `FieldName`, `F_DeleteMark`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='中间数据单元格颜色配置表';

-- Oracle 版本
-- CREATE TABLE "LAB_INTERMEDIATE_DATA_COLOR" (
--   -- 来自 OEntityBase 的字段
--   "F_Id" VARCHAR2(50) NOT NULL PRIMARY KEY,
--   "F_TenantId" VARCHAR2(50) NULL,
--   
--   -- 来自 CLDEntityBase 的字段
--   -- 创建相关字段（全大写，无下划线）
--   "F_CREATORTIME" DATE NULL,
--   "F_CREATORUSERID" VARCHAR2(50) NULL,
--   "F_ENABLEDMARK" NUMBER(10) NULL DEFAULT 1,
--   
--   -- 修改和删除相关字段（F_ 后首字母大写）
--   "F_LastModifyTime" DATE NULL,
--   "F_LastModifyUserId" VARCHAR2(50) NULL,
--   "F_DeleteMark" NUMBER(10) NULL DEFAULT 0,
--   "F_DeleteTime" DATE NULL,
--   "F_DeleteUserId" VARCHAR2(50) NULL,
--   
--   -- 业务字段
--   "IntermediateDataId" VARCHAR2(50) NOT NULL,
--   "FieldName" VARCHAR2(100) NOT NULL,
--   "ColorValue" VARCHAR2(7) NOT NULL,
--   "ProductSpecId" VARCHAR2(50) NOT NULL,
--   "UpdateUserId" VARCHAR2(50) NULL,
--   "UpdateTime" DATE NULL
-- );

-- CREATE INDEX "IX_IntermediateDataColor_DataId" ON "LAB_INTERMEDIATE_DATA_COLOR" ("IntermediateDataId");
-- CREATE INDEX "IX_IntermediateDataColor_ProductSpecId" ON "LAB_INTERMEDIATE_DATA_COLOR" ("ProductSpecId");
-- CREATE INDEX "IX_IntermediateDataColor_FieldName" ON "LAB_INTERMEDIATE_DATA_COLOR" ("FieldName");
-- CREATE INDEX "IX_IntermediateDataColor_TenantId" ON "LAB_INTERMEDIATE_DATA_COLOR" ("F_TenantId");
-- CREATE INDEX "IX_IntermediateDataColor_DeleteMark" ON "LAB_INTERMEDIATE_DATA_COLOR" ("F_DeleteMark");
-- CREATE UNIQUE INDEX "IX_IntermediateDataColor_DataField" ON "LAB_INTERMEDIATE_DATA_COLOR" ("IntermediateDataId", "FieldName", "F_DeleteMark");