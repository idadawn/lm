-- =============================================
-- 修复中间数据单元格颜色配置表字段
-- 用于更新现有表结构，添加缺失的基类字段
-- 注意：本脚本符合 .cursorrules 规范
-- =============================================

-- SQL Server 版本
IF EXISTS (SELECT * FROM sysobjects WHERE name='LAB_INTERMEDIATE_DATA_COLOR' AND xtype='U')
BEGIN
    -- 添加缺失的基类字段（如果不存在）
    
    -- 来自 OEntityBase 的字段
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_TenantId')
    BEGIN
        ALTER TABLE [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ADD [F_TenantId] NVARCHAR(50) NULL;
    END
    
    -- 来自 CLDEntityBase 的字段
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_CREATORTIME')
    BEGIN
        ALTER TABLE [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ADD [F_CREATORTIME] DATETIME NULL;
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_CREATORUSERID')
    BEGIN
        ALTER TABLE [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ADD [F_CREATORUSERID] NVARCHAR(50) NULL;
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_ENABLEDMARK')
    BEGIN
        ALTER TABLE [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ADD [F_ENABLEDMARK] INT NULL DEFAULT 1;
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_LastModifyTime')
    BEGIN
        ALTER TABLE [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ADD [F_LastModifyTime] DATETIME NULL;
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_LastModifyUserId')
    BEGIN
        ALTER TABLE [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ADD [F_LastModifyUserId] NVARCHAR(50) NULL;
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_DeleteMark')
    BEGIN
        ALTER TABLE [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ADD [F_DeleteMark] INT NULL DEFAULT 0;
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_DeleteTime')
    BEGIN
        ALTER TABLE [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ADD [F_DeleteTime] DATETIME NULL;
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_DeleteUserId')
    BEGIN
        ALTER TABLE [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ADD [F_DeleteUserId] NVARCHAR(50) NULL;
    END
    
    -- 重命名主键字段（如果使用的是旧字段名）
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'Id')
        AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_Id')
    BEGIN
        EXEC sp_rename 'LAB_INTERMEDIATE_DATA_COLOR.Id', 'F_Id', 'COLUMN';
    END
    
    -- 迁移旧字段数据到新字段（如果存在旧字段）
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'CreatorUserId')
        AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_CREATORUSERID')
    BEGIN
        UPDATE [dbo].[LAB_INTERMEDIATE_DATA_COLOR]
        SET [F_CREATORUSERID] = [CreatorUserId]
        WHERE [F_CREATORUSERID] IS NULL AND [CreatorUserId] IS NOT NULL;
    END
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'CreatorTime')
        AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR') AND name = 'F_CREATORTIME')
    BEGIN
        UPDATE [dbo].[LAB_INTERMEDIATE_DATA_COLOR]
        SET [F_CREATORTIME] = [CreatorTime]
        WHERE [F_CREATORTIME] IS NULL AND [CreatorTime] IS NOT NULL;
    END
    
    -- 创建缺失的索引
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_IntermediateDataColor_TenantId' AND object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_IntermediateDataColor_TenantId] ON [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ([F_TenantId]);
    END
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_IntermediateDataColor_DeleteMark' AND object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA_COLOR'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_IntermediateDataColor_DeleteMark] ON [dbo].[LAB_INTERMEDIATE_DATA_COLOR] ([F_DeleteMark]);
    END
END

-- MySQL 版本
-- 检查并添加缺失的字段
SET @dbname = DATABASE();
SET @tablename = 'LAB_INTERMEDIATE_DATA_COLOR';
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = 'F_TenantId')
    ) > 0,
    "SELECT 'F_TenantId already exists in LAB_INTERMEDIATE_DATA_COLOR'",
    "ALTER TABLE `LAB_INTERMEDIATE_DATA_COLOR` ADD COLUMN `F_TenantId` VARCHAR(50) DEFAULT NULL COMMENT '租户ID'"
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 添加 F_CREATORTIME
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = 'F_CREATORTIME')
    ) > 0,
    "SELECT 'F_CREATORTIME already exists in LAB_INTERMEDIATE_DATA_COLOR'",
    "ALTER TABLE `LAB_INTERMEDIATE_DATA_COLOR` ADD COLUMN `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间'"
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 添加 F_CREATORUSERID
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = 'F_CREATORUSERID')
    ) > 0,
    "SELECT 'F_CREATORUSERID already exists in LAB_INTERMEDIATE_DATA_COLOR'",
    "ALTER TABLE `LAB_INTERMEDIATE_DATA_COLOR` ADD COLUMN `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID'"
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 添加 F_ENABLEDMARK
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = 'F_ENABLEDMARK')
    ) > 0,
    "SELECT 'F_ENABLEDMARK already exists in LAB_INTERMEDIATE_DATA_COLOR'",
    "ALTER TABLE `LAB_INTERMEDIATE_DATA_COLOR` ADD COLUMN `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）'"
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 添加 F_LastModifyTime
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = 'F_LastModifyTime')
    ) > 0,
    "SELECT 'F_LastModifyTime already exists in LAB_INTERMEDIATE_DATA_COLOR'",
    "ALTER TABLE `LAB_INTERMEDIATE_DATA_COLOR` ADD COLUMN `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间'"
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 添加 F_LastModifyUserId
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = 'F_LastModifyUserId')
    ) > 0,
    "SELECT 'F_LastModifyUserId already exists in LAB_INTERMEDIATE_DATA_COLOR'",
    "ALTER TABLE `LAB_INTERMEDIATE_DATA_COLOR` ADD COLUMN `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID'"
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 添加 F_DeleteMark
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = 'F_DeleteMark')
    ) > 0,
    "SELECT 'F_DeleteMark already exists in LAB_INTERMEDIATE_DATA_COLOR'",
    "ALTER TABLE `LAB_INTERMEDIATE_DATA_COLOR` ADD COLUMN `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）'"
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 添加 F_DeleteTime
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = 'F_DeleteTime')
    ) > 0,
    "SELECT 'F_DeleteTime already exists in LAB_INTERMEDIATE_DATA_COLOR'",
    "ALTER TABLE `LAB_INTERMEDIATE_DATA_COLOR` ADD COLUMN `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间'"
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 添加 F_DeleteUserId
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = 'F_DeleteUserId')
    ) > 0,
    "SELECT 'F_DeleteUserId already exists in LAB_INTERMEDIATE_DATA_COLOR'",
    "ALTER TABLE `LAB_INTERMEDIATE_DATA_COLOR` ADD COLUMN `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID'"
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 重命名主键字段（如果使用的是旧字段名）
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = 'Id')
    ) > 0
    AND
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = 'F_Id')
    ) = 0,
    "ALTER TABLE `LAB_INTERMEDIATE_DATA_COLOR` CHANGE COLUMN `Id` `F_Id` VARCHAR(50) NOT NULL COMMENT '主键ID'",
    "SELECT 'F_Id already exists or Id does not exist'"
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 迁移旧字段数据到新字段（如果存在旧字段）
UPDATE `LAB_INTERMEDIATE_DATA_COLOR`
SET `F_CREATORUSERID` = `CreatorUserId`
WHERE `F_CREATORUSERID` IS NULL AND `CreatorUserId` IS NOT NULL
AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'CreatorUserId')
AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'F_CREATORUSERID');

UPDATE `LAB_INTERMEDIATE_DATA_COLOR`
SET `F_CREATORTIME` = `CreatorTime`
WHERE `F_CREATORTIME` IS NULL AND `CreatorTime` IS NOT NULL
AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'CreatorTime')
AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'F_CREATORTIME');

-- 创建缺失的索引
CREATE INDEX IF NOT EXISTS `IX_IntermediateDataColor_TenantId` ON `LAB_INTERMEDIATE_DATA_COLOR` (`F_TenantId`);
CREATE INDEX IF NOT EXISTS `IX_IntermediateDataColor_DeleteMark` ON `LAB_INTERMEDIATE_DATA_COLOR` (`F_DeleteMark`);
