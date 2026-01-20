-- ============================================
-- 原始数据表 - 添加断头数和单卷重量字段
-- 说明:
--   1. 新增 F_BREAK_COUNT 字段用于存储断头数(个) - 对应Excel的AA列
--   2. 新增 F_SINGLE_COIL_WEIGHT 字段用于存储单卷重量(kg) - 对应Excel的AB列
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 添加断头数字段
ALTER TABLE `LAB_RAW_DATA` 
ADD COLUMN `F_BREAK_COUNT` INT NULL COMMENT '断头数(个)' 
AFTER `F_COIL_WEIGHT`;

-- 添加单卷重量字段
ALTER TABLE `LAB_RAW_DATA` 
ADD COLUMN `F_SINGLE_COIL_WEIGHT` DECIMAL(18, 2) NULL COMMENT '单卷重量(kg)' 
AFTER `F_BREAK_COUNT`;
