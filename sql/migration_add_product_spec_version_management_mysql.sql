-- Product Definition Version Management Migration Script (MySQL)
-- Created: 2026-01-13
-- Description: Adds tables and columns for version management

-- 1. Create Version Table
CREATE TABLE IF NOT EXISTS `lab_product_spec_version` (
  `F_ID` varchar(50) NOT NULL COMMENT '主键',
  `F_TENANTID` varchar(50) DEFAULT NULL COMMENT '租户ID',
  `F_PRODUCT_SPEC_ID` varchar(50) NOT NULL COMMENT '产品规格ID',
  `F_VERSION` int(11) NOT NULL DEFAULT '1' COMMENT '版本号',
  `F_VERSION_NAME` varchar(100) DEFAULT NULL COMMENT '版本名称',
  `F_VERSION_DESCRIPTION` varchar(500) DEFAULT NULL COMMENT '版本说明',
  `F_IS_CURRENT` int(11) NOT NULL DEFAULT '0' COMMENT '是否当前版本',
  `F_CREATOR_TIME` datetime DEFAULT NULL COMMENT '创建时间',
  `F_CREATOR_USER_ID` varchar(50) DEFAULT NULL COMMENT '创建人ID',
  `F_LAST_MODIFY_TIME` datetime DEFAULT NULL COMMENT '最后修改时间',
  `F_LAST_MODIFY_USER_ID` varchar(50) DEFAULT NULL COMMENT '最后修改人ID',
  `F_DELETE_MARK` int(11) DEFAULT '0' COMMENT '删除标记',
  `F_DELETE_TIME` datetime DEFAULT NULL COMMENT '删除时间',
  `F_DELETE_USER_ID` varchar(50) DEFAULT NULL COMMENT '删除人ID',
  PRIMARY KEY (`F_ID`),
  KEY `IX_PRODUCT_SPEC_VERSION_PRODUCT_SPEC_ID` (`F_PRODUCT_SPEC_ID`),
  KEY `IX_PRODUCT_SPEC_VERSION_IS_CURRENT` (`F_IS_CURRENT`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='产品规格版本快照';

-- 1.1 Ensure F_TENANTID exists (in case table was created by previous script without it)
DROP PROCEDURE IF EXISTS `upgrade_lab_product_spec_version`;
DELIMITER $$
CREATE PROCEDURE `upgrade_lab_product_spec_version`()
BEGIN
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_product_spec_version' AND COLUMN_NAME='F_TENANTID') THEN
        ALTER TABLE `lab_product_spec_version` ADD COLUMN `F_TENANTID` varchar(50) DEFAULT NULL COMMENT '租户ID';
    END IF;
END $$
DELIMITER ;
CALL `upgrade_lab_product_spec_version`();
DROP PROCEDURE `upgrade_lab_product_spec_version`;

-- 2. Add columns to lab_product_spec_attribute
DROP PROCEDURE IF EXISTS `upgrade_lab_product_spec_attribute`;
DELIMITER $$
CREATE PROCEDURE `upgrade_lab_product_spec_attribute`()
BEGIN
    -- F_VERSION
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_product_spec_attribute' AND COLUMN_NAME='F_VERSION') THEN
        ALTER TABLE `lab_product_spec_attribute` ADD COLUMN `F_VERSION` int(11) NOT NULL DEFAULT '1' COMMENT '版本号';
        UPDATE `lab_product_spec_attribute` SET `F_VERSION` = 1 WHERE `F_VERSION` IS NULL;
    END IF;

    -- F_VERSION_CREATE_TIME
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_product_spec_attribute' AND COLUMN_NAME='F_VERSION_CREATE_TIME') THEN
        ALTER TABLE `lab_product_spec_attribute` ADD COLUMN `F_VERSION_CREATE_TIME` datetime DEFAULT NULL COMMENT '版本创建时间';
    END IF;

    -- F_VERSION_DESCRIPTION
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_product_spec_attribute' AND COLUMN_NAME='F_VERSION_DESCRIPTION') THEN
        ALTER TABLE `lab_product_spec_attribute` ADD COLUMN `F_VERSION_DESCRIPTION` varchar(500) DEFAULT NULL COMMENT '版本说明';
    END IF;
    
    -- Index for Version
    IF NOT EXISTS(SELECT * FROM information_schema.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_product_spec_attribute' AND INDEX_NAME='IX_PRODUCT_SPEC_ATTRIBUTE_VERSION') THEN
        CREATE INDEX `IX_PRODUCT_SPEC_ATTRIBUTE_VERSION` ON `lab_product_spec_attribute`(`F_PRODUCT_SPEC_ID`, `F_VERSION`);
    END IF;

END $$
DELIMITER ;
CALL `upgrade_lab_product_spec_attribute`();
DROP PROCEDURE `upgrade_lab_product_spec_attribute`;

-- 3. Add column to lab_intermediate_data
DROP PROCEDURE IF EXISTS `upgrade_lab_intermediate_data`;
DELIMITER $$
CREATE PROCEDURE `upgrade_lab_intermediate_data`()
BEGIN
    IF NOT EXISTS(SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='lab_intermediate_data' AND COLUMN_NAME='F_PRODUCT_SPEC_VERSION') THEN
        ALTER TABLE `lab_intermediate_data` ADD COLUMN `F_PRODUCT_SPEC_VERSION` int(11) DEFAULT NULL COMMENT '产品规格版本';
        UPDATE `lab_intermediate_data` SET `F_PRODUCT_SPEC_VERSION` = 1 WHERE `F_PRODUCT_SPEC_VERSION` IS NULL;
    END IF;
END $$
DELIMITER ;
CALL `upgrade_lab_intermediate_data`();
DROP PROCEDURE `upgrade_lab_intermediate_data`;

-- 4. Initial Versions (Insert only if no versions exist for the spec)
INSERT INTO `lab_product_spec_version` (
  `F_ID`, `F_PRODUCT_SPEC_ID`, `F_VERSION`, `F_VERSION_NAME`,
  `F_VERSION_DESCRIPTION`, `F_IS_CURRENT`, `F_CREATOR_TIME`, `F_CREATOR_USER_ID`, `F_DELETE_MARK`
)
SELECT
  UUID(), `F_ID`, 1, 'v1.0',
  '初始版本', 1, NOW(), 'system', 0
FROM `lab_product_spec`
WHERE (`F_DeleteMark` = 0 OR `F_DeleteMark` IS NULL)
AND `F_ID` COLLATE utf8mb4_unicode_ci NOT IN (SELECT `F_PRODUCT_SPEC_ID` COLLATE utf8mb4_unicode_ci FROM `lab_product_spec_version`);

COMMIT;

SELECT 'Migration completed successfully.' AS Result;
