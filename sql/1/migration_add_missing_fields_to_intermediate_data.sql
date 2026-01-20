-- 添加缺失字段到 LAB_INTERMEDIATE_DATA 表
-- 创建时间：2026-01-20
-- 描述：根据 IntermediateDataEntity.cs 实体类，添加缺失的字段

-- 检查并添加字段（MySQL）
DROP PROCEDURE IF EXISTS `add_missing_fields_to_intermediate_data`;
DELIMITER $$
CREATE PROCEDURE `add_missing_fields_to_intermediate_data`()
BEGIN
    -- 1. 添加 F_FIRST_INSPECTION（一次交检）
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

    -- 2. 添加 F_AVG_THICKNESS（平均厚度）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_AVG_THICKNESS'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_AVG_THICKNESS` decimal(18, 2) DEFAULT NULL 
        COMMENT '平均厚度 (μm)：AVG(F_LAM_DIST_1..22) / LAYERS';
    END IF;

    -- 3. 添加 F_FOUR_METER_WT（四米带材重量）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_FOUR_METER_WT'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_FOUR_METER_WT` decimal(18, 1) DEFAULT NULL 
        COMMENT '四米带材重量（g），原始样段称重数据';
    END IF;

    -- 4. 添加 F_STRIP_WIDTH（带宽）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_STRIP_WIDTH'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_STRIP_WIDTH` decimal(18, 2) DEFAULT NULL 
        COMMENT '带宽 (mm)';
    END IF;

    -- 5. 添加 F_APPEAR_FEATURE（外观特性）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_APPEAR_FEATURE'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_APPEAR_FEATURE` varchar(200) DEFAULT NULL 
        COMMENT '外观特性（原始特性汉字），炉号后缀解析（如"脆"）';
    END IF;

    -- 6. 添加 F_APPEARANCE_FEATURE_IDS（外观特性ID列表）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_APPEARANCE_FEATURE_IDS'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_APPEARANCE_FEATURE_IDS` json DEFAULT NULL 
        COMMENT '匹配后的特性ID列表（JSON格式，数组：["feature-id-1", "feature-id-2", ...]）';
    END IF;

    -- 7. 添加 F_APPEAR_JUDGE（外观检验员）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_APPEAR_JUDGE'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_APPEAR_JUDGE` varchar(50) DEFAULT NULL 
        COMMENT '外观检验员，自动获取当前用户';
    END IF;

    -- 8. 添加 F_PERF_JUDGE_NAME（性能判定人）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_PERF_JUDGE_NAME'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_PERF_JUDGE_NAME` varchar(50) DEFAULT NULL 
        COMMENT '性能判定人';
    END IF;

    -- 9. 添加 F_DATE_MONTH（日期月份）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_DATE_MONTH'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_DATE_MONTH` varchar(10) DEFAULT NULL 
        COMMENT '日期（yyyy-MM格式，可手动修改）';
    END IF;

    -- 10. 添加 F_FURNACE_NO（原始炉号）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_FURNACE_NO'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_FURNACE_NO` varchar(100) DEFAULT NULL 
        COMMENT '原始炉号';
    END IF;

    -- 11. 添加 F_FURNACE_NO_PARSED（解析后的炉号）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_FURNACE_NO_PARSED'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_FURNACE_NO_PARSED` varchar(50) DEFAULT NULL 
        COMMENT '炉号（解析后的炉号数字部分）';
    END IF;

    -- 12. 添加 F_SHIFT_NO（班次号）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_SHIFT_NO'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_SHIFT_NO` varchar(100) DEFAULT NULL 
        COMMENT '班次（产线+班次+日期+炉号组合）';
    END IF;

    -- 13. 添加 F_BATCH_NO（批次号）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_BATCH_NO'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_BATCH_NO` varchar(100) DEFAULT NULL 
        COMMENT '批次（产线数字+班次汉字+8位日期-炉号）';
    END IF;

    -- 14. 添加 F_COIL_WEIGHT_KG（单卷重量kg）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_COIL_WEIGHT_KG'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_COIL_WEIGHT_KG` decimal(18, 2) DEFAULT NULL 
        COMMENT '单卷重量（kg）';
    END IF;

    -- 15. 添加 F_LAM_DIST_1 到 F_LAM_DIST_22（叠片分布1-22）
    SET @i = 1;
    WHILE @i <= 22 DO
        SET @col_name = CONCAT('F_LAM_DIST_', @i);
        SET @sql = CONCAT(
            'IF NOT EXISTS(',
            'SELECT * FROM information_schema.COLUMNS ',
            'WHERE TABLE_SCHEMA = DATABASE() ',
            'AND TABLE_NAME = ''lab_intermediate_data'' ',
            'AND COLUMN_NAME = ''', @col_name, ''') ',
            'THEN ',
            'ALTER TABLE `lab_intermediate_data` ',
            'ADD COLUMN `', @col_name, '` decimal(18, 1) DEFAULT NULL ',
            'COMMENT ''叠片系数分布', @i, '，数据库核心存储列（对应原始总厚度）''; ',
            'END IF;'
        );
        SET @sql = CONCAT('SET @stmt = ''', @sql, '''; PREPARE stmt FROM @stmt; EXECUTE stmt; DEALLOCATE PREPARE stmt;');
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        SET @i = @i + 1;
    END WHILE;

    -- 16. 添加 F_SEGMENT_TYPE（分段类型）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_SEGMENT_TYPE'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_SEGMENT_TYPE` varchar(20) DEFAULT NULL 
        COMMENT '分段类型（前端/中端/后端）';
    END IF;

    -- 17. 添加 F_REMARK（备注）
    IF NOT EXISTS(
        SELECT * FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
        AND TABLE_NAME = 'lab_intermediate_data' 
        AND COLUMN_NAME = 'F_REMARK'
    ) THEN
        ALTER TABLE `lab_intermediate_data` 
        ADD COLUMN `F_REMARK` varchar(500) DEFAULT NULL 
        COMMENT '备注';
    END IF;

END $$
DELIMITER ;

CALL `add_missing_fields_to_intermediate_data`();
DROP PROCEDURE `add_missing_fields_to_intermediate_data`;

SELECT 'Migration completed: Missing fields added to lab_intermediate_data' AS Result;
