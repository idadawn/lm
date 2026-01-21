-- ============================================
-- 为 LAB_INTERMEDIATE_DATA 表添加是否刻痕标识字段
-- 数据库类型：MySQL
-- 创建日期：2025-01-21
-- 说明：添加 F_IS_SCRATCHED 列（是否刻痕），用于标识中间数据是否有刻痕数据
-- 注意：如果列已存在，执行会报错，可以忽略该错误
-- ============================================

-- 添加 F_IS_SCRATCHED 列（是否刻痕：0-否，1-是）
-- 对应实体属性 IntermediateDataEntity.IsScratched
-- 如果列已存在，会报错，可以忽略
ALTER TABLE `LAB_INTERMEDIATE_DATA`
ADD COLUMN `F_IS_SCRATCHED` INT DEFAULT NULL COMMENT '是否刻痕（0-否，1-是，标识是否有刻痕数据）'
AFTER `F_AFTER_HC`;

-- ============================================
-- 验证：检查列是否已添加
-- ============================================
-- SELECT
--     COLUMN_NAME AS '字段名',
--     DATA_TYPE AS '数据类型',
--     IS_NULLABLE AS '可空',
--     COLUMN_DEFAULT AS '默认值',
--     COLUMN_COMMENT AS '注释'
-- FROM information_schema.COLUMNS
-- WHERE TABLE_SCHEMA = DATABASE()
--     AND TABLE_NAME = 'LAB_INTERMEDIATE_DATA'
--     AND COLUMN_NAME = 'F_IS_SCRATCHED'
-- ORDER BY ORDINAL_POSITION;
