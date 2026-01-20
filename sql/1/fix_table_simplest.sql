-- 最简化版本：修复 lab_raw_data_import_session 表
-- 执行此脚本即可修复 "解析数据文件不存在" 错误

-- 1. 添加 F_PARSED_DATA_FILE 字段（如果不存在）
ALTER TABLE `lab_raw_data_import_session`
ADD COLUMN IF NOT EXISTS `F_PARSED_DATA_FILE` VARCHAR(500) DEFAULT NULL
COMMENT '解析后的数据JSON文件路径（临时存储，完成导入后才写入数据库）';

-- 2. 扩展 F_SOURCE_FILE_ID 字段长度（如果需要）
ALTER TABLE `lab_raw_data_import_session`
MODIFY COLUMN `F_SOURCE_FILE_ID` VARCHAR(500) DEFAULT NULL
COMMENT 'Excel源文件ID';

-- 3. 显示修复结果
SELECT
    '表结构修复完成' AS 状态,
    CONCAT('已添加/确认字段: F_PARSED_DATA_FILE (长度: 500)') AS 操作详情,
    CONCAT('已扩展字段: F_SOURCE_FILE_ID (新长度: 500)') AS 其他修改;