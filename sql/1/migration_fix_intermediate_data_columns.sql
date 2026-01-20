-- 修复中间数据表缺少字段的问题
-- 此脚本会添加缺失的字段，如果字段已存在会报错但可以忽略

-- MySQL 版本

-- 1. 检查并添加 F_SPRAY_NO 字段（如果不存在）
-- 注意：如果 F_SPRAY_NO 已存在，此语句会报错，可以忽略
ALTER TABLE `LAB_INTERMEDIATE_DATA`
ADD COLUMN `F_SPRAY_NO` VARCHAR(50) NULL COMMENT '喷次（8位日期-炉号）';

-- 2. 检查并添加 F_BATCH_NO 字段（如果不存在）
-- 注意：如果 F_BATCH_NO 已存在，此语句会报错，可以忽略
ALTER TABLE `LAB_INTERMEDIATE_DATA`
ADD COLUMN `F_BATCH_NO` VARCHAR(100) NULL COMMENT '批次（产线数字+班次汉字+8位日期-炉号）';

-- 如果需要指定字段位置，可以使用以下语句（需要先确认相关字段是否存在）
-- ALTER TABLE `LAB_INTERMEDIATE_DATA`
-- ADD COLUMN `F_SPRAY_NO` VARCHAR(50) NULL COMMENT '喷次（8位日期-炉号）' AFTER `F_SHIFT_NO`;
-- ALTER TABLE `LAB_INTERMEDIATE_DATA`
-- ADD COLUMN `F_BATCH_NO` VARCHAR(100) NULL COMMENT '批次（产线数字+班次汉字+8位日期-炉号）' AFTER `F_SPRAY_NO`;

-- SQL Server 版本
/*
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA') 
    AND name = 'F_SPRAY_NO'
)
BEGIN
    ALTER TABLE [LAB_INTERMEDIATE_DATA]
    ADD [F_SPRAY_NO] VARCHAR(50) NULL;
    
    EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'喷次（8位日期-炉号）',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'LAB_INTERMEDIATE_DATA',
    @level2type = N'COLUMN', @level2name = N'F_SPRAY_NO';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('LAB_INTERMEDIATE_DATA') 
    AND name = 'F_BATCH_NO'
)
BEGIN
    ALTER TABLE [LAB_INTERMEDIATE_DATA]
    ADD [F_BATCH_NO] VARCHAR(100) NULL;
    
    EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'批次（产线数字+班次汉字+8位日期-炉号）',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'LAB_INTERMEDIATE_DATA',
    @level2type = N'COLUMN', @level2name = N'F_BATCH_NO';
END
*/
