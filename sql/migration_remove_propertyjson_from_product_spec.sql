-- ============================================
-- 产品规格表 - 移除 PropertyJson 字段迁移脚本
-- 说明: 
--   1. 扩展属性已迁移到 LAB_PRODUCT_SPEC_ATTRIBUTE 表
--   2. 删除 F_PROPERTYJSON 字段
-- 执行前提: 确保所有数据已迁移到属性表
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 检查并删除 F_PROPERTYJSON 字段（如果存在）
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'LAB_PRODUCT_SPEC'
      AND COLUMN_NAME = 'F_PROPERTYJSON'
);

SET @sql = IF(@column_exists > 0,
    'ALTER TABLE `LAB_PRODUCT_SPEC` DROP COLUMN `F_PROPERTYJSON`',
    'SELECT ''Column F_PROPERTYJSON does not exist'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================
-- SQL Server版本
-- ============================================

/*
-- 检查并删除 F_PROPERTYJSON 字段（如果存在）
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'LAB_PRODUCT_SPEC'
      AND COLUMN_NAME = 'F_PROPERTYJSON'
)
BEGIN
    ALTER TABLE [LAB_PRODUCT_SPEC] DROP COLUMN [F_PROPERTYJSON];
END
*/

-- ============================================
-- PostgreSQL版本
-- ============================================

/*
-- 检查并删除 F_PROPERTYJSON 字段（如果存在）
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'LAB_PRODUCT_SPEC'
          AND column_name = 'F_PROPERTYJSON'
    ) THEN
        ALTER TABLE "LAB_PRODUCT_SPEC" DROP COLUMN "F_PROPERTYJSON";
    END IF;
END $$;
*/

-- ============================================
-- 完成提示
-- ============================================
SELECT 'PropertyJson field removed successfully!' AS message;
