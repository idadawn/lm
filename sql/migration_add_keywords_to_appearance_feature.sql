-- ============================================
-- 外观特性表添加关键词字段脚本
-- 表名: LAB_APPEARANCE_FEATURE
-- 说明: 为外观特性表添加关键词字段，用于精确匹配
-- 创建时间: 2026-01-11
-- ============================================

-- ============================================
-- MySQL版本（推荐）
-- ============================================
-- 检查字段是否已存在，如果不存在则添加
-- MySQL语法
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
    CONCAT('ALTER TABLE ', @tablename, ' ADD ', @columnname, ' VARCHAR(1000) NULL COMMENT ''关键词列表（JSON数组或逗号分隔），用于精确匹配''')
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- ============================================
-- SQL Server版本（备用）
-- ============================================
/*
-- 检查字段是否已存在，如果不存在则添加
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'LAB_APPEARANCE_FEATURE') 
    AND name = 'F_KEYWORDS'
)
BEGIN
    -- 添加关键词字段
    -- 存储格式：JSON数组字符串，如：["脆", "碎", "易断", "发脆"]
    -- 也可以存储逗号分隔的字符串，系统会自动解析
    ALTER TABLE LAB_APPEARANCE_FEATURE 
    ADD F_KEYWORDS NVARCHAR(1000) NULL;
    
    PRINT '字段 F_KEYWORDS 添加成功';
END
ELSE
BEGIN
    PRINT '字段 F_KEYWORDS 已存在，跳过添加';
END
GO
*/

-- ============================================
-- 字段注释
-- ============================================

-- MySQL字段注释已在ALTER TABLE语句中添加（使用COMMENT）
-- 如果使用SQL Server，可以取消下面的注释来添加扩展属性
/*
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'关键词列表（JSON数组或逗号分隔），用于精确匹配，如：["脆", "碎", "易断", "发脆"]', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE', 
    @level2type = N'COLUMN', @level2name = N'F_KEYWORDS';
*/

-- ============================================
-- 使用说明
-- ============================================
-- 1. 关键词字段用于规则引擎的精确匹配
-- 2. 存储格式：
--    - JSON数组：["脆", "碎", "易断", "发脆"]
--    - 逗号分隔：脆,碎,易断,发脆
-- 3. 系统会自动解析两种格式
-- 4. 建议为每个特性配置3-10个常用关键词
-- 5. 关键词应该包括：
--    - 特性的常见表述方式
--    - 同义词
--    - 相关描述词
