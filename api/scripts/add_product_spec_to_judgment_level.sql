-- ============================================================
-- 判定等级表增加产品规格字段
-- 表名：LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL
-- 数据库：MySQL
-- ============================================================

-- 方式1：直接添加（如果列已存在会报错，但不影响数据）
ALTER TABLE LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL 
ADD COLUMN F_PRODUCT_SPEC_ID VARCHAR(50) NULL COMMENT '产品规格ID';

ALTER TABLE LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL 
ADD COLUMN F_PRODUCT_SPEC_NAME VARCHAR(100) NULL COMMENT '产品规格名称(冗余字段)';

-- ============================================================
-- 方式2：使用存储过程判断列是否存在（可选）
-- ============================================================
/*
DROP PROCEDURE IF EXISTS add_product_spec_columns;

DELIMITER //
CREATE PROCEDURE add_product_spec_columns()
BEGIN
    -- 检查并添加 F_PRODUCT_SPEC_ID 列
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL' 
        AND COLUMN_NAME = 'F_PRODUCT_SPEC_ID'
    ) THEN
        ALTER TABLE LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL 
        ADD COLUMN F_PRODUCT_SPEC_ID VARCHAR(50) NULL COMMENT '产品规格ID';
    END IF;
    
    -- 检查并添加 F_PRODUCT_SPEC_NAME 列
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL' 
        AND COLUMN_NAME = 'F_PRODUCT_SPEC_NAME'
    ) THEN
        ALTER TABLE LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL 
        ADD COLUMN F_PRODUCT_SPEC_NAME VARCHAR(100) NULL COMMENT '产品规格名称(冗余字段)';
    END IF;
END //
DELIMITER ;

CALL add_product_spec_columns();
DROP PROCEDURE IF EXISTS add_product_spec_columns;
*/
