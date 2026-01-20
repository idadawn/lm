-- ============================================
-- 外观特性表移除关键词字段脚本
-- 表名: LAB_APPEARANCE_FEATURE
-- 说明: 移除外观特性表中的关键词字段，关键词功能将单独实现
-- 创建时间: 2026-01-11
-- ============================================

-- ============================================
-- MySQL版本（推荐）
-- ============================================
-- 检查字段是否存在，如果存在则删除
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
    CONCAT('ALTER TABLE ', @tablename, ' DROP COLUMN ', @columnname),
    'SELECT 1'
));
PREPARE alterIfExists FROM @preparedStatement;
EXECUTE alterIfExists;
DEALLOCATE PREPARE alterIfExists;

-- ============================================
-- SQL Server版本（备用）
-- ============================================
/*
-- 检查字段是否存在，如果存在则删除
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'LAB_APPEARANCE_FEATURE') 
    AND name = 'F_KEYWORDS'
)
BEGIN
    ALTER TABLE LAB_APPEARANCE_FEATURE 
    DROP COLUMN F_KEYWORDS;
    
    PRINT '字段 F_KEYWORDS 删除成功';
END
ELSE
BEGIN
    PRINT '字段 F_KEYWORDS 不存在，跳过删除';
END
GO
*/

-- ============================================
-- 说明
-- ============================================
-- 关键词功能已从外观特性表中移除
-- 后续将单独实现关键词对应特征等级的功能模块
