-- ============================================
-- 产品规格表 - 移除长度、层数、密度字段迁移脚本
-- 说明: 
--   1. 将现有数据中的 F_LENGTH、F_LAYERS、F_DENSITY 迁移到 F_PROPERTYJSON
--   2. 删除 F_LENGTH、F_LAYERS、F_DENSITY 字段
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 步骤1: 将现有数据迁移到 PropertyJson
-- 更新所有记录，将长度、层数、密度添加到 PropertyJson 中
UPDATE `LAB_PRODUCT_SPEC`
SET `F_PROPERTYJSON` = CASE
    -- 如果 PropertyJson 为空或 NULL，创建新的 JSON
    WHEN `F_PROPERTYJSON` IS NULL OR `F_PROPERTYJSON` = '' OR `F_PROPERTYJSON` = '{}' THEN
        JSON_OBJECT(
            'length', IFNULL(`F_LENGTH`, 4),
            'layers', IFNULL(`F_LAYERS`, 20),
            'density', IFNULL(`F_DENSITY`, 7.25)
        )
    -- 如果 PropertyJson 已有数据，合并新字段
    ELSE
        JSON_MERGE_PATCH(
            `F_PROPERTYJSON`,
            JSON_OBJECT(
                'length', IFNULL(`F_LENGTH`, 4),
                'layers', IFNULL(`F_LAYERS`, 20),
                'density', IFNULL(`F_DENSITY`, 7.25)
            )
        )
    END
WHERE `F_DeleteMark` IS NULL;

-- 步骤2: 删除字段
ALTER TABLE `LAB_PRODUCT_SPEC`
DROP COLUMN `F_LENGTH`,
DROP COLUMN `F_LAYERS`,
DROP COLUMN `F_DENSITY`;

-- ============================================
-- SQL Server版本
-- ============================================

/*
-- 步骤1: 将现有数据迁移到 PropertyJson
UPDATE [LAB_PRODUCT_SPEC]
SET [F_PROPERTYJSON] = CASE
    -- 如果 PropertyJson 为空或 NULL，创建新的 JSON
    WHEN [F_PROPERTYJSON] IS NULL OR [F_PROPERTYJSON] = '' OR [F_PROPERTYJSON] = '{}' THEN
        JSON_OBJECT('length', ISNULL([F_LENGTH], 4), 'layers', ISNULL([F_LAYERS], 20), 'density', ISNULL([F_DENSITY], 7.25))
    -- 如果 PropertyJson 已有数据，合并新字段
    ELSE
        JSON_MODIFY(
            JSON_MODIFY(
                JSON_MODIFY([F_PROPERTYJSON], '$.length', ISNULL([F_LENGTH], 4)),
                '$.layers', ISNULL([F_LAYERS], 20)
            ),
            '$.density', ISNULL([F_DENSITY], 7.25)
        )
    END
WHERE [F_DeleteMark] IS NULL;

-- 步骤2: 删除字段
ALTER TABLE [LAB_PRODUCT_SPEC]
DROP COLUMN [F_LENGTH],
DROP COLUMN [F_LAYERS],
DROP COLUMN [F_DENSITY];
*/

-- ============================================
-- PostgreSQL版本
-- ============================================

/*
-- 步骤1: 将现有数据迁移到 PropertyJson
UPDATE "LAB_PRODUCT_SPEC"
SET "F_PROPERTYJSON" = CASE
    -- 如果 PropertyJson 为空或 NULL，创建新的 JSON
    WHEN "F_PROPERTYJSON" IS NULL OR "F_PROPERTYJSON" = '' OR "F_PROPERTYJSON" = '{}' THEN
        jsonb_build_object(
            'length', COALESCE("F_LENGTH", 4),
            'layers', COALESCE("F_LAYERS", 20),
            'density', COALESCE("F_DENSITY", 7.25)
        )::text
    -- 如果 PropertyJson 已有数据，合并新字段
    ELSE
        (
            COALESCE("F_PROPERTYJSON"::jsonb, '{}'::jsonb) ||
            jsonb_build_object(
                'length', COALESCE("F_LENGTH", 4),
                'layers', COALESCE("F_LAYERS", 20),
                'density', COALESCE("F_DENSITY", 7.25)
            )
        )::text
    END
WHERE "F_DeleteMark" IS NULL;

-- 步骤2: 删除字段
ALTER TABLE "LAB_PRODUCT_SPEC"
DROP COLUMN "F_LENGTH",
DROP COLUMN "F_LAYERS",
DROP COLUMN "F_DENSITY";
*/
