-- ============================================
-- 为产品规格属性表添加单位ID字段
-- 说明: 
--   1. 添加 F_UNIT_ID 字段，关联单位定义表
--   2. 保留 F_UNIT 字段用于向后兼容
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 为产品规格扩展属性表添加单位ID字段
ALTER TABLE `LAB_PRODUCT_SPEC_ATTRIBUTE` 
ADD COLUMN `F_UNIT_ID` VARCHAR(50) NULL COMMENT '单位ID（关联单位定义表）' AFTER `F_UNIT`;

-- 添加索引
ALTER TABLE `LAB_PRODUCT_SPEC_ATTRIBUTE` 
ADD INDEX `IDX_UNIT_ID` (`F_UNIT_ID`);

-- 为产品规格公共属性定义表添加单位ID字段
ALTER TABLE `LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE` 
ADD COLUMN `F_UNIT_ID` VARCHAR(50) NULL COMMENT '单位ID（关联单位定义表）' AFTER `F_UNIT`;

-- 添加索引
ALTER TABLE `LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE` 
ADD INDEX `IDX_UNIT_ID` (`F_UNIT_ID`);

-- ============================================
-- SQL Server版本
-- ============================================

/*
-- 为产品规格扩展属性表添加单位ID字段
ALTER TABLE [LAB_PRODUCT_SPEC_ATTRIBUTE] 
ADD [F_UNIT_ID] VARCHAR(50) NULL;

-- 添加索引
CREATE INDEX [IDX_UNIT_ID] ON [LAB_PRODUCT_SPEC_ATTRIBUTE] ([F_UNIT_ID]);

-- 为产品规格公共属性定义表添加单位ID字段
ALTER TABLE [LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE] 
ADD [F_UNIT_ID] VARCHAR(50) NULL;

-- 添加索引
CREATE INDEX [IDX_UNIT_ID] ON [LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE] ([F_UNIT_ID]);
*/

-- ============================================
-- PostgreSQL版本
-- ============================================

/*
-- 为产品规格扩展属性表添加单位ID字段
ALTER TABLE "LAB_PRODUCT_SPEC_ATTRIBUTE" 
ADD COLUMN "F_UNIT_ID" VARCHAR(50) NULL;

-- 添加索引
CREATE INDEX "IDX_UNIT_ID" ON "LAB_PRODUCT_SPEC_ATTRIBUTE" ("F_UNIT_ID");

-- 为产品规格公共属性定义表添加单位ID字段
ALTER TABLE "LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE" 
ADD COLUMN "F_UNIT_ID" VARCHAR(50) NULL;

-- 添加索引
CREATE INDEX "IDX_UNIT_ID" ON "LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE" ("F_UNIT_ID");
*/
