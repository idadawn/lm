-- ============================================
-- 修复 F_DETECTION_COLUMNS 字段类型
-- 数据库类型：MySQL
-- 创建日期：2025-01-21
-- 说明：将 F_DETECTION_COLUMNS 从 VARCHAR 安全转换为 INT
-- ============================================

-- 步骤1：清理无效数据（将非数字字符串转换为 NULL）
-- 如果字段值不是有效的整数，则设置为 NULL
-- 这样可以避免在修改字段类型时出现转换错误
UPDATE `LAB_INTERMEDIATE_DATA`
SET `F_DETECTION_COLUMNS` = NULL
WHERE `F_DETECTION_COLUMNS` IS NOT NULL
  AND (
    -- 检查是否包含非数字字符
    `F_DETECTION_COLUMNS` REGEXP '[^0-9]'
    OR
    -- 检查是否为空字符串或只包含空格
    TRIM(`F_DETECTION_COLUMNS`) = ''
    OR
    -- 检查数值是否超出 INT 范围
    (CAST(`F_DETECTION_COLUMNS` AS UNSIGNED) > 2147483647)
  );

-- 步骤2：修改字段类型为 INT
-- MySQL 会自动将有效的数字字符串转换为整数
-- 如果转换失败（比如包含非数字字符），会报错，所以需要先执行步骤1清理数据
-- 注意：如果字段已经是 INT 类型，这个语句会报错，可以忽略
ALTER TABLE `LAB_INTERMEDIATE_DATA`
MODIFY COLUMN `F_DETECTION_COLUMNS` INT NULL COMMENT '检测列';

-- ============================================
-- 验证：检查字段类型是否已修改
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
--     AND COLUMN_NAME = 'F_DETECTION_COLUMNS';
