-- ============================================
-- 外观特性大类表 - 将顶级分类的父级ID更新为 "-1"
-- 说明: 将 ParentId 为 NULL 的记录更新为 "-1"，与 OrganizeEntity 保持一致
-- 创建时间: 2026-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 将 ParentId 为 NULL 的记录更新为 "-1"
UPDATE `LAB_APPEARANCE_FEATURE_CATEGORY`
SET `F_PARENTID` = '-1'
WHERE (`F_PARENTID` IS NULL OR `F_PARENTID` = '')
  AND `F_DeleteMark` IS NULL;

-- ============================================
-- SQL Server版本
-- ============================================

/*
-- 将 ParentId 为 NULL 的记录更新为 "-1"
UPDATE LAB_APPEARANCE_FEATURE_CATEGORY
SET F_PARENTID = '-1'
WHERE (F_PARENTID IS NULL OR F_PARENTID = '')
  AND F_DeleteMark IS NULL;
*/

-- ============================================
-- 验证数据
-- ============================================

-- 检查是否还有 ParentId 为 NULL 的记录（应该为0）
SELECT COUNT(*) AS NullParentIdCount
FROM LAB_APPEARANCE_FEATURE_CATEGORY
WHERE F_DeleteMark IS NULL 
  AND (F_PARENTID IS NULL OR F_PARENTID = '');

-- 检查顶级分类的数量
SELECT COUNT(*) AS TopLevelCategoryCount
FROM LAB_APPEARANCE_FEATURE_CATEGORY
WHERE F_DeleteMark IS NULL 
  AND F_PARENTID = '-1';

-- ============================================
-- 使用说明
-- ============================================
-- 1. 执行此脚本前，请确保已执行 Path 字段的迁移脚本
-- 2. 执行前请备份数据库
-- 3. 执行后请验证数据正确性
-- 4. 此脚本将把所有顶级分类的 ParentId 设置为 "-1"
