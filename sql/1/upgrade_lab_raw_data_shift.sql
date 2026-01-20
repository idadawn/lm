-- Upgrade LAB_RAW_DATA table to support string Shift (甲/乙/丙) and add ShiftNumeric
-- Fixes: Shift can't convert string to int32 errors and supports new requirement

DROP PROCEDURE IF EXISTS `upgrade_lab_raw_data_shift`;
DELIMITER $$
CREATE PROCEDURE `upgrade_lab_raw_data_shift`()
BEGIN
    -- 1. Add F_SHIFT_NUMERIC if not exists
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_raw_data' AND COLUMN_NAME='F_SHIFT_NUMERIC') THEN
        ALTER TABLE `lab_raw_data` ADD COLUMN `F_SHIFT_NUMERIC` int DEFAULT NULL COMMENT '班次数字';
        
        -- 2. Populate F_SHIFT_NUMERIC from existing F_SHIFT (assuming F_SHIFT is currently INT)
        -- We only do this if we just created the column, to avoid overwriting if run multiple times
        IF EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_raw_data' AND COLUMN_NAME='F_SHIFT' AND DATA_TYPE='int') THEN
             UPDATE `lab_raw_data` SET `F_SHIFT_NUMERIC` = `F_SHIFT`;
        END IF;
    END IF;

    -- 3. Modify F_SHIFT to VARCHAR(10) if it is INT
    IF EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_raw_data' AND COLUMN_NAME='F_SHIFT' AND DATA_TYPE='int') THEN
        ALTER TABLE `lab_raw_data` MODIFY COLUMN `F_SHIFT` varchar(10) DEFAULT NULL COMMENT '班次';
        
        -- 4. Map values: 1->甲, 2->乙, 3->丙
        UPDATE `lab_raw_data` SET `F_SHIFT` = '甲' WHERE `F_SHIFT` = '1';
        UPDATE `lab_raw_data` SET `F_SHIFT` = '乙' WHERE `F_SHIFT` = '2';
        UPDATE `lab_raw_data` SET `F_SHIFT` = '丙' WHERE `F_SHIFT` = '3';
    END IF;

END $$
DELIMITER ;
CALL `upgrade_lab_raw_data_shift`();
DROP PROCEDURE `upgrade_lab_raw_data_shift`;
