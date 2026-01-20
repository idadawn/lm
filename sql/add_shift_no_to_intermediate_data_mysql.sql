-- ============================================
-- 为 LAB_INTERMEDIATE_DATA 表添加班次字段
-- 数据库类型：MySQL
-- 创建日期：2025-01-21
-- 说明：添加 F_SHIFT_NO 列（班次），与实体 IntermediateDataEntity.ShiftNo 对齐
-- 注意：如果列已存在，执行会报错，可以忽略该错误
-- ============================================

-- 添加 F_SHIFT_NO 列（班次）
-- 对应实体属性 IntermediateDataEntity.ShiftNo
-- 如果列已存在，会报错，可以忽略
ALTER TABLE `LAB_INTERMEDIATE_DATA`
ADD COLUMN `F_SHIFT_NO` VARCHAR(20) DEFAULT NULL COMMENT '班次'
AFTER `F_FURNACE_NO_FORMATTED`;

-- ============================================
-- 验证：检查列是否已添加
-- ============================================
-- SELECT
--     COLUMN_NAME AS '字段名',
--     DATA_TYPE AS '数据类型',
--     CHARACTER_MAXIMUM_LENGTH AS '长度',
--     IS_NULLABLE AS '可空',
--     COLUMN_DEFAULT AS '默认值',
--     COLUMN_COMMENT AS '注释'
-- FROM information_schema.COLUMNS
-- WHERE TABLE_SCHEMA = DATABASE()
--     AND TABLE_NAME = 'LAB_INTERMEDIATE_DATA'
--     AND COLUMN_NAME = 'F_SHIFT_NO'
-- ORDER BY ORDINAL_POSITION;
