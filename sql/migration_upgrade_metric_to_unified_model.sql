-- ============================================
-- 指标定义表升级脚本（统一模型）
-- 数据库类型：MySQL
-- 创建日期：2026-01-20
-- 说明：将lab_metric_definition表升级到统一模型
-- 执行顺序：在现有表创建后执行
-- ============================================

-- 1. 重命名status字段为is_enabled（为了兼容性，先添加新字段，迁移数据后再删除旧字段）

-- 添加新字段
ALTER TABLE `lab_metric_definition`
ADD COLUMN `formula_language` VARCHAR(20) DEFAULT 'EXCEL' COMMENT '公式语言：EXCEL（默认）、MATH、SCRIPT',
ADD COLUMN `metric_type` VARCHAR(20) DEFAULT 'STANDARD' COMMENT '指标类型：STANDARD（标准）、COMPOSITE（复合）、ATOMIC（原子）',
ADD COLUMN `return_type` VARCHAR(20) DEFAULT 'NUMBER' COMMENT '返回值类型：NUMBER（默认）、STRING、BOOLEAN、DATETIME',
ADD COLUMN `unit_name` VARCHAR(50) DEFAULT NULL COMMENT '单位名称（用于显示）',
ADD COLUMN `is_system` TINYINT DEFAULT 0 COMMENT '是否为系统内置指标（0-否，1-是）',
ADD COLUMN `version` INT DEFAULT 1 COMMENT '当前版本号',
ADD COLUMN `is_versioned` TINYINT DEFAULT 0 COMMENT '是否启用版本管理（0-否，1-是）',
ADD COLUMN `has_variables` TINYINT DEFAULT 0 COMMENT '是否有变量绑定（0-否，1-是）',
ADD COLUMN `store_results` TINYINT DEFAULT 0 COMMENT '是否存储计算结果（0-否，1-是）',
ADD COLUMN `precision` INT DEFAULT NULL COMMENT '小数位数（精度）',
ADD COLUMN `calculation_order` INT DEFAULT 0 COMMENT '计算顺序（用于解决依赖关系）';

-- 2. 添加is_enabled字段（兼容新模型）
ALTER TABLE `lab_metric_definition`
ADD COLUMN `is_enabled` TINYINT DEFAULT 1 COMMENT '是否启用（1-启用，0-禁用）';

-- 3. 将现有status字段的数据迁移到is_enabled字段
UPDATE `lab_metric_definition`
SET `is_enabled` = CASE
    WHEN `status` = 1 THEN 1
    WHEN `status` = 0 THEN 0
    ELSE 1
END
WHERE `F_DeleteMark` = 0;

-- 4. 创建扩展表（如果不存在）

