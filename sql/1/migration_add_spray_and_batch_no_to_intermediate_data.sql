-- 为中间数据表添加喷次和批次字段
-- 1. 重命名现有的 F_SPRAY_NO 列为 F_SHIFT_NO
-- 2. 添加新的 F_SPRAY_NO 列（喷次：8位日期-炉号）
-- 3. 添加新的 F_BATCH_NO 列（批次：产线数字+班次汉字+8位日期-炉号）

-- MySQL 版本
ALTER TABLE LAB_INTERMEDIATE_DATA
CHANGE COLUMN F_SPRAY_NO F_SHIFT_NO VARCHAR(100) NULL COMMENT '班次（产线+班次+日期+炉号组合）';

ALTER TABLE LAB_INTERMEDIATE_DATA
ADD COLUMN F_SPRAY_NO VARCHAR(50) NULL COMMENT '喷次（8位日期-炉号）' AFTER F_SHIFT_NO;

ALTER TABLE LAB_INTERMEDIATE_DATA
ADD COLUMN F_BATCH_NO VARCHAR(100) NULL COMMENT '批次（产线数字+班次汉字+8位日期-炉号）' AFTER F_SPRAY_NO;

-- SQL Server 版本
/*
EXEC sp_rename 'LAB_INTERMEDIATE_DATA.F_SPRAY_NO', 'F_SHIFT_NO', 'COLUMN';

ALTER TABLE LAB_INTERMEDIATE_DATA
ADD F_SPRAY_NO VARCHAR(50) NULL;

EXEC sp_addextendedproperty
@name = N'MS_Description',
@value = N'喷次（8位日期-炉号）',
@level0type = N'SCHEMA', @level0name = N'dbo',
@level1type = N'TABLE', @level1name = N'LAB_INTERMEDIATE_DATA',
@level2type = N'COLUMN', @level2name = N'F_SPRAY_NO';

ALTER TABLE LAB_INTERMEDIATE_DATA
ADD F_BATCH_NO VARCHAR(100) NULL;

EXEC sp_addextendedproperty
@name = N'MS_Description',
@value = N'批次（产线数字+班次汉字+8位日期-炉号）',
@level0type = N'SCHEMA', @level0name = N'dbo',
@level1type = N'TABLE', @level1name = N'LAB_INTERMEDIATE_DATA',
@level2type = N'COLUMN', @level2name = N'F_BATCH_NO';
*/