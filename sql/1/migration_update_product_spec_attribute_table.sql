-- ============================================
-- 产品规格扩展属性表 - 更新脚本
-- 说明: 
--   1. 移除 F_MIN_VALUE 和 F_MAX_VALUE 字段
--   2. 添加 F_IS_PUBLIC 字段
--   3. 更新 F_VALUE_TYPE 注释（decimal、int、text）
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 添加 F_IS_PUBLIC 字段
ALTER TABLE `LAB_PRODUCT_SPEC_ATTRIBUTE`
ADD COLUMN `F_IS_PUBLIC` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否公共属性（从公共属性定义创建）' AFTER `F_IS_CORE`;

-- 移除 F_MIN_VALUE 和 F_MAX_VALUE 字段
ALTER TABLE `LAB_PRODUCT_SPEC_ATTRIBUTE`
DROP COLUMN `F_MIN_VALUE`,
DROP COLUMN `F_MAX_VALUE`;

-- 更新 F_VALUE_TYPE 注释
ALTER TABLE `LAB_PRODUCT_SPEC_ATTRIBUTE`
MODIFY COLUMN `F_VALUE_TYPE` VARCHAR(20) NOT NULL COMMENT '属性值类型（decimal、int、text）';

-- ============================================
-- SQL Server版本
-- ============================================

/*
ALTER TABLE [LAB_PRODUCT_SPEC_ATTRIBUTE]
ADD [F_IS_PUBLIC] BIT NOT NULL DEFAULT 0;

ALTER TABLE [LAB_PRODUCT_SPEC_ATTRIBUTE]
DROP COLUMN [F_MIN_VALUE], [F_MAX_VALUE];
*/

-- ============================================
-- PostgreSQL版本
-- ============================================

/*
ALTER TABLE "LAB_PRODUCT_SPEC_ATTRIBUTE"
ADD COLUMN "F_IS_PUBLIC" BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE "LAB_PRODUCT_SPEC_ATTRIBUTE"
DROP COLUMN "F_MIN_VALUE",
DROP COLUMN "F_MAX_VALUE";
*/
