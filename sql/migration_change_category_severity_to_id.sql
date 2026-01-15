-- ============================================
-- 外观特性表：将特性大类和特性等级改为使用ID
-- 表名: LAB_APPEARANCE_FEATURE
-- 说明: 
--   1. 将 F_CATEGORY 改为 F_CATEGORY_ID（外键）
--   2. 将 F_SEVERITY_LEVEL 改为 F_SEVERITY_LEVEL_ID（外键）
--   3. 更新唯一索引为使用ID
-- 创建时间: 2026-01-11
-- ============================================

-- ============================================
-- MySQL版本（推荐）
-- ============================================

-- 1. 添加新字段 F_CATEGORY_ID
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

-- 2. 添加新字段 F_SEVERITY_LEVEL_ID
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

-- 3. 数据迁移：根据名称查找ID并填充新字段
-- 注意：此步骤需要确保所有名称都能找到对应的ID，否则会失败
UPDATE LAB_APPEARANCE_FEATURE f
INNER JOIN LAB_APPEARANCE_FEATURE_CATEGORY c ON f.F_CATEGORY = c.F_NAME AND c.F_DELETEMARK IS NULL
SET f.F_CATEGORY_ID = c.F_ID
WHERE f.F_CATEGORY_ID IS NULL OR f.F_CATEGORY_ID = '';

UPDATE LAB_APPEARANCE_FEATURE f
INNER JOIN LAB_APPEARANCE_FEATURE_LEVEL s ON f.F_SEVERITY_LEVEL = s.F_NAME AND s.F_DELETEMARK IS NULL
SET f.F_SEVERITY_LEVEL_ID = s.F_ID
WHERE f.F_SEVERITY_LEVEL_ID IS NULL OR f.F_SEVERITY_LEVEL_ID = '';

-- 4. 删除旧唯一索引（如果存在）
SET @indexname = 'UK_CATEGORY_NAME_SEVERITY';
SET @preparedStatement = (SELECT IF(
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
        WHERE
            (TABLE_SCHEMA = @dbname)
            AND (TABLE_NAME = @tablename)
            AND (INDEX_NAME = @indexname)
    ) > 0,
    CONCAT('ALTER TABLE ', @tablename, ' DROP INDEX ', @indexname),
    'SELECT 1'
));
PREPARE dropIndexIfExists FROM @preparedStatement;
EXECUTE dropIndexIfExists;
DEALLOCATE PREPARE dropIndexIfExists;

-- 5. 创建新的唯一索引：F_CATEGORY_ID + F_NAME + F_SEVERITY_LEVEL_ID
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
DEALLOCATE PREPARE createIndexIfExists;

-- 6. 删除旧字段（可选，建议先保留一段时间用于回滚）
-- ALTER TABLE LAB_APPEARANCE_FEATURE DROP COLUMN F_CATEGORY;
-- ALTER TABLE LAB_APPEARANCE_FEATURE DROP COLUMN F_SEVERITY_LEVEL;

-- ============================================
-- SQL Server版本（备用）
-- ============================================
/*
-- 1. 添加新字段 F_CATEGORY_ID
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

-- 2. 添加新字段 F_SEVERITY_LEVEL_ID
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

-- 3. 数据迁移
UPDATE f
SET f.F_CATEGORY_ID = c.F_ID
FROM LAB_APPEARANCE_FEATURE f
INNER JOIN LAB_APPEARANCE_FEATURE_CATEGORY c ON f.F_CATEGORY = c.F_NAME AND c.F_DELETEMARK IS NULL
WHERE f.F_CATEGORY_ID IS NULL OR f.F_CATEGORY_ID = '';
GO

UPDATE f
SET f.F_SEVERITY_LEVEL_ID = s.F_ID
FROM LAB_APPEARANCE_FEATURE f
INNER JOIN LAB_APPEARANCE_FEATURE_LEVEL s ON f.F_SEVERITY_LEVEL = s.F_NAME AND s.F_DELETEMARK IS NULL
WHERE f.F_SEVERITY_LEVEL_ID IS NULL OR f.F_SEVERITY_LEVEL_ID = '';
GO

-- 4. 删除旧唯一索引
IF EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE object_id = OBJECT_ID(N'LAB_APPEARANCE_FEATURE') 
    AND name = 'UK_CATEGORY_NAME_SEVERITY'
)
BEGIN
    DROP INDEX UK_CATEGORY_NAME_SEVERITY ON LAB_APPEARANCE_FEATURE;
    PRINT '索引 UK_CATEGORY_NAME_SEVERITY 已删除';
END
GO

-- 5. 创建新的唯一索引
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
GO
*/

-- ============================================
-- 说明
-- ============================================
-- 1. 数据迁移步骤会将现有的名称字段转换为ID
-- 2. 建议在迁移后验证数据完整性
-- 3. 旧字段（F_CATEGORY, F_SEVERITY_LEVEL）建议保留一段时间后再删除，用于回滚
