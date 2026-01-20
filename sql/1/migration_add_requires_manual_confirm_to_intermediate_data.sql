-- 为中间数据表添加"需要人工确认"字段
-- 用于标识特性匹配置信度 < 90% 的数据，需要人工确认

-- MySQL
ALTER TABLE `LAB_INTERMEDIATE_DATA`
ADD COLUMN `F_REQUIRES_MANUAL_CONFIRM` TINYINT(1) NULL DEFAULT NULL COMMENT '是否需要人工确认（特性匹配置信度 < 90% 时需要人工确认）' AFTER `F_REMARK`;

-- SQL Server
-- ALTER TABLE [LAB_INTERMEDIATE_DATA]
-- ADD [F_REQUIRES_MANUAL_CONFIRM] BIT NULL DEFAULT NULL;

-- 说明：
-- 1. 该字段用于标识特性匹配置信度 < 90% 的数据
-- 2. true 表示需要人工确认，false 表示不需要，NULL 表示未设置
-- 3. 在生成中间数据时，会根据原始数据的 MatchConfidence 自动设置该字段
