-- 添加 F_PRODUCT_SPEC_VERSION 字段到 LAB_INTERMEDIATE_DATA 表
-- 创建时间：2026-01-20
-- 描述：修复 "Unknown column 'F_PRODUCT_SPEC_VERSION' in 'field list'" 错误
-- 
-- 字段说明：
-- - 原始数据表（LAB_RAW_DATA）中的 F_PRODUCT_SPEC_VERSION 是 varchar(50)，存储版本名称（如 "v1.0"）
-- - 中间数据表（LAB_INTERMEDIATE_DATA）中的 F_PRODUCT_SPEC_VERSION 是 int，存储版本号（如 1, 2, 3）
-- - 该字段记录生成中间数据时使用的产品规格版本号，用于版本追溯
-- - 如果为 NULL，表示使用最新版本（向后兼容旧数据）

-- 检查并添加字段（MySQL）
DROP PROCEDURE IF EXISTS `add_f_product_spec_version_to_intermediate_data`;
DELIMITER $$
CREATE PROCEDURE `add_f_product_spec_version_to_intermediate_data`()
BEGIN
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_PRODUCT_SPEC_VERSION'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_PRODUCT_SPEC_VERSION` int(11) DEFAULT NULL 
        COMMENT '产品规格版本号（记录生成中间数据时使用的版本，整数类型，如 1, 2, 3）';
        
        -- 可选：为现有数据设置默认版本号（如果需要）
        -- UPDATE `lab_intermediate_data` SET `F_PRODUCT_SPEC_VERSION` = 1 WHERE `F_PRODUCT_SPEC_VERSION` IS NULL;
    END IF;
END $$
DELIMITER ;

CALL `add_f_product_spec_version_to_intermediate_data`();
DROP PROCEDURE `add_f_product_spec_version_to_intermediate_data`;

-- 可选：添加索引以提高查询性能（如果需要按版本号查询）
-- CREATE INDEX IF NOT EXISTS `idx_intermediate_data_spec_version` 
-- ON `lab_intermediate_data`(`F_PRODUCT_SPEC_ID`, `F_PRODUCT_SPEC_VERSION`);

SELECT 'Migration completed: F_PRODUCT_SPEC_VERSION column added to lab_intermediate_data' AS Result;
