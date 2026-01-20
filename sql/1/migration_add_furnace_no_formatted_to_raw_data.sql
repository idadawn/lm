-- ============================================
-- 原始数据表 - 添加格式化炉号字段
-- 说明:
--   1. 新增 F_FURNACE_NO_FORMATTED 字段用于存储格式化的炉号
--   2. 格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]
--   3. 不包含特性描述（如"脆"等）
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 检查并添加字段（MySQL）
DROP PROCEDURE IF EXISTS `add_furnace_no_formatted_to_raw_data`;
DELIMITER $$
CREATE PROCEDURE `add_furnace_no_formatted_to_raw_data`()
BEGIN
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'LAB_RAW_DATA' 
        AND COLUMN_NAME = 'F_FURNACE_NO_FORMATTED'
    ) THEN
        ALTER TABLE `LAB_RAW_DATA` 
        ADD COLUMN `F_FURNACE_NO_FORMATTED` VARCHAR(200) DEFAULT NULL COMMENT '炉号（格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]，不包含特性描述）' 
        AFTER `F_FURNACE_NO`;
        
        -- 添加索引（可选，用于查询优化）
        ALTER TABLE `LAB_RAW_DATA` 
        ADD INDEX `idx_furnace_no_formatted` (`F_FURNACE_NO_FORMATTED`);
    END IF;
END $$
DELIMITER ;

CALL `add_furnace_no_formatted_to_raw_data`();
DROP PROCEDURE `add_furnace_no_formatted_to_raw_data`;

-- ============================================
-- 数据迁移说明
-- ============================================
-- 如果需要为现有数据填充 F_FURNACE_NO_FORMATTED 字段，可以执行以下逻辑：
-- 1. 从原始炉号（F_FURNACE_NO）解析出各部分
-- 2. 使用 FurnaceNo.GetFurnaceNo() 方法生成格式化炉号
-- 3. 更新到 F_FURNACE_NO_FORMATTED 字段
-- 
-- 注意：此字段建议在应用层计算并更新，而不是在数据库层计算

SELECT 'Migration completed: F_FURNACE_NO_FORMATTED column added to LAB_RAW_DATA' AS Result;
