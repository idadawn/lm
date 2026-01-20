-- ============================================
-- 外观特性表结构简化脚本
-- 表名: LAB_APPEARANCE_FEATURE
-- 说明: 
--   1. 移除关联表，直接在特性表中添加特性等级ID字段
--   2. 特性大类和特性等级使用ID存储（外键）
--   3. 唯一性约束：特征大类ID + 特性名称 + 特性等级ID 组合唯一
--   4. 保留关键字列表字段
--   5. 前端展示时通过关联查询获取名称
-- 创建时间: 2026-01-11
-- ============================================

-- ============================================
-- MySQL版本（推荐）
-- ============================================

-- 1. 删除关联表（如果存在）
DROP TABLE IF EXISTS `LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL`;

-- 2. 添加特性大类ID字段（如果不存在）
SET @dbname = DATABASE();
SET @tablename = 'LAB_APPEARANCE_FEATURE';
SET @columnname = 'F_CATEGORY_ID';
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = @columnname)
    ) > 0,
    'SELECT 1',
    CONCAT('ALTER TABLE ', @tablename, ' ADD ', @columnname, ' VARCHAR(50) NOT NULL COMMENT ''特性大类ID（外键，关联到 LAB_APPEARANCE_FEATURE_CATEGORY 表）'' AFTER F_CATEGORY')
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 3. 添加特性等级ID字段（如果不存在）
SET @columnname = 'F_SEVERITY_LEVEL_ID';
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (COLUMN_NAME = @columnname)
    ) > 0,
    'SELECT 1',
    CONCAT('ALTER TABLE ', @tablename, ' ADD ', @columnname, ' VARCHAR(50) NOT NULL COMMENT ''特性等级ID（外键，关联到 LAB_SEVERITY_LEVEL 表）'' AFTER F_NAME')
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 4. 确保关键字字段存在
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
    CONCAT('ALTER TABLE ', @tablename, ' ADD ', @columnname, ' VARCHAR(1000) NULL COMMENT ''关键词列表（JSON数组或逗号分隔），用于精确匹配'' AFTER F_SEVERITY_LEVEL')
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 5. 创建唯一索引：特征大类ID + 特性名称 + 特性等级ID
SET @indexname = 'UK_CATEGORY_ID_NAME_SEVERITY_ID';
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (INDEX_NAME = @indexname)
    ) > 0,
    'SELECT 1',
    CONCAT('CREATE UNIQUE INDEX ', @indexname, ' ON ', @tablename, ' (F_CATEGORY_ID, F_NAME, F_SEVERITY_LEVEL_ID)')
));
PREPARE createIndexIfNotExists FROM @preparedStatement;
EXECUTE createIndexIfNotExists;
DEALLOCATE PREPARE createIndexIfNotExists;

-- ============================================
-- SQL Server版本（备用）
-- ============================================
/*
-- 1. 删除关联表（如果存在）
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL') AND type in (N'U'))
BEGIN
    DROP TABLE LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL;
    PRINT '表 LAB_APPEARANCE_FEATURE_SEVERITY_LEVEL 已删除';
END
GO

-- 2. 添加特性等级字段
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'LAB_APPEARANCE_FEATURE') 
    AND name = 'F_SEVERITY_LEVEL'
)
BEGIN
    ALTER TABLE LAB_APPEARANCE_FEATURE 
    ADD F_SEVERITY_LEVEL NVARCHAR(50) NOT NULL DEFAULT '默认';
    
    PRINT '字段 F_SEVERITY_LEVEL 添加成功';
END
ELSE
BEGIN
    PRINT '字段 F_SEVERITY_LEVEL 已存在，跳过添加';
END
GO

-- 3. 确保关键字字段存在
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

-- 4. 添加特性大类ID字段
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'LAB_APPEARANCE_FEATURE') 
    AND name = 'F_CATEGORY_ID'
)
BEGIN
    ALTER TABLE LAB_APPEARANCE_FEATURE 
    ADD F_CATEGORY_ID NVARCHAR(50) NOT NULL DEFAULT '';
    
    PRINT '字段 F_CATEGORY_ID 添加成功';
END
GO

-- 5. 添加特性等级ID字段
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'LAB_APPEARANCE_FEATURE') 
    AND name = 'F_SEVERITY_LEVEL_ID'
)
BEGIN
    ALTER TABLE LAB_APPEARANCE_FEATURE 
    ADD F_SEVERITY_LEVEL_ID NVARCHAR(50) NOT NULL DEFAULT '';
    
    PRINT '字段 F_SEVERITY_LEVEL_ID 添加成功';
END
GO

-- 6. 创建唯一索引：特征大类ID + 特性名称 + 特性等级ID
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE object_id = OBJECT_ID(N'LAB_APPEARANCE_FEATURE') 
    AND name = 'UK_CATEGORY_ID_NAME_SEVERITY_ID'
)
BEGIN
    CREATE UNIQUE INDEX UK_CATEGORY_ID_NAME_SEVERITY_ID 
    ON LAB_APPEARANCE_FEATURE (F_CATEGORY_ID, F_NAME, F_SEVERITY_LEVEL_ID);
    
    PRINT '唯一索引 UK_CATEGORY_ID_NAME_SEVERITY_ID 创建成功';
END
ELSE
BEGIN
    PRINT '唯一索引 UK_CATEGORY_ID_NAME_SEVERITY_ID 已存在，跳过创建';
END
GO
*/

-- ============================================
-- 说明
-- ============================================
-- 1. 每个特性记录代表一个具体的"特征大类+特性名称+特性等级"组合
-- 2. 唯一性约束：特征大类 + 特性名称 + 特性等级 组合唯一
-- 3. 关键字字段用于精确匹配
-- 4. 数据迁移建议：
--    - 如果现有数据中有多个等级，需要拆分为多条记录
--    - 例如：原来一条记录"脆"有多个等级，现在需要拆分为：
--      - 韧性, 脆, 默认
--      - 韧性, 脆, 微
--      - 韧性, 脆, 严重
