-- ============================================
-- 为 LAB_INTERMEDIATE_DATA 表添加缺失的列
-- 数据库类型：MySQL
-- 创建日期：2025-01-20
-- 说明：
--   1) 添加缺失的 F_STRIP_WIDTH 列（带宽）
--   2) 添加缺失的 F_FOUR_METER_WT 列（四米带材重量），与实体 IntermediateDataEntity 对齐
-- 注意：如果列已存在，执行会报错，可以忽略该错误
-- ============================================

-- 添加 F_STRIP_WIDTH 列（带宽）
-- 如果列已存在，会报错，可以忽略
ALTER TABLE `LAB_INTERMEDIATE_DATA`
ADD COLUMN `F_STRIP_WIDTH` DECIMAL(18, 2) DEFAULT NULL COMMENT '带宽'
AFTER `F_ONE_METER_WT`;

-- 添加 F_FOUR_METER_WT 列（四米带材重量）
-- 对应实体属性 IntermediateDataEntity.FourMeterWeight
-- 一米带材重量 F_ONE_METER_WT = F_FOUR_METER_WT / STD_LENGTH
ALTER TABLE `LAB_INTERMEDIATE_DATA`
ADD COLUMN `F_FOUR_METER_WT` DECIMAL(18, 1) DEFAULT NULL COMMENT '四米带材重量'
AFTER `F_LAM_FACTOR_RES`;

-- ============================================
-- 验证：检查列是否已添加
-- ============================================
-- SELECT
--     COLUMN_NAME AS '字段名',
--     DATA_TYPE AS '数据类型',
--     CHARACTER_MAXIMUM_LENGTH AS '长度',
--     NUMERIC_PRECISION AS '精度',
--     NUMERIC_SCALE AS '小数位数',
--     IS_NULLABLE AS '可空',
--     COLUMN_DEFAULT AS '默认值',
--     COLUMN_COMMENT AS '注释'
-- FROM information_schema.COLUMNS
-- WHERE TABLE_SCHEMA = DATABASE()
--     AND TABLE_NAME = 'LAB_INTERMEDIATE_DATA'
--     AND COLUMN_NAME IN ('F_STRIP_WIDTH', 'F_FOUR_METER_WT')
-- ORDER BY ORDINAL_POSITION;
