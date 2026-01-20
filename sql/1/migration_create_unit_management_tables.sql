-- ============================================
-- 通用单位管理与换算模块 - 数据库表结构SQL
-- 数据库类型：MySQL
-- 创建日期：2025-01-XX
-- ============================================

-- ============================================
-- 1. 单位维度表（UNIT_CATEGORY）
-- ============================================
DROP TABLE IF EXISTS `UNIT_CATEGORY`;

CREATE TABLE `UNIT_CATEGORY` (
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
    `F_NAME` VARCHAR(100) NOT NULL COMMENT '维度名称（如：长度、质量、密度、电感）',
    `F_CODE` VARCHAR(50) NOT NULL COMMENT '唯一编码（如：LENGTH, MASS, DENSITY）',
    `F_DESCRIPTION` VARCHAR(500) DEFAULT NULL COMMENT '描述',
    `F_SORTCODE` BIGINT DEFAULT NULL COMMENT '排序码',
    
    PRIMARY KEY (`F_ID`),
    UNIQUE KEY `UK_UNIT_CATEGORY_CODE` (`F_CODE`),
    KEY `IDX_UNIT_CATEGORY_TENANT` (`F_TENANTID`),
    KEY `IDX_UNIT_CATEGORY_DELETE` (`F_DeleteMark`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='单位维度表';

-- ============================================
-- 2. 单位定义表（UNIT_DEFINITION）
-- ============================================
DROP TABLE IF EXISTS `UNIT_DEFINITION`;

CREATE TABLE `UNIT_DEFINITION` (
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
    `F_CATEGORY_ID` VARCHAR(50) NOT NULL COMMENT '关联维度ID',
    `F_NAME` VARCHAR(100) NOT NULL COMMENT '单位全称（如：毫米、微米）',
    `F_SYMBOL` VARCHAR(20) NOT NULL COMMENT '单位符号（如：mm, μm, kg/cm³）',
    `F_IS_BASE` INT NOT NULL DEFAULT 0 COMMENT '是否为该维度的基准单位 (1-是, 0-否)',
    `F_SCALE_TO_BASE` DECIMAL(18, 10) NOT NULL COMMENT '换算至基准单位的比例系数',
    `F_OFFSET` DECIMAL(18, 10) NOT NULL DEFAULT 0 COMMENT '换算偏移量（默认0，用于摄氏度/华氏度等）',
    `F_PRECISION` INT NOT NULL DEFAULT 2 COMMENT '该单位推荐的显示精度（小数位数）',
    `F_SORTCODE` BIGINT DEFAULT NULL COMMENT '排序码',
    
    PRIMARY KEY (`F_ID`),
    KEY `IDX_UNIT_DEFINITION_CATEGORY` (`F_CATEGORY_ID`),
    KEY `IDX_UNIT_DEFINITION_TENANT` (`F_TENANTID`),
    KEY `IDX_UNIT_DEFINITION_DELETE` (`F_DeleteMark`),
    CONSTRAINT `FK_UNIT_DEFINITION_CATEGORY` FOREIGN KEY (`F_CATEGORY_ID`) REFERENCES `UNIT_CATEGORY` (`F_ID`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='单位定义表';

-- ============================================
-- 3. 初始化数据 - 单位维度
-- ============================================
INSERT INTO `UNIT_CATEGORY` (`F_ID`, `F_NAME`, `F_CODE`, `F_DESCRIPTION`, `F_SORTCODE`, `F_CREATORTIME`, `F_ENABLEDMARK`, `F_DeleteMark`) VALUES
('CAT_LENGTH', '长度', 'LENGTH', '长度单位维度', 1, NOW(), 1, 0),
('CAT_MASS', '质量', 'MASS', '质量单位维度', 2, NOW(), 1, 0),
('CAT_DENSITY', '密度', 'DENSITY', '密度单位维度', 3, NOW(), 1, 0),
('CAT_PRESSURE', '压力', 'PRESSURE', '压力单位维度', 4, NOW(), 1, 0),
('CAT_INDUCTANCE', '电感', 'INDUCTANCE', '电感单位维度', 5, NOW(), 1, 0);

-- ============================================
-- 4. 初始化数据 - 长度单位（基准单位：米 m）
-- ============================================
INSERT INTO `UNIT_DEFINITION` (`F_ID`, `F_CATEGORY_ID`, `F_NAME`, `F_SYMBOL`, `F_IS_BASE`, `F_SCALE_TO_BASE`, `F_OFFSET`, `F_PRECISION`, `F_SORTCODE`, `F_CREATORTIME`, `F_ENABLEDMARK`, `F_DeleteMark`) VALUES
('UNIT_M', 'CAT_LENGTH', '米', 'm', 1, 1.0000000000, 0.0000000000, 2, 1, NOW(), 1, 0),
('UNIT_MM', 'CAT_LENGTH', '毫米', 'mm', 0, 0.0010000000, 0.0000000000, 2, 2, NOW(), 1, 0),
('UNIT_UM', 'CAT_LENGTH', '微米', 'μm', 0, 0.0000010000, 0.0000000000, 0, 3, NOW(), 1, 0),
('UNIT_CM', 'CAT_LENGTH', '厘米', 'cm', 0, 0.0100000000, 0.0000000000, 2, 4, NOW(), 1, 0),
('UNIT_KM', 'CAT_LENGTH', '千米', 'km', 0, 1000.0000000000, 0.0000000000, 2, 5, NOW(), 1, 0);

-- ============================================
-- 5. 初始化数据 - 质量单位（基准单位：千克 kg）
-- ============================================
INSERT INTO `UNIT_DEFINITION` (`F_ID`, `F_CATEGORY_ID`, `F_NAME`, `F_SYMBOL`, `F_IS_BASE`, `F_SCALE_TO_BASE`, `F_OFFSET`, `F_PRECISION`, `F_SORTCODE`, `F_CREATORTIME`, `F_ENABLEDMARK`, `F_DeleteMark`) VALUES
('UNIT_KG', 'CAT_MASS', '千克', 'kg', 1, 1.0000000000, 0.0000000000, 2, 1, NOW(), 1, 0),
('UNIT_G', 'CAT_MASS', '克', 'g', 0, 0.0010000000, 0.0000000000, 2, 2, NOW(), 1, 0),
('UNIT_MG', 'CAT_MASS', '毫克', 'mg', 0, 0.0000010000, 0.0000000000, 0, 3, NOW(), 1, 0),
('UNIT_T', 'CAT_MASS', '吨', 't', 0, 1000.0000000000, 0.0000000000, 2, 4, NOW(), 1, 0);

-- ============================================
-- 6. 初始化数据 - 密度单位（基准单位：千克/立方米 kg/m³）
-- ============================================
INSERT INTO `UNIT_DEFINITION` (`F_ID`, `F_CATEGORY_ID`, `F_NAME`, `F_SYMBOL`, `F_IS_BASE`, `F_SCALE_TO_BASE`, `F_OFFSET`, `F_PRECISION`, `F_SORTCODE`, `F_CREATORTIME`, `F_ENABLEDMARK`, `F_DeleteMark`) VALUES
('UNIT_KG_M3', 'CAT_DENSITY', '千克/立方米', 'kg/m³', 1, 1.0000000000, 0.0000000000, 2, 1, NOW(), 1, 0),
('UNIT_G_CM3', 'CAT_DENSITY', '克/立方厘米', 'g/cm³', 0, 1000.0000000000, 0.0000000000, 2, 2, NOW(), 1, 0),
('UNIT_G_L', 'CAT_DENSITY', '克/升', 'g/L', 0, 1.0000000000, 0.0000000000, 2, 3, NOW(), 1, 0);

-- ============================================
-- 7. 初始化数据 - 压力单位（基准单位：帕斯卡 Pa）
-- ============================================
INSERT INTO `UNIT_DEFINITION` (`F_ID`, `F_CATEGORY_ID`, `F_NAME`, `F_SYMBOL`, `F_IS_BASE`, `F_SCALE_TO_BASE`, `F_OFFSET`, `F_PRECISION`, `F_SORTCODE`, `F_CREATORTIME`, `F_ENABLEDMARK`, `F_DeleteMark`) VALUES
('UNIT_PA', 'CAT_PRESSURE', '帕斯卡', 'Pa', 1, 1.0000000000, 0.0000000000, 2, 1, NOW(), 1, 0),
('UNIT_KPA', 'CAT_PRESSURE', '千帕', 'kPa', 0, 1000.0000000000, 0.0000000000, 2, 2, NOW(), 1, 0),
('UNIT_MPA', 'CAT_PRESSURE', '兆帕', 'MPa', 0, 1000000.0000000000, 0.0000000000, 2, 3, NOW(), 1, 0),
('UNIT_BAR', 'CAT_PRESSURE', '巴', 'bar', 0, 100000.0000000000, 0.0000000000, 2, 4, NOW(), 1, 0);

-- ============================================
-- 8. 初始化数据 - 电感单位（基准单位：亨利 H）
-- ============================================
INSERT INTO `UNIT_DEFINITION` (`F_ID`, `F_CATEGORY_ID`, `F_NAME`, `F_SYMBOL`, `F_IS_BASE`, `F_SCALE_TO_BASE`, `F_OFFSET`, `F_PRECISION`, `F_SORTCODE`, `F_CREATORTIME`, `F_ENABLEDMARK`, `F_DeleteMark`) VALUES
('UNIT_H', 'CAT_INDUCTANCE', '亨利', 'H', 1, 1.0000000000, 0.0000000000, 6, 1, NOW(), 1, 0),
('UNIT_MH', 'CAT_INDUCTANCE', '毫亨', 'mH', 0, 0.0010000000, 0.0000000000, 3, 2, NOW(), 1, 0),
('UNIT_UH', 'CAT_INDUCTANCE', '微亨', 'μH', 0, 0.0000010000, 0.0000000000, 0, 3, NOW(), 1, 0);
