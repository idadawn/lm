-- 添加 F_FIRST_INSPECTION 字段到 LAB_INTERMEDIATE_DATA 表
-- 创建时间：2026-01-20
-- 描述：添加"一次交检"字段

-- 检查并添加字段（MySQL）
DROP PROCEDURE IF EXISTS `add_first_inspection_to_intermediate_data`;
DELIMITER $$
CREATE PROCEDURE `add_first_inspection_to_intermediate_data`()
BEGIN
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_FIRST_INSPECTION'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_FIRST_INSPECTION` varchar(50) DEFAULT NULL 
        COMMENT '一次交检';
    END IF;
END $$
DELIMITER ;

CALL `add_first_inspection_to_intermediate_data`();
DROP PROCEDURE `add_first_inspection_to_intermediate_data`;

SELECT 'Migration completed: F_FIRST_INSPECTION column added to lab_intermediate_data' AS Result;

-- 注意：检查报告显示还有其他缺失字段，请参考：
-- doc/IntermediateDataEntity_字段检查报告.md