-- 指标变量绑定表
CREATE TABLE IF NOT EXISTS `lab_metric_variable` (
    -- 基础字段（从CLDEntityBase继承）
    `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
    `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
    `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
    `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
    `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
    `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',

    -- 业务字段
    `metric_id` VARCHAR(50) NOT NULL COMMENT '指标ID',
    `variable_name` VARCHAR(100) NOT NULL COMMENT '变量名称（公式中使用）',
    `source_type` VARCHAR(20) NOT NULL COMMENT '来源类型：COLUMN（列）、DIMENSION（维度）、CONSTANT（常量）、OTHER_METRIC（其他指标）',
    `source_id` VARCHAR(100) DEFAULT NULL COMMENT '来源ID（列名/维度ID/常量值/指标ID）',
    `data_type` VARCHAR(20) NOT NULL COMMENT '数据类型：NUMBER、STRING、BOOLEAN、DATETIME',
    `is_required` TINYINT DEFAULT 1 COMMENT '是否必需（1-是，0-否）',
    `default_value` VARCHAR(200) DEFAULT NULL COMMENT '默认值',
    `sort_order` INT DEFAULT 0 COMMENT '排序序号',

    PRIMARY KEY (`F_ID`),
    UNIQUE KEY `UK_METRIC_VARIABLE` (`metric_id`, `variable_name`, `F_TENANTID`, `F_DeleteMark`),
    KEY `IDX_METRIC_ID` (`metric_id`, `F_TENANTID`),
    KEY `IDX_VARIABLE_NAME` (`variable_name`, `F_TENANTID`),
    KEY `IDX_SOURCE_TYPE` (`source_type`, `F_TENANTID`),
    CONSTRAINT `FK_METRIC_VARIABLE_METRIC` FOREIGN KEY (`metric_id`)
        REFERENCES `lab_metric_definition` (`F_ID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='指标变量绑定表';

-- 指标公式版本表
CREATE TABLE IF NOT EXISTS `lab_metric_formula_version` (
    -- 基础字段（从CLDEntityBase继承）
    `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
    `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
    `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
    `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
    `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
    `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',

    -- 业务字段
    `metric_id` VARCHAR(50) NOT NULL COMMENT '指标ID',
    `version` INT NOT NULL COMMENT '版本号（从1开始）',
    `version_name` VARCHAR(100) DEFAULT NULL COMMENT '版本名称（v1.0, v2.0）',
    `formula` TEXT NOT NULL COMMENT '公式表达式（历史版本）',
    `change_reason` VARCHAR(500) DEFAULT NULL COMMENT '变更原因',
    `is_current` TINYINT DEFAULT 0 COMMENT '是否为当前版本（0-否，1-是）',

    PRIMARY KEY (`F_ID`),
    UNIQUE KEY `UK_METRIC_VERSION` (`metric_id`, `version`, `F_TENANTID`, `F_DeleteMark`),
    KEY `IDX_METRIC_ID` (`metric_id`, `F_TENANTID`),
    KEY `IDX_VERSION` (`version`, `F_TENANTID`),
    KEY `IDX_IS_CURRENT` (`is_current`, `F_TENANTID`),
    CONSTRAINT `FK_METRIC_VERSION_METRIC` FOREIGN KEY (`metric_id`)
        REFERENCES `lab_metric_definition` (`F_ID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='指标公式版本表';

-- 指标计算结果表
CREATE TABLE IF NOT EXISTS `lab_metric_result` (
    -- 基础字段（从CLDEntityBase继承）
    `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
    `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
    `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
    `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
    `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
    `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',

    -- 业务字段
    `source_data_id` VARCHAR(50) NOT NULL COMMENT '源数据ID（中间数据ID或原始数据ID）',
    `metric_code` VARCHAR(50) NOT NULL COMMENT '指标编码',
    `calculated_value` VARCHAR(200) DEFAULT NULL COMMENT '计算结果',
    `calculation_time` DATETIME NOT NULL COMMENT '计算时间',
    `formula_version` INT DEFAULT 1 COMMENT '使用的公式版本',
    `calculation_status` VARCHAR(20) DEFAULT 'SUCCESS' COMMENT '计算状态：SUCCESS（成功）、ERROR（错误）、PENDING（待计算）',
    `error_message` VARCHAR(1000) DEFAULT NULL COMMENT '错误信息',

    PRIMARY KEY (`F_ID`),
    UNIQUE KEY `UK_SOURCE_METRIC` (`source_data_id`, `metric_code`, `F_TENANTID`, `F_DeleteMark`),
    KEY `IDX_METRIC_CODE` (`metric_code`, `F_TENANTID`),
    KEY `IDX_SOURCE_DATA_ID` (`source_data_id`, `F_TENANTID`),
    KEY `IDX_CALCULATION_TIME` (`calculation_time`, `F_TENANTID`),
    KEY `IDX_CALCULATION_STATUS` (`calculation_status`, `F_TENANTID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='指标计算结果表';

-- 5. 创建索引优化查询性能
CREATE INDEX IF NOT EXISTS `IDX_IS_ENABLED` ON `lab_metric_definition` (`is_enabled`, `F_TENANTID`);
CREATE INDEX IF NOT EXISTS `IDX_METRIC_TYPE` ON `lab_metric_definition` (`metric_type`, `F_TENANTID`);
CREATE INDEX IF NOT EXISTS `IDX_RETURN_TYPE` ON `lab_metric_definition` (`return_type`, `F_TENANTID`);
CREATE INDEX IF NOT EXISTS `IDX_IS_VERSIONED` ON `lab_metric_definition` (`is_versioned`, `F_TENANTID`);
CREATE INDEX IF NOT EXISTS `IDX_HAS_VARIABLES` ON `lab_metric_definition` (`has_variables`, `F_TENANTID`);
CREATE INDEX IF NOT EXISTS `IDX_STORE_RESULTS` ON `lab_metric_definition` (`store_results`, `F_TENANTID`);
CREATE INDEX IF NOT EXISTS `IDX_IS_SYSTEM` ON `lab_metric_definition` (`is_system`, `F_TENANTID`);
CREATE INDEX IF NOT EXISTS `IDX_SORT_ORDER` ON `lab_metric_definition` (`sort_order`, `F_TENANTID`);
CREATE INDEX IF NOT EXISTS `IDX_CALCULATION_ORDER` ON `lab_metric_definition` (`calculation_order`, `F_TENANTID`);

-- 6. 迁移Indicator数据（可选步骤，如果需要整合Indicator数据）
-- 注意：这需要根据实际业务需求决定是否执行

/*
-- 从LAB_INDICATOR_DEFINITION迁移数据到lab_metric_definition
INSERT INTO lab_metric_definition (
    F_ID, F_TENANTID, F_CREATORTIME, F_CREATORUSERID, F_ENABLEDMARK,
    F_LastModifyTime, F_LastModifyUserId, F_DeleteMark, F_DeleteTime, F_DeleteUserId,
    name, code, description, formula, unit_id, unit_name, category,
    is_enabled, is_system, sort_order, version, is_versioned, has_variables,
    store_results, precision, calculation_order, remark,
    formula_language, metric_type, return_type
)
SELECT
    F_ID, F_TENANTID, F_CREATORTIME, F_CREATORUSERID, F_ENABLEDMARK,
    F_LastModifyTime, F_LastModifyUserId, F_DeleteMark, F_DeleteTime, F_DeleteUserId,
    F_NAME, F_CODE, F_DESCRIPTION, F_EXPRESSION, F_UNIT_ID, F_UNIT_NAME, NULL,
    CASE F_IS_ENABLED WHEN 1 THEN 1 ELSE 0 END, 0,
    COALESCE(F_SORTCODE, 0), F_VERSION,
    CASE WHEN EXISTS(SELECT 1 FROM LAB_INDICATOR_FORMULA_VERSION WHERE F_INDICATOR_ID = F_ID) THEN 1 ELSE 0 END,
    CASE WHEN EXISTS(SELECT 1 FROM LAB_INDICATOR_VARIABLE WHERE F_INDICATOR_ID = F_ID) THEN 1 ELSE 0 END,
    1, -- store_results默认为1，因为Indicator存储计算结果
    F_PRECISION, 0, NULL,
    'EXCEL', -- formula_language
    CASE F_INDICATOR_TYPE WHEN 1 THEN 'COMPOSITE' WHEN 2 THEN 'ATOMIC' ELSE 'STANDARD' END,
    F_RETURN_TYPE
FROM LAB_INDICATOR_DEFINITION
WHERE F_DeleteMark = 0;
*/

-- 7. 注意事项：
--    a. 执行此脚本前请备份数据库
--    b. status字段可以保留作为兼容性字段，或后续删除
--    c. 如果需要删除status字段，执行：ALTER TABLE `lab_metric_definition` DROP COLUMN `status`;
--    d. 如果需要删除旧索引，执行相应DROP INDEX语句