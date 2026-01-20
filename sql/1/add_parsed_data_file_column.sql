-- 添加缺失的 F_PARSED_DATA_FILE 字段到 lab_raw_data_import_session 表
-- 修复 "解析数据文件不存在" 错误

DROP PROCEDURE IF EXISTS `add_parsed_data_file_column`;
DELIMITER $$
CREATE PROCEDURE `add_parsed_data_file_column`()
BEGIN
    -- 检查 F_PARSED_DATA_FILE 字段是否存在
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS
                  WHERE TABLE_SCHEMA=DATABASE()
                  AND TABLE_NAME='lab_raw_data_import_session'
                  AND COLUMN_NAME='F_PARSED_DATA_FILE') THEN
        -- 添加 F_PARSED_DATA_FILE 字段
        ALTER TABLE `lab_raw_data_import_session`
        ADD COLUMN `F_PARSED_DATA_FILE` VARCHAR(500) DEFAULT NULL
        COMMENT '解析后的数据JSON文件路径（临时存储，完成导入后才写入数据库）';

        SELECT 'F_PARSED_DATA_FILE 字段已成功添加' AS Result;
    ELSE
        SELECT 'F_PARSED_DATA_FILE 字段已存在' AS Result;
    END IF;

    -- 同时检查其他可能缺失的字段
    -- F_DeleteMark
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS
                  WHERE TABLE_SCHEMA=DATABASE()
                  AND TABLE_NAME='lab_raw_data_import_session'
                  AND COLUMN_NAME='F_DeleteMark') THEN
        ALTER TABLE `lab_raw_data_import_session`
        ADD COLUMN `F_DeleteMark` INT(11) DEFAULT '0' COMMENT '删除标志';
    END IF;

    -- F_DeleteTime
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS
                  WHERE TABLE_SCHEMA=DATABASE()
                  AND TABLE_NAME='lab_raw_data_import_session'
                  AND COLUMN_NAME='F_DeleteTime') THEN
        ALTER TABLE `lab_raw_data_import_session`
        ADD COLUMN `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间';
    END IF;

    -- F_DeleteUserId
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS
                  WHERE TABLE_SCHEMA=DATABASE()
                  AND TABLE_NAME='lab_raw_data_import_session'
                  AND COLUMN_NAME='F_DeleteUserId') THEN
        ALTER TABLE `lab_raw_data_import_session`
        ADD COLUMN `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户';
    END IF;

    -- F_LastModifyUserId
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS
                  WHERE TABLE_SCHEMA=DATABASE()
                  AND TABLE_NAME='lab_raw_data_import_session'
                  AND COLUMN_NAME='F_LastModifyUserId') THEN
        ALTER TABLE `lab_raw_data_import_session`
        ADD COLUMN `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户';
    END IF;

    -- F_TenantId
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS
                  WHERE TABLE_SCHEMA=DATABASE()
                  AND TABLE_NAME='lab_raw_data_import_session'
                  AND COLUMN_NAME='F_TenantId') THEN
        ALTER TABLE `lab_raw_data_import_session`
        ADD COLUMN `F_TenantId` VARCHAR(50) DEFAULT NULL COMMENT '租户ID';
    END IF;

    -- F_ENABLEDMARK
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS
                  WHERE TABLE_SCHEMA=DATABASE()
                  AND TABLE_NAME='lab_raw_data_import_session'
                  AND COLUMN_NAME='F_ENABLEDMARK') THEN
        ALTER TABLE `lab_raw_data_import_session`
        ADD COLUMN `F_ENABLEDMARK` INT(11) DEFAULT '1' COMMENT '启用标识';
    END IF;

    -- 修复 F_SOURCE_FILE_ID 长度（从50改为500）
    ALTER TABLE `lab_raw_data_import_session`
    MODIFY COLUMN `F_SOURCE_FILE_ID` VARCHAR(500) DEFAULT NULL COMMENT 'Excel源文件ID';

END $$
DELIMITER ;

-- 执行存储过程
CALL `add_parsed_data_file_column`();

-- 删除存储过程
DROP PROCEDURE `add_parsed_data_file_column`;

-- 验证字段已添加
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_COMMENT
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA=DATABASE()
AND TABLE_NAME='lab_raw_data_import_session'
AND COLUMN_NAME IN ('F_PARSED_DATA_FILE', 'F_SOURCE_FILE_ID')
ORDER BY COLUMN_NAME;