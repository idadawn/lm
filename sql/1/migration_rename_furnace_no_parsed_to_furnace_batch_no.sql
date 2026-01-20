-- ============================================
-- 原始数据表 - 将炉号字段改名为炉次号
-- 说明:
--   1. 将 F_FURNACE_NO_PARSED 字段改名为 F_FURNACE_BATCH_NO（炉次号）
--   2. 正确的炉号格式：[产线数字][班次汉字][8位日期]-[炉次号]
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 重命名字段：F_FURNACE_NO_PARSED -> F_FURNACE_BATCH_NO
ALTER TABLE `LAB_RAW_DATA` 
CHANGE COLUMN `F_FURNACE_NO_PARSED` `F_FURNACE_BATCH_NO` INT NULL COMMENT '炉次号（从炉号解析）';

-- 更新索引（如果有的话）
-- 先删除旧索引（如果存在）
ALTER TABLE `LAB_RAW_DATA` 
DROP INDEX IF EXISTS `idx_furnace_no_parsed`;

-- 添加新索引
ALTER TABLE `LAB_RAW_DATA` 
ADD INDEX `idx_furnace_batch_no` (`F_FURNACE_BATCH_NO`);
