-- 简单版本：只添加 F_PARSED_DATA_FILE 字段
-- 解决 "解析数据文件不存在" 错误

-- 检查并添加 F_PARSED_DATA_FILE 字段
SET @db_name = DATABASE();

-- 检查字段是否存在
SELECT COUNT(*) INTO @field_exists
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db_name
    AND TABLE_NAME = 'lab_raw_data_import_session'
    AND COLUMN_NAME = 'F_PARSED_DATA_FILE';

-- 如果字段不存在，则添加
SET @sql = IF(@field_exists = 0,
    'ALTER TABLE `lab_raw_data_import_session` ADD COLUMN `F_PARSED_DATA_FILE` VARCHAR(500) DEFAULT NULL COMMENT \'解析后的数据JSON文件路径（临时存储，完成导入后才写入数据库）\';',
    'SELECT \"F_PARSED_DATA_FILE 字段已存在，无需添加\" AS Result;'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 验证字段已添加
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_COMMENT,
    CASE WHEN COLUMN_NAME = 'F_PARSED_DATA_FILE' THEN '✓ 已添加/存在' ELSE '原有字段' END AS 状态
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db_name
    AND TABLE_NAME = 'lab_raw_data_import_session'
    AND (COLUMN_NAME = 'F_PARSED_DATA_FILE' OR COLUMN_NAME = 'F_SOURCE_FILE_ID')
ORDER BY COLUMN_NAME;