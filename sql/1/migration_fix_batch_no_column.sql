-- 修复中间数据表缺少 F_BATCH_NO 字段的问题
-- 注意：如果字段已存在，执行此脚本会报错，但不影响数据

-- MySQL 版本
-- 直接添加字段（如果字段已存在会报错，可以忽略）
ALTER TABLE `LAB_INTERMEDIATE_DATA`
ADD COLUMN `F_BATCH_NO` VARCHAR(100) NULL COMMENT '批次（产线数字+班次汉字+8位日期-炉号）';

-- 如果需要指定字段位置，可以使用 AFTER 子句（需要先确认 F_SPRAY_NO 是否存在）
-- ALTER TABLE `LAB_INTERMEDIATE_DATA`
-- ADD COLUMN `F_BATCH_NO` VARCHAR(100) NULL COMMENT '批次（产线数字+班次汉字+8位日期-炉号）' AFTER `F_SPRAY_NO`;

-- SQL Server 版本
/*
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
