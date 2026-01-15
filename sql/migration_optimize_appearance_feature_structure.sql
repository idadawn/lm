-- ============================================
-- 外观特性表结构优化脚本
-- 表名: LAB_APPEARANCE_FEATURE
-- 说明: 
--   1. 增加关键字字段（F_KEYWORDS）
--   2. 移除程度变体字段（F_VARIANTS）
--   3. 创建特性与等级关联表（LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL）
-- 创建时间: 2026-01-11
-- ============================================

-- ============================================
-- MySQL版本（推荐）
-- ============================================

-- 1. 增加关键字字段
SET @dbname = DATABASE();
SET @tablename = 'LAB_APPEARANCE_FEATURE';
SET @columnname = 'F_KEYWORDS';
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = @columnname)
    ) > 0,
    'SELECT 1',
    CONCAT('ALTER TABLE ', @tablename, ' ADD ', @columnname, ' VARCHAR(1000) NULL COMMENT ''关键词列表（JSON数组或逗号分隔），用于精确匹配'' AFTER F_CATEGORY')
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 2. 删除程度变体字段（如果存在）
SET @columnname = 'F_VARIANTS';
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = @columnname)
    ) > 0,
    CONCAT('ALTER TABLE ', @tablename, ' DROP COLUMN ', @columnname),
    'SELECT 1'
));
PREPARE alterIfExists FROM @preparedStatement;
EXECUTE alterIfExists;
DEALLOCATE PREPARE alterIfExists;

-- 3. 创建特性与等级关联表
CREATE TABLE IF NOT EXISTS `LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL` (
    `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_FEATURE_ID` VARCHAR(50) NOT NULL COMMENT '外观特性ID（外键，关联到 LAB_APPEARANCE_FEATURE 表）',
    `F_SEVERITY_LEVEL_ID` VARCHAR(50) NOT NULL COMMENT '特性等级ID（外键，关联到 LAB_SEVERITY_LEVEL 表）',
    `F_VARIANT_NAME` VARCHAR(100) NULL COMMENT '变体名称（可选，如 "微脆"、"严重脆"）',
    `F_SORTCODE` BIGINT NULL COMMENT '排序码（用于同一特性下多个等级的排序）',
    `F_CREATORTIME` DATETIME NULL COMMENT '创建时间',
    `F_CREATORUSERID` VARCHAR(50) NULL COMMENT '创建人ID',
    `F_LASTMODIFYTIME` DATETIME NULL COMMENT '最后修改时间',
    `F_LASTMODIFYUSERID` VARCHAR(50) NULL COMMENT '最后修改人ID',
    `F_DELETEMARK` TINYINT(1) NULL DEFAULT 0 COMMENT '删除标记（0=未删除，1=已删除）',
    PRIMARY KEY (`F_ID`),
    INDEX `IDX_FEATURE_ID` (`F_FEATURE_ID`),
    INDEX `IDX_SEVERITY_LEVEL_ID` (`F_SEVERITY_LEVEL_ID`),
    INDEX `IDX_FEATURE_SEVERITY` (`F_FEATURE_ID`, `F_SEVERITY_LEVEL_ID`),
    INDEX `IDX_DELETEMARK` (`F_DELETEMARK`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='外观特性与特性等级关联表';

-- ============================================
-- SQL Server版本（备用）
-- ============================================
/*
-- 1. 增加关键字字段
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'LAB_APPEARANCE_FEATURE') 
    AND name = 'F_KEYWORDS'
)
BEGIN
    ALTER TABLE LAB_APPEARANCE_FEATURE 
    ADD F_KEYWORDS NVARCHAR(1000) NULL;
    
    PRINT '字段 F_KEYWORDS 添加成功';
END
ELSE
BEGIN
    PRINT '字段 F_KEYWORDS 已存在，跳过添加';
END
GO

-- 2. 删除程度变体字段（如果存在）
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'LAB_APPEARANCE_FEATURE') 
    AND name = 'F_VARIANTS'
)
BEGIN
    ALTER TABLE LAB_APPEARANCE_FEATURE 
    DROP COLUMN F_VARIANTS;
    
    PRINT '字段 F_VARIANTS 删除成功';
END
ELSE
BEGIN
    PRINT '字段 F_VARIANTS 不存在，跳过删除';
END
GO

-- 3. 创建特性与等级关联表
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL') AND type in (N'U'))
BEGIN
    CREATE TABLE LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL (
        F_ID NVARCHAR(50) NOT NULL PRIMARY KEY,
        F_FEATURE_ID NVARCHAR(50) NOT NULL,
        F_SEVERITY_LEVEL_ID NVARCHAR(50) NOT NULL,
        F_VARIANT_NAME NVARCHAR(100) NULL,
        F_SORTCODE BIGINT NULL,
        F_CREATORTIME DATETIME NULL,
        F_CREATORUSERID NVARCHAR(50) NULL,
        F_LASTMODIFYTIME DATETIME NULL,
        F_LASTMODIFYUSERID NVARCHAR(50) NULL,
        F_DELETEMARK BIT NULL DEFAULT 0
    );
    
    CREATE INDEX IDX_FEATURE_ID ON LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL(F_FEATURE_ID);
    CREATE INDEX IDX_SEVERITY_LEVEL_ID ON LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL(F_SEVERITY_LEVEL_ID);
    CREATE INDEX IDX_FEATURE_SEVERITY ON LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL(F_FEATURE_ID, F_SEVERITY_LEVEL_ID);
    CREATE INDEX IDX_DELETEMARK ON LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL(F_DELETEMARK);
    
    PRINT '表 LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL 创建成功';
END
ELSE
BEGIN
    PRINT '表 LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL 已存在，跳过创建';
END
GO
*/

-- ============================================
-- 说明
-- ============================================
-- 1. 关键字字段（F_KEYWORDS）：用于存储特性的关键词列表，支持JSON数组或逗号分隔格式
-- 2. 移除了程度变体字段（F_VARIANTS）：不再使用JSON存储变体信息
-- 3. 新增关联表（LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL）：
--    - 实现特性与等级的多对多关系
--    - 支持为每个特性-等级组合设置独立的变体名称
--    - 支持排序功能
-- 4. 数据迁移建议：
--    - 如果现有数据中有 F_VARIANTS 字段，需要编写脚本将JSON数据迁移到关联表
--    - 迁移时，解析 JSON 中的每个变体，创建对应的关联记录
