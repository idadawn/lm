-- Add F_MATCH_CONFIDENCE to LAB_RAW_DATA table

DROP PROCEDURE IF EXISTS `upgrade_lab_raw_data_confidence`;
DELIMITER $$
CREATE PROCEDURE `upgrade_lab_raw_data_confidence`()
BEGIN
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_raw_data' AND COLUMN_NAME='F_MATCH_CONFIDENCE') THEN
        ALTER TABLE `lab_raw_data` ADD COLUMN `F_MATCH_CONFIDENCE` double DEFAULT NULL COMMENT '匹配置信度';
    END IF;
END $$
DELIMITER ;
CALL `upgrade_lab_raw_data_confidence`();
DROP PROCEDURE `upgrade_lab_raw_data_confidence`;
