-- ============================================
-- 原始数据表排序字段优化 - 数据库修改语句
-- 数据库类型：MySQL
-- 修改日期：2025-01-XX
-- 说明：添加数字字段用于排序优化
-- ============================================

-- ============================================
-- 1. 为 LAB_RAW_DATA 表添加数字排序字段
-- ============================================

-- 添加产线数字字段（用于排序）
ALTER TABLE `LAB_RAW_DATA`
ADD COLUMN `F_LINE_NO_NUMERIC` INT DEFAULT NULL COMMENT '产线数字（用于排序）' AFTER `F_LINE_NO`;

-- 添加班次数字字段（用于排序：甲=1, 乙=2, 丙=3）
ALTER TABLE `LAB_RAW_DATA`
ADD COLUMN `F_SHIFT_NUMERIC` INT DEFAULT NULL COMMENT '班次数字（用于排序：甲=1, 乙=2, 丙=3）' AFTER `F_SHIFT`;

-- 添加炉号数字字段（用于排序）
ALTER TABLE `LAB_RAW_DATA`
ADD COLUMN `F_FURNACE_NO_PARSED_NUMERIC` INT DEFAULT NULL COMMENT '炉号数字（用于排序）' AFTER `F_FURNACE_NO_PARSED`;

-- 添加卷号数字字段（用于排序）
ALTER TABLE `LAB_RAW_DATA`
ADD COLUMN `F_COIL_NO_NUMERIC` INT DEFAULT NULL COMMENT '卷号数字（用于排序）' AFTER `F_COIL_NO`;

-- 添加分卷号数字字段（用于排序）
ALTER TABLE `LAB_RAW_DATA`
ADD COLUMN `F_SUBCOIL_NO_NUMERIC` INT DEFAULT NULL COMMENT '分卷号数字（用于排序）' AFTER `F_SUBCOIL_NO`;

-- ============================================
-- 2. 为数字字段添加索引（可选，用于优化排序查询）
-- ============================================

-- 添加产线数字索引
CREATE INDEX `idx_line_no_numeric` ON `LAB_RAW_DATA` (`F_LINE_NO_NUMERIC`);

-- 添加班次数字索引
CREATE INDEX `idx_shift_numeric` ON `LAB_RAW_DATA` (`F_SHIFT_NUMERIC`);

-- 添加炉号数字索引
CREATE INDEX `idx_furnace_no_parsed_numeric` ON `LAB_RAW_DATA` (`F_FURNACE_NO_PARSED_NUMERIC`);

-- 添加卷号数字索引
CREATE INDEX `idx_coil_no_numeric` ON `LAB_RAW_DATA` (`F_COIL_NO_NUMERIC`);

-- 添加分卷号数字索引
CREATE INDEX `idx_subcoil_no_numeric` ON `LAB_RAW_DATA` (`F_SUBCOIL_NO_NUMERIC`);

-- ============================================
-- 3. 数据迁移：更新现有数据的数字字段（可选）
-- ============================================
-- 注意：此步骤用于更新已存在的数据，将字符串字段转换为数字字段
-- 如果表中已有数据，建议执行此步骤；如果是新表，可跳过

-- 更新产线数字字段
UPDATE `LAB_RAW_DATA`
SET `F_LINE_NO_NUMERIC` = CAST(`F_LINE_NO` AS UNSIGNED)
WHERE `F_LINE_NO` IS NOT NULL 
  AND `F_LINE_NO` REGEXP '^[0-9]+$';

-- 更新班次数字字段（甲=1, 乙=2, 丙=3）
UPDATE `LAB_RAW_DATA`
SET `F_SHIFT_NUMERIC` = CASE
    WHEN `F_SHIFT` LIKE '%甲%' THEN 1
    WHEN `F_SHIFT` LIKE '%乙%' THEN 2
    WHEN `F_SHIFT` LIKE '%丙%' THEN 3
    ELSE NULL
END
WHERE `F_SHIFT` IS NOT NULL;

-- 更新炉号数字字段
UPDATE `LAB_RAW_DATA`
SET `F_FURNACE_NO_PARSED_NUMERIC` = CAST(`F_FURNACE_NO_PARSED` AS UNSIGNED)
WHERE `F_FURNACE_NO_PARSED` IS NOT NULL 
  AND `F_FURNACE_NO_PARSED` REGEXP '^[0-9]+$';

-- 更新卷号数字字段
UPDATE `LAB_RAW_DATA`
SET `F_COIL_NO_NUMERIC` = CAST(`F_COIL_NO` AS UNSIGNED)
WHERE `F_COIL_NO` IS NOT NULL 
  AND `F_COIL_NO` REGEXP '^[0-9]+$';

-- 更新分卷号数字字段
UPDATE `LAB_RAW_DATA`
SET `F_SUBCOIL_NO_NUMERIC` = CAST(`F_SUBCOIL_NO` AS UNSIGNED)
WHERE `F_SUBCOIL_NO` IS NOT NULL 
  AND `F_SUBCOIL_NO` REGEXP '^[0-9]+$';

-- ============================================
-- 4. 验证数据迁移结果（可选）
-- ============================================

-- 检查产线数字字段更新情况
SELECT 
    COUNT(*) AS total_count,
    COUNT(`F_LINE_NO_NUMERIC`) AS numeric_count,
    COUNT(*) - COUNT(`F_LINE_NO_NUMERIC`) AS null_count
