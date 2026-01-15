-- ============================================
-- 修复密度单位换算比例（当基准单位从 kg/m³ 改为 g/cm³ 后）
-- 说明: 
--   1. 如果 g/cm³ 是基准单位，需要修复其他单位的换算比例
--   2. 如果 kg/m³ 不是基准单位，需要修复它的换算比例
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 检查当前状态
SELECT 
  ud.`F_ID` AS unit_id,
  ud.`F_NAME` AS unit_name,
  ud.`F_SYMBOL` AS unit_symbol,
  ud.`F_IS_BASE` AS is_base,
  ud.`F_SCALE_TO_BASE` AS scale_to_base,
  uc.`F_NAME` AS category_name
FROM `UNIT_DEFINITION` ud
INNER JOIN `UNIT_CATEGORY` uc ON ud.`F_CATEGORY_ID` = uc.`F_ID`
WHERE uc.`F_CODE` = 'DENSITY'
  AND ud.`F_DeleteMark` = 0
  AND ud.`F_ENABLEDMARK` = 1
ORDER BY ud.`F_SORTCODE`;

-- ============================================
-- 修复方案1: 如果 g/cm³ 是基准单位，修复其他单位
-- ============================================

-- 假设 g/cm³ 是基准单位（F_IS_BASE = 1），需要修复：
-- 1. kg/m³ 的换算比例应该是 0.001（1 kg/m³ = 0.001 g/cm³）
-- 2. g/L 的换算比例应该是 0.001（1 g/L = 0.001 g/cm³）

-- 修复 kg/m³（如果它不是基准单位）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 0.0010000000,
  `F_IS_BASE` = 0,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_KG_M3'
  AND `F_IS_BASE` != 1
  AND `F_DeleteMark` = 0;

-- 修复 g/cm³（确保它是基准单位，换算比例为1）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 1.0000000000,
  `F_IS_BASE` = 1,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_G_CM3'
  AND `F_DeleteMark` = 0;

-- 修复 g/L（1 g/L = 0.001 g/cm³，因为 1 L = 1000 cm³）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 0.0010000000,
  `F_IS_BASE` = 0,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_G_L'
  AND `F_DeleteMark` = 0;

-- ============================================
-- 修复方案2: 如果 kg/m³ 应该是基准单位，恢复原始状态
-- ============================================

/*
-- 恢复 kg/m³ 为基准单位
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 1.0000000000,
  `F_IS_BASE` = 1,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_KG_M3'
  AND `F_DeleteMark` = 0;

-- 修复 g/cm³（1 g/cm³ = 1000 kg/m³）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 1000.0000000000,
  `F_IS_BASE` = 0,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_G_CM3'
  AND `F_DeleteMark` = 0;

-- 修复 g/L（1 g/L = 1 kg/m³）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 1.0000000000,
  `F_IS_BASE` = 0,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_G_L'
  AND `F_DeleteMark` = 0;
*/

-- ============================================
-- 验证修复结果
-- ============================================

SELECT 
  ud.`F_ID` AS unit_id,
  ud.`F_NAME` AS unit_name,
  ud.`F_SYMBOL` AS unit_symbol,
  ud.`F_IS_BASE` AS is_base,
  ud.`F_SCALE_TO_BASE` AS scale_to_base,
  CASE 
    WHEN ud.`F_IS_BASE` = 1 THEN '基准单位'
    ELSE CONCAT('1 ', ud.`F_SYMBOL`, ' = ', ud.`F_SCALE_TO_BASE`, ' 基准单位')
  END AS conversion_info,
  uc.`F_NAME` AS category_name
FROM `UNIT_DEFINITION` ud
INNER JOIN `UNIT_CATEGORY` uc ON ud.`F_CATEGORY_ID` = uc.`F_ID`
WHERE uc.`F_CODE` = 'DENSITY'
  AND ud.`F_DeleteMark` = 0
  AND ud.`F_ENABLEDMARK` = 1
ORDER BY ud.`F_IS_BASE` DESC, ud.`F_SORTCODE`;

-- ============================================
-- 换算验证示例
-- ============================================

/*
-- 验证换算：
-- 1 g/cm³ = ? kg/m³
-- 如果 g/cm³ 是基准单位，ScaleToBase = 1
-- 如果 kg/m³ 的 ScaleToBase = 0.001
-- 那么：1 g/cm³ = 1 / 0.001 = 1000 kg/m³ ✓

-- 1 kg/m³ = ? g/cm³
-- 如果 kg/m³ 的 ScaleToBase = 0.001（相对于 g/cm³）
-- 那么：1 kg/m³ = 0.001 g/cm³ ✓
*/
