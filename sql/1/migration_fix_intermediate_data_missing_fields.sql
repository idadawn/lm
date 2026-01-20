-- 修复 LAB_INTERMEDIATE_DATA 表缺失字段
-- 创建时间：2026-01-20
-- 描述：根据 IntermediateDataEntity.cs 实体类检查，添加缺失的字段

-- 检查并添加字段（MySQL）
DROP PROCEDURE IF EXISTS `fix_intermediate_data_missing_fields`;
DELIMITER $$
CREATE PROCEDURE `fix_intermediate_data_missing_fields`()
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

    -- 2. 添加 F_APPEARANCE_FEATURE_IDS（外观特性ID列表）- 代码中已引用但未定义
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

    -- 3. 添加 F_FOUR_METER_WT（四米带材重量）- 计算公式中需要
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

    -- 4. 添加 F_AVG_THICKNESS（平均厚度）- 计算公式中需要
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

    -- 5. 添加 F_STRIP_WIDTH（带宽）
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

    -- 6. 添加 F_APPEAR_FEATURE（外观特性）
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

    -- 7. 添加 F_LAM_DIST_1 到 F_LAM_DIST_22（叠片分布1-22）- 计算公式中需要
    SET @i = 1;
    WHILE @i <= 22 DO
        SET @col_name = CONCAT('F_LAM_DIST_', @i);
        IF NOT EXISTS(
            SELECT * FROM information_schema.COLUMNS 
            WHERE TABLE_SCHEMA = DATABASE() 
            AND TABLE_NAME = 'lab_intermediate_data' 
            AND COLUMN_NAME = @col_name
        ) THEN
            SET @sql = CONCAT(
                'ALTER TABLE `lab_intermediate_data` ',
                'ADD COLUMN `', @col_name, '` decimal(18, 1) DEFAULT NULL ',
                'COMMENT ''叠片系数分布', @i, '，数据库核心存储列（对应原始总厚度）'';'
            );
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
        END IF;
        SET @i = @i + 1;
    END WHILE;

END $$
DELIMITER ;

CALL `fix_intermediate_data_missing_fields`();
DROP PROCEDURE `fix_intermediate_data_missing_fields`;

SELECT 'Migration completed: Missing fields added to lab_intermediate_data' AS Result;
