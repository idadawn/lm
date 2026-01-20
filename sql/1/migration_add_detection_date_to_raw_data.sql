-- ============================================
-- 原始数据表 - 添加检测日期字段
-- 说明:
--   1. 新增 F_DETECTION_DATE 字段用于存储检测日期（从Excel读取）
--   2. F_PROD_DATE 字段将使用从炉号解析出的日期
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 添加检测日期字段
ALTER TABLE `LAB_RAW_DATA` 
ADD COLUMN `F_DETECTION_DATE` DATETIME NULL COMMENT '检测日期（从Excel读取）' 
AFTER `F_PROD_DATE`;

-- 添加索引（可选，用于查询优化）
ALTER TABLE `LAB_RAW_DATA` 
ADD INDEX `idx_detection_date` (`F_DETECTION_DATE`);

-- ============================================
-- 数据迁移说明
-- ============================================
-- 如果需要将现有的 F_PROD_DATE 数据迁移到 F_DETECTION_DATE：
-- UPDATE LAB_RAW_DATA SET F_DETECTION_DATE = F_PROD_DATE WHERE F_DETECTION_DATE IS NULL;
-- 
-- 然后更新 F_PROD_DATE 为从炉号解析出的日期（需要根据实际业务逻辑执行）
