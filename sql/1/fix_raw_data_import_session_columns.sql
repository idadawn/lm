-- Add missing columns to lab_raw_data_import_session (CamelCase to match Base Class)
-- Created to fix Unknown Column errors

DROP PROCEDURE IF EXISTS `upgrade_lab_raw_data_import_session_columns`;
DELIMITER $$
CREATE PROCEDURE `upgrade_lab_raw_data_import_session_columns`()
BEGIN
    -- F_DeleteMark
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_raw_data_import_session' AND COLUMN_NAME='F_DeleteMark') THEN
        ALTER TABLE `lab_raw_data_import_session` ADD COLUMN `F_DeleteMark` int(11) DEFAULT '0' COMMENT '删除标志';
    END IF;

    -- F_DeleteTime
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_raw_data_import_session' AND COLUMN_NAME='F_DeleteTime') THEN
        ALTER TABLE `lab_raw_data_import_session` ADD COLUMN `F_DeleteTime` datetime DEFAULT NULL COMMENT '删除时间';
    END IF;

    -- F_DeleteUserId
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_raw_data_import_session' AND COLUMN_NAME='F_DeleteUserId') THEN
        ALTER TABLE `lab_raw_data_import_session` ADD COLUMN `F_DeleteUserId` varchar(50) DEFAULT NULL COMMENT '删除用户';
    END IF;

    -- F_LastModifyUserId
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_raw_data_import_session' AND COLUMN_NAME='F_LastModifyUserId') THEN
        ALTER TABLE `lab_raw_data_import_session` ADD COLUMN `F_LastModifyUserId` varchar(50) DEFAULT NULL COMMENT '修改用户';
    END IF;

    -- F_TenantId
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_raw_data_import_session' AND COLUMN_NAME='F_TenantId') THEN
        ALTER TABLE `lab_raw_data_import_session` ADD COLUMN `F_TenantId` varchar(50) DEFAULT NULL COMMENT '租户ID';
    END IF;

    -- F_ENABLEDMARK
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_raw_data_import_session' AND COLUMN_NAME='F_ENABLEDMARK') THEN
        ALTER TABLE `lab_raw_data_import_session` ADD COLUMN `F_ENABLEDMARK` int(11) DEFAULT '1' COMMENT '启用标识';
    END IF;

    -- Fix F_SOURCE_FILE_ID length
    ALTER TABLE `lab_raw_data_import_session` MODIFY COLUMN `F_SOURCE_FILE_ID` varchar(500) DEFAULT NULL COMMENT 'Excel源文件ID';

END $$
DELIMITER ;
CALL `upgrade_lab_raw_data_import_session_columns`();
DROP PROCEDURE `upgrade_lab_raw_data_import_session_columns`;