FROM `LAB_RAW_DATA`
WHERE `F_LINE_NO` IS NOT NULL;

-- 检查班次数字字段更新情况
SELECT 
    COUNT(*) AS total_count,
    COUNT(`F_SHIFT_NUMERIC`) AS numeric_count,
    COUNT(*) - COUNT(`F_SHIFT_NUMERIC`) AS null_count
FROM `LAB_RAW_DATA`
WHERE `F_SHIFT` IS NOT NULL;

-- 检查炉号数字字段更新情况
SELECT 
    COUNT(*) AS total_count,
    COUNT(`F_FURNACE_NO_PARSED_NUMERIC`) AS numeric_count,
    COUNT(*) - COUNT(`F_FURNACE_NO_PARSED_NUMERIC`) AS null_count
FROM `LAB_RAW_DATA`
WHERE `F_FURNACE_NO_PARSED` IS NOT NULL;

-- 检查卷号数字字段更新情况
SELECT 
    COUNT(*) AS total_count,
    COUNT(`F_COIL_NO_NUMERIC`) AS numeric_count,
    COUNT(*) - COUNT(`F_COIL_NO_NUMERIC`) AS null_count
FROM `LAB_RAW_DATA`
WHERE `F_COIL_NO` IS NOT NULL;

-- 检查分卷号数字字段更新情况
SELECT 
    COUNT(*) AS total_count,
    COUNT(`F_SUBCOIL_NO_NUMERIC`) AS numeric_count,
    COUNT(*) - COUNT(`F_SUBCOIL_NO_NUMERIC`) AS null_count
FROM `LAB_RAW_DATA`
WHERE `F_SUBCOIL_NO` IS NOT NULL;

-- ============================================
-- 5. 字段说明
-- ============================================
-- 新增字段说明：
-- 
-- 1. F_LINE_NO_NUMERIC: 产线数字（INT）
--    - 用途：用于按数字排序产线（1, 2, 3...）
--    - 来源：从 F_LINE_NO 字段解析
--    - 可为空：是
--
-- 2. F_SHIFT_NUMERIC: 班次数字（INT）
--    - 用途：用于按数字排序班次（甲=1, 乙=2, 丙=3）
--    - 来源：从 F_SHIFT 字段解析
--    - 可为空：是
--    - 转换规则：甲=1, 乙=2, 丙=3
--
-- 3. F_FURNACE_NO_PARSED_NUMERIC: 炉号数字（INT）
--    - 用途：用于按数字排序炉号
--    - 来源：从 F_FURNACE_NO_PARSED 字段解析
--    - 可为空：是
--
-- 4. F_COIL_NO_NUMERIC: 卷号数字（INT）
--    - 用途：用于按数字排序卷号
--    - 来源：从 F_COIL_NO 字段解析
--    - 可为空：是
--
-- 5. F_SUBCOIL_NO_NUMERIC: 分卷号数字（INT）
--    - 用途：用于按数字排序分卷号
--    - 来源：从 F_SUBCOIL_NO 字段解析
--    - 可为空：是
--
-- ============================================
-- 6. 注意事项
-- ============================================
-- 1. 所有新增字段都是可空的（NULL），因为：
--    - 可能存在无法解析的数据
--    - 历史数据可能没有这些字段
--
-- 2. 排序逻辑：
--    - 使用数字字段进行排序，NULL值会排到最后（使用 int.MaxValue）
--    - 保留原有的字符串字段，确保向后兼容
--
-- 3. 数据迁移：
--    - 如果表中已有数据，建议执行第3步的数据迁移脚本
--    - 数据迁移后，新导入的数据会自动填充这些字段
--
-- 4. 索引：
--    - 为数字字段添加了索引，用于优化排序查询
--    - 如果数据量不大，可以考虑不添加索引
--
-- 5. 性能优化：
--    - 使用数字字段排序比字符串排序更高效
--    - 数字字段索引可以显著提升排序查询性能
--
-- ============================================
-- 7. 回滚语句（如果需要）
-- ============================================

-- 删除索引
-- DROP INDEX `idx_line_no_numeric` ON `LAB_RAW_DATA`;
-- DROP INDEX `idx_shift_numeric` ON `LAB_RAW_DATA`;
-- DROP INDEX `idx_furnace_no_parsed_numeric` ON `LAB_RAW_DATA`;
-- DROP INDEX `idx_coil_no_numeric` ON `LAB_RAW_DATA`;
-- DROP INDEX `idx_subcoil_no_numeric` ON `LAB_RAW_DATA`;

-- 删除字段
-- ALTER TABLE `LAB_RAW_DATA` DROP COLUMN `F_LINE_NO_NUMERIC`;
-- ALTER TABLE `LAB_RAW_DATA` DROP COLUMN `F_SHIFT_NUMERIC`;
-- ALTER TABLE `LAB_RAW_DATA` DROP COLUMN `F_FURNACE_NO_PARSED_NUMERIC`;
-- ALTER TABLE `LAB_RAW_DATA` DROP COLUMN `F_COIL_NO_NUMERIC`;
-- ALTER TABLE `LAB_RAW_DATA` DROP COLUMN `F_SUBCOIL_NO_NUMERIC`;
